using DotNet.Testcontainers.Builders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Testcontainers.MongoDb;

namespace WebApi.Authentication.IntegrationTests.Fixtures;

public class ContainerFixture : IAsyncLifetime
{
	private readonly MongoDbContainer _mongoContainer;

	public ContainerFixture()
	{
		BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
		_mongoContainer = new MongoDbBuilder("mongo:latest")
			.WithUsername("mongo")
			.WithPassword("secret")
			.WithPortBinding(27017, 27017)
			.WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(27017))
			.Build();
	}

	public string MongoConnectionString => _mongoContainer.GetConnectionString();

	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
	}

	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.DisposeAsync();
	}
}