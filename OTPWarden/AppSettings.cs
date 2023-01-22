using Microsoft.IdentityModel.Tokens;

namespace OTPWarden;

public sealed class AppSettings
{
    public string Host { get; init; }
    public SymmetricSecurityKey JwtKey { get; init; }
    public bool AllowRegistration { get; init; }
}
