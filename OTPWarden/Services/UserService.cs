using System;
using System.Threading.Tasks;
using OTPWarden.DataAccess.Abstractions;
using OTPWarden.DataAccess.Data;
using OTPWarden.Services.Abstractions;

namespace OTPWarden.Services;

public sealed class UserService : IUserService
{
    #region Fields

    private readonly IUserRepository _userRepository;
    private readonly ICryptographyService _cryptographyService;

    #endregion

    #region Constructor

    public UserService(IUserRepository userRepository, ICryptographyService cryptographyService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _cryptographyService = cryptographyService ?? throw new ArgumentNullException(nameof(cryptographyService));
    }

    #endregion

    #region Public Methods

    public async Task<UserData> GetUser(long id)
    {
        return await _userRepository.GetUser(id);
    }

    public async Task<UserData> GetUser(string email)
    {
        return await _userRepository.GetUser(email);
    }

    public async Task<UserData> CreateUser(string email, string password)
    {
        string passwordHash = _cryptographyService.HashPassword(password);
        return await _userRepository.CreateUser(email.Trim(), passwordHash);
    }

    #endregion
}
