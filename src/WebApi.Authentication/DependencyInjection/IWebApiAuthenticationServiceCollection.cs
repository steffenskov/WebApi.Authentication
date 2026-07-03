namespace WebApi.Authentication.DependencyInjection;

/// <summary>
///     Custom ServiceCollection for adding repositories to the WebApi.Authentication configuration.
/// </summary>
/// <typeparam name="TApiSecret">Type of ApiSecret</typeparam>
public interface IWebApiAuthenticationServiceCollection<TApiSecret>
	where TApiSecret : ApiSecret
{
	/// <summary>
	///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
	/// </summary>
	/// <typeparam name="TRepository">Type of repository</typeparam>
	/// <returns>service collection</returns>
	IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>()
		where TRepository : class, IApiSecretRepository<TApiSecret>;

	/// <summary>
	///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
	/// </summary>
	/// <param name="repository">Instance of the repository</param>
	/// <typeparam name="TRepository">Type of repository</typeparam>
	/// <returns>service collection</returns>
	IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>(TRepository repository)
		where TRepository : class, IApiSecretRepository<TApiSecret>;

	/// <summary>
	///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
	/// </summary>
	/// <param name="repositoryFactory">Factory method to create the repository</param>
	/// <typeparam name="TRepository">Type of repository</typeparam>
	/// <returns>service collection</returns>
	IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>(Func<IServiceProvider, TRepository> repositoryFactory)
		where TRepository : class, IApiSecretRepository<TApiSecret>;

	/// <summary>
	///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type and a segregation
	///     key for the underlying repositories.
	/// </summary>
	/// <param name="repositoryFactory">Factory method to create the repository</param>
	/// <typeparam name="TSegregatedApiSecret">Type of ApiSecret, must inherit SegregatedApiSecret</typeparam>
	/// <typeparam name="TKey">Type of key to use for repository segregation</typeparam>
	/// <typeparam name="TRepository">Type of repository</typeparam>
	/// <returns>service collection</returns>
	IWebApiAuthenticationServiceCollection<TApiSecret> AddSegregatedApiSecretRepository<TSegregatedApiSecret, TKey, TRepository>(Func<IServiceProvider, TKey, TRepository> repositoryFactory)
		where TSegregatedApiSecret : SegregatedApiSecret<TKey>, TApiSecret
		where TKey : IParsable<TKey>
		where TRepository : class, IApiSecretRepository<TSegregatedApiSecret>;
}