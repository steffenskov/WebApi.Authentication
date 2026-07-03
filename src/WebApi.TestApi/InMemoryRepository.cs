using System.Collections.Concurrent;
using System.Security.Claims;
using WebApi.Authentication;
using WebApi.Authentication.Repositories;

namespace Api.WebApi;

public class InMemoryRepository<TApiSecret> : IApiSecretRepository<TApiSecret>
	where TApiSecret : ApiSecret

{
	private readonly ConcurrentDictionary<Guid, TApiSecret> _secrets = new();

	public ValueTask<TApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		var subClaim = claims.FirstOrDefault(claim => claim.Type == "sub");
		if (subClaim is null)
		{
			return ValueTask.FromResult<TApiSecret?>(null);
		}

		if (!Guid.TryParse(subClaim.Value, out var secretId))
		{
			return ValueTask.FromResult<TApiSecret?>(null);
		}

		_secrets.TryGetValue(secretId, out var secret);
		return ValueTask.FromResult(secret);
	}

	public ValueTask PersistAsync(TApiSecret secret, CancellationToken cancellationToken = default)
	{
		_secrets[secret.Id] = secret;
		return ValueTask.CompletedTask;
	}
}