using System.Collections.Concurrent;
using System.Globalization;

namespace WebApi.Authentication.Repositories;

internal class SegregatedApiSecretRepositoryAdapter<TKey, TApiSecret, TRepository> : IApiSecretRepository<TApiSecret>
	where TApiSecret : SegregatedApiSecret<TKey>
	where TKey : IParsable<TKey>
	where TRepository : IApiSecretRepository<TApiSecret>
{
	private readonly Func<IServiceProvider, TKey, TRepository> _factoryMethod;
	private readonly Lock _lock = new();

	private readonly ConcurrentDictionary<TKey, TRepository> _repositories = [];
	private readonly IServiceProvider _serviceProvider;

	public SegregatedApiSecretRepositoryAdapter(IServiceProvider serviceProvider, Func<IServiceProvider, TKey, TRepository> factoryMethod)
	{
		_serviceProvider = serviceProvider;
		_factoryMethod = factoryMethod;
	}

	public async ValueTask<TApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		var segregationClaim = claims.SingleOrDefault(claim => claim.Type == Consts.SegregationClaim) ??
		                       throw new InvalidOperationException($"Claims do not contain {Consts.SegregationClaim} used by Segregation. You likely have a dependency injection mismatch somewhere.");
		if (!TKey.TryParse(segregationClaim.Value, CultureInfo.InvariantCulture, out var key))
		{
			throw new InvalidOperationException($"Could not parse a {typeof(TKey).Name} from {segregationClaim.Value}");
		}

		var repository = GetRepository(key);

		return await repository.GetByClaimsAsync(claims, cancellationToken);
	}

	public async ValueTask PersistAsync(TApiSecret secret, CancellationToken cancellationToken = default)
	{
		var repository = GetRepository(secret.Key);

		await repository.PersistAsync(secret, cancellationToken);
	}

	private TRepository GetRepository(TKey key)
	{
		if (!_repositories.TryGetValue(key, out var repository))
		{
			lock (_lock)
			{
				if (!_repositories.TryGetValue(key, out repository))
				{
					_repositories[key] = repository = _factoryMethod.Invoke(_serviceProvider, key);
				}
			}
		}

		return repository;
	}
}