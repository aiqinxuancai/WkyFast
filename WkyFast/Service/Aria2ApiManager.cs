
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

        public IObservable<Aria2Event> EventReceived => _eventReceivedSubject.AsObservable();

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
                _client = new Aria2NetClient(AppConfig.Instance.ConfigData.Aria2Rpc, AppConfig.Instance.ConfigData.Aria2Token);
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

                var a = _client.GetGlobalOptionAsync().Result;

                while (true)
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

            var result = await _client.AddUriAsync(new List<String>
            {
                url
            },
            new Dictionary<String, Object>
            {
                { "dir", System.IO.Path.Combine(path, taskName)}
            }, 0);


            Debug.WriteLine($"DownloadBtFileUrl结果：{result}");

            WkyDownloadResult downloadResult = new WkyDownloadResult();
            downloadResult.SuccessCount = 1;

            
            
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
            downloadResult.SuccessCount = 1;



            return downloadResult;
        }

        public async Task<WkyDownloadResult> DownloadBtFile(string filePath, string savePath = "")
        {
            var config = new Dictionary<String, Object>
                    {
                       { "dir", savePath}
                    };

            var result = await _client.AddTorrentAsync(File.ReadAllBytes(filePath), options: config);

            Debug.WriteLine($"DownloadBtFile结果：{result}");

            WkyDownloadResult downloadResult = new WkyDownloadResult();
            downloadResult.SuccessCount = 1;
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
                _client = new Aria2NetClient(AppConfig.Instance.ConfigData.Aria2Rpc, AppConfig.Instance.ConfigData.Aria2Token);


                var result = await _client.GetGlobalOptionAsync();

                if (result.Count > 0)
                {
                    _eventReceivedSubject.OnNext(new LoginResultEvent(true));
                    return true;
                }
                else
                {
                    _eventReceivedSubject.OnNext(new LoginResultEvent(false));
                    return false;
                }

            }
            catch (Exception ex)
            {


            }
            return false;
        }
    }
}
