namespace OTPWarden.Models;

public sealed class AuthenticateUserResult
{
    public bool Success { get; init; }
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
    public int ExpiresIn { get; init; }
}
