namespace WebApi.Authentication.DependencyInjection;

internal class WebApiAuthenticationServiceCollection<TApiSecret> : BaseWebApiAuthenticationServiceCollection<TApiSecret>, IWebApiAuthenticationServiceCollection<TApiSecret>
	where TApiSecret : ApiSecret
{
	public WebApiAuthenticationServiceCollection(IServiceCollection services) : base(services)
	{
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
}

internal class WebApiAuthenticationServiceCollection<TApiSecret, TKey> : BaseWebApiAuthenticationServiceCollection<TApiSecret>, IWebApiAuthenticationServiceCollection<TApiSecret, TKey>
	where TApiSecret : SegregatedApiSecret<TKey>
	where TKey : IParsable<TKey>
{
	public WebApiAuthenticationServiceCollection(IServiceCollection services) : base(services)
	{
	}

	public IWebApiAuthenticationServiceCollection<TApiSecret, TKey> AddSegregatedApiSecretRepository<TRepository>(Func<IServiceProvider, TKey, TRepository> repositoryFactory)
		where TRepository : class, IApiSecretRepository<TApiSecret>
	{
		if (_services.Any(sd => sd.ServiceType == typeof(IApiSecretRepository)))
		{
			throw new InvalidOperationException("An ApiSecret repository has already been registered.");
		}

		_services.AddSingleton<IApiSecretRepository<TApiSecret>>(provider => new SegregatedApiSecretRepositoryAdapter<TKey, TApiSecret, TRepository>(provider, repositoryFactory));
		AddAdapterRepository();
		return this;
	}
}

internal abstract class BaseWebApiAuthenticationServiceCollection<TApiSecret>
	where TApiSecret : class, IApiSecret
{
	protected readonly IServiceCollection _services;

	protected BaseWebApiAuthenticationServiceCollection(IServiceCollection services)
	{
		_services = services;
	}

	protected void AddAdapterRepository()
	{
		_services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryAdapter<TApiSecret>(provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
	}
}