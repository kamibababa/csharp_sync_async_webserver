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
        static async Task Main()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            Console.WriteLine("异步版本：Listening on http://localhost:5000/ ...");

            while (true)
            {
                var context = await listener.GetContextAsync();  // 异步等待请求
                //ThreadPool.SetMaxThreads(4, 4);
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    try
                    {
                        await HandleRequestAsync(context);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                });
            }
        }

        static async Task HandleRequestAsync(HttpListenerContext context)
        {
            var requestId = Guid.NewGuid().ToString();
            try
            {
                Console.WriteLine($"[异步处理] {requestId}请求到达，线程ID={Thread.CurrentThread.ManagedThreadId}, 时间={DateTime.Now:HH:mm:ss.fff}");
                // 模拟异步IO，不占线程
                await Task.Delay(1000);

                // 构造响应内容
                byte[] buffer = Encoding.UTF8.GetBytes("Async OK");

                // 写入响应体
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

                // 安全关闭响应
                context.Response.Close();
                Console.WriteLine($"[异步处理] {requestId}响应完成，线程ID={Thread.CurrentThread.ManagedThreadId}, 时间={DateTime.Now:HH:mm:ss.fff}");
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"HttpListener 异常: {ex.Message}");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("连接已关闭，不能写入");
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他错误: " + ex.Message);
            }
        }
    }

}
