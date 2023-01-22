using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.DataAccess.Abstractions;

public interface IUserSessionRepository
{
    Expression<Func<UserSessionData, UserSessionData>> DefaultUserSessionDataMapper { get; }

    Task<UserSessionData> CreateUserSession(long userId, string encryptedDevice, string encryptedIpAddress,
        string refreshToken, DateTime refreshTokenExpiration);

    Task<UserSessionData> GetUserSession(string refreshToken,
        Expression<Func<UserSessionData, UserSessionData>> mapper = null);

    Task<IEnumerable<UserSessionData>> GetActiveUserSessions(long userId,
        Expression<Func<UserSessionData, UserSessionData>> mapper = null);

    Task<bool> UpdateUserSession(long sessionId, string refreshToken, DateTime refreshTokenExpiration);

    Task<bool> EndUserSession(long sessionId, long userId);
}
