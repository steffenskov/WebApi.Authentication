using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Authentication.UnitTests.Services;

public class TokenValidatorServiceTests
{
	[Fact]
	public async Task OnTokenValidated_NoPrincipal_Fails()
	{
		// Arrange
		var context = CreateContext();

		// Act
		await TokenValidatorService.OnTokenValidated(context);

		// Assert
		Assert.NotNull(context.Result);
		Assert.NotNull(context.Result.Failure);
		Assert.Equal("Principal is null.", context.Result.Failure.Message);
	}

	[Fact]
	public async Task OnTokenValidated_SecretNotFound_Fails()
	{
		// Arrange
		var context = CreateContext();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton<IApiSecretRepository>(new FakeApiSecretRepository(null));
		context.HttpContext.RequestServices = serviceCollection.BuildServiceProvider();
		context.Principal = new ClaimsPrincipal();

		// Act
		await TokenValidatorService.OnTokenValidated(context);

		// Assert
		Assert.NotNull(context.Result);
		Assert.NotNull(context.Result.Failure);
		Assert.Equal("Secret not found.", context.Result.Failure.Message);
	}

	[Fact]
	public async Task OnTokenValidated_SecretRevoked_Fails()
	{
		// Arrange
		var secret = new ApiSecret();
		secret.Revoke();
		var context = CreateContext();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton<IApiSecretRepository>(new FakeApiSecretRepository(secret));
		context.HttpContext.RequestServices = serviceCollection.BuildServiceProvider();
		context.Principal = new ClaimsPrincipal();

		// Act
		await TokenValidatorService.OnTokenValidated(context);

		// Assert
		Assert.NotNull(context.Result);
		Assert.NotNull(context.Result.Failure);
		Assert.Equal($"Secret is revoked: {secret.Id}", context.Result.Failure.Message);
	}

	[Fact]
	public async Task OnTokenValidated_SecretIsValid_DoesNotFail()
	{
		// Arrange
		var secret = new ApiSecret();
		var context = CreateContext();
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton<IApiSecretRepository>(new FakeApiSecretRepository(secret));
		context.HttpContext.RequestServices = serviceCollection.BuildServiceProvider();
		context.Principal = new ClaimsPrincipal();

		// Act
		await TokenValidatorService.OnTokenValidated(context);

		// Assert
		Assert.Null(context.Result);
	}

	private static TokenValidatedContext CreateContext()
	{
		return new TokenValidatedContext(new DefaultHttpContext(), new AuthenticationScheme(JwtBearerDefaults.AuthenticationScheme, null, typeof(FakeAuthenticationHandler)), new JwtBearerOptions());
	}
}

file sealed class FakeAuthenticationHandler : IAuthenticationHandler
{
	public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
	{
		throw new NotImplementedException();
	}

	public Task<AuthenticateResult> AuthenticateAsync()
	{
		throw new NotImplementedException();
	}

	public Task ChallengeAsync(AuthenticationProperties? properties)
	{
		throw new NotImplementedException();
	}

	public Task ForbidAsync(AuthenticationProperties? properties)
	{
		throw new NotImplementedException();
	}
}

file sealed class FakeApiSecretRepository : IApiSecretRepository
{
	private readonly ApiSecret? _returnValue;

	public FakeApiSecretRepository(ApiSecret? returnValue)
	{
		_returnValue = returnValue;
	}

	public ValueTask<ApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
	{
		return ValueTask.FromResult(_returnValue);
	}
}