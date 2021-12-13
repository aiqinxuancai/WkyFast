using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Service.Model
{
    public class SubscriptionModel : BaseNotificationModel
    {
        /// <summary>
        /// 订阅地址
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 存储路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 订阅名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 过滤条件
        /// </summary>
        public string Filter { get; set; } = string.Empty;

        /// <summary>
        /// 过滤条件是否是正则
        /// </summary>
        public bool IsFilterRegex { get; set; } = false;

        /// <summary>
        /// 已经添加下载的URL
        /// </summary>
        public ObservableCollection<SubscriptionSubTaskModel> AlreadyAddedDownloadModel { get; set; } = new ObservableCollection<SubscriptionSubTaskModel>();
    }


    public class SubscriptionSubTaskModel : BaseNotificationModel
    {
        /// <summary>
        /// 订阅地址
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 订阅名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
