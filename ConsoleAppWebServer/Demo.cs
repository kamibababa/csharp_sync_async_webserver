//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ConsoleAppWebServer
//{
//    public class Demo
//    {
//        static async Task<string> GetString()
//        {
//            HttpClient client = new HttpClient();
//            string url = "https://httpbin.org/delay/1";
//            string content = await client.GetStringAsync(url);
//            return content;
//        }
//        static async Task Main(string[] args)
//        {
//            string msg = await GetString();
//            await Console.Out.WriteLineAsync(msg);
//        }
//    }
//}
