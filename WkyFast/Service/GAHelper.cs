using System;
using System.Collections.Generic;
using Flurl.Http;
using System.Threading.Tasks;
using WkyFast.Service;
using System.Diagnostics;

namespace WkyFast.Service
{
    /// <summary>
    /// 相关文档：
    /// GA4教程 https://firebase.google.com/codelabs/firebase_mp
    /// 测试 https://ga-dev-tools.google/ga4/event-builder/
    /// </summary>
    public class GAHelper
    {
        private static GAHelper instance = null;
        private static readonly object obj = new object();

        public static GAHelper Instance
        {
            get
            {
                lock (obj)
                {
                    if (instance == null)
                    {
                        instance = new GAHelper();
                    }
                    return instance;
                }
            }
        }

        private const string GAUrl = "https://www.google-analytics.com/mp/collect?api_secret=O5FOlVSiSmGmuGtj2xSZcQ&measurement_id=G-SDSDNLC0JV";
        private static readonly string cid = AppConfig.Instance.ConfigData.ClientId;
        public string UserAgent { get; set; }

        public GAHelper()
        {
            UserAgent = $"Google Analytics Tracker/1.1 ({Environment.OSVersion.Platform.ToString()}; {Environment.OSVersion.Version.ToString()}; {Environment.OSVersion.VersionString})";
        }

        public async Task RequestPageViewAsync(string page, string title = null)
        {
            try
            {
                if (page.StartsWith("/"))
                {
                    page = page.Remove(0, 1);
                }
                page = page.Replace("/", "_").Replace(".", "_");
                var values = new
                {
                    client_id = UserAgent,
                    user_id = cid,
                    non_personalized_ads = "false",
                    events = new List<object>
                    {
                        new
                        {
                            name = page,
                            @params = new
                            {
                                engagement_time_msec = "1",
                            },
                        }
                    },
                };
                var response = await GAUrl.PostJsonAsync(values);
                Debug.WriteLine(response.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GAHelper:" + ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public async Task Login()
        {
            try
            {
                var values = new
                {
                    client_id = UserAgent,
                    user_id = cid,
                    events = new List<object>
                    {
                        new
                        {
                            name = "login",
                            @params = new
                            {
                                method = "WkyFast",
                            },
                        }
                    },
                };
                var response = await GAUrl.PostJsonAsync(values);
                Debug.WriteLine(response.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GAHelper:" + ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public void RequestPageView(string page, string title = null)
        {
            Task.Run(() => RequestPageViewAsync(page, title));
        }
    }
}