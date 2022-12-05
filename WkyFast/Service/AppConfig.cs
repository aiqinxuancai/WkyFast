using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Diagnostics;
using PropertyChanged;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WkyFast.Service
{
    [AddINotifyPropertyChangedInterface]
    public class AppConfigData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChangedEventArgs propertyChangedEventArgs = new PropertyChangedEventArgs(PropertyName);
            PropertyChanged(this, propertyChangedEventArgs);
            AppConfig.Save();
        }

        /// <summary>
        /// 最后使用的设备ID
        /// </summary>
        public string LastDeviceId { get; set; } = string.Empty;

        ///// <summary>
        ///// 最后增加订阅的路径
        ///// </summary>
        //public string LastAddSubscriptionPath { get; set; } = string.Empty;


        /// <summary>
        /// 设备默认选择下载的分区对应表 
        /// 【设备ID->分区路径】
        /// </summary>
        public Dictionary<string, string> AddTaskSavePartitionDict { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 分区路径->存储路径
        /// 【分区路径->存储路径】
        /// </summary>
        public Dictionary<string, string> AddTaskSavePathDict { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 设备默认选择订阅的分区对应表 
        /// 【设备ID->分区路径】
        /// </summary>
        public Dictionary<string, string> AddSubscriptionSavePartitionDict { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 分区路径->存储路径
        /// 【分区路径->存储路径】
        /// </summary>
        public Dictionary<string, string> AddSubscriptionSavePathDict { get; set; } = new Dictionary<string, string>();

        //OSS相关设置

        public bool OSSSynchronizeOpen { get; set; } = false;


        public string OSSEndpoint { get; set; } = string.Empty;

        public string OSSBucket { get; set; } = string.Empty;

        public string OSSAccessKeyId { get; set; } = string.Empty;

        public string OSSAccessKeySecret { get; set; } = string.Empty;


        public bool PushDeerOpen { get; set; } = false;

        public string PushDeerKey { get; set; } = string.Empty;

        //获取
    }

    /// <summary>
    /// 配置项读取、写入、存储逻辑
    /// </summary>
    public class AppConfig
    {
        public static AppConfigData ConfigData { set; get; } = new AppConfigData();

        private static string _configPath = AppContext.BaseDirectory + @"Config.json";

        private static object _lock = new object();

        static AppConfig()
        {
            Init();
            Debug.WriteLine(_configPath);
        }

        public static void InitDefault() //载入默认配置
        {
            ConfigData.LastDeviceId = string.Empty;
        }


        public static bool Init()
        {
            try
            {
                if (File.Exists(_configPath) == false)
                {
                    InitDefault();
                    Save();
                    return false;
                }
                ConfigData = JsonConvert.DeserializeObject<AppConfigData>(System.IO.File.ReadAllText(_configPath));
                return true;
            }
            catch (System.Exception ex)
            {
                InitDefault();
                Save();
                Debug.WriteLine(ex);
                return false;
            }
        }


        public static void Save()
        {
            Debug.WriteLine("存储配置");
            try
            {
                lock(_lock)
                {
                    //Debug.WriteLine($"存储配置目录{_configPath}");
                    System.IO.File.WriteAllText(_configPath, JsonConvert.SerializeObject(ConfigData));
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


    }
}
