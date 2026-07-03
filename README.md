# Want to see what's changed?

[Changelog](https://github.com/steffenskov/WebApi.Authentication/blob/main/CHANGELOG.md)

# WebApi.Authentication

Simplifies issuing, authenticating, and maintaining Api Secrets for you publicly facing WebApi.

It's using Microsoft's JwtBearer package, and centered around Jwt as the primary authentication mechanism.

On top of that it adds a storage repository where secrets are stored. This in turn allows you to revoke a secret, something that's otherwise not possible with a long-lived Jwt token.

# Installation

I recommend using the NuGet package: [WebApi.Authentication](https://www.nuget.org/packages/WebApi.Authentication) however feel free to
clone the source instead if that suits your needs better.

# Usage

## Separation of concerns

Since this solution is mainly intended for public facing WebApi's, I'd suggest splitting your solution into two WebApi projects:

- One that requires User based Authentication, which allows the user to issue Api secrets for the publicly facing Api
- One publicly facing Api that requires Api secrets for its usage

This has 2 benefits:

- Simplifies configuration of Authentication, as you'll only have one mechanism per project (User based in one, Api secret based in the other)
- Avoids the "Noisy neighbor" syndrome, where a lot of traffic hitting your publicly facing Api could render your application Api unavailable

As such each has a slightly different setup, as shown below.

## Issuing new tokens

The solution is based around .Net's built-in Dependency Injection system. As such start by injecting the necessary bits:

```csharp
var builder = WebApplication.CreateBuilder(args);

var secretKey = RandomNumberGenerator.GetBytes(64); // You should generate this key once per environment you run, and store it securely.

var configuration = new AuthenticationConfiguration
{
    SecretKey = secretKey, 
    Issuer = "YOUR_NAME", // Could be the name of your company, yourself or similar - just pick one and stick with it.
    Audience = "YOUR_AUDIENCE", // Could be the name of your application or similar - just pick one and stick with it.
    Expiration = TimeSpan.FromDays(30) // How long should the token be valid for
};

// For this example we'll use a MongoDB database for storage
var client = new MongoClient("mongodb://localhost:27017"); // You should use a connection string here.
var db = client.GetDatabase("my_db");

builder.Services.AddApiSecretProvider(configuration) // Adds the IApiSecretProvider to the DI container
                .AddApiSecretMongoRepository(db, "api-secrets");
```

Now you'll have access to the `IApiSecretProvider` which you can use to store new Api secrets:

```csharp
var secret = new ApiSecret(); 
await provider.PersistSecretAsync(secret);

// secret.JwtToken is now a valid JWT token and can be given to the 3rd party for usage.
```

## Configuring Authentication of the tokens

Just like with issuing tokens, we'll need the same configuration:

```csharp
var builder = WebApplication.CreateBuilder(args);

var secretKey = RandomNumberGenerator.GetBytes(64); // You should generate this key once per environment you run, and store it securely.

var configuration = new AuthenticationConfiguration
{
    SecretKey = secretKey, 
    Issuer = "YOUR_NAME", // Could be the name of your company, yourself or similar - just pick one and stick with it.
    Audience = "YOUR_AUDIENCE", // Could be the name of your application or similar - just pick one and stick with it.
    Expiration = TimeSpan.FromDays(30) // How long should the token be valid for
};

// For this example we'll use a MongoDB database for storage
var client = new MongoClient("mongodb://localhost:27017"); // You should use a connection string here.
var db = client.GetDatabase("my_db");

builder.Services.AddApiSecretAuthentication(configuration, jwtBearerOptions =>
{
    if (builder.Environment.IsDevelopment()) // Disables HTTPs requirement in development
    {
        jwtBearerOptions.RequireHttpsMetadata = false;
    }
})
.AddApiSecretMongoRepository(db, "api-secrets");

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

And that's it for the authentication part, now you can mark any endpoints that requires Authentication with either `[Authorize]` or `.RequireAuthorization()` depending on whether you're using Controllers or Minimal APIs.

# Data segregation

The above Usage examples are all based on the default data model, where the `ApiSecret` class is used to store the secrets in a single repository.
It only contains the bare essentials for issuing and revoking tokens. However if you want to do data segregation, that's built-in as well.

I'd suggest defining a custom type for your secrets, rather than using generics everywhere, but it's up to you.

```csharp
public class CustomerApiSecret : SegregatedApiSecret<CustomerId>;
```

And that's all for configuration.

For your Dependency Injection you'll just need to add the generic types like so and use the `.AddSegregatedApiSecretRepository` method:

```csharp
builder.Services.AddApiSecretAuthentication<CustomerApiSecret>(configuration, jwtBearerOptions =>
{
    if (builder.Environment.IsDevelopment())
    {
        jwtBearerOptions.RequireHttpsMetadata = false;
    }
});

builder.Services.AddApiSecretProvider<CustomerApiSecret>(configuration)
                .AddSegregatedApiSecretMongoRepository<CustomerApiSecret, CustomerId>((provider, key) =>
                {
                    var client = provider.GetRequiredService<IMongoClient>();
                    var db = client.GetDatabase($"my_db_{key}"); // Segregate at db level
                    var collection = db.GetCollection<CustomerApiSecret>("api-secrets"); // You could segregate here instead/as well
                    return collection;
                });
```

# Customization

The above examples are very bare-bones in terms of data, so what if you'd like to include e.g. roles in your secrets?

Luckily this is really simple, just define a custom type for your secrets, and extend the model as you see fit. e.g.

```csharp
public class RoleBasedApiSecret : ApiSecret
{
    public required string[] Roles {get; init;}
}
```

And specify `RoleBasedApiSecret` when doing your DI, and you're all set :-)

```csharp
builder.Services.AddApiSecretAuthentication<RoleBasedApiSecret>(configuration, jwtBearerOptions =>
{
    if (builder.Environment.IsDevelopment())
    {
        jwtBearerOptions.RequireHttpsMetadata = false;
    }
});

builder.Services.AddApiSecretProvider<RoleBasedApiSecret>(configuration)
    .AddApiSecretMongoRepository(db, "api-secrets");
```

# Documentation

Auto generated documentation via [DocFx](https://github.com/dotnet/docfx) is available
here: https://steffenskov.github.io/WebApi.Authentication/

# Compatibility

## MongoDB

This is supported through the package [WebApi.Authentication.MongoDB](https://www.nuget.org/packages/WebApi.Authentication.MongoDB).