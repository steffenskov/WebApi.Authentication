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