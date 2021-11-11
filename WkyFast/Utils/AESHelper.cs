using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WkyFast.Utils
{
    public class AESHelper
    {
        IBufferedCipher aesCipher = null;
        ICipherParameters cipherParameters;
        UTF8Encoding Byte_Transform = new UTF8Encoding();


        public AESHelper()
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        }

        //	private static int AES_KEY_SIZE = 128 ;
        //  private static int IV_SIZE = 16;

        /// <summary>
        /// 加密
        /// </summary>
        /// <returns></returns>
        public static string AesEncode(string text, string key, string iv)
        {
            AESHelper helper = new AESHelper();
            helper.InitAESCodec(key, iv);
            return helper.AesEncode(text);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string AesDecode(string base64, string key, string iv)
        {
            AESHelper helper = new AESHelper();
            helper.InitAESCodec(key, iv);
            return helper.AesDecode(base64);
        }


        public void InitAESCodec(string messageKey, string ivText)
        {
            //byte[] messageKeyArray = System.Convert.FromBase64String(messageKey);
            //byte[] ivTextArray = System.Convert.FromBase64String(ivText);

            byte[] messageKeyArray = Encoding.UTF8.GetBytes(messageKey);
            byte[] ivTextArray = Encoding.UTF8.GetBytes(ivText);

            aesCipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter("AES", messageKeyArray);
            cipherParameters = new ParametersWithIV(keyParameter, ivTextArray, 0, 16);

        }

        public string AesEncode(string plainText)
        {
            byte[] plainBytes = Byte_Transform.GetBytes(plainText);
            byte[] outputBytes = new byte[aesCipher.GetOutputSize(plainBytes.Length)];
            aesCipher.Reset();
            aesCipher.Init(true, cipherParameters);
            int length = aesCipher.ProcessBytes(plainBytes, outputBytes, 0);
            aesCipher.DoFinal(outputBytes, length); //Do the final block
            return Convert.ToBase64String(outputBytes);
        }

        public string AesDecode(string cipherText)
        {
            byte[] encryptBytes = System.Convert.FromBase64String(cipherText);
            byte[] comparisonBytes = new byte[aesCipher.GetOutputSize(encryptBytes.Length)];

            aesCipher.Reset();
            aesCipher.Init(false, cipherParameters);
            int length = aesCipher.ProcessBytes(encryptBytes, comparisonBytes, 0);
            aesCipher.DoFinal(comparisonBytes, length); //Do the final block

            return Encoding.UTF8.GetString(comparisonBytes);
        }

    }
}