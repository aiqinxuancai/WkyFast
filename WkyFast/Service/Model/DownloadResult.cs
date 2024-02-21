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
        public bool isSuccessed { get; set; }

        public string Gid { get; set; }


        public string InfoHash { get; set; }
        
    }
}
