namespace WebApi.Authentication.Services;

public interface IApiSecretProvider
{
	ValueTask PersistSecretAsync(ApiSecret secret, CancellationToken cancellationToken = default);
}

internal class ApiSecretProvider<TApiSecret> : IApiSecretProvider
	where TApiSecret : ApiSecret
{
	private readonly IApiSecretRepository<TApiSecret> _repository;
	private readonly IJwtTokenProvider _tokenProvider;

	public ApiSecretProvider(IJwtTokenProvider tokenProvider, IApiSecretRepository<TApiSecret> repository)
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

		await _repository.PersistAsync((TApiSecret)secret, cancellationToken);
	}
}