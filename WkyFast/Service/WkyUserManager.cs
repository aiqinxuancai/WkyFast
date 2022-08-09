using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WkyFast.Utils;

namespace WkyFast.Service
{
    /// <summary>
    /// 账户的配置
    /// </summary>
    public class WkyUserManager
    {
        private static WkyUserManager instance = new WkyUserManager();

        public static WkyUserManager Instance
        {
            get
            {
                return instance;
            }
        }

        private const string kConfigPath = @".\UserConfig.json";

        private const string kAesKey = @"T7v5Te7hbq5kukbB";

        private const string kAesIv = @"vBKdjmGzHRgvghByH7zzXdHhNx44QoNh";


        /// <summary>
        /// 勾选保存密码后并且登录成功保存
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="password"></param>
        public void SavePassword(string mail, string password, bool autoLogin)
        {
            try
            {
                JObject root = new JObject();
                root["mail"] = mail;
                root["password"] = AESHelper.AesEncode(password, kAesKey, kAesIv); //EncryptionHelper.AesEncryptionBase64(password, kAesKey); //加密
                root["autoLogin"] = autoLogin;
                File.WriteAllText(kConfigPath, root.ToString());
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// 登录UI中使用，获取是否有保存的密码
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="password"></param>
        public void LoadPasswrod(out string mail, out string password, out bool autoLogin)
        {
            try
            {
                if (File.Exists(kConfigPath))
                {
                    JObject root = JObject.Parse(File.ReadAllText(kConfigPath));
                    mail = root["mail"].ToString();
                    //password = EncryptionHelper.AesDecryptBase64(root["password"].ToString(), kAesKey);
                    password = AESHelper.AesDecode(root["password"].ToString(), kAesKey, kAesIv);

                    if (root.ContainsKey("autoLogin"))
                    {
                        autoLogin = root["autoLogin"].ToObject<bool>();
                    }
                    else
                    {
                        autoLogin = false;
                    }


                    return;
                }

            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
            }
            mail = "";
            password = "";
            autoLogin = false;
            return;
        }

        public void Clear()
        {
            try
            {
                if (File.Exists(kConfigPath))
                {
                    File.Delete(kConfigPath);
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
