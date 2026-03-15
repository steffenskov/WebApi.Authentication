# Want to see what's changed?

[Changelog](https://github.com/steffenskov/WebApi.Authentication/blob/main/CHANGELOG.md)

# WebApi.Authentication.MongoDB

This package add support for using MongoDB as storage repository for Api secrets.

# Usage

Dependency inject an `IApiSecretRepository` using the `services.AddApiSecretMongoRepository(db, collectionName)` method.

For example:

```csharp
var client = new MongoClient(connectionString);
var db = client.GetDatabase("myDb");
services.AddApiSecretMongoRepository(db, "api-secrets");
```

# Documentation

Auto generated documentation via [DocFx](https://github.com/dotnet/docfx) is available
here: https://steffenskov.github.io/WebApi.Authentication/