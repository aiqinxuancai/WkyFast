
using ChatGPTSharp;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using WkyFast.Utils;

namespace WkyFast.Service
{
    internal class ChatGPTTranslatorManager
    {
        const string kSystemMessage = """
            请从指定文本中返回作品名称，请返回如下JSON格式：
            { "title": <string> }
            请注意：
            1.不要添加解释。
            2.如果包含多种语言的作品名称，请使用第一个。
            3.请避免将字幕组名称及字幕名称等识别为标题。
            以下为需要提取的内容：

            """;

        static Dictionary<string, string> _cache = new Dictionary<string, string>();

        /// <summary>
        /// 完整调用一次提取episode
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static async Task<string> GetEpisode(string s)
        {
            var client = new ChatGPTClient(AppConfig.Instance.ConfigData.OpenAIKey, timeoutSeconds: 60, proxyUri: AppConfig.Instance.ConfigData.OpenAIProxy);

            if (client != null)
            {
                try
                {
                    s = $"{kSystemMessage}{s}";

                    if (_cache.TryGetValue(s, out var r))
                    {
                        return r;
                    }

                    var result = await client.SendMessage(s);

                    if (!string.IsNullOrEmpty( result.Response))
                    {
                        JObject root = JObject.Parse(result.Response);
                        var title = string.IsNullOrEmpty((string)root["title"]) ? "" : result.Response;
                        _cache[s] = title;
                        return title;
                    }

                    return string.Empty;
                }
                catch (Exception ex)
                {
                    //SimpleLogger.Instance.Error(ex.ToString());
                }
            }
            return string.Empty;
        }

    }

}
