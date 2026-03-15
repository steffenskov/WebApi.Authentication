using Microsoft.IdentityModel.JsonWebTokens;

namespace WebApi.Authentication.UnitTests.Services;

public class JwtTokenProviderTests
{
	[Fact]
	public void CreateToken_DefaultSecret_SetsClaims()
	{
		// Arrange
		var secret = new ApiSecret();
		var configuration = new AuthenticationConfiguration
		{
			SecretKey = RandomNumberGenerator.GetBytes(64),
			Issuer = "WebApi.Authentication",
			Audience = "UnitTests",
			Expiration = TimeSpan.FromMinutes(5)
		};

		var tokenProvider = new JwtTokenProvider(configuration);

		// Act
		var (token, expires) = tokenProvider.CreateToken(secret);

		// Assert
		Assert.NotNull(token);
		Assert.True(expires > DateTime.UtcNow.AddMinutes(4));
		Assert.True(expires < DateTime.UtcNow.AddMinutes(6));

		var handler = new JsonWebTokenHandler();
		var jwt = handler.ReadJsonWebToken(token);
		var claims = jwt.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
		Assert.Equal(secret.Id.ToString(), claims["sub"]);
		Assert.Equal(configuration.Issuer, claims["iss"]);
		Assert.Equal(configuration.Audience, claims["aud"]);
		var expectedExpiration = new DateTimeOffset(expires, TimeSpan.Zero).ToUnixTimeSeconds();
		Assert.Equal(expectedExpiration.ToString(), claims["exp"]);
	}
}