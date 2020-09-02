using HttpServer.Server;
using HttpServer.Server.Resources;
using ServerTest.Servlets;
using System;

namespace ServerTest
{
    class Program
    {
        static void Main(string[] args) {
            Program program = new Program();
            program.ApplyConfig();
            program.Init();
            program.Run();

            Console.ReadKey();
            program.Terminate();
        }

        ServerConfig serverConfig;
        HttpServerContext serverContext;
        public void ApplyConfig() {

            HttpResource notfound = new HttpResource(ResourceMappingType.FILE);
            notfound.BasePath = $"{Environment.CurrentDirectory}/web/404.html";
            notfound.MIMEType = "text/html";
            

            serverConfig = new ServerConfig();
            serverConfig.DocumentRoot = $"{Environment.CurrentDirectory}/web/";
            serverConfig.MaxCleanUpNumber = 5;
            serverConfig.MaxConnectionAccepted = 100;
            serverConfig.ServerPort = 8080;
            serverConfig.DeadConnectionCleanUpInterval = 4000;
            serverConfig.IdleConnectionCleanUpInterval = 4000;
            serverConfig.NotFound404Page
                = new ResourceLocator("", "", notfound);
        }


        public void Init() {
            serverContext = new HttpServerContext(serverConfig);

            serverContext.ResourceMapping.RegisterServlet<ExampleServlet>();
            serverContext.ResourceMapping.RegisterServlet<RESTfulServlet>();
        }

        public void Run() {
            serverContext.StartServer();
            Console.WriteLine("Started");
        }

        public void Terminate() {
            serverContext.StopServer();
            Console.WriteLine("Terminated");
        }
    }
}
