namespace Api.Authentication.Services;

internal interface IJwtTokenProvider
{
	string CreateToken(ApiSecret secret);
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

	public string CreateToken(ApiSecret secret)
	{
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(
			[
				new Claim(JwtRegisteredClaimNames.Sub, secret.Id.ToString())
			]),
			Issuer = _issuer,
			Audience = _audience,
			Expires = DateTime.UtcNow.Add(_expiration),
			SigningCredentials = _signingCredentials
		};

		var handler = new JsonWebTokenHandler();

		return handler.CreateToken(tokenDescriptor);
	}
}