namespace ConsoleAppTestClient
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    class DetailedLoadTester
    {
        static async Task Main()
        {
            //HttpClient client = new HttpClient();
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            int totalRequests = 100;       // 总请求数
            int concurrentRequests = 20;   // 并发数（同一时间最多20个请求）

            Console.WriteLine($"开始压力测试，总请求数={totalRequests}，并发数={concurrentRequests}");

            SemaphoreSlim semaphore = new SemaphoreSlim(concurrentRequests);

            Task<(int id, long elapsedMs, int threadId)>[] tasks = new Task<(int, long, int)>[totalRequests];

            for (int i = 0; i < totalRequests; i++)
            {
                int requestId = i + 1;
                await semaphore.WaitAsync();

                tasks[i] = Task.Run(async () =>
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    try
                    {
                        var response = await client.GetAsync("http://localhost:5000/");
                        response.EnsureSuccessStatusCode();

                        string content = await response.Content.ReadAsStringAsync();

                        sw.Stop();

                        int threadId = Thread.CurrentThread.ManagedThreadId;

                        Console.WriteLine($"请求 #{requestId} 响应耗时: {sw.ElapsedMilliseconds}ms, 线程ID: {threadId}, 内容长度: {content.Length}");

                        return (requestId, sw.ElapsedMilliseconds, threadId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"请求 #{requestId} 失败: {ex.Message}");
                        return (requestId, -1, -1);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }

            var results = await Task.WhenAll(tasks);

            AnalyzeResults(results);
        }

        static void AnalyzeResults((int id, long elapsedMs, int threadId)[] results)
        {
            Console.WriteLine();
            Console.WriteLine("==== 测试结果分析 ====");

            int successCount = 0;
            long totalTime = 0;
            int minTime = int.MaxValue;
            int maxTime = int.MinValue;

            var threadIdSet = new System.Collections.Generic.HashSet<int>();

            foreach (var r in results)
            {
                if (r.elapsedMs >= 0)
                {
                    successCount++;
                    totalTime += r.elapsedMs;
                    if (r.elapsedMs < minTime) minTime = (int)r.elapsedMs;
                    if (r.elapsedMs > maxTime) maxTime = (int)r.elapsedMs;
                    threadIdSet.Add(r.threadId);
                }
            }

            Console.WriteLine($"成功请求数：{successCount}/{results.Length}");
            Console.WriteLine($"平均响应时间：{(successCount > 0 ? (totalTime / successCount) : 0)} ms");
            Console.WriteLine($"最短响应时间：{minTime} ms");
            Console.WriteLine($"最长响应时间：{maxTime} ms");
            Console.WriteLine($"参与处理请求的线程数量（线程ID唯一值）：{threadIdSet.Count}");
            Console.WriteLine("=====================");
        }
    }

}
