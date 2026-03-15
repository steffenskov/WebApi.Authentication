using WebApi.Authentication.Configuration;

namespace WebApi.Authentication.Services;

public interface IJwtTokenProvider
{
	string CreateToken(ApiSecret secret);
}

internal class JwtTokenProvider<T> : IJwtTokenProvider
	where T : ApiSecret
{
	private readonly string _audience;
	private readonly Func<T, IEnumerable<Claim>>? _customClaimsFactory;
	private readonly TimeSpan _expiration;
	private readonly string _issuer;
	private readonly SigningCredentials _signingCredentials;

	public JwtTokenProvider(AuthenticationConfiguration<T> configuration)
	{
		_issuer = configuration.Issuer;
		_audience = configuration.Audience;
		_expiration = configuration.Expiration;
		_signingCredentials = configuration.SigningCredentials;
		_customClaimsFactory = configuration.CustomClaimsFactory;
	}

	public string CreateToken(ApiSecret secret)
	{
		var customClaims = _customClaimsFactory?.Invoke((T)secret) ?? [];

		Claim[] claims = [new(JwtRegisteredClaimNames.Sub, secret.Id.ToString()), ..customClaims];
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Issuer = _issuer,
			Audience = _audience,
			Expires = DateTime.UtcNow.Add(_expiration),
			SigningCredentials = _signingCredentials
		};

		var handler = new JsonWebTokenHandler();

		return handler.CreateToken(tokenDescriptor);
	}
}