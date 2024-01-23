using System.Security.Cryptography;
using System.Text;

namespace IpDnsSync.Helpers;

internal static class JwtSignature
{
    internal static string Sign(string data, string privateKey)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKey);
        var dataBytes = Encoding.ASCII.GetBytes(data);
        var signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signatureBytes);
    }
}