using Api.Authentication.Repositories;

namespace Api.Authentication.Services;

public interface IApiSecretProvider
{
	ValueTask<ApiSecret> CreateSecretAsync(CancellationToken cancellationToken = default);
}

internal class ApiSecretProvider : IApiSecretProvider
{
	private readonly IApiSecretRepository _repository;
	private readonly IJwtTokenProvider _tokenProvider;

	public ApiSecretProvider(IJwtTokenProvider tokenProvider, IApiSecretRepository repository)
	{
		_tokenProvider = tokenProvider;
		_repository = repository;
	}

	public async ValueTask<ApiSecret> CreateSecretAsync(CancellationToken cancellationToken)
	{
		var result = ApiSecret.Create(_tokenProvider);
		await _repository.PersistAsync(result, cancellationToken);

		return result;
	}
}