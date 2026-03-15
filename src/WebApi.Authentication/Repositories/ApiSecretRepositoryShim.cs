namespace WebApi.Authentication.Repositories;

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