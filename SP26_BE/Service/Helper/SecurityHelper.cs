using System.Security.Cryptography;
using System.Text;

namespace Service.Helpers
{
    public static class SecurityHelper
    {
        public static string Encrypt(string plainText, string keyString)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(keyString));
            byte[] iv = new byte[16]; 

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        
                    }

                   
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, string keyString)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(keyString));
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Nếu giải mã lỗi (sai key hoặc sai format), trả về chuỗi gốc hoặc báo lỗi
                return "Error: Cannot Decrypt";
            }
        }
    }
}