// (c) Microsoft. All rights reserved
using HealthVault.Foundation.Types;

namespace HealthVault.Foundation
{
    public interface ICryptographer
    {
        string HashAlgorithm { get; }
        string HmacAlgorithm { get; }
        string EncryptAlgorithm { get; }

        Hmac Hmac(string keyMaterial, string text);
        Hash Hash(string text);
        Encrypted Encrypt(string keyMaterial, string text);
        string Decrypt(string keyMaterial, Encrypted encrypted);
    }
}