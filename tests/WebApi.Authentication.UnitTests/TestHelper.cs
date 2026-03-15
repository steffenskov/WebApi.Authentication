namespace WebApi.Authentication.UnitTests;

static internal class TestHelper
{
	public static JwtTokenProvider CreateTokenProvider()
	{
		var configuration = new AuthenticationConfiguration
		{
			SecretKey = RandomNumberGenerator.GetBytes(64),
			Issuer = "WebApi.Authentication",
			Audience = "UnitTests",
			Expiration = TimeSpan.FromMinutes(5)
		};

		var tokenProvider = new JwtTokenProvider(configuration);
		return tokenProvider;
	}
}