# Want to see what's changed?

[Changelog](https://github.com/steffenskov/WebApi.Authentication/blob/main/CHANGELOG.md)

# WebApi.Authentication.MongoDB

This package add support for using MongoDB as storage repository for Api secrets.

# Usage

Dependency inject an `IApiSecretRepository` using the `.AddApiSecretMongoRepository(db, collectionName)` method.

For example:

```csharp
var client = new MongoClient(connectionString);
var db = client.GetDatabase("myDb");

services.AddApiSecretAuthentication(configuration) // OR services.AddApiSecretProvider(configuration)
        .AddApiSecretMongoRepository(db, "api-secrets");
```

or, if you want data segregation:

```csharp
public class CustomerApiSecret : SegregatedApiSecret<CustomerId>;
```

```csharp
var client = new MongoClient(connectionString);
var db = client.GetDatabase("myDb");

services.AddApiSecretProvider<CustomerApiSecret>(configuration)
        .AddSegregatedApiSecretMongoRepository<CustomerApiSecret, CustomerId>((provider, key) =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            var db = client.GetDatabase($"my_db_{key}"); // Segregate at db level
            var collection = db.GetCollection<CustomerApiSecret>("api-secrets"); // You could segregate here instead/as well
            return collection;
        });
```

# Documentation

Auto generated documentation via [DocFx](https://github.com/dotnet/docfx) is available
here: https://steffenskov.github.io/WebApi.Authentication/