using HttpServer.Http.Common;
using HttpServer.Http.Request.Parser;
using HttpServer.Utils;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            program.PathVariable();

            stopwatch.Stop();
            Console.WriteLine("{0}ms", stopwatch.ElapsedMilliseconds);
        }


        void URL()
        {
            foreach (var item in URLParameterParser.GetURLParameter(
                "ie=utf-8&f=3&rsv_bp=1&rsv_idx=1&tn=baidu&wd=urlencode&" +
                "fenlei=256&rsv_pq=df2827be000003d5&rsv_t=6e74LdxfHX1BvOaupeu" +
                "sZD%2BGICCedwexKxmity3FXxie28zPpKfc5n%2FWDck&rqlang=cn&rsv_ente" +
                "r=1&rsv_dl=ts_0&rsv_sug3=8&rsv_sug1=11&rsv_sug7=101&rsv_sug2=0&r" +
                "sv_btype=i&prefixsug=urlen&rsp=0&inputT=4403&rsv_sug4=4404"))
            {
                Console.WriteLine("{0}:{1}", item.Key, item.Value);
            }
        }

        void PathVariable() {
            string patthen = "g/{id}/{extract}/{var}";
            string test =    "g/0/2343324/dcsdasccd-c-dsas-s";
            foreach (var item in PathURLExtractor.ExtractPathVariable(patthen,test)) {
                Console.WriteLine("{0}:{1}", item.Key, item.Value);
            }
        }

        void Header()
        {
            string region_1 =
                "GET / HTTP/1.1\r\n" +
                "Host: blog.lunaixsky.com\r\n" +
                "Connection: keep-alive\r\n" +
                "Cache-Control: max-age=0\r\n" +
                "Upgrade-Insecure-Requests: 1\r\n" +
                "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36";
            
            string region_2 = "\r\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9\r\n" +
                "Sec-Fetch-Site: none\r\n" +
                "Sec-Fetch-Mode: navigate\r\nSec-Fetch-User: ?1\r\nSec-Fetch-Dest: document\r\n" +
                "Accept-Encoding: gzip, deflate, br\r\n" +
                "Accept-Language: en-GB,en;q=0.9,zh-CN;q=0.8,zh;q=0.7,ru-RU;q=0.6,ru;q=0.5,en-US;q=0.4\r\n" +
                "Cookie: _ga=GA1.2.1727535589.1597313386; _gid=GA1.2.1552299765.1598851724; _gat_gtag_UA_175290221_1=1\r\n\r\n" +
                "<html>some html tags</html>";

            byte[] r1 = Encoding.ASCII.GetBytes(region_1);
            byte[] r2 = Encoding.ASCII.GetBytes(region_2);

            HeaderParser headerParser = new HeaderParser();
            ParserStatus status1 = headerParser.ProcessBuffer(r1,r1.Length);
            Console.WriteLine("Region 1 : {0}", status1);
            if(status1 == ParserStatus.REQUIRE_MORE)
            {
                status1 = headerParser.ProcessBuffer(r2,r2.Length);
                Console.WriteLine("Region 2 : {0}", status1);
            }

            if(status1 == ParserStatus.MOVE_NEXT_NEW_BUFFER || status1 == ParserStatus.MOVE_NEXT_SAME_BUFFER)
            {
                Console.WriteLine("Next State start pointer={0}", headerParser.BufferOffset);
                Console.WriteLine("Hint: {0}", region_2[headerParser.BufferOffset]);
                Console.WriteLine(headerParser.Content);
            }
        }
    }
}
