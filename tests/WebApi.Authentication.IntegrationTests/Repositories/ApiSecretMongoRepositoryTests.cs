namespace WebApi.Authentication.IntegrationTests.Repositories;

[Collection(nameof(ConfigurationCollection))]
public partial class ApiSecretMongoRepositoryTests
{
	private readonly ApiSecretMongoRepository<CustomApiSecret> _customRepository;
	private readonly ApiSecretMongoRepository<ApiSecret> _repository;

	public ApiSecretMongoRepositoryTests(ContainerFixture fixture)
	{
		var client = new MongoClient(fixture.MongoConnectionString);
		var db = client.GetDatabase("WebApiAuthentication");
		_repository = new ApiSecretMongoRepository<ApiSecret>(db, "api-secrets");
		_customRepository = new ApiSecretMongoRepository<CustomApiSecret>(db, "custom-secrets");
	}

	[Fact]
	public async Task GetByClaimsAsync_NoClaims_ReturnsNull()
	{
		// Arrange
		Claim[] claims = [];

		// Act
		var secret = await _repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken);

		// Assert
		Assert.Null(secret);
	}

	[Fact]
	public async Task GetByClaimsAsync_SubNotAGuid_ReturnsNull()
	{
		// Arrange
		Claim[] claims = [new("sub", "not-a-guid")];

		// Act
		var secret = await _repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken);

		// Assert
		Assert.Null(secret);
	}

	[Fact]
	public async Task GetByClaimsAsync_SubValidGuidButDoesNotExist_ReturnsNull()
	{
		// Arrange
		Claim[] claims = [new("sub", Guid.NewGuid().ToString())];

		// Act
		var secret = await _repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken);

		// Assert
		Assert.Null(secret);
	}

	[Fact]
	public async Task GetByClaimsAsync_SubExists_ReturnsSecret()
	{
		// Arrange
		var secret = new ApiSecret();
		secret.GenerateJwtToken(TestHelper.CreateTokenProvider());
		Claim[] claims = [new("sub", secret.Id.ToString())];
		await _repository.PersistAsync(secret, TestContext.Current.CancellationToken);

		// Act
		var fetched = await _repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(fetched);
		Assert.Equal(secret.Id, fetched.Id);
		Assert.Equal(secret.JwtToken, fetched.JwtToken);
		Assert.True(fetched.HasGeneratedToken);
		Assert.True(secret.Expires - fetched.Expires < TimeSpan.FromSeconds(1)); // MongoDB lacks the precision of .Net, so some rounding occurs in the DB. This asserts that the value is roughly the same
		Assert.Equal(secret.IsRevoked, fetched.IsRevoked);
	}

	[Fact]
	public async Task GetByClaimsAsync_CustomType_ReturnsCustomType()
	{
		// Arrange
		var secret = new CustomApiSecret
		{
			CustomerId = Guid.NewGuid()
		};
		Claim[] claims = [new("sub", secret.Id.ToString())];
		await _customRepository.PersistAsync(secret, TestContext.Current.CancellationToken);

		// Act
		var fetched = await _customRepository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(fetched);
		Assert.Equal(secret.CustomerId, fetched.CustomerId);
	}

	[Fact]
	public async Task PersistAsync_DoesNotExist_IsCreated()
	{
		// Arrange
		var secret = new ApiSecret();

		// Act
		await _repository.PersistAsync(secret, TestContext.Current.CancellationToken);

		// Assert
		var fetched = await _repository.GetByClaimsAsync([new Claim("sub", secret.Id.ToString())], TestContext.Current.CancellationToken);
		Assert.NotNull(fetched);
	}

	[Fact]
	public async Task PersistAsync_Exists_IsUpdated()
	{
		// Arrange
		var secret = new ApiSecret();
		await _repository.PersistAsync(secret, TestContext.Current.CancellationToken);
		Assert.False(secret.HasGeneratedToken); // Sanity check

		secret.GenerateJwtToken(TestHelper.CreateTokenProvider());

		// Act
		await _repository.PersistAsync(secret, TestContext.Current.CancellationToken);

		// Assert
		var fetched = await _repository.GetByClaimsAsync([new Claim("sub", secret.Id.ToString())], TestContext.Current.CancellationToken);
		Assert.NotNull(fetched);
		Assert.True(fetched.HasGeneratedToken);
	}
}

public partial class ApiSecretMongoRepositoryTests
{
	private sealed class CustomApiSecret : ApiSecret
	{
		public Guid CustomerId { get; init; }
	}
}