using System.Threading.Tasks;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.Services.Abstractions;

public interface IUserService
{
    Task<UserData> GetUser(long id);
    Task<UserData> GetUser(string email);
    Task<UserData> CreateUser(string email, string password);
}
