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
		AddAdapterRepository<TApiSecret>();
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
		AddAdapterRepository<TApiSecret>();
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
		AddAdapterRepository<TApiSecret>();
		return this;
	}

	public IWebApiAuthenticationServiceCollection<TApiSecret> AddSegregatedApiSecretRepository<TSegregatedApiSecret, TKey, TRepository>(Func<IServiceProvider, TKey, TRepository> repositoryFactory)
		where TSegregatedApiSecret : SegregatedApiSecret<TKey>, TApiSecret
		where TKey : IParsable<TKey>
		where TRepository : class, IApiSecretRepository<TSegregatedApiSecret>
	{
		if (_services.Any(sd => sd.ServiceType == typeof(IApiSecretRepository)))
		{
			throw new InvalidOperationException("An ApiSecret repository has already been registered.");
		}

		_services.AddSingleton<IApiSecretRepository<TSegregatedApiSecret>>(provider => new SegregatedApiSecretRepositoryAdapter<TKey, TSegregatedApiSecret, TRepository>(provider, repositoryFactory));
		AddAdapterRepository<TSegregatedApiSecret>();
		return this;
	}

	private void AddAdapterRepository<TCustomApiSecret>()
		where TCustomApiSecret : TApiSecret
	{
		_services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryAdapter<TCustomApiSecret>(provider.GetRequiredService<IApiSecretRepository<TCustomApiSecret>>()));
	}
}