namespace Fake.Detection.Auth.Configure;

public class JWTOptions
{
    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpireMinutes { get; init; }
}