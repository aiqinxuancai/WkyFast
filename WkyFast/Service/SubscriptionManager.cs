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
using WkyApiSharp.Service.Model;
using System.Collections;
using System.Net.Http;
using System.Net;

namespace WkyFast.Service
{
    public class SubscriptionManager
    {
        
        private static SubscriptionManager instance = new SubscriptionManager();

        public static SubscriptionManager Instance
        {
            get
            {
                return instance;
            }
        }

        private string _user;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();


        /// <summary>
        /// 来自账户，根据不同账户订阅不同？
        /// </summary>
        public string User {
            get
            {
                return _user;
            }
            set
            {
                _user = value;

            }
        }

        public ObservableCollection<SubscriptionModel> SubscriptionModel { get; set; } = new ObservableCollection<SubscriptionModel>();

        public Hashtable TaskUrlToSubscriptionName { get; set; } = new Hashtable();

        public SubscriptionManager()
        {
            SubscriptionModel = new ObservableCollection<SubscriptionModel>();
            //SubscriptionModel.CollectionChanged += SubscriptionModel_CollectionChanged;
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
                string summary = item.Summary.Text;
                if (CheckTitle(model, subject))
                {
                    count++;
                }

            }
            return count;

        }


        public string GetSubscriptionName(string downloadName)
        {
            foreach (var sub in SubscriptionModel)
            {
                foreach (var model in sub.AlreadyAddedDownloadModel)
                {
                    if (model.Result != null && model.Result.Tasks.Count() > 0)
                    {
                        foreach (var task in model.Result.Tasks)
                        {
                            if (task.Name == downloadName)
                            {
                                return model.Name;
                            }
                        }
                        
                    }
                }
            }
            return "";
        }

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
                        var handler = new HttpClientHandler
                        {
                            UseProxy = true,
                            Proxy = new WebProxy(proxyUrl)
                        };
                        var client = new HttpClient(handler);
                        var flurlClient = new FlurlClient(client);
                        var data = url
                            .WithClient(flurlClient).GetStreamAsync().Result;

                        reader = XmlReader.Create(data);
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
            EasyLogManager.Logger.Info("检查订阅...");

            var copyList = new List<SubscriptionModel>(SubscriptionModel);

            foreach (var subscription in copyList)
            {
                string url = subscription.Url;
                
                EasyLogManager.Logger.Info($"订阅地址：{url}");


                XmlReader reader ;
                SyndicationFeed feed ;

                try
                {

                    if (AppConfig.Instance.ConfigData.SubscriptionProxyOpen && !string.IsNullOrEmpty(AppConfig.Instance.ConfigData.SubscriptionProxy))
                    {
                        var proxyUrl = AppConfig.Instance.ConfigData.SubscriptionProxy;
                        var handler = new HttpClientHandler
                        {
                            UseProxy = true,
                            Proxy = new WebProxy(proxyUrl)
                        };
                        var client = new HttpClient(handler);
                        var flurlClient = new FlurlClient(client);
                        var data = url
                            .WithClient(flurlClient).GetStreamAsync().Result;

                        reader = XmlReader.Create(data);
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
                    continue;
                }


                subscription.TaskFullCount = feed.Items.Count();
                subscription.Name = feed.Title.Text;
                subscription.TaskMatchCount = GetMatchTaskCount(feed.Items, subscription);

                EasyLogManager.Logger.Info($"订阅标题：{subscription.Name} 订阅总任务数：{subscription.TaskFullCount} 符合任务数：{subscription.TaskMatchCount}");

                foreach (SyndicationItem item in feed.Items)
                {
                    string subject = item.Title.Text;
                    string summary = item.Summary.Text;


                    if (CheckTitle(subscription, subject))
                    {
                        //EasyLogManager.Logger.Info($"标题验证 {subject} 通过，准备下载");
                    }
                    else
                    {
                        //EasyLogManager.Logger.Info($"标题验证 {subject} 未通过");
                        continue;
                    }
                    
                    foreach (var link in item.Links)
                    {
                        if (link.RelationshipType == "enclosure" || 
                            (!string.IsNullOrWhiteSpace(link.MediaType) && link.MediaType.Contains("bittorrent")))
                        {
                            //"application/x-bittorrent"
                            string downloadUrl = link.Uri.ToString();

                            if (subscription.AlreadyAddedDownloadModel == null)
                            {
                                subscription.AlreadyAddedDownloadModel = new ObservableCollection<SubscriptionSubTaskModel> { };
                            }


                            //如果没有下载过
                            if (!subscription.AlreadyAddedDownloadModel.Any(a => a.Url.Contains(downloadUrl)))
                            {
                                try
                                {
                                    //TODO 开始下载

                                    var basePaths = WkyApiManager.Instance.GetUsbInfoDefPath();

                                    if (basePaths.Count == 0)
                                    {
                                        EasyLogManager.Logger.Error($"添加失败，没有获取到存储设备");
                                        return;
                                    }

                                    // subscription.Path 为基础目录，一般来说是带有标题的如“/download/订阅标题”
                                    var savePath = basePaths[0] + (subscription.Path.StartsWith("/") ? "" : "/") + subscription.Path;
                                    var episodeTitle = "";
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

                                    //如果是一个完整路径，则直接使用，否则使用旧逻辑
                                    bool isFullPath = basePaths.Any(a => subscription.Path.StartsWith(a));
                                    if (isFullPath)
                                    {
                                        savePath = subscription.Path;
                                    }

                                    if (!string.IsNullOrEmpty(episodeTitle))
                                    {
                                        savePath = savePath + "/" + PathHelper.RemoveInvalidChars(episodeTitle);
                                    }

                                    EasyLogManager.Logger.Info($"添加下载{subject} {link} {savePath}");

                                    var addResult = WkyApiManager.Instance.DownloadBtFileUrl(downloadUrl, subscription.Device, savePath).Result;

                                    var taskUrl = addResult?.Result?.Tasks?.FirstOrDefault()?.Url;
                                    if (!string.IsNullOrWhiteSpace(taskUrl))
                                    {
                                        TaskUrlToSubscriptionName[taskUrl] = subject;
                                    }


                                    if (addResult.SuccessCount > 0)
                                    {
                                        subscription.AlreadyAddedDownloadModel.Add(new SubscriptionSubTaskModel() { Name = subject, Url = downloadUrl, Time = DateTime.Now, Result = addResult.Result } );
                                        EasyLogManager.Logger.Info($"添加成功");
                                    }
                                    else if (addResult.DuplicateAddTaskCount > 0)
                                    {
                                        subscription.AlreadyAddedDownloadModel.Add(new SubscriptionSubTaskModel() { Name = subject, Url = downloadUrl, Time = DateTime.Now, Result = addResult.Result });
                                        EasyLogManager.Logger.Info($"成功，任务已经存在，不再重复添加");
                                    }
                                    else
                                    {
                                        EasyLogManager.Logger.Error($"添加失败");
                                        //下载失败
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
        }



        public void Load()
        {
            string fileName = @$"Subscription_{_user}.json";
            Debug.WriteLine($"准备载入{fileName}");
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
                            var taskUrl = m.Result?.Tasks?.FirstOrDefault()?.Url;
                            if (!string.IsNullOrWhiteSpace(taskUrl))
                            {
                                TaskUrlToSubscriptionName[taskUrl] = m.Name;
                            }
                                
                        }
                    }
                }

            }
        }


        public void Save()
        {
            Debug.WriteLine("保存订阅");
            string fileName = @$"Subscription_{_user}.json";
            var content = JsonConvert.SerializeObject(SubscriptionModel);
            File.WriteAllText(fileName, content);
        }

        //存储订阅，读取加载订阅

        public bool Add(string url, WkyDevice device, string path, string keyword = "", bool keywordIsRegex = false, bool autoDir = false)
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
            model.Device = device;
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
