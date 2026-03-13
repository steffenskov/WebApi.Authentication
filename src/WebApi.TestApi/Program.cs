using System.Security.Cryptography;
using Api.Authentication.Configuration;
using Api.Authentication.Repositories;
using Api.Authentication.Services;
using Api.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var configuration = new AuthenticationConfiguration
{
	SecretKey = RandomNumberGenerator.GetBytes(64),
	Issuer = "ApiAuthentication",
	Audience = "WebApi",
	Expiration = TimeSpan.FromMinutes(5)
};
builder.Services.AddApiAuthentication<InMemoryRepository>(configuration, jwtBearerOptions =>
{
	if (builder.Environment.IsDevelopment())
	{
		jwtBearerOptions.RequireHttpsMetadata = false;
	}
});
builder.Services.AddApiSecretProvider<InMemoryRepository>(configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwaggerUi(options => { options.DocumentPath = "openapi/v1.json"; });
}

app.UseHttpsRedirection();

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
	{
		var forecast = Enumerable.Range(1, 5).Select(index =>
				new WeatherForecast
				(
					DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
					Random.Shared.Next(-20, 55),
					summaries[Random.Shared.Next(summaries.Length)]
				))
			.ToArray();
		return forecast;
	})
	.WithName("GetWeatherForecast")
	.RequireAuthorization();

app.MapGet("/login", async (IApiSecretProvider provider) =>
	{
		var secret = await provider.CreateSecretAsync();
		return secret;
	})
	.WithName("Login")
	.AllowAnonymous();

app.MapGet("/revoke/{id:guid}", async (IApiSecretRepository repository, Guid id) =>
	{
		var secret = await repository.GetByIdAsync(id);
		if (secret is null)
		{
			return Results.NotFound();
		}

		secret.Revoke();
		await repository.PersistAsync(secret);

		return Results.Ok(secret);
	}).WithName("Revoke")
	.RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}