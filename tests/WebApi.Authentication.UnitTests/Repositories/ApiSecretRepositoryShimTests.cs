using System.Security.Claims;

namespace WebApi.Authentication.UnitTests.Repositories;

public class ApiSecretRepositoryShimTests
{
	[Fact]
	public async Task GetByClaimsAsync_NoState_ForwardsCallToInnerRepository()
	{
		// Arrange
		ICollection<Claim>? capturedClaims = null;
		Claim[] claims = [new("sub", Guid.NewGuid().ToString())];
		var innerRepository = Substitute.For<IApiSecretRepository<ApiSecret>>();
		innerRepository.When(x => x.GetByClaimsAsync(Arg.Any<ICollection<Claim>>(), Arg.Any<CancellationToken>()))
			.Do(callInfo => { capturedClaims = callInfo.Arg<ICollection<Claim>>(); });

		var repository = new ApiSecretRepositoryShim<ApiSecret>(innerRepository);

		// Act
		await repository.GetByClaimsAsync(claims, TestContext.Current.CancellationToken);

		// Assert
		Assert.NotNull(capturedClaims);
		Assert.Same(claims, capturedClaims);
	}
}