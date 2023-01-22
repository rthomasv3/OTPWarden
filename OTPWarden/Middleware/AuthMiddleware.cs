using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace OTPWarden.Middleware;

public sealed class AuthMiddleware
{
    #region Fields

    private static readonly HashSet<string> _endpointWhitelist = new HashSet<string>()
    {
        "/api/v1/auth/authenticate",
        "/api/v1/auth/register",
    };

    private readonly RequestDelegate _next;

    #endregion

    #region Constructor

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    #endregion

    #region Public Methods

    public async Task Invoke(HttpContext httpContext, AppSettings appSettings)
    {
        string path = httpContext.Request.Path.Value ?? String.Empty;

        if (!path.StartsWith("/api") || _endpointWhitelist.Contains(path))
        {
            await _next(httpContext);
        }
        else
        {
            bool isValid = false;
            JwtSecurityTokenHandler accessTokenHandler = new JwtSecurityTokenHandler();
            string authorizationHeader = httpContext.Request.Headers.Authorization.ToString();

            if (!String.IsNullOrWhiteSpace(authorizationHeader) &&
                authorizationHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                string accessToken = authorizationHeader.Replace("Bearer", String.Empty, StringComparison.OrdinalIgnoreCase).Trim();

                if (accessTokenHandler.CanReadToken(accessToken))
                {
                    TokenValidationResult tokenValidationResult = await accessTokenHandler.ValidateTokenAsync(accessToken, new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = appSettings.JwtKey,
                        ValidateIssuer = true,
                        ValidIssuer = "OTPWarden",
                        ValidateLifetime = true,
                        ValidateAudience = false
                    });

                    if (tokenValidationResult.IsValid)
                    {
                        httpContext.User = new ClaimsPrincipal(tokenValidationResult.ClaimsIdentity);
                        isValid = true;
                    }
                }
            }

            if (isValid)
            {
                await _next(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await httpContext.Response.WriteAsJsonAsync(new UnauthorizedResult());
            }
        }
    }

    #endregion
}
