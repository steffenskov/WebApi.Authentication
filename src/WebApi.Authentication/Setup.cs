using Api.Authentication.Repositories;
using Api.Authentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

public static class Setup
{
	/// <param name="services">service collection to add this to.</param>
	extension(IServiceCollection services)
	{
		/// <summary>
		///     Adds the authentication and authorization services to the service collection with validation.
		/// </summary>
		/// <param name="configuration">Configuration of the authentication scheme</param>
		/// <param name="configureJwtBearerOptions">
		///     Optional: Configure the JwtBearerOptions, do note some configurations will be
		///     overwritten if used!
		/// </param>
		/// <param name="configureAuthorization">Optional: Configure the AddAuthorization call.</param>
		/// <typeparam name="TRepository">Type of IApiSecretRepository to use for storage of secrets</typeparam>
		/// <returns>The service collection</returns>
		public IServiceCollection AddApiAuthentication<TRepository>(AuthenticationConfiguration configuration, Action<JwtBearerOptions>? configureJwtBearerOptions = null, Action<AuthorizationOptions>? configureAuthorization = null)
			where TRepository : class, IApiSecretRepository
		{
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					configureJwtBearerOptions?.Invoke(options);
					options.TokenValidationParameters = new TokenValidationParameters
					{
						IssuerSigningKey = configuration.SigningKey,
						ValidIssuer = configuration.Issuer,
						ValidAudience = configuration.Audience,
						ClockSkew = TimeSpan.Zero,
						ValidateAudience = true,
						ValidateIssuer = true,
						ValidateIssuerSigningKey = true,
						ValidateLifetime = true
					};

					options.Events = new JwtBearerEvents
					{
						OnTokenValidated = OnTokenValidated
					};
				});


			if (configureAuthorization != null)
			{
				services.AddAuthorization(configureAuthorization);
			}
			else
			{
				services.AddAuthorization();
			}

			services.AddApiSecretRepository<TRepository>();
			return services;
		}

		private static async Task OnTokenValidated(TokenValidatedContext context)
		{
			var subClaim = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub) ?? context.Principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
			if (subClaim is null)
			{
				context.Fail("Missing token identifier claim.");
				return;
			}

			if (!Guid.TryParse(subClaim.Value, out var secretId))
			{
				context.Fail($"Invalid token identifier claim: {subClaim}");
				return;
			}

			var secretRepository = context.HttpContext.RequestServices.GetRequiredService<IApiSecretRepository>();

			var secret = await secretRepository.GetByIdAsync(secretId);
			if (secret is null)
			{
				context.Fail($"Secret not found: {subClaim}");
				return;
			}

			if (secret.IsRevoked)
			{
				context.Fail($"Secret is revoked: {subClaim}");
			}
		}

		/// <summary>
		///     Adds the IApiSecretProvider to the service collection. This can be used for issuing new api secrets.
		/// </summary>
		/// <param name="configuration">
		///     configuration of your authentication, make sure you use the same configuration when
		///     authenticating.
		/// </param>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretProvider<TRepository>(AuthenticationConfiguration configuration)
			where TRepository : class, IApiSecretRepository
		{
			services.AddSingleton<IJwtTokenProvider>(new JwtTokenProvider(configuration));
			services.AddSingleton<IApiSecretProvider, ApiSecretProvider>();
			services.AddApiSecretRepository<TRepository>();

			return services;
		}

		private IServiceCollection AddApiSecretRepository<TRepository>()
			where TRepository : class, IApiSecretRepository
		{
			services.AddSingleton<IApiSecretRepository, TRepository>();
			return services;
		}
	}
}