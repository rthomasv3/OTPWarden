using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTPWarden.DataAccess.Abstractions;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.DataAccess;

public sealed class UserSessionRepository : IUserSessionRepository
{
    #region Fields

    private readonly OTPDbContext _context;

    #endregion

    #region Constructor

    public UserSessionRepository(OTPDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion

    #region Properties

    public Expression<Func<UserSessionData, UserSessionData>> DefaultUserSessionDataMapper
    {
        get
        {
            return x => new UserSessionData()
            {
                Created = x.Created,
                Id = x.Id,
                IpAddress = x.IpAddress,
                IsLoggedIn = x.IsLoggedIn,
                IsRevoked = x.IsRevoked,
                RefreshToken = x.RefreshToken,
                RefreshTokenExpiration = x.RefreshTokenExpiration,
                Updated = x.Updated,
                User = x.User,
                UserDevice = x.UserDevice,
                UserId = x.UserId
            };
        }
    }

    #endregion

    #region Public Methods

    public async Task<UserSessionData> CreateUserSession(long userId, string encryptedDevice,
        string encryptedIpAddress, string refreshToken, DateTime refreshTokenExpiration)
    {
        UserSessionData userSessionData = new UserSessionData()
        {
            Created = DateTime.UtcNow,
            IpAddress = encryptedIpAddress,
            IsLoggedIn = true,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration,
            UserDevice = encryptedDevice,
            UserId = userId
        };
        _context.UserSessions.Add(userSessionData);

        await _context.SaveChangesAsync();

        return userSessionData;
    }

    public async Task<UserSessionData> GetUserSession(string refreshToken,
        Expression<Func<UserSessionData, UserSessionData>> mapper = null)
    {
        return await _context.UserSessions
            .Where(x => x.RefreshToken == refreshToken)
            .Select(mapper ?? DefaultUserSessionDataMapper)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserSessionData>> GetActiveUserSessions(long userId,
        Expression<Func<UserSessionData, UserSessionData>> mapper = null)
    {
        return await _context.UserSessions
            .Where(x => x.UserId == userId && x.IsLoggedIn && x.RefreshTokenExpiration > DateTime.UtcNow)
            .Select(mapper ?? DefaultUserSessionDataMapper)
            .ToListAsync();
    }

    public async Task<bool> UpdateUserSession(long sessionId, string refreshToken, DateTime refreshTokenExpiration)
    {
        return await _context.UserSessions
            .Where(x => x.Id == sessionId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.RefreshToken, refreshToken)
                                      .SetProperty(y => y.RefreshTokenExpiration, refreshTokenExpiration)
                                      .SetProperty(y => y.Updated, DateTime.UtcNow)) > 0;
    }

    public async Task<bool> EndUserSession(long sessionId, long userId)
    {
        return await _context.UserSessions
            .Where(x => x.Id == sessionId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.RefreshToken, String.Empty)
                                      .SetProperty(y => y.RefreshTokenExpiration, DateTime.MinValue)
                                      .SetProperty(y => y.IsLoggedIn, false)
                                      .SetProperty(y => y.Updated, DateTime.UtcNow)) > 0;
    }

    #endregion
}
