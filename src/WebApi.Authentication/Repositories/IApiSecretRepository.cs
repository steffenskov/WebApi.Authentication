namespace Api.Authentication.Repositories;

public interface IApiSecretRepository
{
	ValueTask<ApiSecret?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	ValueTask PersistAsync(ApiSecret secret, CancellationToken cancellationToken = default);
}