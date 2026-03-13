using Api.Authentication.Services;

namespace Api.Authentication;

public class ApiSecret
{
	public Guid Id { get; private set; }
	public string JwtToken { get; private set; } = "";
	public bool IsRevoked { get; private set; }

	internal static ApiSecret Create(IJwtTokenProvider tokenProvider)
	{
		var result = new ApiSecret
		{
			Id = Guid.CreateVersion7()
		};

		result.JwtToken = tokenProvider.CreateToken(result);

		return result;
	}

	/// <summary>
	///     Revokes the secret, preventing it from being used for authentication.
	/// </summary>
	public void Revoke()
	{
		IsRevoked = true;
	}
}