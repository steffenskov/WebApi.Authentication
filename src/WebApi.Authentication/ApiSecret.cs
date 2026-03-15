using WebApi.Authentication.Services;

namespace WebApi.Authentication;

/// <summary>
///     ApiSecret entity, can be inherited to extend with additional properties.
/// </summary>
public class ApiSecret
{
	/// <summary>
	///     Jwt token to use for authentication with this ApiSecret.
	/// </summary>
	public string JwtToken { get; private set; } = "";

	/// <summary>
	///     Whether the ApiSecret has been revoked.
	/// </summary>
	public bool IsRevoked { get; private set; }

	/// <summary>
	///     Id of the the ApiSecret, automatically generated using Guid.CreateVersion7()
	/// </summary>
	public Guid Id { get; } = Guid.CreateVersion7();

	internal bool HasGeneratedToken => !string.IsNullOrWhiteSpace(JwtToken);

	public DateTime Expires { get; private set; }

	/// <summary>
	///     Revokes the secret, preventing it from being used for authentication.
	/// </summary>
	public void Revoke()
	{
		IsRevoked = true;
	}

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
	///     Method to override for adding custom claims to the Jwt token generated.
	/// </summary>
	/// <returns>Custom claims to add to the token</returns>
	public virtual IEnumerable<Claim> GetCustomClaims()
	{
		return [];
	}
}