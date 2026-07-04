using WebApi.Authentication.Services;

namespace WebApi.Authentication;

public interface IApiSecret
{
	/// <summary>
	///     Jwt token to use for authentication with this ApiSecret.
	/// </summary>
	string JwtToken { get; }

	/// <summary>
	///     Whether the ApiSecret has been revoked.
	/// </summary>
	bool IsRevoked { get; }

	/// <summary>
	///     Id of the the ApiSecret, automatically generated using Guid.CreateVersion7()
	/// </summary>
	Guid Id { get; }

	/// <summary>
	///     Date of expiration for the token.
	/// </summary>
	DateTime Expires { get; }

	internal bool HasGeneratedToken { get; }
	internal void GenerateJwtToken(IJwtTokenProvider tokenProvider);
}