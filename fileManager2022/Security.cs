using System.Security.Cryptography;
using System.IO;
using System;

namespace fileManager2022
{
    internal static class Security
    {
        private static byte[] key = new byte[32] {218, 116, 155, 51, 85, 120, 106, 132, 179, 242, 215, 146, 237, 52, 111, 81,
                                                  174, 162, 110, 51, 159, 249, 243, 218, 122, 88, 33, 99, 204, 114, 189, 187};
        private static byte[] IV = new byte[16] { 81, 208, 175, 135, 221, 81, 194, 38, 73, 163, 210, 194, 223, 220, 156, 132 };
        internal static string Encrypt(string text)
        {
            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = IV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }
        internal static string Decrypt(string cipher)
        {
            string text;
            byte[] cipherText = Convert.FromBase64String(cipher);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            text = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return text;
        }
    }
}
