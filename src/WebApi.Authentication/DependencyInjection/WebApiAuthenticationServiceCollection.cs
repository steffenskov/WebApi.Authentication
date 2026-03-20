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
		_services.AddSingleton<IApiSecretRepository<TApiSecret>, TRepository>();
		_services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryAdapter<TApiSecret>(provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
		return this;
	}

	public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>(TRepository repository)
		where TRepository : class, IApiSecretRepository<TApiSecret>
	{
		_services.AddSingleton<IApiSecretRepository<TApiSecret>>(repository);
		_services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryAdapter<TApiSecret>(provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
		return this;
	}

	public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>(Func<IServiceProvider, TRepository> repositoryFactory)
		where TRepository : class, IApiSecretRepository<TApiSecret>
	{
		_services.AddSingleton<IApiSecretRepository<TApiSecret>, TRepository>(repositoryFactory);
		_services.AddSingleton<IApiSecretRepository>(provider => new ApiSecretRepositoryAdapter<TApiSecret>(provider.GetRequiredService<IApiSecretRepository<TApiSecret>>()));
		return this;
	}
}