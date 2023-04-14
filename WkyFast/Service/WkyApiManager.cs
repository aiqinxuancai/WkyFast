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
using WkyApiSharp.Events;
using System.Reactive.Subjects;

namespace WkyFast.Service
{
    public class WkyApiManager
    {
        private static int kMaxTaskListCount = 100;

        private static WkyApiManager instance = new WkyApiManager();

        //事件
        //public IObservable<EventBase> EventReceived => _eventReceivedSubject.AsObservable();

        //private readonly Subject<EventBase> _eventReceivedSubject = new();

        public static WkyApiManager Instance
        {
            get
            {
                return instance;
            }
        }

        public WkyApi API
        {
            get
            {
                return _api;
            }
        }


        private WkyApi _api = new();

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


        WkyApiManager()
        {
            SetupEvent();
        }
       
        private void SetupEvent()
        {
            //安装事件监听

            _api?.EventReceived
                .OfType<UpdateTaskListEvent>()
                .Subscribe(async r =>
                {
                    Console.WriteLine("任务列表更新，UI刷新");

                    //_eventReceivedSubject.OnNext(r);

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

            _api?.EventReceived
                .OfType<LoginResultEvent>()
                .Subscribe(async r =>
                {
                    //_eventReceivedSubject.OnNext(r);
                });
        }


        /// <summary>
        /// 选中设备，优先从上次选择中选中
        /// </summary>
        public async Task<WkyDevice> SelectDevice()
        {
            var device = _api?.GetDeviceWithId(AppConfig.Instance.ConfigData.LastDeviceId);
            if (device != null)
            {
                _nowDevice = device;
                return device;
            }
            
            return null;
        }

        public string GetUsbInfoDefDownloadPath()
        {
            var savePath = string.Empty;
            foreach (var partition in _nowDevice.Partitions)
            {
                savePath = partition.Partition.Path + "/onecloud/tddownload";
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
            foreach (var partition in _nowDevice.Partitions)
            {
                path.Add(partition.Partition.Path);
            }
            return path;
        }

        /// <summary>
        /// 从一个BT的URL添加到下载中（用于订阅的下载）
        /// </summary>
        /// <param name="url"></param>
        public async Task<WkyDownloadResult> DownloadBtFileUrl(string url, WkyDevice device,  string path)
        {
            WkyDownloadResult downloadResult = new WkyDownloadResult();
            downloadResult.SuccessCount = 0;

            WkyDevice? wkyDevice = device;

            if (wkyDevice == null)
            {
                wkyDevice = _nowDevice;
            }

            try
            {
                var data = await url.WithTimeout(15).GetBytesAsync();

                var bcCheck = await _api?.BtCheck(wkyDevice?.Device.Peerid, data);
                Debug.WriteLine(bcCheck.ToString());
                if (bcCheck.Rtn == 0)
                {
                    var result = await _api?.CreateTaskWithBtCheck(wkyDevice.Device.Peerid, path, bcCheck);
                    if (result.Rtn == 0)
                    {
                        downloadResult.Result = result;
                        downloadResult.AllTaskCount = result.Tasks.Length;
                        foreach (var item in result.Tasks)
                        {
                            if (item.Result == 202)
                            {
                                Debug.WriteLine($"重复添加任务：{item.Name}");
                                downloadResult.DuplicateAddTaskCount++;
                            }
                            else if (item.Result == 0)
                            {
                                downloadResult.SuccessCount++;
                            }

                        }
                        
                    }
                    else
                    {
                        downloadResult.hasError = true;
                    }
                } 
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                downloadResult.hasError = true;

            }
            
            return downloadResult;
        }

        public async Task<WkyDownloadResult> DownloadUrl(string url, string savePath = "")
        {
            WkyDownloadResult downloadResult = new WkyDownloadResult();
            downloadResult.SuccessCount = 0;

            try
            {
                if (string.IsNullOrWhiteSpace(savePath))
                {
                    savePath = GetUsbInfoDefDownloadPath();
                }
                var urlResoleResult = await _api?.UrlResolve(_nowDevice?.Device.Peerid, url);
                if (urlResoleResult.Rtn == 0)
                {
                    var createResult = await _api?.CreateBatchTaskWithUrlResolve(_nowDevice.Device.Peerid, savePath, urlResoleResult, null);
                    if (createResult.Rtn == 0)
                    {
                        foreach (var item in createResult.Tasks)
                        {
                            if (item.Result == 202)
                            {
                                Debug.WriteLine($"重复添加任务：{item.Name}");
                                downloadResult.DuplicateAddTaskCount++;
                            }
                            else if (item.Result == 0)
                            {
                                downloadResult.SuccessCount++;
                            }
                        }
                    }
                    else
                    {
                        downloadResult.hasError = true;
                    }
                }
                else
                {
                    downloadResult.hasError = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                downloadResult.hasError = true;
            }

            


            return downloadResult;
        }

        public async Task<WkyDownloadResult> DownloadBtFile(string filePath, string savePath = "")
        {
            WkyDownloadResult downloadResult = new WkyDownloadResult();
            downloadResult.SuccessCount = 0;

            try
            {
                if (string.IsNullOrWhiteSpace(savePath))
                {
                    savePath = GetUsbInfoDefDownloadPath();
                }
                var btResoleResult = await _api?.BtCheck(NowDevice?.Device.Peerid, filePath);
                if (btResoleResult.Rtn == 0)
                {
                    var createResult = await _api?.CreateBatchTaskWithBtCheck(NowDevice.Device.Peerid, savePath, btResoleResult, null);
                    if (createResult.Rtn == 0)
                    {
                        foreach (var item in createResult.Tasks)
                        {
                            if (item.Result == 202)
                            {
                                Debug.WriteLine($"重复添加任务：{item.Name}");
                                downloadResult.DuplicateAddTaskCount++;
                            }
                            else if (item.Result == 0)
                            {
                                downloadResult.SuccessCount++;
                            }
                        }
                    }
                    else
                    {
                        downloadResult.hasError = true;
                    }
                }
                else
                {
                    downloadResult.hasError = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                downloadResult.hasError = true;
            }
            
            return downloadResult;
        }


    }
}
