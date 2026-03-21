namespace WebApi.Authentication.DependencyInjection;

internal class WebApiAuthenticationServiceCollection<TApiSecret> : IWebApiAuthenticationServiceCollection<TApiSecret>
	where TApiSecret : ApiSecret
{
	private readonly IServiceCollection _services;

	public WebApiAuthenticationServiceCollection(IServiceCollection services)
	{
		_services = services;
	}

	public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>()
		where TRepository : class, IApiSecretRepository<TApiSecret>
	{
		if (_services.Any(sd => sd.ServiceType == typeof(IApiSecretRepository)))
		{
			throw new InvalidOperationException("An ApiSecret repository has already been registered.");
		}

		_services.AddSingleton<IApiSecretRepository<TApiSecret>, TRepository>();
		AddAdapterRepository();
		return this;
	}


	public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>(TRepository repository)
		where TRepository : class, IApiSecretRepository<TApiSecret>
	{
		if (_services.Any(sd => sd.ServiceType == typeof(IApiSecretRepository)))
		{
			throw new InvalidOperationException("An ApiSecret repository has already been registered.");
		}

		_services.AddSingleton<IApiSecretRepository<TApiSecret>>(repository);
		AddAdapterRepository();
		return this;
	}

	public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>(Func<IServiceProvider, TRepository> repositoryFactory)
		where TRepository : class, IApiSecretRepository<TApiSecret>
	{
		if (_services.Any(sd => sd.ServiceType == typeof(IApiSecretRepository)))
		{
			throw new InvalidOperationException("An ApiSecret repository has already been registered.");
		}

		_services.AddSingleton<IApiSecretRepository<TApiSecret>, TRepository>(repositoryFactory);
		AddAdapterRepository();
		return this;
	}

	private void AddAdapterRepository()
	{
		_services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryAdapter<TApiSecret>(provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
	}
}