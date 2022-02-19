using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Service.Model
{





    public enum TaskState
    {
        Adding = 0,//0 => "添加中",
        Downloading = 1,//1 => "下载中",
        Waiting = 8,//8 => "等待中",
        Pause = 9,//9 => "已暂停",
        Completed = 11, //11 => "已完成",
        PreparingAdd//14 => "准备添加中",
    }

    public class TaskModel : BaseNotificationModel
    {
        public WkyApiSharp.Service.Model.RemoteDownloadList.Task Data { get; set; }
    }
}
