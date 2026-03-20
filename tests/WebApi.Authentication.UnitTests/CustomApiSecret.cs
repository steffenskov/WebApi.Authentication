namespace WebApi.Authentication.UnitTests;

public class CustomApiSecret : ApiSecret
{
	public string Role { get; init; } = "";
}