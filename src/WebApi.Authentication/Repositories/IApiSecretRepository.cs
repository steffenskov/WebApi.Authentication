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

internal class ApiSecretRepositoryShim<TApiSecret> : IApiSecretRepository
	where TApiSecret : ApiSecret
{
	private readonly IApiSecretRepository<TApiSecret> _innerRepository;

	public ApiSecretRepositoryShim(IApiSecretRepository<TApiSecret> innerRepository)
	{
		_innerRepository = innerRepository;
	}

	public async ValueTask<ApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		var result = await _innerRepository.GetByClaimsAsync(claims, cancellationToken);
		return result;
	}
}