using System.Security.Claims;

namespace WebApi.Authentication.UnitTests.Repositories;

public class SegregatedApiSecretRepositoryAdapterTests
{
	[Fact]
	public async Task GetByClaimsAsync_MissingSegregationClaim_ThrowsInvalidOperationException()
	{
		// Arrange
		Claim[] claims = [new("sub", Guid.NewGuid().ToString())];

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, _) => Substitute.For<IApiSecretRepository<CustomApiSecret>>());

		// Act && Assert
		var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken));
		Assert.Contains("Claims do not contain segregation-key used by Segregation. You likely have a dependency injection mismatch somewhere.", ex.Message);
	}

	[Fact]
	public async Task GetByClaimsAsync_InvalidSegregationClaimValue_ThrowsInvalidOperationException()
	{
		// Arrange
		const string invalidKey = "not-a-guid";
		Claim[] claims = [new(Consts.SegregationClaim, invalidKey)];

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, _) => Substitute.For<IApiSecretRepository<CustomApiSecret>>());

		// Act && Assert
		var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken));
		Assert.Equal($"Could not parse a Guid from {invalidKey}", ex.Message);
	}

	[Fact]
	public async Task GetByClaimsAsync_DuplicateSegregationClaims_ThrowsInvalidOperationException()
	{
		// Arrange
		var firstKey = Guid.NewGuid();
		var secondKey = Guid.NewGuid();
		Claim[] claims =
		[
			new(Consts.SegregationClaim, firstKey.ToString()),
			new(Consts.SegregationClaim, secondKey.ToString())
		];

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, _) => Substitute.For<IApiSecretRepository<CustomApiSecret>>());

		// Act && Assert
		var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken));
		Assert.Contains("Sequence contains more than one matching element", ex.Message);
	}

	[Fact]
	public async Task GetByClaimsAsync_ValidSegregationClaim_ForwardsCallToKeyedRepository()
	{
		// Arrange
		var key = Guid.NewGuid();
		var expectedSecret = new CustomApiSecret { Key = key };
		Claim[] claims = [new(Consts.SegregationClaim, key.ToString())];
		var cancellationToken = TestContext.Current.CancellationToken;

		var innerRepository = Substitute.For<IApiSecretRepository<CustomApiSecret>>();
		innerRepository.GetByClaimsAsync(claims, cancellationToken).Returns(expectedSecret);

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, requestedKey) =>
			{
				Assert.Equal(key, requestedKey);
				return innerRepository;
			});

		// Act
		var result = await repository.GetByClaimsAsync(claims, cancellationToken);

		// Assert
		Assert.Same(expectedSecret, result);
		await innerRepository.Received(1).GetByClaimsAsync(claims, cancellationToken);
	}

	[Fact]
	public async Task GetByClaimsAsync_SameKeyUsedMultipleTimes_ReusesRepository()
	{
		// Arrange
		var key = Guid.NewGuid();
		Claim[] claims = [new(Consts.SegregationClaim, key.ToString())];
		var innerRepository = Substitute.For<IApiSecretRepository<CustomApiSecret>>();
		var factoryCallCount = 0;

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, _) =>
			{
				factoryCallCount++;
				return innerRepository;
			});

		// Act
		await repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken);
		await repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(1, factoryCallCount);
		await innerRepository.Received(2).GetByClaimsAsync(claims, TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task GetByClaimsAsync_DifferentKeysUsed_CreatesRepositoryForEachKey()
	{
		// Arrange
		var firstKey = Guid.NewGuid();
		var secondKey = Guid.NewGuid();
		Claim[] firstClaims = [new(Consts.SegregationClaim, firstKey.ToString())];
		Claim[] secondClaims = [new(Consts.SegregationClaim, secondKey.ToString())];

		var firstRepository = Substitute.For<IApiSecretRepository<CustomApiSecret>>();
		var secondRepository = Substitute.For<IApiSecretRepository<CustomApiSecret>>();
		var repositories = new Dictionary<Guid, IApiSecretRepository<CustomApiSecret>>
		{
			[firstKey] = firstRepository,
			[secondKey] = secondRepository
		};

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, requestedKey) => repositories[requestedKey]);

		// Act
		await repository.GetByClaimsAsync(firstClaims, TestContext.Current.CancellationToken);
		await repository.GetByClaimsAsync(secondClaims, TestContext.Current.CancellationToken);

		// Assert
		await firstRepository.Received(1).GetByClaimsAsync(firstClaims, TestContext.Current.CancellationToken);
		await secondRepository.Received(1).GetByClaimsAsync(secondClaims, TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task PersistAsync_SecretWithKey_ForwardsCallToKeyedRepository()
	{
		// Arrange
		var key = Guid.NewGuid();
		var secret = new CustomApiSecret { Key = key };
		var cancellationToken = TestContext.Current.CancellationToken;

		var innerRepository = Substitute.For<IApiSecretRepository<CustomApiSecret>>();

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, requestedKey) =>
			{
				Assert.Equal(key, requestedKey);
				return innerRepository;
			});

		// Act
		await repository.PersistAsync(secret, cancellationToken);

		// Assert
		await innerRepository.Received(1).PersistAsync(secret, cancellationToken);
	}

	[Fact]
	public async Task PersistAsync_SameKeyUsedMultipleTimes_ReusesRepository()
	{
		// Arrange
		var key = Guid.NewGuid();
		var firstSecret = new CustomApiSecret { Key = key };
		var secondSecret = new CustomApiSecret { Key = key };
		var innerRepository = Substitute.For<IApiSecretRepository<CustomApiSecret>>();
		var factoryCallCount = 0;

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, _) =>
			{
				factoryCallCount++;
				return innerRepository;
			});

		// Act
		await repository.PersistAsync(firstSecret, TestContext.Current.CancellationToken);
		await repository.PersistAsync(secondSecret, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(1, factoryCallCount);
		await innerRepository.Received(1).PersistAsync(firstSecret, TestContext.Current.CancellationToken);
		await innerRepository.Received(1).PersistAsync(secondSecret, TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task PersistAsync_DifferentKeysUsed_CreatesRepositoryForEachKey()
	{
		// Arrange
		var firstKey = Guid.NewGuid();
		var secondKey = Guid.NewGuid();
		var firstSecret = new CustomApiSecret { Key = firstKey };
		var secondSecret = new CustomApiSecret { Key = secondKey };

		var firstRepository = Substitute.For<IApiSecretRepository<CustomApiSecret>>();
		var secondRepository = Substitute.For<IApiSecretRepository<CustomApiSecret>>();
		var repositories = new Dictionary<Guid, IApiSecretRepository<CustomApiSecret>>
		{
			[firstKey] = firstRepository,
			[secondKey] = secondRepository
		};

		var repository = new SegregatedApiSecretRepositoryAdapter<Guid, CustomApiSecret, IApiSecretRepository<CustomApiSecret>>(
			Substitute.For<IServiceProvider>(),
			(_, requestedKey) => repositories[requestedKey]);

		// Act
		await repository.PersistAsync(firstSecret, TestContext.Current.CancellationToken);
		await repository.PersistAsync(secondSecret, TestContext.Current.CancellationToken);

		// Assert
		await firstRepository.Received(1).PersistAsync(firstSecret, TestContext.Current.CancellationToken);
		await secondRepository.Received(1).PersistAsync(secondSecret, TestContext.Current.CancellationToken);
	}
}