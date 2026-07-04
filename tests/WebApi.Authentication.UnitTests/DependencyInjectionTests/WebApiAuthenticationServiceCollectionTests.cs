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
	public void AddApiSecretRepository_AddedTwice_Throws()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication(TestHelper.CreateConfiguration());
		webApiAuthenticationServiceCollection.AddApiSecretRepository(new FakeRepository<ApiSecret>());

		// Act && Assert
		var ex = Assert.Throws<InvalidOperationException>(() => webApiAuthenticationServiceCollection.AddApiSecretRepository(new FakeRepository<ApiSecret>()));

		Assert.Equal("An ApiSecret repository has already been registered.", ex.Message);
	}

	[Fact]
	public void AddApiSecretRepositoryGeneric_CustomType_AddedWithCustomType()
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
	public void AddApiSecretRepositoryGeneric_AddedTwice_Throws()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication(TestHelper.CreateConfiguration());
		webApiAuthenticationServiceCollection.AddApiSecretRepository<FakeRepository<ApiSecret>>();

		// Act && Assert
		var ex = Assert.Throws<InvalidOperationException>(() => webApiAuthenticationServiceCollection.AddApiSecretRepository<FakeRepository<ApiSecret>>());

		Assert.Equal("An ApiSecret repository has already been registered.", ex.Message);
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

	[Fact]
	public void AddApiSecretRepositoryFactory_AddedTwice_Throws()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddApiSecretAuthentication(TestHelper.CreateConfiguration());
		webApiAuthenticationServiceCollection.AddApiSecretRepository(_ => new FakeRepository<ApiSecret>());

		// Act && Assert
		var ex = Assert.Throws<InvalidOperationException>(() => webApiAuthenticationServiceCollection.AddApiSecretRepository(_ => new FakeRepository<ApiSecret>()));

		Assert.Equal("An ApiSecret repository has already been registered.", ex.Message);
	}

	[Fact]
	public void AddSegregatedApiSecretRepositoryFactory_AddedTwice_Throws()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddSegregatedApiSecretAuthentication<SegregatedApiSecret<Guid>, Guid>(TestHelper.CreateConfiguration());
		webApiAuthenticationServiceCollection.AddSegregatedApiSecretRepository<FakeRepository<SegregatedApiSecret<Guid>>>((_, _) =>
			new FakeRepository<SegregatedApiSecret<Guid>>());

		// Act && Assert
		var ex = Assert.Throws<InvalidOperationException>(() =>
			webApiAuthenticationServiceCollection.AddSegregatedApiSecretRepository<FakeRepository<SegregatedApiSecret<Guid>>>((_, _) =>
				new FakeRepository<SegregatedApiSecret<Guid>>()));

		Assert.Equal("An ApiSecret repository has already been registered.", ex.Message);
	}

	[Fact]
	public void AddSegregatedApiSecretRepositoryGeneric_CustomType_AddedWithCustomType()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddSegregatedApiSecretAuthentication<CustomSegregatedApiSecret, Guid>(TestHelper.CreateConfiguration());

		// Act
		webApiAuthenticationServiceCollection.AddSegregatedApiSecretRepository<FakeRepository<CustomSegregatedApiSecret>>((_, _) => new FakeRepository<CustomSegregatedApiSecret>());

		var provider = services.BuildServiceProvider();

		// Assert
		var genericRepository = provider.GetRequiredService<IApiSecretRepository<CustomSegregatedApiSecret>>();
		var plainRepository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(genericRepository);
		Assert.NotNull(plainRepository);
		Assert.IsType<SegregatedApiSecretRepositoryAdapter<Guid, CustomSegregatedApiSecret, FakeRepository<CustomSegregatedApiSecret>>>(genericRepository);
		Assert.IsType<ApiSecretRepositoryAdapter<CustomSegregatedApiSecret>>(plainRepository);
	}

	[Fact]
	public void AddSegregatedApiSecretRepositoryGeneric_DefaultType_AddedWithDefaultType()
	{
		// Arrange
		var services = new ServiceCollection();
		var webApiAuthenticationServiceCollection = services.AddSegregatedApiSecretAuthentication<SegregatedApiSecret<Guid>, Guid>(TestHelper.CreateConfiguration());

		// Act
		webApiAuthenticationServiceCollection.AddSegregatedApiSecretRepository<FakeRepository<SegregatedApiSecret<Guid>>>((_, _) =>
			new FakeRepository<SegregatedApiSecret<Guid>>());


		var provider = services.BuildServiceProvider();

		// Assert
		var genericRepository = provider.GetRequiredService<IApiSecretRepository<SegregatedApiSecret<Guid>>>();
		var plainRepository = provider.GetRequiredService<IApiSecretRepository>();
		Assert.NotNull(genericRepository);
		Assert.NotNull(plainRepository);
		Assert.IsType<SegregatedApiSecretRepositoryAdapter<Guid, SegregatedApiSecret<Guid>, FakeRepository<SegregatedApiSecret<Guid>>>>(genericRepository);
		Assert.IsType<ApiSecretRepositoryAdapter<SegregatedApiSecret<Guid>>>(plainRepository);
	}
}

file class FakeRepository<T> : IApiSecretRepository<T>
	where T : class, IApiSecret
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