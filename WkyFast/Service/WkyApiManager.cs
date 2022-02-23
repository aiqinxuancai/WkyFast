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
using WkyApiSharp.Service.Model.GetUsbInfo;
using WkyFast.Utils;
using Flurl.Http;
using Newtonsoft.Json;
using WkyFast.Service.Model;

namespace WkyFast.Service
{
    public class WkyApiManager
    {
        private static int kMaxTaskListCount = 100;

        private static WkyApiManager instance = new WkyApiManager();

        public static WkyApiManager Instance
        {
            get
            {
                return instance;
            }
        }


        public WkyApi WkyApi { set; get; } 

        public ObservableCollection<WkyApiSharp.Service.Model.ListPeer.ResultClass> PeerList { set; get; } = new();

        public WkyApiSharp.Service.Model.ListPeer.Device NowDevice { set; get; }

        public ObservableCollection<WkyApiSharp.Service.Model.ListPeer.Device> DeviceList { set; get; } = new();

        public WkyApiGetUsbInfoResultModel NowUsbInfo { set; get; }

        public ObservableCollection<TaskModel> TaskList { set; get; } = new ObservableCollection<TaskModel>();


        public async Task UpdateTask()
        {
            var remoteDownloadListResult = await WkyApiManager.Instance.WkyApi.RemoteDownloadList(WkyApiManager.Instance.NowDevice.Peerid);

            if (remoteDownloadListResult.Rtn == 0)
            {
                var obList = remoteDownloadListResult.Tasks.ToList();
                MainWindow.Instance.Dispatcher.Invoke(() => {
                    //更顺滑的更新任务
                    //List<TaskModel> newTask = new List<TaskModel>();

                    //for (int i = 0; i < obList.Count; i++)
                    //{
                    //    //在当前列表中筛选？
                    //    var index = TaskList.ToList().FindIndex(a => a.Data.Id == obList[i].Id);

                    //    if (index != -1)
                    //    {
                    //        TaskList[index].Data = obList[i];
                    //    }
                    //    else
                    //    {
                    //        newTask.Add(new TaskModel() { Data = obList[i] } );
                    //    }
                    //}
                    //newTask.Reverse();
                    //foreach (var task in newTask)
                    //{
                    //    TaskList.Insert(0, task);
                    //}

                    ////保持等于kMaxTaskListCount的数量
                    //while (TaskList.Count > kMaxTaskListCount)
                    //{
                    //    TaskList.RemoveAt(TaskList.Count - 1);
                    //}
                    
                    if (obList.Count - TaskList.Count > 0)
                    {
                        while (obList.Count - TaskList.Count > 0)
                        {
                            TaskList.Add(new TaskModel());
                        }
                    }
                    else if (obList.Count - TaskList.Count < 0)
                    {
                        while (obList.Count - TaskList.Count < 0)
                        {
                            TaskList.RemoveAt(TaskList.Count - 1);
                        }
                    }

                    for (int i = 0;i < obList.Count; i++)
                    {
                        TaskList[i].Data = obList[i];
                    }
                });
            }
        }

        /// <summary>
        /// 玩客云登录成功后的信息处理
        /// </summary>
        public async Task<int> UpdateDevice()
        {
            var wkyApi = WkyApi;
            //获取设备信息
            var listPeerResult = await wkyApi.ListPeer();

            if (listPeerResult.Rtn == 0)
            {
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
            }
            else
            {
                throw new Exception("获取Peer失败");
            }


            return DeviceList.Count;
        }

        /// <summary>
        /// 选中设备，优先从上次选择中选中
        /// </summary>
        public async Task<WkyApiSharp.Service.Model.ListPeer.Device?> SelectDevice()
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
                    //更新USB信息
                    NowUsbInfo = await WkyApi.GetUsbInfo(selectDevice.DeviceId);
                    NowDevice = selectDevice;
                    return selectDevice;
                }

                //到这里说明没有配置AppConfig.ConfigData.LastDeviceId 或者 peer.Devices为空

                selectDevice = PeerList?.First()?.Devices?.First();
                if (selectDevice != null)
                {
                    NowUsbInfo = await WkyApi.GetUsbInfo(selectDevice.DeviceId);
                    NowDevice = selectDevice;
                }
                
                return selectDevice;
            }
            return null;
        }

        public string GetUsbInfoDefPath()
        {
            var savePath = string.Empty;
            if (NowUsbInfo != null && NowUsbInfo.Rtn == 0)
            {
                foreach (var disk in NowUsbInfo.Result)
                {
                    if (disk.ResultClass != null)
                    {
                        foreach (var partition in disk.ResultClass.Partitions)
                        {
                            savePath = partition.Path + "/onecloud/tddownload";
                        }
                    }

                }
            }
            return savePath;
        }

        /// <summary>
        /// 从一个BT的URL添加到下载中
        /// </summary>
        /// <param name="url"></param>
        public async Task<bool> DownloadBtFileUrl(string url, string path)
        {
            try
            {
                var data = await url.WithTimeout(15).GetBytesAsync();

                var bcCheck = await WkyApi.BtCheck(NowDevice.Peerid, data);
                Debug.WriteLine(bcCheck.ToString());
                if (bcCheck.Rtn == 0)
                {
                    var result = await WkyApi.CreateTaskWithBtCheck(NowDevice.Peerid, path, bcCheck);
                    if (result.Rtn == 0)
                    {
                        foreach (var item in result.Tasks)
                        {
                            if (item.Result == 202)
                            {
                                Debug.WriteLine($"重复添加任务：{item.Name}");
                                return false;
                            }
                        }
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }

        public async Task<bool> DownloadUrl(string url, string savePath = "")
        {
            if (string.IsNullOrWhiteSpace( savePath))
            {
                savePath = GetUsbInfoDefPath();
            }
            var urlResoleResult = await WkyApi.UrlResolve(NowDevice.Peerid, url);
            if (urlResoleResult.Rtn == 0)
            {
                var createResult = await WkyApi.CreateBatchTaskWithUrlResolve(NowDevice.Peerid, savePath, urlResoleResult, null);
                if (createResult.Rtn == 0)
                {
                    foreach (var item in createResult.Tasks)
                    {
                        if (item.Result == 202)
                        {
                            Debug.WriteLine($"重复添加任务：{item.Name}");
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> DownloadBtFile(string filePath, string savePath = "")
        {
            if (string.IsNullOrWhiteSpace(savePath))
            {
                savePath = GetUsbInfoDefPath();
            }
            var btResoleResult = await WkyApi.BtCheck(NowDevice.Peerid, filePath);
            if (btResoleResult.Rtn == 0)
            {
                var createResult = await WkyApi.CreateBatchTaskWithBtCheck(NowDevice.Peerid, savePath, btResoleResult, null);
                if (createResult.Rtn == 0)
                {
                    foreach (var item in createResult.Tasks)
                    {
                        if (item.Result == 202)
                        {
                            Debug.WriteLine($"重复添加任务：{item.Name}");
                            return false;
                        }
                    }

                    return true;
                }
            }
            return false;
        }


    }
}
