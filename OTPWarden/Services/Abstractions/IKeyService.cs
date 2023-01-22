namespace OTPWarden.Services.Abstractions;

public interface IKeyService
{
    void AddUserKey(long userId, string password);
    string GetUserKey(long userId);
    void RemoveUserKey(long userId);
}
