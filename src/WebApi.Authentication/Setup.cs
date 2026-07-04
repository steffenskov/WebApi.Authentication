using Microsoft.AspNetCore.Authorization;
using WebApi.Authentication;
using WebApi.Authentication.Configuration;
using WebApi.Authentication.DependencyInjection;
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
		public IWebApiAuthenticationServiceCollection<ApiSecret> AddApiSecretAuthentication(AuthenticationConfiguration configuration, Action<JwtBearerOptions>? configureJwtBearerOptions = null,
			Action<AuthorizationOptions>? configureAuthorization = null)
		{
			return services.AddApiSecretAuthentication<ApiSecret>(configuration, configureJwtBearerOptions, configureAuthorization);
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
		public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretAuthentication<TApiSecret>(AuthenticationConfiguration configuration,
			Action<JwtBearerOptions>? configureJwtBearerOptions = null,
			Action<AuthorizationOptions>? configureAuthorization = null)
			where TApiSecret : ApiSecret
		{
			services.AddApiSecretAuthenticationInternal(configuration, configureJwtBearerOptions, configureAuthorization);

			return new WebApiAuthenticationServiceCollection<TApiSecret>(services);
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
		/// <typeparam name="TApiSecret">Type of ApiSecret. Must inherit SegregatedApiSecret</typeparam>
		/// <typeparam name="TKey">Type of key to use for repository segregation</typeparam>
		/// <returns>The service collection</returns>
		public IWebApiAuthenticationServiceCollection<TApiSecret, TKey> AddSegregatedApiSecretAuthentication<TApiSecret, TKey>(AuthenticationConfiguration configuration,
			Action<JwtBearerOptions>? configureJwtBearerOptions = null,
			Action<AuthorizationOptions>? configureAuthorization = null)
			where TApiSecret : SegregatedApiSecret<TKey>
			where TKey : IParsable<TKey>
		{
			services.AddApiSecretAuthenticationInternal(configuration, configureJwtBearerOptions, configureAuthorization);

			return new WebApiAuthenticationServiceCollection<TApiSecret, TKey>(services);
		}

		private void AddApiSecretAuthenticationInternal(AuthenticationConfiguration configuration, Action<JwtBearerOptions>? configureJwtBearerOptions,
			Action<AuthorizationOptions>? configureAuthorization)
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
						OnTokenValidated = TokenValidatorService.OnTokenValidated
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
		}

		/// <summary>
		///     Adds the ApiSecretProvider to the service collection. This can be used for issuing new api secrets.
		/// </summary>
		/// <param name="configuration">
		///     configuration of your authentication, make sure you use the same configuration when
		///     authenticating.
		/// </param>
		/// <returns>service collection</returns>
		public IWebApiAuthenticationServiceCollection<ApiSecret> AddApiSecretProvider(AuthenticationConfiguration configuration)
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
		public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretProvider<TApiSecret>(AuthenticationConfiguration configuration)
			where TApiSecret : ApiSecret
		{
			services.AddApiSecretProviderInternal<TApiSecret>(configuration);

			return new WebApiAuthenticationServiceCollection<TApiSecret>(services);
		}

		/// <summary>
		///     Adds the ApiSecretProvider to the service collection using a custom ApiSecret type with data segregation.
		///     This can be used for issuing new api secrets.
		/// </summary>
		/// <param name="configuration">
		///     configuration of your authentication, make sure you use the same configuration when
		///     authenticating.
		/// </param>
		/// <returns>service collection</returns>
		public IWebApiAuthenticationServiceCollection<TApiSecret, TKey> AddSegregatedApiSecretProvider<TApiSecret, TKey>(AuthenticationConfiguration configuration)
			where TApiSecret : SegregatedApiSecret<TKey>
			where TKey : IParsable<TKey>
		{
			services.AddApiSecretProviderInternal<TApiSecret>(configuration);

			return new WebApiAuthenticationServiceCollection<TApiSecret, TKey>(services);
		}

		private void AddApiSecretProviderInternal<TApiSecret>(AuthenticationConfiguration configuration)
			where TApiSecret : class, IApiSecret
		{
			services.AddSingleton<IApiSecretProvider>(provider =>
				new ApiSecretProvider<TApiSecret>(new JwtTokenProvider(configuration), provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
		}
	}
}