using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using WebApi.Authentication.MongoDB.Repositories;

namespace WebApi.Authentication.UnitTests.DependencyInjectionTests;

public class MongoDBTests
{
	[Fact]
	public void AddApiSecretMongoRepository_CustomType_AddsRepositoryWithCustomType()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddApiSecretAuthentication<CustomApiSecret>(TestHelper.CreateConfiguration())
			.AddApiSecretMongoRepository(Substitute.For<IMongoDatabase>(), "secrets");

		var provider = services.BuildServiceProvider();

		// Assert
		var repository = provider.GetRequiredService<IApiSecretRepository<CustomApiSecret>>();
		Assert.NotNull(repository);
		Assert.IsType<ApiSecretMongoRepository<CustomApiSecret>>(repository);
	}

	[Fact]
	public void AddApiSecretMongoRepository_CustomType_AddsRepositoryAdapter()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddApiSecretAuthentication<CustomApiSecret>(TestHelper.CreateConfiguration())
			.AddApiSecretMongoRepository(Substitute.For<IMongoDatabase>(), "secrets");

		var provider = services.BuildServiceProvider();

		// Assert
		var repository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(repository);
		Assert.IsType<ApiSecretRepositoryAdapter<CustomApiSecret>>(repository);
	}

	[Fact]
	public void AddSegregatedApiSecretMongoRepository_CustomType_AddsRepositoryAdapter()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddSegregatedApiSecretAuthentication<CustomSegregatedApiSecret, Guid>(TestHelper.CreateConfiguration())
			.AddSegregatedApiSecretMongoRepository((_, _) => Substitute.For<IMongoCollection<CustomSegregatedApiSecret>>());

		var provider = services.BuildServiceProvider();

		// Assert
		var repository = provider.GetRequiredService<IApiSecretRepository<CustomSegregatedApiSecret>>();
		Assert.NotNull(repository);
		Assert.IsType<SegregatedApiSecretRepositoryAdapter<Guid, CustomSegregatedApiSecret, ApiSecretMongoRepository<CustomSegregatedApiSecret>>>(repository);
	}
}