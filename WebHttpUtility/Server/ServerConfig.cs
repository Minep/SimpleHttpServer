using HttpServer.Common;
using HttpServer.Server.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Server
{
    public class ServerConfig
    {

        public readonly ServerVersion SERVER_VERSION
                        = new ServerVersion("MyServer", "Lunaixsky", 0, 1);

        public string DocumentRoot { get; set; } = Environment.CurrentDirectory;

        public int MaxContentSizeByte { get; set; } = 10 * 1024 * 1024;

        /// <summary>
        /// Maximum bytes of a resource such that needed to be transferred as 
        /// chunked. Set to negative number to disable chunked transfer mode.
        /// </summary>
        public int ChunkedModeThershold { get; set; } = 2 * 1024 * 1024;
        
        /// <summary>
        /// Maximum bytes of data in each chunk 
        /// </summary>
        public int MaximumChunkedUnitSize { get; set; } = 2 * 1024;

        public double IdleConnectionCleanUpInterval { get; set; } = 4000;   //ms
        public double DeadConnectionCleanUpInterval { get; set; } = 4000;   //ms
        public double CleanUpThersholdRatio { get; set; } = 0.8;    // 0-1
        public int MaxCleanUpNumber { get; set; } = 5;
        public int MaxConnectionAccepted { get; set; } = 100;
        public int SessionMaxInactiveMinutes { get; set; } = 15;

        public ResourceLocator NotFound404Page { get; set; }
        public ResourceLocator InternalError500Page { get; set; }
        public ResourceLocator Foridden403Page { get; set; }
        public ResourceLocator WelcomePage { get; set; }

        public int ServerPort { get; set; } = 8080;
        public string ServerIP { get; set; } = "127.0.0.1";
    }
}
