namespace Dsw2026Tpi.Api.Configurations
{
    public record RateLimitSettings
    {
        public int PermitLimit { get; init; }
        public int Seconds { get; init;  }
    }
}