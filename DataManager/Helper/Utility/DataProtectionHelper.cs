using System.Security.Cryptography;
using System.Text;

namespace DataManager.Helper.Utility;
public class DataProtectionHelper
{
    private static readonly DataProtectionScope _scope = DataProtectionScope.CurrentUser;
    private static readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();
    private readonly byte[] _salt = new byte[32];

    public DataProtectionHelper()
    {
        _randomNumberGenerator.GetBytes(_salt);
    }

    public static string Encrypt(string data, out string salt)
    {
        var userData = Encoding.UTF8.GetBytes(data);
        var saltArray = new byte[32];
        _randomNumberGenerator.GetBytes(saltArray);

        byte[] encryptedData = ProtectedData.Protect(
            userData: userData,
            optionalEntropy: saltArray,
            scope: _scope);

        salt = Convert.ToBase64String(saltArray);
        return Convert.ToBase64String(encryptedData);
    }

    public static string Decrypt(string data, string salt)
    {
        byte[] saltArray = Convert.FromBase64String(salt);
        byte[] encryptedData = Convert.FromBase64String(data);

        byte[] decryptedData = ProtectedData.Unprotect(
            encryptedData: encryptedData,
            optionalEntropy: saltArray,
            scope: _scope);

        return Encoding.UTF8.GetString(decryptedData);
    }
}