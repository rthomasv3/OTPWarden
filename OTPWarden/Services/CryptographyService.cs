using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using OTPWarden.Services.Abstractions;

namespace OTPWarden.Services;

public sealed class CryptographyService : ICryptographyService
{
    #region Fields

    private static readonly int _aesSaltSize = 16;
    private static readonly int _aesIVSize = 16;
    private static readonly int _aesKeySize = 32;
    private static readonly int _rfc2898Iterations = 1024;

    #endregion

    #region Public Methods

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);

        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashString)
    {
        byte[] hashBytes = Convert.FromBase64String(hashString);

        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);

        return hash.SequenceEqual(hashBytes.Skip(16));
    }

    public string Encrypt(string plainText, string key)
    {
        string encryptedValue = String.Empty;

        if (plainText != null)
        {
            // create a salt
            byte[] saltBytes = RandomNumberGenerator.GetBytes(_aesSaltSize);
            // create an initialization vector
            byte[] ivBytes = RandomNumberGenerator.GetBytes(_aesIVSize);

            using (Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(key, saltBytes, _rfc2898Iterations))
            {
                byte[] keyBytes = password.GetBytes(_aesKeySize);

                using (Aes symmetricAlgorithm = Aes.Create())
                {
                    symmetricAlgorithm.Mode = CipherMode.CBC;
                    symmetricAlgorithm.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = symmetricAlgorithm.CreateEncryptor(keyBytes, ivBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                                {
                                    streamWriter.Write(plainText);
                                }
                            }

                            // create the final byte array as a concatenation of the salt bytes, the iv bytes, and the cipher bytes
                            byte[] cipherTextBytes = saltBytes.Concat(ivBytes).Concat(memoryStream.ToArray()).ToArray();

                            encryptedValue = Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }

        return encryptedValue;
    }

    public string Decrypt(string cipherText, string key)
    {
        string decryptedValue = String.Empty;

        if (cipherText != null)
        {
            // separate salt, iv, and encrypted message
            byte[] cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            byte[] saltBytes = cipherTextBytesWithSaltAndIv.Take(_aesSaltSize).ToArray();
            byte[] ivBytes = cipherTextBytesWithSaltAndIv.Skip(_aesSaltSize).Take(_aesIVSize).ToArray();
            byte[] cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip(_aesSaltSize + _aesIVSize).ToArray();

            using (Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(key, saltBytes, _rfc2898Iterations))
            {
                byte[] keyBytes = password.GetBytes(_aesKeySize);

                using (Aes symmetricAlgorithm = Aes.Create())
                {
                    symmetricAlgorithm.Mode = CipherMode.CBC;
                    symmetricAlgorithm.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = symmetricAlgorithm.CreateDecryptor(keyBytes, ivBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader sr = new StreamReader(cryptoStream))
                                {
                                    decryptedValue = sr.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
        }

        return decryptedValue;
    }

    #endregion
}
