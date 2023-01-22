using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTPWarden.Controllers.V1.Contract.Requests;
using OTPWarden.Controllers.V1.Contract.Responses;
using OTPWarden.Models;
using OTPWarden.Services.Abstractions;

namespace OTPWarden.Controllers.V1;

[Route("api/v1/[controller]")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    #region Fields

    private readonly IAuthService _authService;

    #endregion

    #region Constructor

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    #endregion

    #region Public Methods

    [HttpPost("authenticate")]
    public async Task<ActionResult<AuthenticateUserApiResponse>> Authenticate([FromForm] AuthenticateUserApiRequest request)
    {
        ActionResult<AuthenticateUserApiResponse> response = Unauthorized();
        string userDevice = HttpContext.Request.Headers.UserAgent.ToString();
        string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        AuthenticateUserResult authenticateResult = await _authService.Authenticate(request.GrantType, userDevice, userIpAddress,
            request.Username, request.Password, request.RefreshToken);

        if (authenticateResult.Success)
        {
            response = Ok(new AuthenticateUserApiResponse()
            {
                AccessToken = authenticateResult.AccessToken,
                ExpiresIn = authenticateResult.ExpiresIn,
                RefreshToken = authenticateResult.RefreshToken,
                TokenType = "Bearer"
            });
        }

        return response;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthenticateUserApiResponse>> Register([FromBody] RegisterUserApiRequest request)
    {
        ActionResult<AuthenticateUserApiResponse> response = BadRequest();
        string userDevice = HttpContext.Request.Headers.UserAgent.ToString() ?? String.Empty;
        string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;

        if (!String.IsNullOrWhiteSpace(request.Email) && !String.IsNullOrWhiteSpace(request.Password))
        {
            AuthenticateUserResult registerResult = await _authService.Register(request.Email, request.Password, userDevice, userIpAddress);

            if (registerResult?.Success == true)
            {
                response = Ok(new AuthenticateUserApiResponse()
                {
                    AccessToken = registerResult.AccessToken,
                    ExpiresIn = registerResult.ExpiresIn,
                    RefreshToken = registerResult.RefreshToken,
                    TokenType = "Bearer"
                });
            }
        }

        return response;
    }

    [HttpPost("logout")]
    public async Task<ActionResult<UserLogoutApiResponse>> Logout()
    {
        ActionResult<UserLogoutApiResponse> response = BadRequest();
        Int64.TryParse(HttpContext.User.FindFirstValue(ClaimTypes.Sid), out long sessionId);
        Int64.TryParse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out long userId);

        bool loggedOutSuccessfully = await _authService.Logout(userId, sessionId);

        if (loggedOutSuccessfully)
        {
            response = Ok(new UserLogoutApiResponse()
            {
                LoggedOutSuccessfully = true
            });
        }

        return response;
    }

    #endregion
}
