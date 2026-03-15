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
builder.Services.AddApiSecretMongoRepository(db, "api-secrets");

builder.Services.AddApiSecretProvider(configuration); // Adds the IApiSecretProvider to the DI container
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
builder.Services.AddApiSecretRepositoryMongoDb(db, "api-secrets");

builder.Services.AddApiAuthentication(configuration, jwtBearerOptions =>
{
    if (builder.Environment.IsDevelopment()) // Disables HTTPs requirement in development
    {
        jwtBearerOptions.RequireHttpsMetadata = false;
    }
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
```

And that's it for the authentication part, now you can mark any endpoints that requires Authentication with either `[Authorize]` or `.RequireAuthorization()` depending on whether you're using Controllers or Minimal APIs.

# Customization

The above Usage examples are all based on the default data model, where the `ApiSecret` class is used to store the secrets.
It only contains the bare essentials for issuing and revoking tokens. However if you wanted to extend the model, i.e. for data segregation purposes, you can do so.

Here's an example of how you could add a `CustomerId` property to the secrets and do segregated storage of secrets based on that CustomerId.

```csharp
public class SegregatedApiSecret : ApiSecret
{
    public const string CustomerIdClaimType = "CustomerId";
    public Guid CustomerId { get; init; }
    
    public override IEnumerable<Claim> GetCustomClaims()
    {
        yield return new Claim(CustomerIdClaimType, CustomerId.ToString());
    }
}

public class SegregatedApiSecretRepository : IApiSecretRepository<SegregatedApiSecret>
{
    private readonly Func<CustomerId, IApiSecretRepository<SegregatedApiSecret>> _customerRepositoryFactory;
    public SegregatedApiSecretRepository(Func<CustomerId, IApiSecretRepository<SegregatedApiSecret>> customerRepositoryFactory)
    {
        _customerRepositoryFactory = customerRepositoryFactory;
    }
    
    public ValueTask<SegregatedApiSecret?> GetByClaimsAsync(ICollection<Claim> claims, CancellationToken cancellationToken = default)
    {
        var customerClaim = claims.FirstOrDefault(claim => claim.Type == SegregatedApiSecret.CustomerIdClaimType);

        if (!Guid.TryParse(customerClaim?.Value, out var customerId))
        {
            return ValueTask.FromResult<SegregatedApiSecret?>(null);
        }
        
        var customerRepository = _customerRepositoryFactory(customerId);
        return customerRepository.GetByClaimsAsync(claims, cancellationToken);
    }
}
```

And that's it for configuration, do not you'll need to instantiate the `SegregatedApiSecretRepository` with some way of obtaining a per-Customer repository.

Finally for your Dependency Injection you'll just need to add the generic types like so:

```csharp
builder.Services.AddApiSecretRepository<SegregatedApiSecret, SegregatedApiSecretRepository>();

builder.Services.AddApiAuthentication<SegregatedApiSecret>(configuration, jwtBearerOptions =>
{
    if (builder.Environment.IsDevelopment())
    {
        jwtBearerOptions.RequireHttpsMetadata = false;
    }
});

builder.Services.AddApiSecretProvider<SegregatedApiSecret>(configuration);
```

And just to complete the picture, here's an example of how you could inject `SegregatedApiSecretRepository` with a factory method:

```csharp
builder.Services.AddApiSecretRepository<SegregatedApiSecret, SegregatedApiSecretRepository>(provider => 
{
    var client = provider.GetRequiredService<IMongoClient>(); // example based on MongoDB
    return new SegregatedApiSecretRepository(customerId =>
    {
        return new ApiSecretMongoRepository(client.GetDatabase($"my_db_{customerId}"), "api-secrets"));    
    });
});
```

# Documentation

Auto generated documentation via [DocFx](https://github.com/dotnet/docfx) is available
here: https://steffenskov.github.io/WebApi.Authentication/

# Compatibility

## MongoDB

This is supported through the package [WebApi.Authentication.MongoDB](https://www.nuget.org/packages/WebApi.Authentication.MongoDB).