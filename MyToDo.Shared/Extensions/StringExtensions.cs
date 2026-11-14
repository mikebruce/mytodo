using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MyToDo.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string GetMD5(this string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        private static readonly byte[] SM4key = Encoding.UTF8.GetBytes("Sm4Key@2026#Abcd");

        /// <summary>
        ///SM4 ECB 加密，返回 Base64 密文
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">16 字节密钥</param>
        public static string Encrypt(this string plainText)
        {
            byte[] input = Encoding.UTF8.GetBytes(plainText);

            var cipher = new PaddedBufferedBlockCipher(new SM4Engine(), new Pkcs7Padding());

            cipher.Init(true, new KeyParameter(SM4key));

            byte[] output = new byte[cipher.GetOutputSize(input.Length)];
            int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            len += cipher.DoFinal(output, len);

            Array.Resize(ref output, len);
            return Convert.ToBase64String(output);
        }

        /// <summary>
        /// SM4ECB 解密，传入 Base64 密文，返回明文
        /// </summary>
        public static string Decrypt(this string cipherText)
        {
            byte[] input = Convert.FromBase64String(cipherText);

            var cipher = new PaddedBufferedBlockCipher(new SM4Engine(), new Pkcs7Padding());
            cipher.Init(false, new KeyParameter(SM4key)); // false = 解密

            byte[] output = new byte[cipher.GetOutputSize(input.Length)];
            int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            len += cipher.DoFinal(output, len);

            Array.Resize(ref output, len);
            return Encoding.UTF8.GetString(output);
        }
    }
}