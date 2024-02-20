﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WkyFast.Service.Model;
using Aria2NET;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Net.Http;
using System.Net;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Flurl.Http;


namespace WkyFast.Service
{
    public class Aria2ApiManager
    {
        public const string KARIA2_STATUS_ACTIVE = "active";
        public const string KARIA2_STATUS_WAITING = "waiting";
        public const string KARIA2_STATUS_PAUSED = "paused";
        public const string KARIA2_STATUS_ERROR = "error";
        public const string KARIA2_STATUS_COMPLETE = "complete";
        public const string KARIA2_STATUS_REMOVED = "removed";

        private static int kMaxTaskListCount = 100;

        private static Aria2ApiManager instance = new Aria2ApiManager();

        private Aria2NetClient _client ;

        private DateTime _lastRpcUpdateTime;
        private TimeSpan _debounceDelay = TimeSpan.FromSeconds(2);  // Set your own delay


        public IObservable<Aria2Event> EventReceived => _eventReceivedSubject.AsObservable();

        public bool Connected;
        public string ConnectedRpc;


        private readonly Subject<Aria2Event> _eventReceivedSubject = new();


        public static Aria2ApiManager Instance
        {
            get
            {

                return instance;
            }
        }

        public ObservableCollection<TaskModel> TaskList { set; get; } = new ObservableCollection<TaskModel>();


        Aria2ApiManager()
        {
            try {
                UpdateRpc();
            }
            catch (Exception ex)
            {

            }
            SetupEvent();
        }
       
        private void SetupEvent()
        {
            //TODO 安装事件监听 更新列表

            //_client.TellWaitingAsync()Task
            


            Task.Run(async () =>
            {

                while (true)
                {
                    try
                    {
                        if (Connected)
                        {
                            var tasks = await _client.TellAllAsync();

                            MainWindow.Instance.Dispatcher.Invoke(() =>
                            {
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
                                    TaskList[i].Data = tasks[i];
                                }

                            });
                        }
                        
                    }
                    catch (Exception ex)
                    {

                    }
                    await Task.Delay(5000);

                }
            });


            

            //_api?.EventReceived
            //    .OfType<LoginResultEvent>()
            //    .Subscribe(async r =>
            //    {
            //        //_eventReceivedSubject.OnNext(r);
            //    });


            //_api?.EventReceived
            //    .OfType<DownloadSuccessEvent>()
            //    .Subscribe(async r =>
            //    {
            //        EasyLogManager.Logger.Info($"下载完成 {r.Task.Data.Name} {r.Task.Data.Path}");
            //        if (AppConfig.Instance.ConfigData.PushDeerOpen)
            //        {
            //            await PushDeer.SendPushDeer($"下载完成 {r.Task.Data.Name}", $"用时 {TimeHelper.SecondsToFormatString((int)r.Task.Data.DownTime)}");
            //        }
            //    });
        }

        /// <summary>
        /// 从一个BT的URL添加到下载中（用于订阅的下载）
        /// </summary>
        /// <param name="url"></param>
        public async Task<WkyDownloadResult> DownloadBtFileUrl(string url, string path, string taskName = "")
        {
            //先下载BT文件

            byte[] data = { };

            if (AppConfig.Instance.ConfigData.SubscriptionProxyOpen && !string.IsNullOrEmpty(AppConfig.Instance.ConfigData.SubscriptionProxy))
            {
                var proxyUrl = AppConfig.Instance.ConfigData.SubscriptionProxy;

                var proxy = new WebProxy(proxyUrl);
                var handler = new HttpClientHandler() { Proxy = proxy };
                var client = new HttpClient(handler);

                // 注意这里的GET请求的地址需要替换为你需要请求的地址
                var response = client.GetAsync(url).Result;

                data = await response.Content.ReadAsByteArrayAsync();

               
            } 
            else
            {
                data = await url.GetBytesAsync();
            }

            var config = new Dictionary<String, Object>
            {
                { "dir", System.IO.Path.Combine(path, taskName)}
            };

            WkyDownloadResult downloadResult = new WkyDownloadResult();
            if (data.Length > 0)
            {
                var result = await _client.AddTorrentAsync(data, options: config, position: 0);
                Debug.WriteLine($"DownloadBtFileUrl结果：{result}");
                
                downloadResult.isSuccessed = IsGid(result);
                downloadResult.Gid = result;
            }
            else
            {
                downloadResult.isSuccessed = false;
            }

            return downloadResult;
        }

        public async Task<WkyDownloadResult> DownloadUrl(string url, string savePath = "")
        {
            var result = await _client.AddUriAsync(new List<String>
            {
                url
            },
            new Dictionary<String, Object>
            {
                            { "dir", savePath}
            }, 0);


            Debug.WriteLine($"DownloadBtFileUrl结果：{result}");

            WkyDownloadResult downloadResult = new WkyDownloadResult();
            downloadResult.isSuccessed = IsGid(result);
            downloadResult.Gid = result;


            return downloadResult;
        }

        public async Task<WkyDownloadResult> DownloadBtFile(string filePath, string savePath = "")
        {
            var config = new Dictionary<String, Object>
                    {
                       { "dir", savePath}
                    };

            var result = await _client.AddTorrentAsync(File.ReadAllBytes(filePath), options: config, position: 0);

            Debug.WriteLine($"DownloadBtFile结果：{result}");

            WkyDownloadResult downloadResult = new WkyDownloadResult();
            downloadResult.isSuccessed = IsGid(result);
            downloadResult.Gid = result;


            return downloadResult;
        }

        public async Task<bool> DeleteFile(string gid)
        {
            var result = await _client.RemoveAsync(gid);

            return true;
        }

        internal async Task<bool> UpdateTask()
        {
            throw new NotImplementedException();
        }

        internal async Task<bool> StartTask(string gid)
        {
            throw new NotImplementedException();
        }

        internal async Task<bool> StopTask(string gid)
        {
            throw new NotImplementedException();
        }

        internal async Task<bool> UpdateRpc()
        {
            try
            {
                // If last update time was within the delay, ignore this update
                if (DateTime.Now - _lastRpcUpdateTime < _debounceDelay)
                    return false;

                _lastRpcUpdateTime = DateTime.Now;

                var rpc = AppConfig.Instance.ConfigData.Aria2Rpc;
                var token = AppConfig.Instance.ConfigData.Aria2Token;

                _client = new Aria2NetClient(rpc, token);

                var result = await _client.GetGlobalOptionAsync();

                if (result.Count > 0)
                {
                    Connected = true;
                    ConnectedRpc = rpc;
                    _eventReceivedSubject.OnNext(new LoginResultEvent(true));

                    return true;
                }
                else
                {
                    Connected = false;
                    ConnectedRpc = "";
                    _eventReceivedSubject.OnNext(new LoginResultEvent(false));

                    return false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return false;
        }

        private static bool IsGid(string gid)
        {
            if (string.IsNullOrWhiteSpace(gid))
            {
                return false;
            }
            //2089b05ecca3d829
            if (gid.Length == 16)
            {
                return true;
            }
            return false;
        }
    }
}