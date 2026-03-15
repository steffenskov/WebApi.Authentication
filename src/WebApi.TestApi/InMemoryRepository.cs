using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using WebApi.Authentication;
using WebApi.Authentication.Repositories;

namespace Api.WebApi;

public class InMemoryRepository : IApiSecretRepository<ApiSecret>
{
	private readonly ConcurrentDictionary<Guid, ApiSecret> _secrets = new();

	public ValueTask<ApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		var subClaim = claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);
		if (subClaim is null)
		{
			return ValueTask.FromResult<ApiSecret?>(null);
		}

		if (!Guid.TryParse(subClaim.Value, out var secretId))
		{
			return ValueTask.FromResult<ApiSecret?>(null);
		}

		_secrets.TryGetValue(secretId, out var secret);
		return ValueTask.FromResult(secret);
	}

	public ValueTask PersistAsync(ApiSecret secret, CancellationToken cancellationToken = default)
	{
		_secrets[secret.Id] = secret;
		return ValueTask.CompletedTask;
	}
}

public class SegregatedInMemoryRepository : IApiSecretRepository<SegregatedApiSecret>
{
	private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, SegregatedApiSecret>> _customerBuckets = new();

	public ValueTask<SegregatedApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		var customerClaim = claims.FirstOrDefault(claim => claim.Type == "CustomerId");
		if (customerClaim is null)
		{
			return ValueTask.FromResult<SegregatedApiSecret?>(null);
		}

		if (!Guid.TryParse(customerClaim.Value, out var customerId))
		{
			return ValueTask.FromResult<SegregatedApiSecret?>(null);
		}

		if (!_customerBuckets.TryGetValue(customerId, out var customerSecrets))
		{
			return ValueTask.FromResult<SegregatedApiSecret?>(null);
		}

		var subClaim = claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);
		if (subClaim is null)
		{
			return ValueTask.FromResult<SegregatedApiSecret?>(null);
		}

		if (!Guid.TryParse(subClaim.Value, out var secretId))
		{
			return ValueTask.FromResult<SegregatedApiSecret?>(null);
		}

		customerSecrets.TryGetValue(secretId, out var secret);
		return ValueTask.FromResult(secret);
	}

	public ValueTask PersistAsync(SegregatedApiSecret secret, CancellationToken cancellationToken = default)
	{
		var customerSecrets = _customerBuckets.GetOrAdd(secret.CustomerId, _ => new ConcurrentDictionary<Guid, SegregatedApiSecret>());
		customerSecrets[secret.Id] = secret;
		return ValueTask.CompletedTask;
	}
}