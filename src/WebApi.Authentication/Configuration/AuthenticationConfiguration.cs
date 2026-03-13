namespace Api.Authentication.Configuration;

public class AuthenticationConfiguration
{
	/// <summary>
	///     Secret key used to sign the token, do not share this with anyone, do not hardcode it, and do not store it in
	///     plain-text!
	///     Consider generating one using System.Security.Cryptography.RandomNumberGenerator with a length of at least 64
	///     bytes.
	/// </summary>
	public required byte[] SecretKey { get; init; }

	/// <summary>
	///     Issuer of the token generated, will be validated.
	/// </summary>
	public required string Issuer { get; init; }

	/// <summary>
	///     Audiance of the token generated, will be validated.
	/// </summary>
	public required string Audience { get; init; }

	/// <summary>
	///     Relative expiration of the token issued.
	/// </summary>
	public required TimeSpan Expiration { get; init; }

	/// <summary>
	///     Algorithm to use, use one of the constants from SecurityAlgorithms.XYZ. Defaults to SecurityAlgorithms.HmacSha512.
	/// </summary>
	public string Algorithm { get; init; } = SecurityAlgorithms.HmacSha512;

	internal SymmetricSecurityKey SigningKey => new(SecretKey);
	internal SigningCredentials SigningCredentials => new(SigningKey, Algorithm);
}