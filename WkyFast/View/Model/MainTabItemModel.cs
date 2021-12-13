using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.View.Model
{

    public enum MainTabItemModelType
    {
        DownloadList,
        SubscriptionList,
    }

    public class MainTabItemModel
    {
        public string Title {set; get;}

        public MainTabItemModelType Type { set; get; }
    }
}
