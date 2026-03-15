using System.Security.Claims;
using WebApi.Authentication;

namespace Api.WebApi;

public class SegregatedApiSecret : ApiSecret
{
	public Guid CustomerId { get; init; }

	public override IEnumerable<Claim> GetCustomClaims()
	{
		yield return new Claim("CustomerId", CustomerId.ToString());
	}
}