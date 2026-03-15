using WebApi.Authentication.Repositories;

namespace WebApi.Authentication.Services;

public interface IApiSecretProvider
{
	ValueTask PersistSecretAsync(ApiSecret secret, CancellationToken cancellationToken = default);
}

internal class ApiSecretProvider<T> : IApiSecretProvider
	where T : ApiSecret
{
	private readonly IApiSecretRepository<T> _repository;
	private readonly IJwtTokenProvider _tokenProvider;

	public ApiSecretProvider(IJwtTokenProvider tokenProvider, IApiSecretRepository<T> repository)
	{
		_tokenProvider = tokenProvider;
		_repository = repository;
	}

	public async ValueTask PersistSecretAsync(ApiSecret secret, CancellationToken cancellationToken = default)
	{
		if (!secret.HasGeneratedToken)
		{
			secret.GenerateJwtToken(_tokenProvider);
		}

		await _repository.PersistAsync((T)secret, cancellationToken);
	}
}