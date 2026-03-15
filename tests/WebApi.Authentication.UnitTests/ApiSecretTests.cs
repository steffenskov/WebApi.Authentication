namespace WebApi.Authentication.UnitTests;

public class ApiSecretTests
{
	[Fact]
	public void GenerateJwtToken_AlreadyHasToken_Throws()
	{
		// Arrange
		var secret = new ApiSecret();
		var tokenProvider = TestHelper.CreateTokenProvider();

		secret.GenerateJwtToken(tokenProvider);

		// Act & Assert
		var ex = Assert.Throws<InvalidOperationException>(() => secret.GenerateJwtToken(tokenProvider));
		Assert.Equal($"JwtToken already generated for ApiSecret {secret.Id}.", ex.Message);
	}

	[Fact]
	public void GenerateJwtToken_HadNoToken_GeneratesTokenAndExpires()
	{
		// Arrange
		var secret = new ApiSecret();
		var tokenProvider = TestHelper.CreateTokenProvider();

		// Act
		secret.GenerateJwtToken(tokenProvider);

		// Assert
		Assert.False(string.IsNullOrWhiteSpace(secret.JwtToken));
		Assert.True(secret.Expires > DateTime.UtcNow);
	}

	[Fact]
	public void HasGeneratedToken_HadNoToken_ReturnsFalse()
	{
		// Arrange
		var secret = new ApiSecret();

		// Assert
		Assert.False(secret.HasGeneratedToken);
	}

	[Fact]
	public void HasGeneratedToken_HasToken_ReturnsTrue()
	{
		// Arrange
		var secret = new ApiSecret();
		var tokenProvider = TestHelper.CreateTokenProvider();

		// Act
		secret.GenerateJwtToken(tokenProvider);

		// Assert
		Assert.True(secret.HasGeneratedToken);
	}

	[Fact]
	public void Revoke_NotRevoked_BecomesRevoked()
	{
		// Arrange
		var secret = new ApiSecret();

		// Act
		Assert.False(secret.IsRevoked); // Sanity check
		secret.Revoke();

		// Assert
		Assert.True(secret.IsRevoked);
	}

	[Fact]
	public void Revoke_AlreadyRevoked_DoesNothing()
	{
		// Arrange
		var secret = new ApiSecret();
		secret.Revoke();

		// Act
		secret.Revoke();

		// Assert
		Assert.True(secret.IsRevoked);
	}
}