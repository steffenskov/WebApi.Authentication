namespace WebApi.Authentication.DependencyInjection;

public interface IWebApiAuthenticationServiceCollection<TApiSecret>
	where TApiSecret : ApiSecret
{
	/// <summary>
	///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
	/// </summary>
	/// <typeparam name="TApiSecret">Type of ApiSecret</typeparam>
	/// <typeparam name="TRepository">Type of repository</typeparam>
	/// <returns>service collection</returns>
	IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>()
		where TRepository : class, IApiSecretRepository<TApiSecret>;

	/// <summary>
	///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
	/// </summary>
	/// <param name="repository">Instance of the repository</param>
	/// <typeparam name="TApiSecret">Type of ApiSecret</typeparam>
	/// <typeparam name="TRepository">Type of repository</typeparam>
	/// <returns>service collection</returns>
	IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>(TRepository repository)
		where TRepository : class, IApiSecretRepository<TApiSecret>;

	/// <summary>
	///     Adds a repository to the service collection for storing ApiSecrets using a custom ApiSecret type.
	/// </summary>
	/// <param name="repositoryFactory">Factory method to create the repository</param>
	/// <typeparam name="TApiSecret">Type of ApiSecret</typeparam>
	/// <typeparam name="TRepository">Type of repository</typeparam>
	/// <returns>service collection</returns>
	IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretRepository<TRepository>(Func<IServiceProvider, TRepository> repositoryFactory)
		where TRepository : class, IApiSecretRepository<TApiSecret>;
}