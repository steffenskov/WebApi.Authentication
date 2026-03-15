using System.Security.Cryptography;
using WebApi.Authentication.Configuration;
using WebApi.Authentication.Services;

namespace WebApi.Authentication.IntegrationTests;

internal static class TestHelper
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