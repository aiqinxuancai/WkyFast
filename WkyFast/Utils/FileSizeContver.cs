using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Utils
{
    internal class FileSizeContver
    {
        public static string GetSizeString(long size)
        {
            string strSize = "";
            long FactSize = size;
            if (FactSize < 1024.00)
                strSize = FactSize.ToString("F2") + "B";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                strSize = (FactSize / 1024.00).ToString("F2") + "K";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + "M";
            else if (FactSize >= 1073741824)
                strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + "G";
            return strSize;
        }
    }
}
