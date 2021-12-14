using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WkyFast.Utils
{
   
    public class TaskHelper
    {

        public static void Sleep(int millisecondsToWait )
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                //some other processing to do STILL POSSIBLE
                if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
                {
                    break;
                }
                Thread.Sleep(1); //so processor can rest for a while
            }
        }

        public static void Sleep(int millisecondsToWait, int millisecondsTocycle)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                //some other processing to do STILL POSSIBLE
                if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
                {
                    break;
                }
                Thread.Sleep(millisecondsTocycle); //so processor can rest for a while
            }
        }

        
        public static void Sleep(int millisecondsToWait, int millisecondsTocycle, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                //some other processing to do STILL POSSIBLE
                if (stopwatch.ElapsedMilliseconds >= millisecondsToWait)
                {
                    break;
                }

                Thread.Sleep(millisecondsTocycle); //so processor can rest for a while

            }
        }
    }
}
