namespace WebApi.Authentication.Repositories;

public interface IApiSecretRepository<TApiSecret>
	where TApiSecret : ApiSecret
{
	ValueTask<TApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default);
	ValueTask PersistAsync(TApiSecret secret, CancellationToken cancellationToken = default);
}

internal interface IApiSecretRepository
{
	ValueTask<ApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default);
}