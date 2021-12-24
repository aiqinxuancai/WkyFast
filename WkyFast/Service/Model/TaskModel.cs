using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Service.Model
{
    public class TaskModel : BaseNotificationModel
    {
        public WkyApiSharp.Service.Model.RemoteDownloadList.Task Data { get; set; }
    }
}
