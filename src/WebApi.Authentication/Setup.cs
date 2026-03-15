using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Authentication;
using WebApi.Authentication.Configuration;
using WebApi.Authentication.Repositories;
using WebApi.Authentication.Services;

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
		/// <returns>The service collection</returns>
		public IServiceCollection AddApiAuthentication(AuthenticationConfiguration configuration, Action<JwtBearerOptions>? configureJwtBearerOptions = null,
			Action<AuthorizationOptions>? configureAuthorization = null)
		{
			return services.AddApiAuthentication<ApiSecret>(configuration, configureJwtBearerOptions, configureAuthorization);
		}

		/// <summary>
		///     Adds the authentication and authorization services to the service collection with validation using a custom
		///     ApiSecret type.
		/// </summary>
		/// <param name="configuration">Configuration of the authentication scheme</param>
		/// <param name="configureJwtBearerOptions">
		///     Optional: Configure the JwtBearerOptions, do note some configurations will be
		///     overwritten if used!
		/// </param>
		/// <param name="configureAuthorization">Optional: Configure the AddAuthorization call.</param>
		/// <typeparam name="TApiSecret">Custom type of ApiSecret to use</typeparam>
		/// <returns>The service collection</returns>
		public IServiceCollection AddApiAuthentication<TApiSecret>(AuthenticationConfiguration configuration, Action<JwtBearerOptions>? configureJwtBearerOptions = null,
			Action<AuthorizationOptions>? configureAuthorization = null)
		{
			JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear(); // Disable mapping of sub claim to nameidentifier claim
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

			return services;
		}

		private static async Task OnTokenValidated(TokenValidatedContext context)
		{
			if (context.Principal is null)
			{
				context.Fail("Principal is null.");
				return;
			}

			var claims = context.Principal.Claims.ToList();

			var secretRepository = context.HttpContext.RequestServices.GetRequiredService<IApiSecretRepository>();

			var secret = await secretRepository.GetByClaimsAsync(claims);
			if (secret is null)
			{
				context.Fail("Secret not found.");
				return;
			}

			if (secret.IsRevoked)
			{
				context.Fail($"Secret is revoked: {secret.Id}");
			}
		}

		/// <summary>
		///     Adds the ApiSecretProvider to the service collection. This can be used for issuing new api secrets.
		/// </summary>
		/// <param name="configuration">
		///     configuration of your authentication, make sure you use the same configuration when
		///     authenticating.
		/// </param>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretProvider(AuthenticationConfiguration configuration)
		{
			return services.AddApiSecretProvider<ApiSecret>(configuration);
		}

		/// <summary>
		///     Adds the ApiSecretProvider to the service collection using a custom ApiSecret type.
		///     This can be used for issuing new api secrets.
		/// </summary>
		/// <param name="configuration">
		///     configuration of your authentication, make sure you use the same configuration when
		///     authenticating.
		/// </param>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretProvider<TApiSecret>(AuthenticationConfiguration configuration)
			where TApiSecret : ApiSecret
		{
			services.AddSingleton<IApiSecretProvider>(provider =>
				new ApiSecretProvider<TApiSecret>(new JwtTokenProvider(configuration), provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));

			return services;
		}

		/// <summary>
		///     Adds a repository to the service collection for storing ApiSecrets.
		/// </summary>
		/// <typeparam name="TRepository">Type of repository</typeparam>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretRepository<TRepository>()
			where TRepository : class, IApiSecretRepository<ApiSecret>
		{
			return services.AddApiSecretRepository<ApiSecret, TRepository>();
		}

		/// <summary>
		///     Adds a repository to the service collection for storing ApiSecrets.
		/// </summary>
		/// <param name="repository">Instance of the repository</param>
		/// <typeparam name="TRepository">Type of repository</typeparam>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretRepository<TRepository>(TRepository repository)
			where TRepository : class, IApiSecretRepository<ApiSecret>
		{
			return services.AddApiSecretRepository<ApiSecret, TRepository>(repository);
		}

		/// <summary>
		///     Adds a repository to the service collection for storing ApiSecrets.
		/// </summary>
		/// <param name="repositoryFactory">Factory method to create the repository</param>
		/// <typeparam name="TRepository">Type of repository</typeparam>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretRepository<TRepository>(Func<IServiceProvider, TRepository> repositoryFactory)
			where TRepository : class, IApiSecretRepository<ApiSecret>
		{
			return services.AddApiSecretRepository<ApiSecret, TRepository>(repositoryFactory);
		}

		/// <summary>
		///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
		/// </summary>
		/// <typeparam name="TApiSecret">Type of ApiSecret</typeparam>
		/// <typeparam name="TRepository">Type of repository</typeparam>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretRepository<TApiSecret, TRepository>()
			where TApiSecret : ApiSecret
			where TRepository : class, IApiSecretRepository<TApiSecret>
		{
			services.AddSingleton<IApiSecretRepository<TApiSecret>, TRepository>();
			services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryShim<TApiSecret>(provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
			return services;
		}

		/// <summary>
		///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
		/// </summary>
		/// <param name="repository">Instance of the repository</param>
		/// <typeparam name="TApiSecret">Type of ApiSecret</typeparam>
		/// <typeparam name="TRepository">Type of repository</typeparam>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretRepository<TApiSecret, TRepository>(TRepository repository)
			where TApiSecret : ApiSecret
			where TRepository : class, IApiSecretRepository<TApiSecret>
		{
			services.AddSingleton<IApiSecretRepository<TApiSecret>>(repository);
			services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryShim<TApiSecret>(provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
			return services;
		}

		/// <summary>
		///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
		/// </summary>
		/// <param name="repositoryFactory">Factory method to create the repository</param>
		/// <typeparam name="TApiSecret">Type of ApiSecret</typeparam>
		/// <typeparam name="TRepository">Type of repository</typeparam>
		/// <returns>service collection</returns>
		public IServiceCollection AddApiSecretRepository<TApiSecret, TRepository>(Func<IServiceProvider, TRepository> repositoryFactory)
			where TApiSecret : ApiSecret
			where TRepository : class, IApiSecretRepository<TApiSecret>
		{
			services.AddSingleton<IApiSecretRepository<TApiSecret>, TRepository>(repositoryFactory);
			services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryShim<TApiSecret>(provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
			return services;
		}
	}
}