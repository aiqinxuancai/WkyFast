using Aria2NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WkyFast.Service.Model
{


    public class TaskModel : BaseNotificationModel
    {
        //获取展示用的名字

        public DownloadStatusResult Data { set; get; }


        public bool FromSubscription
        {
            get
            {
                return SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Gid);
            }

        }

        /// <summary>
        /// 绑定的是此代码来显示名字
        /// </summary>
        public string ShowName {
            get
            {
                if (Data != null)
                {
                    if (Data.Bittorrent != null)
                    {
                        var name = Data.Bittorrent.Info?.Name!;
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = Data.InfoHash + ".torrent";
                        }
                        return name;
                    } 
                    else if (Data.Files != null && Data.Files.Count > 0)
                    {
                        return Data.Files[0].Uris.FirstOrDefault().Uri;
                    }
                    return Data.Gid;
                }

                return "";//SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Url);
            }
        }

        public string SubscriptionName
        {
            get
            {
                if (FromSubscription)
                {
                    var name = (string)SubscriptionManager.Instance.TaskUrlToSubscriptionName[Data.Gid];
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                    return ShowName;

                }
                else
                {
                    return ShowName;
                }
            }
        }

    
    }
}
