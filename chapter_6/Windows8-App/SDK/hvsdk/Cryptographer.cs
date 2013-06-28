// (c) Microsoft. All rights reserved
using System;
using HealthVault.Foundation.Types;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace HealthVault.Foundation
{
    public class Cryptographer : ICryptographer
    {
        #region ICryptographer Members

        public string HashAlgorithm
        {
            get { return "SHA256"; }
        }

        public string HmacAlgorithm
        {
            get { return "HMACSHA256"; }
        }
        
        public string EncryptAlgorithm
        {
            get { return SymmetricAlgorithmNames.AesEcbPkcs7; }
        }

        public Hmac Hmac(string keyMaterial, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            CryptographicKey key = KeyFromMaterial(keyMaterial);
            IBuffer hmac = CryptographicEngine.Sign(key, StringToBinary(text));

            return new Hmac(HmacAlgorithm, Base64Encode(hmac));
        }

        public Hash Hash(string text)
        {
            IBuffer hash = CreateHashProvider().HashData(StringToBinary(text));
            return new Hash(HashAlgorithm, Base64Encode(hash));
        }

        /// <summary>
        /// Encrypts the text using the keyMaterial. This encryption only works with
        /// PKCS7 algorithms which handle the padding and also non CBC algorithms so 
        /// that it doesn't have to deal with creating an initialization vector
        /// </summary>
        public Encrypted Encrypt(string keyMaterial, string text)
        {
            CryptographicKey key = SymmetricKeyFromMaterial(keyMaterial);

            // Create a buffer that contains the encoded message to be encrypted. 
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);

            IBuffer encryptedBuffer = CryptographicEngine.Encrypt(key, buffMsg, null);
            string encryptedValue = CryptographicBuffer.EncodeToBase64String(encryptedBuffer);
            var encrypted = new Encrypted(EncryptAlgorithm, BinaryStringEncoding.Utf8, encryptedValue);

            return encrypted;
        }

        public string Decrypt(string keyMaterial, Encrypted encrypted)
        {
            CryptographicKey key = SymmetricKeyFromMaterial(keyMaterial);

            IBuffer encryptedBuffer = CryptographicBuffer.DecodeFromBase64String(encrypted.Value);
            IBuffer decryptedBuffer = CryptographicEngine.Decrypt(key, encryptedBuffer, null);

            return CryptographicBuffer.ConvertBinaryToString(encrypted.Encoding, decryptedBuffer);
        }

        #endregion

        private CryptographicKey SymmetricKeyFromMaterial(string keyMaterial)
        {
            IBuffer keyData = CryptographicBuffer.DecodeFromBase64String(keyMaterial);
            SymmetricKeyAlgorithmProvider provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(EncryptAlgorithm);
            CryptographicKey key = provider.CreateSymmetricKey(keyData);

            return key;
        }

        private CryptographicKey KeyFromMaterial(string keyMaterial)
        {
            if (string.IsNullOrEmpty(keyMaterial))
            {
                throw new ArgumentException("keyMaterial");
            }

            IBuffer keyData = CryptographicBuffer.DecodeFromBase64String(keyMaterial);
            return CreateMacProvider().CreateKey(keyData);
        }

        private HashAlgorithmProvider CreateHashProvider()
        {
            return HashAlgorithmProvider.OpenAlgorithm("SHA256");
        }

        private MacAlgorithmProvider CreateMacProvider()
        {
            return MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA256");
        }

        private string Base64Encode(IBuffer data)
        {
            return CryptographicBuffer.EncodeToBase64String(data);
        }

        private IBuffer StringToBinary(string text)
        {
            return CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
        }
    }
}