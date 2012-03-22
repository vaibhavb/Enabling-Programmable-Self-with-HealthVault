// Copyright (c) Microsoft Corp.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
using Microsoft.Phone.Info;
#endif

namespace Microsoft.Health.Mobile
{
    /// <summary>
    /// Contains the platform-specific methods used by
    /// the <see cref="HealthVaultService"/> class.
    /// </summary>
    public class MobilePlatform
    {
        /// <summary>
        /// Gets a name and version for this platform/library combination.
        /// </summary>
        /// <remarks>
        /// Must be between 7 and 19 characters in length.
        /// </remarks>
        public static string PlatformAbbreviationAndVersion
        {
            get { return "WP7 V1.0"; }
        }

        /// <summary>
        /// Gets a name for the device.
        /// </summary>
        public static string DeviceName
        {
            get
            {
                return "Windows Phone";
            }
        }

        /// <summary>
        /// Computes a SHA 256 hash.
        /// </summary>
        /// <param name="xmlData">The data to hash.</param>
        /// <returns>The hash as a base64-encoded string.</returns>
        public static string ComputeSha256Hash(string xmlData)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(xmlData);

            byte[] results = null;
            using (SHA256 shaManaged = new SHA256Managed())
            {
                results = shaManaged.ComputeHash(bytes, 0, bytes.Length);
            }

            string returnValue = Convert.ToBase64String(results);

            return returnValue;
        }

        /// <summary>
        /// Computes a SHA 256 hash and wraps the result in XML.
        /// </summary>
        /// <param name="xmlData">The data to hash.</param>
        /// <returns>The wrapped hash.</returns>
        public static XElement ComputeSha256HashAndWrap(string xmlData)
        {
            return new XElement("hash-data",
                        new XAttribute("algName", "SHA256"),
                        ComputeSha256Hash(xmlData)
                    );
        }

        /// <summary>
        /// Computes a SHA 256 HMAC.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="xmlData">The input data.</param>
        /// <returns>A base-64 encoded HMAC.</returns>
        public static string ComputeSha256Hmac(byte[] key, string xmlData)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(xmlData);

            byte[] results = null;
            using (HMACSHA256 hmacSha256 = new HMACSHA256(key))
            {
                results = hmacSha256.ComputeHash(bytes);
            }

            string returnValue = Convert.ToBase64String(results);

            return returnValue;
        }

        /// <summary>
        /// Computes a SHA 256 HMAC and wraps the result in XML.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="xmlData">The input data.</param>
        /// <returns>The wrapped result.</returns>
        public static XElement ComputeSha256HmacAndWrap(byte[] key, string xmlData)
        {
            return new XElement("hmac-data",
                        new XAttribute("algName", "HMACSHA256"),
                        ComputeSha256Hmac(key, xmlData)
                    );
        }

        /// <summary>
        /// Saves a string to a filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="contents">The string to save.</param>
        public static void SaveTextToFile(string filename, string contents)
        {
#if WINDOWS_PHONE
            IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication();
            storageFile.CreateDirectory("HealthVault");
            IsolatedStorageFileStream stream = storageFile.CreateFile(@"HealthVault\" + filename);
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(contents);
            }
#else
            string fullPath = Path.GetTempPath() + "\\" + filename;

            using (StreamWriter writer = File.CreateText(fullPath))
            {
                writer.Write(contents);
            }
#endif
        }

        /// <summary>
        /// Loads the contents of a file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The contents of the file. </returns>
        public static string ReadTextFromFile(string filename)
        {
#if WINDOWS_PHONE
            IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream stream = null;

            try
            {
                stream = storageFile.OpenFile(@"HealthVault\" + filename, FileMode.Open);
            }
            catch (IsolatedStorageException)
            {
                return null;
            }
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
#else
            string fullPath = Path.GetTempPath() + "\\" + filename;

            try
            {
                using (StreamReader reader = File.OpenText(fullPath))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
#endif
        }
    }
}
