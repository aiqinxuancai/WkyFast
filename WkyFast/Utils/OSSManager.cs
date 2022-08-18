using Aliyun.OSS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WkyFast.Service;

namespace WkyFast.Utils
{
    class OSSManager
    {

        private static OssClient CreateOssClient()
        {
            var client = new OssClient(AppConfig.ConfigData.OSSEndpoint, AppConfig.ConfigData.OSSAccessKeyId, AppConfig.ConfigData.OSSAccessKeySecret);
            return client;
        }

        public static string ReadFile(string fileName)
        {
            var objectName = fileName.Replace(@"/", "_");
            string ret = string.Empty;
            var client = CreateOssClient();
            try
            {
                //文件是否存在
                if (client.DoesObjectExist(AppConfig.ConfigData.OSSBucket, objectName))
                {
                    var oldData = client.GetObject(AppConfig.ConfigData.OSSBucket, objectName).Content;
                    StreamReader reader = new StreamReader(oldData);
                    string oldText = reader.ReadToEnd();
                    ret = oldText;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
            return ret;
        }

        public static Stream ReadFileStream(string fileName)
        {
            var objectName = fileName.Replace(@"/", "_");
            var client = CreateOssClient();
            try
            {
                //文件是否存在
                if (client.DoesObjectExist(AppConfig.ConfigData.OSSBucket, objectName))
                {
                    var oldData = client.GetObject(AppConfig.ConfigData.OSSBucket, objectName).Content;
                    StreamReader reader = new StreamReader(oldData);
                    reader.BaseStream.Position = 0;
                    return reader.BaseStream;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        /// <summary>
        /// 文字内容写出
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        public static void WriteFile(string fileName, string fileContent)
        {
            var objectName = fileName.Replace(@"/", "_");

            var client = CreateOssClient();
            try
            {
                byte[] array = Encoding.UTF8.GetBytes(fileContent);
                MemoryStream stream = new MemoryStream(array);
                client.PutObject(AppConfig.ConfigData.OSSBucket, objectName, stream);
                stream.Dispose();
 
            }
            catch (Exception ex)
            {
                
            }

        }


        /// <summary>
        /// 流文件写入
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        public static void WriteFile(string fileName, Stream stream)
        {
            var objectName = fileName.Replace(@"/", "_");

            var client = CreateOssClient();
            try
            {
                client.PutObject(AppConfig.ConfigData.OSSBucket, objectName, stream);
                stream.Dispose();

            }
            catch (Exception ex)
            {

            }

        }
    }
}
