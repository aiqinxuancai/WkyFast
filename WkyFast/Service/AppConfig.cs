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
using System.Threading;

namespace WkyFast.Service
{
    [AddINotifyPropertyChangedInterface]
    public class AppConfigData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //public void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        //{
        //    PropertyChangedEventArgs propertyChangedEventArgs = new PropertyChangedEventArgs(PropertyName);
        //    PropertyChanged(this, propertyChangedEventArgs);
        //    AppConfig.Instance.Save();
        //}

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


        public bool SubscriptionProxyOpen { get; set; } = false;

        public string SubscriptionProxy { get; set; } = string.Empty;

        /// <summary>
        /// OpenAIKey 用于RSS为集中订阅时，使用OpenAI提取连接中的作品名称
        /// </summary>
        public string OpenAIKey { get; set; } = string.Empty;

        public string OpenAIProxy { get; set; } = string.Empty;

        public bool OpenAIOpen { get; set; } = false;


        /// <summary>
        /// 用于第三方转发服务的实现
        /// </summary>
        public string OpenAIHost { get; set; } = string.Empty;


    }

    /// <summary>
    /// 配置项读取、写入、存储逻辑
    /// </summary>
    public class AppConfig
    {
        private static readonly Lazy<AppConfig> lazy = new Lazy<AppConfig>(() => new AppConfig());
        
        public static AppConfig Instance => lazy.Value;

        public AppConfigData ConfigData { set; get; } = new AppConfigData();

        private string _configPath = AppContext.BaseDirectory + @"Config.json";

        private object _lock = new object();

        private AppConfig()
        {
            Init();
            Debug.WriteLine(_configPath);
        }

        public void InitDefault() //载入默认配置
        {
            ConfigData.LastDeviceId = string.Empty;
        }


        public bool Init()
        {
            try
            {

                Debug.WriteLine($"初始化配置" + Thread.CurrentThread.ManagedThreadId);
                if (File.Exists(_configPath) == false)
                {
                    Debug.WriteLine($"默认初始化");
                    InitDefault();
                    Save();
                    return false;
                }
                lock (_lock)
                {
                    var fileContent = File.ReadAllText(_configPath);
                    var appData = JsonConvert.DeserializeObject<AppConfigData>(fileContent);
                    ConfigData = appData;
                    ConfigData.PropertyChanged += AppConfigData_PropertyChanged;
                }

                return true;
            }
            catch (Exception ex)
            {
                InitDefault();
                Save();
                Debug.WriteLine(ex);
                return false;
            }
        }
        private void AppConfigData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        public void Save()
        {

            try
            {
                lock (_lock)
                {
                    var data = JsonConvert.SerializeObject(ConfigData);
                    Debug.WriteLine($"存储配置{Thread.CurrentThread.ManagedThreadId} {data}");
                    File.WriteAllText(_configPath, data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


    }
}
