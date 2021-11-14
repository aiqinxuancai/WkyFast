using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;


namespace WkyFast.Service
{
    public enum LogLevel { Trace, Debug, Info, Warn, Error }

    static class EasyLogManager
    {
        private static object flag = new object();

        private static string baseLogPath = @".\log\run.log";

        /// <summary>
        /// 如果日志大于10M则清除
        /// </summary>
        static EasyLogManager ()
        {
            if (Directory.Exists(Directory.GetCurrentDirectory() + @"\log") == false)
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\log");
            }
            

            FileInfo finfo = new FileInfo(baseLogPath);
            if (!finfo.Exists)
            {
                FileStream fs = File.Create(baseLogPath);
                fs.Close();
            }
            else
            {
                try
                {
                    if (finfo.Length > 1024 * 1024 * 20)
                    {
                        File.Delete(baseLogPath);
                        FileStream fs = File.Create(baseLogPath);
                        fs.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }


        public static void Write(object obj, LogLevel type = LogLevel.Info)
        {
            ThreadPool.QueueUserWorkItem(h =>
            {
                lock (flag)
                {
                    var file = baseLogPath; //AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyyy_MM_dd") + ".log";
                    string head = string.Format(">>>{0}[{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), type.ToString());
                    Debug.WriteLine(head + obj.ToString());
                    File.AppendAllText(file, head + obj.ToString() + "\r\n");

                    if (type == LogLevel.Info)
                    {
                        // 仅仅展示info型
                        //GlobalNotification.Default.Post(GlobalNotificationType.NotificationOutputLogInfo, string.Format("{0}", obj));
                    }
                    
                }
            });
        }


        public static void WriteInfo(object obj, params object[] args)
        {
            ThreadPool.QueueUserWorkItem(h =>
            {
                lock (flag)
                {
                    LogLevel type = LogLevel.Info;
                    var file = baseLogPath; //AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyyy_MM_dd") + ".log";

                    string head = string.Format(">>>{0}[{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),  type.ToString());

                    Debug.WriteLine(head + string.Format(obj.ToString(), args));
                    File.AppendAllText(file, head + string.Format(obj.ToString(), args) + "\r\n");

                    if (type == LogLevel.Info)
                    {
                        // 仅仅展示info型
                        //GlobalNotification.Default.Post(GlobalNotificationType.NotificationOutputLogInfo, string.Format(obj.ToString(), args));
                    }

                }
            });
        }

        public static void WriteDebug(object obj, params object[] args)
        {
            ThreadPool.QueueUserWorkItem(h =>
            {
                lock (flag)
                {
                    LogLevel type = LogLevel.Debug;
                    var file = baseLogPath; //AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyyy_MM_dd") + ".log";

                    string head = string.Format(">>>{0}[{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), type.ToString());

                    Debug.WriteLine(head + string.Format(obj.ToString(), args));
                    File.AppendAllText(file, head + string.Format(obj.ToString(), args) + "\r\n");
                }
            });
        }

        public static void WriteError(object obj, params object[] args)
        {
            ThreadPool.QueueUserWorkItem(h =>
            {
                lock (flag)
                {
                    LogLevel type = LogLevel.Error;
                    var file = baseLogPath; //AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyyy_MM_dd") + ".log";

                    string head = string.Format(">>>{0}[{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), type.ToString());

                    Debug.WriteLine(head + string.Format(obj.ToString(), args));
                    File.AppendAllText(file, head + string.Format(obj.ToString(), args) + "\r\n");
                }
            });
        }

    }
}
