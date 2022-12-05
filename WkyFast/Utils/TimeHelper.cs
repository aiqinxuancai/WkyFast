using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WkyFast.Utils
{
    class TimeHelper
    {

        public static string SecondsToFormatString(int seconds)
        {
            TimeSpan ts = new TimeSpan(0, 0, seconds);
            string r = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
            return r;
        }
    }
}
