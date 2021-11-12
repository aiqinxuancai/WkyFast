using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WkyApiSharp.Service;
using WkyFast.Utils;

namespace WkyFast.Service
{
    public class WkyApiManager
    {
        public static WkyApi WkyApi { set; get; } 

        public static ObservableCollection<WkyApiSharp.Service.Model.ListPeer.ResultClass> PeerList { set; get; } = new();

        public static WkyApiSharp.Service.Model.ListPeer.Device NowDevice { set; get; }

        public static ObservableCollection<WkyApiSharp.Service.Model.ListPeer.Device> DeviceList { set; get; } = new();

        /// <summary>
        /// 玩客云登录成功后的信息处理
        /// </summary>
        public static async Task<int> UpdateDevice()
        {
            var wkyApi = WkyApi;
            //获取设备信息
            var listPeerResult = await wkyApi.ListPeer();
            PeerList.Clear();
            foreach (var item in listPeerResult.Result)
            {
                if (item.ResultClass != null)
                {
                    PeerList.Add(item.ResultClass);
                }
            }

            DeviceList.Clear();
            foreach (var peer in PeerList)
            {
                foreach (var device in peer.Devices)
                {
                    DeviceList.Add(device);
                }
            }
            return DeviceList.Count;
        }

        /// <summary>
        /// 选中设备，优先从上次选择中选中
        /// </summary>
        public static WkyApiSharp.Service.Model.ListPeer.Device? SelectDevice()
        {
            if (PeerList != null && PeerList.Count > 0)
            {
                WkyApiSharp.Service.Model.ListPeer.Device? selectDevice = null;

                foreach (var peer in PeerList)
                {
                    foreach (var device in peer.Devices)
                    {
                        if (!string.IsNullOrWhiteSpace(AppConfig.ConfigData.LastDeviceId))
                        {
                            if (device.DeviceId == AppConfig.ConfigData.LastDeviceId)
                            {
                                selectDevice = device;
                            }
                        }
                    }
                }


                if (selectDevice != null)
                {
                    return selectDevice;
                }

                //到这里说明没有配置AppConfig.ConfigData.LastDeviceId 或者 peer.Devices为空
                return PeerList?.First()?.Devices?.First();
            }
            return null;
        }

        

    }
}
