using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace POSPatronImage.Support
{
    public static class EncryptionHelper
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public static string EncryptString(string plainText, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    throw new ArgumentNullException(nameof(plainText));
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException(nameof(key));

                byte[] keyBytes = Convert.FromBase64String(key);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.GenerateIV();
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream())
                    {
                        try
                        {
                            ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV to the ciphertext
                            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                            using (var sw = new StreamWriter(cs))
                            {
                                sw.Write(plainText);
                            }
                            var encryptedData = ms.ToArray();
                            logger.Info("Data encrypted successfully.");
                            return Convert.ToBase64String(encryptedData);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error while encrypting data.");
                            throw new Exception("Error while encrypting data", ex);
                        }
                    }
                }
            }
            catch (FormatException ex)
            {
                logger.Error(ex, "Invalid key format.");
                throw new Exception("Invalid key format", ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred during encryption.");
                throw new Exception("An error occurred during encryption", ex);
            }
        }

        public static string DecryptString(string cipherText, string key)
        {
            logger.Info("Decryption started.");
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                    throw new ArgumentNullException(nameof(cipherText));
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException(nameof(key));

                byte[] keyBytes = Convert.FromBase64String(key);
                byte[] fullCipher = Convert.FromBase64String(cipherText);

                if (fullCipher.Length < 16)
                {
                    logger.Error("The cipher text is invalid or too short.");
                    throw new ArgumentException("The cipher text is invalid or too short.");
                }

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.Padding = PaddingMode.PKCS7;

                    var iv = new byte[aes.BlockSize / 8];
                    var cipher = new byte[fullCipher.Length - iv.Length];

                    Array.Copy(fullCipher, iv, iv.Length); // Extract IV from the beginning
                    Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length); // Extract the actual ciphertext

                    aes.IV = iv;

                    try
                    {
                        using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                        using (var ms = new MemoryStream(cipher))
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        using (var sr = new StreamReader(cs))
                        {
                            try
                            {
                                var decryptedText = sr.ReadToEnd();
                                logger.Info("Data decrypted successfully.");
                                return decryptedText;

                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "Error while reading decrypted data.");
                                throw new Exception("Error while reading decrypted data", ex);
                            }
                        }
                    }
                    catch (CryptographicException ex)
                    {
                        logger.Error(ex, "Invalid cipher text.");
                        throw new Exception("Invalid cipher text", ex);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "An error occurred during decryption.");
                        throw new Exception("An error occurred during decryption", ex);
                    }
                }
            }
            catch (FormatException ex)
            {
                logger.Error(ex, "Invalid key or cipher text format.");
                throw new Exception("Invalid key or cipher text format", ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred during decryption process.");
                throw new Exception("An error occurred during decryption process", ex);
            }


        }

    }
}
