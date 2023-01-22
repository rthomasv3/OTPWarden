using Microsoft.AspNetCore.Mvc;

namespace OTPWarden.Controllers.V1.Contract.Requests;

public sealed class AuthenticateUserApiRequest
{
    [FromForm(Name = "grant_type")]
    public string GrantType { get; init; }
    [FromForm(Name = "username")]
    public string Username { get; init; }
    [FromForm(Name = "password")]
    public string Password { get; init; }
    [FromForm(Name = "refresh_token")]
    public string RefreshToken { get; init; }
}
