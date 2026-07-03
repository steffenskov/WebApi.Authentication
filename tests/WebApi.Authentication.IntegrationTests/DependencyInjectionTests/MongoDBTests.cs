using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Authentication.Configuration;
using WebApi.Authentication.Repositories;

namespace WebApi.Authentication.IntegrationTests.DependencyInjectionTests;

[Collection(nameof(ConfigurationCollection))]
public partial class MongoDBTests
{
	private readonly IApiSecretRepository<CustomApiSecret> _repository;

	public MongoDBTests(ContainerFixture fixture)
	{
		var client = new MongoClient(fixture.MongoConnectionString);
		var db = client.GetDatabase("WebApiAuthentication");
		var configuration = new AuthenticationConfiguration
		{
			SecretKey = RandomNumberGenerator.GetBytes(64),
			Issuer = "ApiAuthentication",
			Audience = "WebApi",
			Expiration = TimeSpan.FromMinutes(5)
		};
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddApiSecretProvider<CustomApiSecret>(configuration)
			.AddSegregatedApiSecretMongoRepository<CustomApiSecret, Guid>((_, key) => db.GetCollection<CustomApiSecret>($"api-secrets-{key.ToString()}"));

		var provider = serviceCollection.BuildServiceProvider();

		_repository = provider.GetRequiredService<IApiSecretRepository<CustomApiSecret>>();
	}

	[Fact]
	public async Task GetByClaimsAsync_TwoSecretsWithDifferentKeys_FetchesSecretFromMatchingRepository()
	{
		// Arrange
		var firstKey = Guid.NewGuid();
		var secondKey = Guid.NewGuid();
		var firstSecret = new CustomApiSecret
		{
			Key = firstKey,
			CustomerId = Guid.NewGuid()
		};
		var secondSecret = new CustomApiSecret
		{
			Key = secondKey,
			CustomerId = Guid.NewGuid()
		};

		await _repository.PersistAsync(firstSecret, TestContext.Current.CancellationToken);
		await _repository.PersistAsync(secondSecret, TestContext.Current.CancellationToken);

		Claim[] firstClaims =
		[
			new("sub", firstSecret.Id.ToString()),
			new(Consts.SegregationClaim, firstKey.ToString())
		];
		Claim[] secondClaims =
		[
			new("sub", secondSecret.Id.ToString()),
			new(Consts.SegregationClaim, secondKey.ToString())
		];

		// Act
		var firstFetched = await _repository.GetByClaimsAsync(firstClaims, TestContext.Current.CancellationToken);
		var secondFetched = await _repository.GetByClaimsAsync(secondClaims, TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(firstFetched);
		Assert.Equal(firstSecret.Id, firstFetched.Id);
		Assert.Equal(firstSecret.Key, firstFetched.Key);
		Assert.Equal(firstSecret.CustomerId, firstFetched.CustomerId);

		Assert.NotNull(secondFetched);
		Assert.Equal(secondSecret.Id, secondFetched.Id);
		Assert.Equal(secondSecret.Key, secondFetched.Key);
		Assert.Equal(secondSecret.CustomerId, secondFetched.CustomerId);
	}

	[Fact]
	public async Task GetByClaimsAsync_SecretIdExistsInDifferentKeyRepository_ReturnsNull()
	{
		// Arrange
		var persistedKey = Guid.NewGuid();
		var differentKey = Guid.NewGuid();
		var secret = new CustomApiSecret
		{
			Key = persistedKey,
			CustomerId = Guid.NewGuid()
		};

		await _repository.PersistAsync(secret, TestContext.Current.CancellationToken);

		Claim[] claimsForWrongKey =
		[
			new("sub", secret.Id.ToString()),
			new(Consts.SegregationClaim, differentKey.ToString())
		];

		Claim[] claimsForRightKey =
		[
			new("sub", secret.Id.ToString()),
			new(Consts.SegregationClaim, persistedKey.ToString())
		];

		// Act
		var fetchedByWrongKey = await _repository.GetByClaimsAsync(claimsForWrongKey, TestContext.Current.CancellationToken);
		var fetchedByRightKey = await _repository.GetByClaimsAsync(claimsForRightKey, TestContext.Current.CancellationToken);

		// Assert
		Assert.Null(fetchedByWrongKey);
		Assert.NotNull(fetchedByRightKey);
	}
}

public partial class MongoDBTests
{
	private sealed class CustomApiSecret : SegregatedApiSecret<Guid>
	{
		public Guid CustomerId { get; init; }
	}
}