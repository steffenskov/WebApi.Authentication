using WebApi.Authentication.Configuration;

namespace WebApi.Authentication.Services;

internal interface IJwtTokenProvider
{
	(string Token, DateTime Expires) CreateToken(ApiSecret secret);
}

internal class JwtTokenProvider : IJwtTokenProvider
{
	private readonly string _audience;
	private readonly TimeSpan _expiration;
	private readonly string _issuer;
	private readonly SigningCredentials _signingCredentials;

	public JwtTokenProvider(AuthenticationConfiguration configuration)
	{
		_issuer = configuration.Issuer;
		_audience = configuration.Audience;
		_expiration = configuration.Expiration;
		_signingCredentials = configuration.SigningCredentials;
	}

	public (string Token, DateTime Expires) CreateToken(ApiSecret secret)
	{
		var customClaims = secret.GetCustomClaims();

		Claim[] claims = [new(JwtRegisteredClaimNames.Sub, secret.Id.ToString()), ..customClaims];

		var expires = DateTime.UtcNow.Add(_expiration);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Issuer = _issuer,
			Audience = _audience,
			Expires = expires,
			SigningCredentials = _signingCredentials
		};

		var handler = new JsonWebTokenHandler();

		return (handler.CreateToken(tokenDescriptor), expires);
	}
}