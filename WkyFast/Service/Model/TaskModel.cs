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

        public bool FromSubscription
        {
            get
            {
                return SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Url);
            }

        }

        public string ShowName { 
            get 
            { 
                if (FromSubscription)
                {
                    if (SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Url))
                    {
                        var name = (string)SubscriptionManager.Instance.TaskUrlToSubscriptionName[Data.Url];
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            return name;
                        }
                    }
                    return Data.Name;

                }
                else
                {
                    return Data.Name;
                }
            
            
            } 
        }

        public string SubscriptionName
        {
            get
            {
                if (FromSubscription)
                {
                    if (SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Name))
                    {
                        var name = (string)SubscriptionManager.Instance.TaskUrlToSubscriptionName[Data.Name];
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            return name;
                        }
                    }
                    return Data.Name;

                }
                else
                {
                    return Data.Name;
                }


            }
        }

        public WkyApiSharp.Service.Model.RemoteDownloadList.Task Data { get; set; }
    }
}
