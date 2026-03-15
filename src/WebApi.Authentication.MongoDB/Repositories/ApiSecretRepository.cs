using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using MongoDB.Driver;
using WebApi.Authentication.Repositories;

namespace WebApi.Authentication.MongoDB.Repositories;

public class ApiSecretRepository<T> : IApiSecretRepository<T>
	where T : ApiSecret
{
	private readonly IMongoCollection<T> _collection;

	public ApiSecretRepository(IMongoDatabase db, string collectionName)
	{
		_collection = db.GetCollection<T>(collectionName);
	}

	/// <summary>
	///     Gets the ApiSecret by the claims of the user. Default implementation only checks for id == "sub" claim.
	///     Override this if you want to use custom claims to lookup the secret.
	/// </summary>
	/// <param name="claims">Claims from Jwt token.</param>
	/// <param name="cancellationToken">CancellationToken</param>
	/// <returns>ApiSecret if found, otherwise null</returns>
	public virtual async ValueTask<T?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		var subClaim = claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);
		if (subClaim is null)
		{
			return null;
		}

		if (!Guid.TryParse(subClaim.Value, out var secretId))
		{
			return null;
		}

		return await _collection.Find(apiSecret => apiSecret.Id == secretId)
			.FirstOrDefaultAsync(cancellationToken);
	}

	/// <summary>
	///     Persists the ApiSecret to the repository via en Upsert. Default implementation only compares by Id.
	///     Override this if you want to use custom logic to persist the secret.
	/// </summary>
	/// <param name="secret">ApiSecret to persist</param>
	/// <param name="cancellationToken">CancellationToken</param>
	public virtual async ValueTask PersistAsync(T secret, CancellationToken cancellationToken = default)
	{
		await _collection.ReplaceOneAsync(apiSecret => apiSecret.Id == secret.Id, secret, new ReplaceOptions { IsUpsert = true }, cancellationToken);
	}
}