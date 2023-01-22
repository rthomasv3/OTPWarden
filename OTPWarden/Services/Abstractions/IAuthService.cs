using System.Threading.Tasks;
using OTPWarden.Models;

namespace OTPWarden.Services.Abstractions;

public interface IAuthService
{
    Task<AuthenticateUserResult> Authenticate(string grantType, string userDevice, string userIpAddress,
        string email, string password, string refreshToken);
    Task<AuthenticateUserResult> Register(string email, string password, string userDevice, string userIpAddress);
    Task<bool> Logout(long userId, long sessionId);
}
