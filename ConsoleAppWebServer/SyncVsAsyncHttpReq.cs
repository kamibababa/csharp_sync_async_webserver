using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    const int RequestCount = 80;
    const string Url = "https://httpbin.org/delay/2"; // 每个请求延迟1秒返回

    static async Task Main()
    {
        ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);
        Console.WriteLine($"{availableWorkerThreads} {availableCompletionPortThreads}");
        //ThreadPool.SetMinThreads(100, 100);

        Console.WriteLine("【异步 HttpClient 测试】");
        await RunHttpClientAsync();

        //Console.WriteLine("\n【同步 WebRequest 测试】");
        //RunWebRequestSync();

        //Console.WriteLine("\n测试完成，按任意键退出...");
        //Console.ReadKey();
    }

    static async Task RunHttpClientAsync()
    {
        var client = new HttpClient();
        Stopwatch sw = Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < RequestCount; i++)
        {
            int id = i;
            tasks.Add(Task.Run(async () =>
            {
                Console.WriteLine($"[Async ] Task {id:D3} start @ {sw.Elapsed.TotalSeconds:0.000}s");
                ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);
                Console.WriteLine($"{availableWorkerThreads} {availableCompletionPortThreads}");
                try
                {
                    var result = await client.GetStringAsync(Url);
                    Console.WriteLine($"[Async ] Task {id:D3} done  @ {sw.Elapsed.TotalSeconds:0.000}s");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Async ] Task {id:D3} error: {ex.Message}");
                }
            }));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine($"[Async ] 所有任务完成，总耗时：{sw.Elapsed.TotalSeconds:0.000}s");
    }

    static void RunWebRequestSync()
    {
        Stopwatch sw = Stopwatch.StartNew();
        var tasks = new List<Task>();

        for (int i = 0; i < RequestCount; i++)
        {
            int id = i;
            tasks.Add(Task.Run(() =>
            {
                Console.WriteLine($"[Sync  ] Task {id:D3} start @ {sw.Elapsed.TotalSeconds:0.000}s");
                ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);
                Console.WriteLine($"{availableWorkerThreads} {availableCompletionPortThreads}");

                try
                {
                    var req = (HttpWebRequest)WebRequest.Create(Url);
                    using var resp = (HttpWebResponse)req.GetResponse(); // 同步阻塞
                    using var reader = new StreamReader(resp.GetResponseStream());
                    var result = reader.ReadToEnd();
                    Console.WriteLine($"[Sync  ] Task {id:D3} done  @ {sw.Elapsed.TotalSeconds:0.000}s");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Sync  ] Task {id:D3} error: {ex.Message}");
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"[Sync  ] 所有任务完成，总耗时：{sw.Elapsed.TotalSeconds:0.000}s");
    }
}
