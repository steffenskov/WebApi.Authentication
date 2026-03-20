using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Authentication.UnitTests.DependencyInjectionTests;

public class WebApiAuthenticationServiceCollectionTests
{
	[Fact]
	public void AddApiSecretRepository_CustomType_AddedWithCustomType()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication<CustomApiSecret>(TestHelper.CreateConfiguration());

		// Act
		webApiAuthenticationServiceCollection.AddApiSecretRepository(new FakeRepository<CustomApiSecret>());

		var provider = services.BuildServiceProvider();

		// Assert
		var genericRepository = provider.GetRequiredService<IApiSecretRepository<CustomApiSecret>>();
		var plainRepository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(genericRepository);
		Assert.NotNull(plainRepository);
		Assert.IsType<FakeRepository<CustomApiSecret>>(genericRepository);
		Assert.IsType<ApiSecretRepositoryAdapter<CustomApiSecret>>(plainRepository);
	}

	[Fact]
	public void AddApiSecretRepository_DefaultType_AddedWithDefaultType()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication(TestHelper.CreateConfiguration());

		// Act
		webApiAuthenticationServiceCollection.AddApiSecretRepository(new FakeRepository<ApiSecret>());

		var provider = services.BuildServiceProvider();

		// Assert
		var genericRepository = provider.GetRequiredService<IApiSecretRepository<ApiSecret>>();
		var plainRepository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(genericRepository);
		Assert.NotNull(plainRepository);
		Assert.IsType<FakeRepository<ApiSecret>>(genericRepository);
		Assert.IsType<ApiSecretRepositoryAdapter<ApiSecret>>(plainRepository);
	}

	[Fact]
	public void AddApiSecretRepositoryGenerics_CustomType_AddedWithCustomType()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication<CustomApiSecret>(TestHelper.CreateConfiguration());

		// Act
		webApiAuthenticationServiceCollection.AddApiSecretRepository<FakeRepository<CustomApiSecret>>();

		var provider = services.BuildServiceProvider();

		// Assert
		var genericRepository = provider.GetRequiredService<IApiSecretRepository<CustomApiSecret>>();
		var plainRepository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(genericRepository);
		Assert.NotNull(plainRepository);
		Assert.IsType<FakeRepository<CustomApiSecret>>(genericRepository);
		Assert.IsType<ApiSecretRepositoryAdapter<CustomApiSecret>>(plainRepository);
	}

	[Fact]
	public void AddApiSecretRepositoryGeneric_DefaultType_AddedWithDefaultType()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication(TestHelper.CreateConfiguration());

		// Act
		webApiAuthenticationServiceCollection.AddApiSecretRepository<FakeRepository<ApiSecret>>();

		var provider = services.BuildServiceProvider();

		// Assert
		var genericRepository = provider.GetRequiredService<IApiSecretRepository<ApiSecret>>();
		var plainRepository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(genericRepository);
		Assert.NotNull(plainRepository);
		Assert.IsType<FakeRepository<ApiSecret>>(genericRepository);
		Assert.IsType<ApiSecretRepositoryAdapter<ApiSecret>>(plainRepository);
	}

	[Fact]
	public void AddApiSecretRepositoryFactory_CustomType_AddedWithCustomType()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication<CustomApiSecret>(TestHelper.CreateConfiguration());

		// Act
		webApiAuthenticationServiceCollection.AddApiSecretRepository(_ => new FakeRepository<CustomApiSecret>());

		var provider = services.BuildServiceProvider();

		// Assert
		var genericRepository = provider.GetRequiredService<IApiSecretRepository<CustomApiSecret>>();
		var plainRepository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(genericRepository);
		Assert.NotNull(plainRepository);
		Assert.IsType<FakeRepository<CustomApiSecret>>(genericRepository);
		Assert.IsType<ApiSecretRepositoryAdapter<CustomApiSecret>>(plainRepository);
	}

	[Fact]
	public void AddApiSecretRepositoryFactory_DefaultType_AddedWithDefaultType()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication(TestHelper.CreateConfiguration());

		// Act
		webApiAuthenticationServiceCollection.AddApiSecretRepository(_ => new FakeRepository<ApiSecret>());

		var provider = services.BuildServiceProvider();

		// Assert
		var genericRepository = provider.GetRequiredService<IApiSecretRepository<ApiSecret>>();
		var plainRepository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(genericRepository);
		Assert.NotNull(plainRepository);
		Assert.IsType<FakeRepository<ApiSecret>>(genericRepository);
		Assert.IsType<ApiSecretRepositoryAdapter<ApiSecret>>(plainRepository);
	}
}

file class FakeRepository<T> : IApiSecretRepository<T>
	where T : ApiSecret
{
	public ValueTask<T?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public ValueTask PersistAsync(T secret, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}