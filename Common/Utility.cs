using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EvilComic.Common
{
    public static class Utility
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static Encoding DefaultEncoding { get; } = CodePagesEncodingProvider.Instance.GetEncoding("gb2312");

        public static async Task<byte[]> LoadResource(Uri Location)
        {
            HttpWebRequest request = WebRequest.Create(Location) as HttpWebRequest;
            request.Method = "GET";
            request.Accept = "image/webp,image/*,*/*;q=0.8";
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,GBK;q=0.2");
            request.KeepAlive = true;
            request.Headers.Add("DNT", "1");
            //request.Referer = Referer.ToString();
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/46.0.2490.86 Safari/535.1";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //request.Proxy = new WebProxy("127.0.0.1", 8888);
            request.Proxy = new WebProxy();

            Stream stream = null;
            byte[] DataBuff = null;
            var AsyncContext = request.BeginGetResponse((IAsyncResult Result) =>
            {

            }, null);

            RegisteredWaitHandle Wait = null;
            Wait = ThreadPool.RegisterWaitForSingleObject(AsyncContext.AsyncWaitHandle, (object state, bool timeout) =>
            {
                if (timeout)
                    request.Abort();
                Wait.Unregister(AsyncContext.AsyncWaitHandle);
                AsyncContext.AsyncWaitHandle.Close();
            }, null, 20 * 1000, true);//超时时间为20s

            AsyncContext.AsyncWaitHandle.WaitOne();
            HttpWebResponse Response = null;
            try
            {
                Response = request.EndGetResponse(AsyncContext) as HttpWebResponse;
                stream = Response.GetResponseStream();
                using (var ms = new MemoryStream())
                {
                    byte[] buff = new byte[1024];
                    while (true)
                    {
                        int num = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);
                        if (num == 0)
                            break;
                        ms.Write(buff, 0, num);
                    }
                    DataBuff = ms.ToArray();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                Response?.Dispose();
                stream?.Dispose();
            }

            if (DataBuff == null)
                throw new TimeoutException();
            return DataBuff;
        }

        public static byte[] LoadResource(Uri Location, int RetryTimes)
        {
            byte[] buff = null;
            for (int i = 0; i != RetryTimes && buff == null; i++)
            {
                try
                {
                    var t = LoadResource(Location);
                    t.Wait();
                    buff = t.Result;
                }
                catch
                {

                }
            }
            if (buff == null)
                throw new TimeoutException();
            return buff;
        }
    }
}
