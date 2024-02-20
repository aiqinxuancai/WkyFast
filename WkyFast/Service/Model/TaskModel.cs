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
                return false;//SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Url);
            }

        }

        /// <summary>
        /// 绑定的是此代码来显示名字
        /// </summary>
        public string ShowName {
            get
            {
                return "";//SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Url);
            }
            //get 
            //{ 
            //    if (FromSubscription)
            //    {
            //        if (SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Url))
            //        {
            //            var name = (string)SubscriptionManager.Instance.TaskUrlToSubscriptionName[Data.Url];
            //            if (!string.IsNullOrWhiteSpace(name))
            //            {
            //                return name;
            //            }
            //        }
            //        return Data.Name;

            //    }
            //    else
            //    {
            //        return Data.Name;
            //    }


            //} 
        }

        public string SubscriptionName
        {
            get
            {
                return "";//SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Url);
            }
            //get
            //{
            //    if (FromSubscription)
            //    {
            //        if (SubscriptionManager.Instance.TaskUrlToSubscriptionName.ContainsKey(Data.Name))
            //        {
            //            var name = (string)SubscriptionManager.Instance.TaskUrlToSubscriptionName[Data.Name];
            //            if (!string.IsNullOrWhiteSpace(name))
            //            {
            //                return name;
            //            }
            //        }
            //        return Data.Name;

            //    }
            //    else
            //    {
            //        return Data.Name;
            //    }


            //}
        }

    
    }
}
