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
using WkyApiSharp.Service.Model;
using System.Reactive.Linq;
using WkyApiSharp.Events.Account;

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

        public WkyApi? API
        {
            set
            {
                _api = value;
                //开始监听
                SetupEvent();
            }
            get
            {
                return _api;
            }
        }


        private WkyApi? _api;

        public WkyDevice? NowDevice
        {
            set
            {
                _nowDevice = value;
            }
            get
            {
                return _nowDevice;
            }

        }


        public WkyDevice? _nowDevice;


        public ObservableCollection<TaskModel> TaskList { set; get; } = new ObservableCollection<TaskModel>();


        private void SetupEvent()
        {
            //安装事件监听

            _api?.EventReceived
                .OfType<UpdateTaskListEvent>()
                .Subscribe(async r =>
                {
                    Console.WriteLine("任务列表更新，UI刷新");

                    if (r.Peer != null && r.Peer.PeerId == _nowDevice?.PeerId)
                    {
                        var tasks = r.Peer.Tasks;

                        MainWindow.Instance.Dispatcher.Invoke(() => {
                            //TODO 更顺滑的更新任务

                            if (tasks.Count - TaskList.Count > 0)
                            {
                                while (tasks.Count - TaskList.Count > 0)
                                {
                                    TaskList.Add(new TaskModel());
                                }
                            }
                            else if (tasks.Count - TaskList.Count < 0)
                            {
                                while (tasks.Count - TaskList.Count < 0)
                                {
                                    TaskList.RemoveAt(TaskList.Count - 1);
                                }
                            }

                            for (int i = 0; i < tasks.Count; i++)
                            {
                                TaskList[i].Data = tasks[i].Data;
                            }
                        });
                    }


                });



        }

        public async Task UpdateTask()
        {
            var remoteDownloadListResult = await API.RemoteDownloadList(NowDevice.PeerId);

            if (remoteDownloadListResult.Rtn == 0)
            {
                var obList = remoteDownloadListResult.Tasks.ToList();
                MainWindow.Instance.Dispatcher.Invoke(() => {
                    //TODO 更顺滑的更新任务
                    
                    //if (obList.Count - TaskList.Count > 0)
                    //{
                    //    while (obList.Count - TaskList.Count > 0)
                    //    {
                    //        TaskList.Add(new TaskModel());
                    //    }
                    //}
                    //else if (obList.Count - TaskList.Count < 0)
                    //{
                    //    while (obList.Count - TaskList.Count < 0)
                    //    {
                    //        TaskList.RemoveAt(TaskList.Count - 1);
                    //    }
                    //}

                    //for (int i = 0;i < obList.Count; i++)
                    //{
                    //    TaskList[i].Data = obList[i];
                    //}
                });
            }
        }

        ///// <summary>
        ///// 玩客云登录成功后的信息处理
        ///// </summary>
        //public async Task<int> UpdateDevice()
        //{
        //    var wkyApi = WkyApi;
        //    //获取设备信息
        //    var listPeerResult = await wkyApi.ListPeer();

        //    if (listPeerResult.Rtn == 0)
        //    {
        //        PeerList.Clear();
        //        foreach (var item in listPeerResult.Result)
        //        {
        //            if (item.ResultClass != null)
        //            {
        //                PeerList.Add(item.ResultClass);
        //            }
        //        }
        //        DeviceList.Clear();
        //        foreach (var peer in PeerList)
        //        {
        //            foreach (var device in peer.Devices)
        //            {
        //                DeviceList.Add(device);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("获取Peer失败");
        //    }


        //    return DeviceList.Count;
        //}

        /// <summary>
        /// 选中设备，优先从上次选择中选中
        /// </summary>
        public async Task<WkyDevice> SelectDevice()
        {
            var device = API.GetDeviceWithId(AppConfig.ConfigData.LastDeviceId);
            if (device != null)
            {
                NowDevice = device;
                return device;
            }
            
            return null;
        }

        public string GetUsbInfoDefDownloadPath()
        {
            var savePath = string.Empty;
            foreach (var partition in NowDevice.Partitions)
            {
                savePath = partition.Path + "/onecloud/tddownload";
            }
            return savePath;
        }

        /// <summary>
        /// 获取默认默认存储设备的路径
        /// </summary>
        /// <returns></returns>
        public List<string> GetUsbInfoDefPath()
        {
            List<string> path = new List<string>();
            foreach (var partition in NowDevice.Partitions)
            {
                path.Add(partition.Path);
            }
            return path;
        }

        /// <summary>
        /// 从一个BT的URL添加到下载中（用于订阅的下载）
        /// </summary>
        /// <param name="url"></param>
        public async Task<DownloadResult> DownloadBtFileUrl(string url, string path)
        {
            DownloadResult downloadResult = new DownloadResult();
            downloadResult.Result = false;

            try
            {
                var data = await url.WithTimeout(15).GetBytesAsync();

                var bcCheck = await API.BtCheck(NowDevice.Device.Peerid, data);
                Debug.WriteLine(bcCheck.ToString());
                if (bcCheck.Rtn == 0)
                {
                    var result = await API.CreateTaskWithBtCheck(NowDevice.Device.Peerid, path, bcCheck);
                    if (result.Rtn == 0)
                    {
                        downloadResult.Result = true;
                        foreach (var item in result.Tasks)
                        {
                            if (item.Result == 202)
                            {
                                Debug.WriteLine($"重复添加任务：{item.Name}");
                                downloadResult.isDuplicateAddTask = true;
                                downloadResult.Result = false;
                            }
                        }
                        
                    }
                } 
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
 
            }
            
            return downloadResult;
        }

        public async Task<bool> DownloadUrl(string url, string savePath = "")
        {
            if (string.IsNullOrWhiteSpace( savePath))
            {
                savePath = GetUsbInfoDefDownloadPath();
            }
            var urlResoleResult = await API.UrlResolve(NowDevice.Device.Peerid, url);
            if (urlResoleResult.Rtn == 0)
            {
                var createResult = await API.CreateBatchTaskWithUrlResolve(NowDevice.Device.Peerid, savePath, urlResoleResult, null);
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
                savePath = GetUsbInfoDefDownloadPath();
            }
            var btResoleResult = await API.BtCheck(NowDevice.Device.Peerid, filePath);
            if (btResoleResult.Rtn == 0)
            {
                var createResult = await API.CreateBatchTaskWithBtCheck(NowDevice.Device.Peerid, savePath, btResoleResult, null);
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
