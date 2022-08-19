using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace WkyFast.Utils
{
    public class ActionVersion
    {
        //版本号完整样例
        //v0.1.1.33
        public const string Version = "{VERSION}";

        public const string Build = "{BUILD}";

        public const string Url = "https://api.github.com/repos/aiqinxuancai/WkyFast/releases/latest";


        //检测最后的build，如果不符合，则
        public static string NowGithubVersion { set; get; } = "";

        public static bool HasNewVersion { set; get; }

        public static async Task CheckVersion ()
        {
            try
            {

                var result = await Url.WithTimeout(20)
                    .WithHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36")
                    .GetStringAsync();

                if (!string.IsNullOrWhiteSpace(result))
                {
                    JObject root = JObject.Parse(result);
                    if (root.ContainsKey("tag_name"))
                    {
                        string tagName = root["tag_name"].ToString();

                        NowGithubVersion = tagName;

                        if (Version != tagName)
                        {
                            //新版本
                            HasNewVersion = true;
                        }

                    }


                }
            } 
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


        }


    }
}
