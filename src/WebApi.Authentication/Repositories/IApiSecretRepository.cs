namespace WebApi.Authentication.Repositories;

public interface IApiSecretRepository<TSecret>
	where TSecret : ApiSecret
{
	ValueTask<TSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default);
	ValueTask PersistAsync(TSecret secret, CancellationToken cancellationToken = default);
}

internal interface IApiSecretRepository
{
	ValueTask<ApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default);
}

internal class ApiSecretRepositoryShim<TSecret> : IApiSecretRepository
	where TSecret : ApiSecret
{
	private readonly IApiSecretRepository<TSecret> _innerRepository;

	public ApiSecretRepositoryShim(IApiSecretRepository<TSecret> innerRepository)
	{
		_innerRepository = innerRepository;
	}

	public async ValueTask<ApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		var result = await _innerRepository.GetByClaimsAsync(claims, cancellationToken);
		return result;
	}
}