using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Service.Model
{
    public class DownloadResult
    {
        //是否成功添加
        public bool Result { get; set; }

        //是否是添加重复任务
        public bool isDuplicateAddTask { get; set; }
    }
}
