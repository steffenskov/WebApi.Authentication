namespace WebApi.Authentication.Repositories;

public interface IApiSecretRepository<TSecret>
	where TSecret : ApiSecret
{
	ValueTask<TSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default);
	ValueTask PersistAsync(TSecret secret, CancellationToken cancellationToken = default);
}