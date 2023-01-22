namespace OTPWarden.Models;

public sealed class User
{
    public long Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public string PasswordHash { get; init; }
    public string Username { get; init; }
}
