using WebApi.Authentication.Services;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local - they're required for deserialization purposes from DB

namespace WebApi.Authentication;

/// <summary>
///     Abstract base for ApiSecret and SegregatedApiSecret. Not intended for inheritance outside of the library itself.
/// </summary>
public abstract class BaseApiSecret : IApiSecret
{
	private protected BaseApiSecret()
	{
		// private protected ctor prevents inheritance outside of the project
	}

	internal bool HasGeneratedToken => !string.IsNullOrWhiteSpace(JwtToken);

	/// <summary>
	///     Jwt token to use for authentication with this ApiSecret.
	/// </summary>
	public string JwtToken { get; private set; } = "";

	/// <summary>
	///     Whether the ApiSecret has been revoked.
	/// </summary>
	public bool IsRevoked { get; private set; }

	/// <summary>
	///     Id of the ApiSecret, automatically generated using Guid.CreateVersion7()
	/// </summary>
	public Guid Id { get; private set; } = Guid.CreateVersion7();

	/// <summary>
	///     Date of expiration for the token.
	/// </summary>
	public DateTime Expires { get; private set; }

	/// <summary>
	///     Generates the JwtToken using the provided tokenProvider.
	///     Throws if a token already exists.
	/// </summary>
	/// <param name="tokenProvider">TokenProvider from Dependency Injection</param>
	internal void GenerateJwtToken(IJwtTokenProvider tokenProvider)
	{
		if (HasGeneratedToken)
		{
			throw new InvalidOperationException($"JwtToken already generated for ApiSecret {Id}.");
		}

		var result = tokenProvider.CreateToken(this);
		JwtToken = result.Token;
		Expires = result.Expires;
	}

	/// <summary>
	///     Revokes the secret, preventing it from being used for authentication.
	/// </summary>
	public void Revoke()
	{
		IsRevoked = true;
	}

	/// <summary>
	///     Method to override for adding custom claims to the Jwt token generated.
	/// </summary>
	/// <returns>Custom claims to add to the token</returns>
	public virtual IEnumerable<Claim> GetCustomClaims()
	{
		return [];
	}
}