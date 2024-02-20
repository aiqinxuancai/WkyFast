using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Service.Model
{
    public class WkyDownloadResult
    {
        //返回值非0或网络请求有异常
        public bool hasError { get; set; }

        //总量
        public int AllTaskCount { get; set; }

        //成功添加的数量
        public int SuccessCount { get; set; }

        //已经存在的数量
        public int DuplicateAddTaskCount { get; set; }

    }
}
