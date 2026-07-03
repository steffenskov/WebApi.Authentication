namespace WebApi.Authentication;

public class SegregatedApiSecret<TKey> : ApiSecret
	where TKey : notnull
{
	public required TKey Key { get; init; }

	public override IEnumerable<Claim> GetCustomClaims()
	{
		var value = Key.ToString() ?? throw new InvalidOperationException($"{typeof(TKey).Name}.ToString() returns null!");

		yield return new Claim(Consts.SegregationClaim, value);
	}
}