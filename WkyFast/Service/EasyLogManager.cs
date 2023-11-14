using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using WkyFast.Utils;
using System.Diagnostics.Eventing.Reader;

namespace WkyFast.Service
{
    public class EasyLogManager
    {
        private static string _logFilename = "";

        public static SimpleLogger Logger { set; get; }

        /// <summary>
        /// 如果日志大于10M则清除
        /// </summary>
        static EasyLogManager ()
        {
            string machineName = Environment.MachineName;
            if (!string.IsNullOrWhiteSpace(machineName))
            {
                _logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "-" + machineName + ".log";
            } 
            else 
            {
                _logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".log";
            }
            

            if (File.Exists(_logFilename))
            {
                FileInfo finfo = new FileInfo(_logFilename);
                if (!finfo.Exists)
                {
                    FileStream fs = File.Create(_logFilename);
                    fs.Close();
                }
                else
                {
                    try
                    {
                        if (finfo.Length > 1024 * 1024 * 20)
                        {
                            File.Delete(_logFilename);
                            FileStream fs = File.Create(_logFilename);
                            fs.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            Logger = new SimpleLogger();


        }


    }
}
