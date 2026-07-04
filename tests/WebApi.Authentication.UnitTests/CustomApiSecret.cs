namespace WebApi.Authentication.UnitTests;

public class CustomApiSecret : ApiSecret;

public class CustomSegregatedApiSecret : SegregatedApiSecret<Guid>;