namespace WebApi.Authentication.UnitTests;

public class SegregatedApiSecretTests
{
	[Fact]
	public void GetCustomClaims_GuidKey_ReturnsSegregationClaimWithKeyValue()
	{
		// Arrange
		var key = Guid.NewGuid();
		var secret = new SegregatedApiSecret<Guid> { Key = key };

		// Act
		var claim = Assert.Single(secret.GetCustomClaims());

		// Assert
		Assert.Equal(Consts.SegregationClaim, claim.Type);
		Assert.Equal(key.ToString(), claim.Value);
	}

	[Fact]
	public void GetCustomClaims_StringKey_ReturnsSegregationClaimWithKeyValue()
	{
		// Arrange
		const string key = "tenant-a";
		var secret = new SegregatedApiSecret<string> { Key = key };

		// Act
		var claim = Assert.Single(secret.GetCustomClaims());

		// Assert
		Assert.Equal(Consts.SegregationClaim, claim.Type);
		Assert.Equal(key, claim.Value);
	}

	[Fact]
	public void GetCustomClaims_KeyToStringReturnsNull_ThrowsInvalidOperationException()
	{
		// Arrange
		var secret = new SegregatedApiSecret<NullToStringKey> { Key = new NullToStringKey() };

		// Act
		var act = () => secret.GetCustomClaims().ToArray();

		// Assert
		var ex = Assert.Throws<InvalidOperationException>(act);
		Assert.Equal("NullToStringKey.ToString() returns null!", ex.Message);
	}

	private sealed class NullToStringKey
	{
		public override string? ToString()
		{
			return null;
		}
	}
}