namespace WebApi.Authentication.Services;

public interface IApiSecretProvider
{
	ValueTask PersistSecretAsync(IApiSecret secret, CancellationToken cancellationToken = default);
}

internal class ApiSecretProvider<TApiSecret> : IApiSecretProvider
	where TApiSecret : BaseApiSecret
{
	private readonly IApiSecretRepository<TApiSecret> _repository;
	private readonly IJwtTokenProvider _tokenProvider;

	public ApiSecretProvider(IJwtTokenProvider tokenProvider, IApiSecretRepository<TApiSecret> repository)
	{
		_tokenProvider = tokenProvider;
		_repository = repository;
	}

	public async ValueTask PersistSecretAsync(IApiSecret secret, CancellationToken cancellationToken = default)
	{
		if (secret is not TApiSecret actualSecret)
		{
			throw new ArgumentException($"Expected a secret of type {typeof(TApiSecret).Name} but got {secret.GetType().Name}", nameof(secret));
		}

		if (!actualSecret.HasGeneratedToken)
		{
			actualSecret.GenerateJwtToken(_tokenProvider);
		}

		await _repository.PersistAsync(actualSecret, cancellationToken);
	}
}