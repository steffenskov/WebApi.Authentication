using System.Collections.Concurrent;
using Api.Authentication;
using Api.Authentication.Repositories;

namespace Api.WebApi;

public class InMemoryRepository : IApiSecretRepository
{
	private readonly ConcurrentDictionary<Guid, ApiSecret> _secrets = new();

	public ValueTask<ApiSecret?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		_secrets.TryGetValue(id, out var secret);
		return ValueTask.FromResult(secret);
	}

	public ValueTask PersistAsync(ApiSecret secret, CancellationToken cancellationToken = default)
	{
		_secrets[secret.Id] = secret;
		return ValueTask.CompletedTask;
	}
}