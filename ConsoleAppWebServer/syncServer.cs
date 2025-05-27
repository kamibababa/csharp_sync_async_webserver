using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppWebServer
{
    using System;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static void Main()
        {

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            Console.WriteLine("同步版本：Listening on http://localhost:5000/ ...");

            while (true)
            {
                var context = listener.GetContext();  // **同步阻塞等待请求**
                //ThreadPool.SetMaxThreads(4, 4);
                // 用线程池处理请求
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        HandleRequestSync(context);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                });
            }
        }

        static void HandleRequestSync(HttpListenerContext context)
        {
            var requestId = Guid.NewGuid().ToString();
            Console.WriteLine($"[同步处理]{requestId} 请求到达，线程ID={Thread.CurrentThread.ManagedThreadId}, 时间={DateTime.Now:HH:mm:ss.fff}");

            // 模拟同步阻塞数据库查询（耗时1秒）
            Thread.Sleep(1000);

            string responseString = "{\"data\":\"这是同步模拟数据库返回的数据\"}";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;

            context.Response.OutputStream.Write(buffer, 0, buffer.Length); // 同步写响应
            context.Response.OutputStream.Close();

            Console.WriteLine($"[同步处理]{requestId} 响应完成，线程ID={Thread.CurrentThread.ManagedThreadId}, 时间={DateTime.Now:HH:mm:ss.fff}");
        }
    }

}
