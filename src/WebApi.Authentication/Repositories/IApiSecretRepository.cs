namespace WebApi.Authentication.Repositories;

public interface IApiSecretRepository<TApiSecret>
	where TApiSecret : ApiSecret
{
	/// <summary>
	///     Method to retrieve the ApiSecret from the repository based on claims. It's used by the Authentication middleware.
	///     If you're doing custom Claims and want to lookup secrets based on those claims, this is the method to do so in.
	/// </summary>
	/// <param name="claims">Claims from the JWT token.</param>
	/// <param name="cancellationToken">CancellationToken</param>
	/// <returns>Secret if found, otherwise null</returns>
	ValueTask<TApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default);

	/// <summary>
	///     Persists the ApiSecret to the repository using an Upsert.
	/// </summary>
	/// <param name="secret">Secret to persist</param>
	/// <param name="cancellationToken">CancellationToken</param>
	ValueTask PersistAsync(TApiSecret secret, CancellationToken cancellationToken = default);
}

internal interface IApiSecretRepository
{
	/// <summary>
	///     Method used internally by the middleware to get the ApiSecret from the repository.
	///     You should not directly implement this interface, rather implement <see cref="IApiSecretRepository{TApiSecret}" />.
	/// </summary>
	ValueTask<ApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default);
}