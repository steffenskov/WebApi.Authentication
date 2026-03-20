using Microsoft.Extensions.DependencyInjection;
using WebApi.Authentication.DependencyInjection;

namespace WebApi.Authentication.UnitTests.DependencyInjectionTests;

public class SetupTests
{
	[Fact]
	public void AddApiSecretAuthentication_NoGenerics_UsesDefaultType()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		var result = services.AddApiSecretAuthentication(TestHelper.CreateConfiguration());

		// Assert
		Assert.IsType<IWebApiAuthenticationServiceCollection<ApiSecret>>(result, false);
	}

	[Fact]
	public void AddApiSecretAuthentication_Generic_UsesCustomType()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		var result = services.AddApiSecretAuthentication<CustomApiSecret>(TestHelper.CreateConfiguration());

		// Assert
		Assert.IsType<IWebApiAuthenticationServiceCollection<CustomApiSecret>>(result, false);
	}

	[Fact]
	public void AddApiSecretProvider_NoGenerics_UsesDefaultType()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		var result = services.AddApiSecretProvider(TestHelper.CreateConfiguration());

		// Assert
		Assert.IsType<IWebApiAuthenticationServiceCollection<ApiSecret>>(result, false);
	}

	[Fact]
	public void AddApiSecretProvider_Generic_UsesCustomType()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		var result = services.AddApiSecretProvider<CustomApiSecret>(TestHelper.CreateConfiguration());

		// Assert
		Assert.IsType<IWebApiAuthenticationServiceCollection<CustomApiSecret>>(result, false);
	}

	[Fact]
	public void AddApiSecretProvider_NoGenerics_InjectsPlainProviderWithDefaultType()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddApiSecretProvider(TestHelper.CreateConfiguration())
			.AddApiSecretRepository(Substitute.For<IApiSecretRepository<ApiSecret>>());

		var serviceProvider = services.BuildServiceProvider();

		// Assert
		var result = serviceProvider.GetRequiredService<IApiSecretProvider>();
		Assert.IsType<ApiSecretProvider<ApiSecret>>(result);
	}

	[Fact]
	public void AddApiSecretProvider_Generic_InjectsPlainProviderWithCustomType()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddApiSecretProvider<CustomApiSecret>(TestHelper.CreateConfiguration())
			.AddApiSecretRepository(Substitute.For<IApiSecretRepository<CustomApiSecret>>());
		var serviceProvider = services.BuildServiceProvider();

		// Assert
		var result = serviceProvider.GetRequiredService<IApiSecretProvider>();
		Assert.IsType<ApiSecretProvider<CustomApiSecret>>(result);
	}

	[Fact]
	public void AddApiSecretProvider_NoRepositoryAdded_InstantiationThrows()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddApiSecretProvider<CustomApiSecret>(TestHelper.CreateConfiguration());
		var serviceProvider = services.BuildServiceProvider();

		// Act && Assert
		var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IApiSecretProvider>());

		Assert.StartsWith("No service for type 'WebApi.Authentication.Repositories.IApiSecretRepository", ex.Message);
	}
}