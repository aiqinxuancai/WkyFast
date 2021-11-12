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

        public string LastDeviceId { get; set; } = string.Empty;

    }

    /// <summary>
    /// 配置项读取、写入、存储逻辑
    /// </summary>
    public class AppConfig
    {
        public static AppConfigData ConfigData { set; get; } = new AppConfigData();

        private static string _configPath = Directory.GetCurrentDirectory() + @"\Config.json";

        private static object _lock = new object();

        static AppConfig()
        {
            Init();
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
