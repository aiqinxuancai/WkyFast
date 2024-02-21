using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WkyFast.Service.Model;
using Newtonsoft.Json;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Xml.Linq;
using System.Diagnostics;
using Flurl.Http;
using System.Threading;
using System.Collections.ObjectModel;
using WkyFast.Utils;
using WkyFast.Service.Model.SubscriptionModel;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;
using MemoryPack;
using System.Reactive.Linq;

namespace WkyFast.Service
{
    public class SubscriptionManager
    {
        public event Action<int, int> OnSubscriptionProgressChanged;

        private static SubscriptionManager instance = new SubscriptionManager();

        public static SubscriptionManager Instance
        {
            get
            {
                return instance;
            }
        }

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public ObservableCollection<SubscriptionModel> SubscriptionModel { get; set; } = new ObservableCollection<SubscriptionModel>();

        /// <summary>
        /// 由每次加载SubscriptionModel时，进行初始化，本身不存储
        /// TODO 存储所有的订阅下载 使用MemoryPack
        /// </summary>
        public Dictionary<string, string> TaskUrlToSubscriptionName { get; set; } = new ();

        public bool Subscribing { get; set; } = false;


        private object _look = new object();

        public SubscriptionManager()
        {
            SubscriptionModel = new ObservableCollection<SubscriptionModel>();
            //SubscriptionModel.CollectionChanged += SubscriptionModel_CollectionChanged;

            Aria2ApiManager.Instance.EventReceived
                .OfType<LoginResultEvent>()
                .Subscribe(async r =>
                {
                    if (r.IsSuccess)
                    {
                        Restart();
                    }
                    else
                    {
                        //this.AddTaskButton.IsEnabled = false;
                    }
                });
        }

        ~SubscriptionManager()
        {
            _tokenSource.Cancel();
        }

        //private void SubscriptionModel_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    Save();
        //}

        /// <summary>
        /// 载入并启动订阅刷新
        /// </summary>
        public void Restart()
        {
            Load();
            Start();
        }

        public void Start()
        {
            if (_tokenSource != null) //TODO 停止任务
            {
                _tokenSource.Cancel();
            }

            _tokenSource = new CancellationTokenSource();
            Task.Run(() => TimerFunc(_tokenSource.Token), _tokenSource.Token);
        }

        public void Stop()
        {
            if (_tokenSource != null) //TODO 停止任务
            {
                _tokenSource.Cancel();
            }
        }

        private void TimerFunc(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    CheckSubscription();
                }
                catch (Exception ex)
                {
                    EasyLogManager.Logger.Error(ex.ToString());
                }
                

                TaskHelper.Sleep(1000 * 60 * 5, 100, cancellationToken);
            }
        }


        /// <summary>
        /// 是否可以下载
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        private bool CheckTitle(SubscriptionModel subscription, string title)
        {
            //检查是否符合过滤方法

            try
            {
                if (!string.IsNullOrWhiteSpace(subscription.Filter))
                {
                    if (subscription.IsFilterRegex)
                    {
                        Regex regex = new Regex(subscription.Filter);
                        if (!regex.IsMatch(title))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //检查是否包含文本
                        var strList = subscription.Filter.Split("|");
                        var count = 0;
                        foreach (var str in strList)
                        {
                            if (title.Contains(str))
                            {
                                count++;
                            }
                        }
                        if (count == 0)
                        {
                            return false;
                        }
                    }
                }

            } 
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex.ToString());
                return true;
            }

            
            return true;
        }

        private int GetMatchTaskCount(IEnumerable<SyndicationItem> Items, SubscriptionModel model)
        {
            int count = 0;
            foreach (SyndicationItem item in Items)
            {
                string subject = item.Title.Text;
                string summary = item.Summary?.Text;
                if (CheckTitle(model, subject))
                {
                    count++;
                }

            }
            return count;

        }


        //public string GetSubscriptionName(string downloadName)
        //{
        //    foreach (var sub in SubscriptionModel)
        //    {
        //        foreach (var model in sub.AlreadyAddedDownloadModel)
        //        {
        //            if (model.Result != null && model.Result.Tasks.Count() > 0)
        //            {
        //                foreach (var task in model.Result.Tasks)
        //                {
        //                    if (task.Name == downloadName)
        //                    {
        //                        return model.Name;
        //                    }
        //                }
                        
        //            }
        //        }
        //    }
        //    return "";
        //}

        /// <summary>
        /// 通过网络获取订阅地址的Title
        /// </summary>
        /// <param name="url"></param>
        public string GetSubscriptionTitle(string url)
        {
            try
            {
                EasyLogManager.Logger.Info($"订阅地址：{url}");
                XmlReader reader;
                SyndicationFeed feed;

                try
                {

                    if (AppConfig.Instance.ConfigData.SubscriptionProxyOpen && !string.IsNullOrEmpty(AppConfig.Instance.ConfigData.SubscriptionProxy))
                    {
                        var proxyUrl = AppConfig.Instance.ConfigData.SubscriptionProxy;

                        var proxy = new WebProxy(proxyUrl);
                        var handler = new HttpClientHandler() { Proxy = proxy };
                        var client = new HttpClient(handler);

                        // 注意这里的GET请求的地址需要替换为你需要请求的地址
                        var response = client.GetAsync(url).Result;


                        reader = XmlReader.Create(response.Content.ReadAsStreamAsync().Result);
                        feed = SyndicationFeed.Load(reader);
                        reader.Close();
                    }
                    else
                    {
                        reader = XmlReader.Create(url);
                        feed = SyndicationFeed.Load(reader);
                        reader.Close();
                    }
                    EasyLogManager.Logger.Info($"获取订阅标题：{feed.Title.Text}");
                    return feed.Title.Text;

                }
                catch (Exception e)
                {
                    EasyLogManager.Logger.Error($"无法访问订阅：{url} \n {e}");
                }


            }
            catch (Exception e)
            {
                EasyLogManager.Logger.Error($"获取订阅标题失败");
                
            }
            return "";
        }

        /// <summary>
        /// 检查一次订阅
        /// </summary>
        private async void CheckSubscription()
        {
            Subscribing = true;

            EasyLogManager.Logger.Info("检查订阅...");

            var copyList = new List<SubscriptionModel>(SubscriptionModel);


            OnSubscriptionProgressChanged?.Invoke(0, copyList.Count);

            for (int i = 0; i < copyList.Count; i++)
            {
                lock (_look)
                {
                    CheckSubscriptionOne(copyList[i]);
                }

                OnSubscriptionProgressChanged?.Invoke(i, copyList.Count);
            }

            OnSubscriptionProgressChanged?.Invoke(copyList.Count, copyList.Count);

            Subscribing = false;
        }

        public async void CheckSubscriptionOne(SubscriptionModel subscription)
        {
            string url = subscription.Url;

            EasyLogManager.Logger.Info($"订阅地址：{url}");


            XmlReader reader;
            SyndicationFeed feed = null;

            if (subscription.AlreadyAddedDownloadModel == null)
            {
                subscription.AlreadyAddedDownloadModel = new ObservableCollection<SubscriptionSubTaskModel> { };
            }

            try
            {

                if (AppConfig.Instance.ConfigData.SubscriptionProxyOpen && !string.IsNullOrEmpty(AppConfig.Instance.ConfigData.SubscriptionProxy))
                {
                    var proxyUrl = AppConfig.Instance.ConfigData.SubscriptionProxy;

                    var proxy = new WebProxy(proxyUrl);
                    var handler = new HttpClientHandler() { Proxy = proxy };
                    var client = new HttpClient(handler);

                    // 注意这里的GET请求的地址需要替换为你需要请求的地址
                    var response = client.GetAsync(url).Result;


                    reader = XmlReader.Create(response.Content.ReadAsStreamAsync().Result);
                    feed = SyndicationFeed.Load(reader);
                    reader.Close();
                }
                else
                {
                    reader = XmlReader.Create(url);
                    feed = SyndicationFeed.Load(reader);
                    reader.Close();
                }

            }
            catch (Exception e)
            {
                EasyLogManager.Logger.Error($"无法访问订阅：{url} \n {e}");
                //continue;
                return;
            }


            subscription.TaskFullCount = feed!.Items.Count();
            subscription.Name = feed.Title.Text;
            subscription.TaskMatchCount = GetMatchTaskCount(feed.Items, subscription);

            EasyLogManager.Logger.Info($"订阅标题：{subscription.Name} 订阅总任务数：{subscription.TaskFullCount} 符合任务数：{subscription.TaskMatchCount}");

            foreach (SyndicationItem item in feed.Items)
            {
                string subject = item.Title.Text;
                string summary = item.Summary?.Text;
                

                if (CheckTitle(subscription, subject))
                {
                    //EasyLogManager.Logger.Info($"标题验证 {subject} 通过，准备下载");
                }
                else
                {
                    //EasyLogManager.Logger.Info($"标题验证 {subject} 未通过");
                    continue;
                }

                var savePath = (subscription.Path.StartsWith("/") ? "" : "/") + subscription.Path;

                foreach (var link in item.Links)
                {
                    string downloadUrl = link.Uri.ToString();
                    //没有下载过
                    if (!subscription.AlreadyAddedDownloadModel.Any(a => a.Url.Contains(downloadUrl)))
                    {
                        if (link.RelationshipType == "enclosure" ||
                           (!string.IsNullOrWhiteSpace(link.MediaType) && link.MediaType.Contains("bittorrent")))  //"application/x-bittorrent"
                        {
                            try
                            {
                                savePath = await AutoEpisodeTitle(subscription, subject, savePath);

                                EasyLogManager.Logger.Info($"添加下载{subject} {link.Uri} {savePath}");

                                //支持由http开头的bt文件和magnet:?xt=urn:btih:开头的文件
                                var aria2Result = Aria2ApiManager.Instance.DownloadBtFileUrl(downloadUrl, savePath).Result;

                                if (aria2Result.isSuccessed)
                                {
                                    TaskUrlToSubscriptionName[aria2Result.Gid] = subject;
                                    if (!string.IsNullOrWhiteSpace(aria2Result.InfoHash))
                                    {
                                        TaskUrlToSubscriptionName[aria2Result.InfoHash] = subject;
                                    }
                                }

                                if (aria2Result.isSuccessed)
                                {
                                    subscription.AlreadyAddedDownloadModel.Add(new SubscriptionSubTaskModel() { Name = subject, Url = downloadUrl, Time = DateTime.Now });
                                    EasyLogManager.Logger.Info($"添加成功");
                                }
                                else
                                {
                                    EasyLogManager.Logger.Error($"添加失败");
                                }
                            }
                            catch (Exception ex)
                            {
                                EasyLogManager.Logger.Error(ex.ToString());
                            }


                        }
                        else
                        {
                            //其他类型订阅，如https、ftp
                            try
                            {
                                savePath = await AutoEpisodeTitle(subscription, subject, savePath);

                                EasyLogManager.Logger.Info($"添加下载{subject} {link.Uri} {savePath}");
                                var aria2Result = Aria2ApiManager.Instance.DownloadUrl(downloadUrl, savePath).Result;

                                if (aria2Result.isSuccessed)
                                {
                                    TaskUrlToSubscriptionName[aria2Result.Gid] = subject;
                                    if (!string.IsNullOrWhiteSpace(aria2Result.InfoHash))
                                    {
                                        TaskUrlToSubscriptionName[aria2Result.InfoHash] = subject;
                                    }
                                }

                                if (aria2Result.isSuccessed)
                                {
                                    subscription.AlreadyAddedDownloadModel.Add(new SubscriptionSubTaskModel() { Name = subject, Url = downloadUrl, Time = DateTime.Now });
                                    EasyLogManager.Logger.Info($"添加成功");
                                }
                                else
                                {
                                    EasyLogManager.Logger.Error($"添加失败");
                                }
                            }
                            catch (Exception ex)
                            {
                                EasyLogManager.Logger.Error(ex.ToString());
                            }
                        }

                    }
                       
                }



                //foreach (SyndicationElementExtension extension in item.ElementExtensions)
                //{
                //    XElement ele = extension.GetObject<XElement>();

                //    Debug.WriteLine("节点名称:" + ele.Name);

                //    foreach (XElement node in ele.Nodes())
                //    {
                //        Debug.WriteLine(node.Name.LocalName + " " + node.Value);
                //        if (node.Name.LocalName == "link")
                //        {

                //        }
                //    }

                //}

            }
            Save();
            
        }

        private static async Task<string> AutoEpisodeTitle(SubscriptionModel subscription, string subject, string savePath)
        {
            var episodeTitle = "";
            //使用自动的剧集名称，来源gpt
            if (subscription.AutoDir)
            {
                EasyLogManager.Logger.Info($"使用自动目录分组");
                if (subscription.EpisodeTitleList == null)
                {
                    subscription.EpisodeTitleList = new List<string>();
                }
                episodeTitle = subscription.EpisodeTitleList.FirstOrDefault(a => subject.Contains(a));
                if (string.IsNullOrEmpty(episodeTitle) && AppConfig.Instance.ConfigData.OpenAIOpen)
                {
                    EasyLogManager.Logger.Info($"获取剧集名称...");
                    //使用ChatGPT
                    episodeTitle = await ChatGPTTranslatorManager.GetEpisode(subject);

                    EasyLogManager.Logger.Info($"剧集名称：{episodeTitle}");
                    if (!string.IsNullOrEmpty(episodeTitle))
                    {
                        subscription.EpisodeTitleList.Add(episodeTitle);
                    }
                }
            }

            if (!string.IsNullOrEmpty(episodeTitle))
            {
                savePath = savePath + "/" + PathHelper.RemoveInvalidChars(episodeTitle);
            }

            return savePath;
        }

        public void LoadTrueName()
        {
            string fileName = @$"UrlNameTable.bin";
            TaskUrlToSubscriptionName = new Dictionary<string, string>();

            if (File.Exists(fileName))
            {
                var fileContent = File.ReadAllBytes(fileName);
                var val = MemoryPackSerializer.Deserialize<Dictionary<string, string>>(fileContent);

                if (val != null)
                {
                    TaskUrlToSubscriptionName = val;
                }

            }
        }


        public void SaveTrueName()
        {
            string fileName = @$"UrlNameTable.bin";

            var bin = MemoryPackSerializer.Serialize(TaskUrlToSubscriptionName);

            try
            {
                File.WriteAllBytes(fileName, bin);
            }
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex);
            }




        }

        public void Load()
        {
            lock (_look)
            {

                LoadTrueName();

                var rpc = Aria2ApiManager.Instance.ConnectedRpc;
                var uri = new Uri(rpc);

                string fileName = @$"Subscription_{uri.Host}.json";
                Debug.WriteLine($"准备载入{fileName}");

                //名称URL
                //载入TaskUrlToSubscriptionName

                if (File.Exists(fileName))
                {
                    SubscriptionModel.Clear();

                    List<SubscriptionModel> subscriptionModel = JsonConvert.DeserializeObject<List<SubscriptionModel>>(File.ReadAllText(fileName));

                    if (subscriptionModel != null)
                    {
                        foreach (SubscriptionModel item in subscriptionModel)
                        {
                            SubscriptionModel.Add(item);

                            //加载名称表
                            foreach (var m in item.AlreadyAddedDownloadModel)
                            {
                                var taskUrl = m.Url;
                                if (!string.IsNullOrWhiteSpace(taskUrl))
                                {
                                    TaskUrlToSubscriptionName[taskUrl] = m.Name;
                                }
                                
                            }
                        }
                    }

                }

                SaveTrueName();




            }
        }


        public void Save()
        {
            lock (_look)
            {
                Debug.WriteLine("保存订阅");

                try
                {
                    var rpc = Aria2ApiManager.Instance.ConnectedRpc;
                    var uri = new Uri(rpc);
                    string fileName = @$"Subscription_{uri.Host}.json";
                    var content = JsonConvert.SerializeObject(SubscriptionModel);
                    File.WriteAllText(fileName, content);
                }
                catch (Exception ex)
                {
                    EasyLogManager.Logger.Error(ex);
                }
                try
                {
                    SaveTrueName();
                }
                catch (Exception ex)
                {
                    EasyLogManager.Logger.Error(ex);
                }
            }

        }

        //存储订阅，读取加载订阅

        public bool Add(string url, string path, string keyword = "", bool keywordIsRegex = false, bool autoDir = false)
        {
            if (SubscriptionModel.ToList().Find( a => { return a.Url == url; }) != null)
            {
                //找到了存在相同
                EasyLogManager.Logger.Error($"添加失败，重复的订阅");
                return false;
            }

            SubscriptionModel model = new SubscriptionModel();
            model.Url = url;
            model.Filter = keyword;
            model.IsFilterRegex = keywordIsRegex;
            model.Path = path;
            model.AutoDir = autoDir;
            EasyLogManager.Logger.Error($"添加订阅：{model.Url}");

            SubscriptionModel.Add(model);


            Save();


            Task.Run(() => {
                CheckSubscription();
            });
            
            return true;
        }

        public void Remove(string url)
        {
            for (int i = 0; i < SubscriptionModel.Count; i++)
            {
                if (SubscriptionModel[i].Url == url)
                {
                    SubscriptionModel.RemoveAt(i);
                    break; //只删除一个
                }
            }
        }

    }
}
