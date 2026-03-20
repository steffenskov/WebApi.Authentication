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
}