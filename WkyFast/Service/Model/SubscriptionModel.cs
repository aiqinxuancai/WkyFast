using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Service.Model
{
    public class SubscriptionModel
    {
        /// <summary>
        /// 订阅地址
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 已经添加下载的URL
        /// </summary>
        public List<string> AlreadyAddedDownloadURL { get; set; } = new List<string>();
    }
}
