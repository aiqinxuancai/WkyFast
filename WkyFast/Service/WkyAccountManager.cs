using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WkyApiSharp.Service;
using WkyFast.Utils;

namespace WkyFast.Service
{
    public class WkyAccountManager
    {
        public static WkyApi WkyApi { set; get; }

        private const string kConfigPath = @".\UserConfig.json";

        private const string kAesKey = @"T7v5Te7hbq5kukbB";

        private const string kAesIv = @"vBKdjmGzHRgvghByH7zzXdHhNx44QoNh";

        /// <summary>
        /// 勾选保存密码后并且登录成功保存
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="password"></param>
        public static void SavePassword(string mail, string password, bool autoLogin)
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
        public static void LoadPasswrod(out string mail, out string password, out bool autoLogin)
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


        /// <summary>
        /// 玩客云登录成功后的信息处理
        /// </summary>
        public static void GetWkyAccountData()
        {
            var wkyApi = WkyApi;
            //获取设备信息
            var listPeerResult = wkyApi.ListPeer().Result;
            foreach (var item in listPeerResult.Result)
            {
                if (item.ResultClass != null)
                {
                    foreach (var device in item.ResultClass.Devices)
                    {
                        //获取设备的USB信息（存储设备信息）
                        var getUsbInfoResult = wkyApi.GetUsbInfo(device.DeviceId).Result;

                        Console.Write(getUsbInfoResult.ToString());

                        //登录远程下载设备
                        var remoteDownloadLoginResult = wkyApi.RemoteDownloadLogin(device.Peerid).Result;

                        if (remoteDownloadLoginResult != null && remoteDownloadLoginResult.Rtn == 0)
                        {
                            //读取设备当前的下载任务列表
                            var test = wkyApi.RemoteDownloadList(device.Peerid).Result;

                            //获取URL信息（不管是磁力还是http等下载链接均需要）
                            var urlResult = wkyApi.UrlResolve(device.Peerid, "magnet:?xt=urn:btih:1fbd4ead642a5da026d8819b6ada3ad257d0964b&tr=http%3a%2f%2ft.nyaatracker.com%2fannounce&tr=http%3a%2f%2ftracker.kamigami.org%3a2710%2fannounce&tr=http%3a%2f%2fshare.camoe.cn%3a8080%2fannounce&tr=http%3a%2f%2fopentracker.acgnx.se%2fannounce&tr=http%3a%2f%2fanidex.moe%3a6969%2fannounce&tr=http%3a%2f%2ft.acg.rip%3a6699%2fannounce&tr=https%3a%2f%2ftr.bangumi.moe%3a9696%2fannounce&tr=udp%3a%2f%2ftr.bangumi.moe%3a6969%2fannounce&tr=http%3a%2f%2fopen.acgtracker.com%3a1096%2fannounce&tr=udp%3a%2f%2ftracker.opentrackr.org%3a1337%2fannounce").Result;

                            //上传本地BT文件获取信息
                            //var btResult = wkyApi.BtCheck(device.Peerid, @"C:\Users\aiqin\OneDrive\种子\【酢浆草字幕组】[咲-Saki-][天才麻将少女][01-25][GB][848x480][MKV][全].torrent").Result;

                            //获取本地USB的路径生成下载目录
                            var savePath = "";
                            foreach (var disk in getUsbInfoResult.Result)
                            {
                                if (disk.ResultClass != null)
                                {
                                    foreach (var partition in disk.ResultClass.Partitions)
                                    {
                                        savePath = partition.Path + "/onecloud/tddownload";
                                    }
                                }

                            }

                            //创建任务
                            var createResult = wkyApi.CreateTaskWithUrlResolve(device.Peerid, savePath, urlResult).Result;
                            if (createResult.Rtn == 0)
                            {
                                //添加任务成功
                            }

                        }
                    }
                }
            }
        }

    }
}
