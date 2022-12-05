using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WkyFast.Service;

namespace WkyFast.Utils
{
    internal class PushDeer
    {

        public static async Task SendPushDeer(string title, string msg = "")
        {

            if (string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(AppConfig.ConfigData.PushDeerKey))
            {
                return;
            }

            Console.WriteLine($"SendPushDeer {msg}");


            HttpClient client = new HttpClient();

            try
            {
                var url = "https://api2.pushdeer.com/message/push";

                var postData = new FormUrlEncodedContent(new Dictionary<string, string>()
                    {
                        {"pushkey" , AppConfig.ConfigData.PushDeerKey },
                        {"text" , title },
                        {"desp" , msg },
                    });

                var ret = await client.PostAsync(url, postData);

                ret.EnsureSuccessStatusCode();
                Console.WriteLine(await ret.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
