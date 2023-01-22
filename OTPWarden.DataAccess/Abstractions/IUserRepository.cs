using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.DataAccess.Abstractions;

public interface IUserRepository
{
    Expression<Func<UserData, UserData>> DefaultUserDataMapper { get; }

    Task<UserData> CreateUser(string email, string passwordHash);

    Task<UserData> GetUser(long id, Expression<Func<UserData, UserData>> mapper = null);

    Task<UserData> GetUser(string email, Expression<Func<UserData, UserData>> mapper = null);
}
