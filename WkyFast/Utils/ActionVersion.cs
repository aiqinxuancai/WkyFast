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

        public const string Url = "https://api.github.com/repos/aiqinxuancai/WkyFast/releases";


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
                    JArray root = JArray.Parse(result);

                    foreach (JObject item in root)
                    {
                        if (!item.ContainsKey("draft") || (bool)item["draft"] == true)
                            continue;
                        if (!item.ContainsKey("prerelease") || (bool)item["prerelease"] == true)
                            continue;


                        if (item.ContainsKey("tag_name"))
                        {
                            string tagName = item["tag_name"].ToString();
                            NowGithubVersion = tagName;
                            if (Version != tagName)
                            {
                                //新版本
                                HasNewVersion = true;
                            }

                        }
                        break;

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
