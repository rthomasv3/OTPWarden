using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OTPWarden.DataAccess.Abstractions;
using OTPWarden.DataAccess.Data;
using OTPWarden.Models;
using OTPWarden.Services.Abstractions;

namespace OTPWarden.Services;

public sealed class AuthService : IAuthService
{
    #region Fields

    private static readonly TimeSpan _accessTokenLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromHours(1);

    private readonly IUserService _userService;
    private readonly AppSettings _appSettings;
    private readonly ICryptographyService _cryptographyService;
    private readonly IKeyService _keyService;
    private readonly IUserSessionRepository _userSessionRepository;

    #endregion

    #region Constructor

    public AuthService(IUserService userService, AppSettings appSettings, ICryptographyService cryptographyService, 
        IKeyService keyService, IUserSessionRepository userSessionRepository)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _cryptographyService = cryptographyService ?? throw new ArgumentNullException(nameof(cryptographyService));
        _keyService = keyService ?? throw new ArgumentNullException(nameof(keyService));
        _userSessionRepository = userSessionRepository ?? throw new ArgumentNullException(nameof(userSessionRepository));
    }

    #endregion

    #region Public Methods

    public async Task<AuthenticateUserResult> Authenticate(string grantType, string userDevice, string userIpAddress,
        string email, string password, string refreshToken)
    {
        AuthenticateUserResult authenticateResult = new AuthenticateUserResult();

        if (String.Equals("password", grantType, StringComparison.OrdinalIgnoreCase) &&
           !String.IsNullOrWhiteSpace(email) && !String.IsNullOrWhiteSpace(password))
        {
            authenticateResult = await Login(email, password, userDevice, userIpAddress);
        }
        else if (String.Equals("refresh_token", grantType, StringComparison.OrdinalIgnoreCase) &&
                !String.IsNullOrWhiteSpace(refreshToken))
        {
            authenticateResult = await RefreshUserSession(refreshToken, userDevice, userIpAddress);
        }

        return authenticateResult;
    }

    public async Task<AuthenticateUserResult> Register(string email, string password, string userDevice, string userIpAddress)
    {
        AuthenticateUserResult authenticateUserResult = null;

        if (_appSettings.AllowRegistration)
        {
            if (MailAddress.TryCreate(email, out MailAddress _) && !String.IsNullOrWhiteSpace(password))
            {
                UserData user = await _userService.CreateUser(email, password);

                if (user != null)
                {
                    authenticateUserResult = await Login(email, password, userDevice, userIpAddress);
                }
            }
        }

        return authenticateUserResult;
    }

    public async Task<bool> Logout(long userId, long sessionId)
    {
        bool loggedOut = await _userSessionRepository.EndUserSession(sessionId, userId);

        IEnumerable<UserSessionData> activeUserSessions = await _userSessionRepository
            .GetActiveUserSessions(userId, x => new UserSessionData()
            {
                Id = x.Id
            });

        if (!activeUserSessions.Any())
        {
            _keyService.RemoveUserKey(userId);
        }

        return loggedOut;
    }

    #endregion

    #region Private Methods

    private async Task<AuthenticateUserResult> Login(string email, string password, string userDevice, string userIpAddress)
    {
        int expiresIn = 0;
        bool succes = false;
        string accessToken = String.Empty;
        string refreshToken = String.Empty;

        UserData user = await _userService.GetUser(email);

        if (user != null && _cryptographyService.VerifyPassword(password, user.Password))
        {
            // store the user's encryption key in memory
            _keyService.AddUserKey(user.Id, password);

            // create user session
            string userEncryptionKey = _keyService.GetUserKey(user.Id);
            string encryptedUserDevice = _cryptographyService.Encrypt(userDevice, userEncryptionKey) ?? String.Empty;
            string encryptedUserIpAddress = _cryptographyService.Encrypt(userIpAddress, userEncryptionKey) ?? String.Empty;

            UserSessionData userSessionData = await _userSessionRepository.CreateUserSession(user.Id, encryptedUserDevice,
                encryptedUserIpAddress, CreateUserRefreshToken(), DateTime.UtcNow + _refreshTokenLifetime);

            accessToken = CreateUserAccessToken(user, userSessionData);
            refreshToken = userSessionData.RefreshToken;
            expiresIn = (int)_accessTokenLifetime.TotalSeconds;
            succes = true;
        }

        return new AuthenticateUserResult()
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            Success = succes
        };
    }

    private async Task<AuthenticateUserResult> RefreshUserSession(string refreshToken, string userDevice, string userIpAddress)
    {
        int expiresIn = 0;
        bool succes = false;
        string accessToken = String.Empty;
        string updatedRefreshToken = String.Empty;

        UserSessionData userSessionData = await _userSessionRepository.GetUserSession(refreshToken);

        if (userSessionData != null && !userSessionData.IsRevoked && userSessionData.RefreshTokenExpiration > DateTime.UtcNow)
        {
            string userEncryptionKey = _keyService.GetUserKey(userSessionData.UserId);
            string decryptedUserDevice = _cryptographyService.Decrypt(userSessionData.UserDevice, userEncryptionKey);
            string decryptedUserIpAddress = _cryptographyService.Decrypt(userSessionData.IpAddress, userEncryptionKey);

            if (decryptedUserDevice == userDevice && decryptedUserIpAddress == userIpAddress)
            {
                UserData user = await _userService.GetUser(userSessionData.UserId);

                if (user != null)
                {
                    updatedRefreshToken = CreateUserRefreshToken();

                    await _userSessionRepository.UpdateUserSession(userSessionData.Id, updatedRefreshToken,
                        DateTime.UtcNow + _refreshTokenLifetime);

                    accessToken = CreateUserAccessToken(user, userSessionData);
                    expiresIn = (int)_accessTokenLifetime.TotalSeconds;
                    succes = true;
                }
            }
        }

        return new AuthenticateUserResult()
        {
            AccessToken = accessToken,
            ExpiresIn = expiresIn,
            RefreshToken = updatedRefreshToken,
            Success = succes
        };
    }

    private string CreateUserAccessToken(UserData user, UserSessionData userSessionData)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Sid, userSessionData.Id.ToString()),
            }),
            Expires = DateTime.UtcNow.Add(_accessTokenLifetime),
            Issuer = "OTPWarden",
            Audience = "",
            SigningCredentials = new SigningCredentials(_appSettings.JwtKey, SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private string CreateUserRefreshToken()
    {
        byte[] refreshBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(refreshBytes);
    }

    #endregion
}
