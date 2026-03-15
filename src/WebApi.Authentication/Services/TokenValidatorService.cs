namespace WebApi.Authentication.Services;

internal static class TokenValidatorService
{
	public static async Task OnTokenValidated(TokenValidatedContext context)
	{
		if (context.Principal is null)
		{
			context.Fail("Principal is null.");
			return;
		}

		var claims = context.Principal.Claims.ToList();

		var secretRepository = context.HttpContext.RequestServices.GetRequiredService<IApiSecretRepository>();

		var secret = await secretRepository.GetByClaimsAsync(claims);
		if (secret is null)
		{
			context.Fail("Secret not found.");
			return;
		}

		if (secret.IsRevoked)
		{
			context.Fail($"Secret is revoked: {secret.Id}");
		}
	}
}