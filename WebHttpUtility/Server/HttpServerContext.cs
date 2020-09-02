﻿using HttpServer.Http;
using HttpServer.Server.Resources;
using HttpServer.Tcp;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Server
{
    public class HttpServerContext
    {
        public ServerConfig ServerConfig { get; }
        public HttpResourceMapping ResourceMapping { get; }
        public HttpRequestDispatcher RequestDispatcher { get; }

        private TcpConnection tcpConnection;

        public HttpServerContext(ServerConfig config) {
            ServerConfig = config;
            ResourceMapping = new HttpResourceMapping(this);
            RequestDispatcher = new HttpRequestDispatcher(this);
            tcpConnection = new TcpConnection(this);
        }

        public void StartServer() {
            tcpConnection.StartListening();
        }

        public void StopServer() {
            tcpConnection.StopListening();
        }
    }
}
