using MongoDB.Driver;
using WebApi.Authentication;
using WebApi.Authentication.DependencyInjection;
using WebApi.Authentication.MongoDB.Repositories;

public static class Setup
{
	extension<TApiSecret>(IWebApiAuthenticationServiceCollection<TApiSecret> services)
		where TApiSecret : ApiSecret
	{
		/// <summary>
		///     Adds a MongoDB based repository for ApiSecrets to the service collection using a custom ApiSecret type
		/// </summary>
		/// <param name="db">Mongo database to stored secrets in</param>
		/// <param name="collectionName">Name of the collection to store secrets in</param>
		/// <returns>The service collection</returns>
		public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretMongoRepository(IMongoDatabase db, string collectionName)
		{
			return services.AddApiSecretRepository(new ApiSecretMongoRepository<TApiSecret>(db, collectionName));
		}
	}

	extension<TApiSecret, TKey>(IWebApiAuthenticationServiceCollection<TApiSecret, TKey> services)
		where TApiSecret : SegregatedApiSecret<TKey>
		where TKey : IParsable<TKey>
	{
		/// <summary>
		///     Adds a MongoDB based repository for ApiSecrets to the service collection using a custom ApiSecret type
		/// </summary>
		/// <typeparam name="TApiSecret">Type of ApiSecret. Must inherit SegregatedApiSecret</typeparam>
		/// <typeparam name="TKey">Type of key to use for repository segregation</typeparam>
		/// <param name="factoryMethod">Factory method to obtain a mongo collection for a specific key</param>
		/// <returns>The service collection</returns>
		public IWebApiAuthenticationServiceCollection<TApiSecret, TKey> AddSegregatedApiSecretMongoRepository(
			Func<IServiceProvider, TKey, IMongoCollection<TApiSecret>> factoryMethod)
		{
			return services.AddSegregatedApiSecretRepository<ApiSecretMongoRepository<TApiSecret>>((provider, key) =>
			{
				var collection = factoryMethod(provider, key);
				return new ApiSecretMongoRepository<TApiSecret>(collection);
			});
		}
	}
}