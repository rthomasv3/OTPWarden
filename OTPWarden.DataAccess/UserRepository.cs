using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTPWarden.DataAccess.Abstractions;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.DataAccess;

public sealed class UserRepository : IUserRepository
{
    #region Fields

    private readonly OTPDbContext _context;

    #endregion

    #region Constructor

    public UserRepository(OTPDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion

    #region Properties

    public Expression<Func<UserData, UserData>> DefaultUserDataMapper
    {
        get
        {
            return x => new UserData()
            {
                Created = x.Created,
                Email = x.Email,
                Id = x.Id,
                LastLogin = x.LastLogin,
                Password = x.Password,
                Sessions = x.Sessions,
                Username = x.Username,
                VaultEntries = x.VaultEntries
            };
        }
    }

    #endregion

    #region Public Methods

    public async Task<UserData> CreateUser(string email, string passwordHash)
    {
        UserData userData = null;

        bool userExists = await _context.Users
            .AnyAsync(x => x.Email.ToLower() == email.ToLower());

        if (!userExists)
        {
            userData = new UserData()
            {
                Created = DateTime.UtcNow,
                Email = email,
                Password = passwordHash
            };
            _context.Users.Add(userData);
            await _context.SaveChangesAsync();
        }

        return userData;
    }

    public async Task<UserData> GetUser(long id, Expression<Func<UserData, UserData>> mapper = null)
    {
        return await _context.Users
            .Where(x => x.Id == id)
            .Select(mapper ?? DefaultUserDataMapper)
            .FirstOrDefaultAsync();
    }

    public async Task<UserData> GetUser(string email, Expression<Func<UserData, UserData>> mapper = null)
    {
        return await _context.Users
            .Where(x => x.Email.ToLower() == email.ToLower())
            .Select(mapper ?? DefaultUserDataMapper)
            .FirstOrDefaultAsync();
    }

    #endregion
}
