
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
            请从我提供的内容中告诉我这个作品的名称，请返回如下JSON格式：
            { "title": <string> }
            请注意：
            1.不要添加解释。
            2.内容中可能包含多种语言的作品名称，通常可能会使用符号/进行分割，请返回第一种语言名称。
            3.请避免将字幕组名称及字幕名称等识别为标题。
            4.不要对作品名称进行删减或翻译以及字符的转换。
            5.作品名称不会包含包含[]【】等符号。
            以下为内容：

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

            if (!string.IsNullOrEmpty(AppConfig.Instance.ConfigData.OpenAIHost))
            {
                client.Settings.OpenAIAPIBaseUri = AppConfig.Instance.ConfigData.OpenAIHost;
            }

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

                    if (!string.IsNullOrEmpty(result.Response))
                    {
                        JObject root = JObject.Parse(result.Response);
                        var title = (string)root["title"];

                        //if (title.Contains("/"))
                        //{
                        //    title.Split("/");
                        //}


                        _cache[s] = title;
                        return title;
                    }

                    return string.Empty;
                }
                catch (Exception ex)
                {
                    EasyLogManager.Logger.Error(ex.ToString());
                }
            }
            return string.Empty;
        }

    }

}
