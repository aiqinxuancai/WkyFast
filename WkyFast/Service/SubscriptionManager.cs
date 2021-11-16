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
        private Task _task;

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
                Start();
            }
        }

        public List<SubscriptionModel> SubscriptionModel { get; set; } = new List<SubscriptionModel>();



        public void Start()
        {
            if (_task != null)
            {
                //停止任务
            }
            Load();
        }


        public void TimerFunc()
        {
            foreach (var subscription in SubscriptionModel)
            {
                string url = subscription.Url;
                XmlReader reader = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();

                subscription.Name = feed.Title.Text;
                foreach (SyndicationItem item in feed.Items)
                {
                    string subject = item.Title.Text;
                    string summary = item.Summary.Text;
                    foreach (SyndicationElementExtension extension in item.ElementExtensions)
                    {
                        XElement ele = extension.GetObject<XElement>();
                        foreach (XElement node in ele.Nodes())
                        {
                            Debug.WriteLine(node.Name.LocalName + " " + node.Value);
                            if (node.Name.LocalName == "link")
                            {
                                string downloadUrl = node.Value;
                                //如果没有下载过
                                if (!subscription.AlreadyAddedDownloadURL.Any(a => a.Contains(downloadUrl)))
                                {
                                    //TODO 开始下载
                                    if (WkyApiManager.Instance.DownloadBtFileUrl(downloadUrl, subscription.Path).Result)
                                    {
                                        subscription.AlreadyAddedDownloadURL.Add(downloadUrl);
                                    }
                                    else
                                    {
                                        //下载失败
                                    }
                                }
                            }
                        }

                    }

                }
            }
        }



        public void Load()
        {
            string fileName = @$"Subscription_{_user}.json";
            if (File.Exists(fileName))
            {
                SubscriptionModel.Clear();
                List<SubscriptionModel> subscriptionModel = JsonConvert.DeserializeObject<List<SubscriptionModel>>(File.ReadAllText(fileName));
                SubscriptionModel.AddRange(subscriptionModel);
            }
        }


        public void Save()
        {
            string fileName = @$"Subscription_{_user}.json";
            var content = JsonConvert.SerializeObject(SubscriptionModel);
            File.WriteAllText(fileName, content);
        }

        //存储订阅，读取加载订阅

        public bool Add(string url)
        {
            if (SubscriptionModel.Find( a => { return a.Url == url; }) != null)
            {
                //找到了存在相同
                return false;
            }

            SubscriptionModel model = new SubscriptionModel();
            model.Url = url;
            SubscriptionModel.Add(model);
            Save();
            return true;
        }

        public void Remove(string url)
        {
            SubscriptionModel.RemoveAll(a =>  a.Url == url);
        }

    }
}
