namespace WebApi.Authentication.Repositories;

internal class ApiSecretRepositoryAdapter<TApiSecret> : IApiSecretRepository
	where TApiSecret : ApiSecret
{
	private readonly IApiSecretRepository<TApiSecret> _innerRepository;

	public ApiSecretRepositoryAdapter(IApiSecretRepository<TApiSecret> innerRepository)
	{
		_innerRepository = innerRepository;
	}

	public async ValueTask<ApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		var result = await _innerRepository.GetByClaimsAsync(claims, cancellationToken);
		return result;
	}
}