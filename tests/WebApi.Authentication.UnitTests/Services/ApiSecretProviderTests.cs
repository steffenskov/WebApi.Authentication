namespace WebApi.Authentication.UnitTests.Services;

public class ApiSecretProviderTests
{
	[Fact]
	public async Task PersistSecretAsync_AlreadyHasJwtToken_PersistAsIs()
	{
		// Arrange
		ApiSecret? capturedSecret = null;
		var secret = new ApiSecret();
		var tokenProvider = TestHelper.CreateTokenProvider();
		var repository = Substitute.For<IApiSecretRepository<ApiSecret>>();
		repository.When(x => x.PersistAsync(Arg.Any<ApiSecret>(), Arg.Any<CancellationToken>()))
			.Do(callInfo => { capturedSecret = callInfo.Arg<ApiSecret>(); });

		secret.GenerateJwtToken(tokenProvider);
		var provider = new ApiSecretProvider<ApiSecret>(tokenProvider, repository);

		// Act
		await provider.PersistSecretAsync(secret, TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(capturedSecret);
		Assert.Equal(secret.JwtToken, capturedSecret.JwtToken);
	}

	[Fact]
	public async Task PersistSecretAsync_HasNoJwtToken_GeneratesAndPersists()
	{
		// Arrange
		ApiSecret? capturedSecret = null;
		var secret = new ApiSecret();
		var tokenProvider = TestHelper.CreateTokenProvider();
		var repository = Substitute.For<IApiSecretRepository<ApiSecret>>();
		repository.When(x => x.PersistAsync(Arg.Any<ApiSecret>(), Arg.Any<CancellationToken>()))
			.Do(callInfo => { capturedSecret = callInfo.Arg<ApiSecret>(); });

		var provider = new ApiSecretProvider<ApiSecret>(tokenProvider, repository);

		// Act
		Assert.False(secret.HasGeneratedToken); // Sanity check
		await provider.PersistSecretAsync(secret, TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(capturedSecret);
		Assert.True(capturedSecret.HasGeneratedToken);
	}

	[Fact]
	public async Task PersistSecretAsync_CustomSecretType_PersistsAsCustomType()
	{
		// Arrange
		ApiSecret? capturedSecret = null;
		var secret = new CustomApiSecret
		{
			CustomerId = Guid.NewGuid()
		};
		var tokenProvider = TestHelper.CreateTokenProvider();
		var repository = Substitute.For<IApiSecretRepository<ApiSecret>>();
		repository.When(x => x.PersistAsync(Arg.Any<ApiSecret>(), Arg.Any<CancellationToken>()))
			.Do(callInfo => { capturedSecret = callInfo.Arg<ApiSecret>(); });

		secret.GenerateJwtToken(tokenProvider);
		var provider = new ApiSecretProvider<ApiSecret>(tokenProvider, repository);

		// Act
		await provider.PersistSecretAsync(secret, TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(capturedSecret);
		var capturedCustomSecret = Assert.IsType<CustomApiSecret>(capturedSecret);
		Assert.Equal(secret.CustomerId, capturedCustomSecret.CustomerId);
	}
}

file sealed class CustomApiSecret : ApiSecret
{
	public Guid CustomerId { get; init; }
}