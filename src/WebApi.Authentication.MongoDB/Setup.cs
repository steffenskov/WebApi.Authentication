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
		/// <typeparam name="TApiSecret">Type of ApiSecret</typeparam>
		/// <returns>The service collection</returns>
		public IWebApiAuthenticationServiceCollection<TApiSecret> AddApiSecretMongoRepository(IMongoDatabase db, string collectionName)
		{
			return services.AddApiSecretRepository(new ApiSecretMongoRepository<TApiSecret>(db, collectionName));
		}
	}
}