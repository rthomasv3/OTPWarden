namespace OTPWarden.Services.Abstractions;

public interface ICryptographyService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashString);
    string Encrypt(string plainText, string key);
    string Decrypt(string cipherText, string key);
}
