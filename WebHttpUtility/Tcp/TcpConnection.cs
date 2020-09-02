using HttpServer.Common;
using HttpServer.Server;
using HttpServer.tcp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HttpServer.Tcp
{
    public class TcpConnection
    {
        private Socket listeningSocket;

        private IPAddress address;
        private IPEndPoint endPoint;

        public static ManualResetEvent done = new ManualResetEvent(false);

        public ConnectionPooling connectionPool;
        private HttpServerContext serverContext;

        private bool listenLoop = false;

        private Thread ListenThreading;

        public TcpConnection(HttpServerContext serverContext)
        {
            this.serverContext = serverContext;
            connectionPool = new ConnectionPooling(serverContext);

            address = IPAddress.Parse(serverContext.ServerConfig.ServerIP);
            endPoint = new IPEndPoint(address, serverContext.ServerConfig.ServerPort);
            listeningSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            listeningSocket.Bind(endPoint);

            ListenThreading = new Thread(new ThreadStart(Loop));
        }

        public void StartListening()
        {
            listenLoop = true;
            connectionPool.StartChecking();
            ListenThreading.Start();
        }

        public void StopListening() {
            listenLoop = false;
            done.Set();
            connectionPool.StopChecking();
        }

        private void Loop() {
            listeningSocket.Listen(50);
            Console.WriteLine("Server listened on {0}:{1}", endPoint.Address, endPoint.Port);
            while (listenLoop) {
                done.Reset();

                listeningSocket.BeginAccept(new AsyncCallback(AcceptedCallback), listeningSocket);

                done.WaitOne();
            }
            listeningSocket.Close();
        }

        private void AcceptedCallback(IAsyncResult result)
        {
            done.Set();
            Console.WriteLine("Connection Received");
            Socket listenerSocket = (Socket)result.AsyncState;
            Socket workingSocekt = listenerSocket.EndAccept(result);

            HttpSession connection = new HttpSession(workingSocekt, serverContext, new Http.HttpHandler());
            connectionPool.PutConnection(connection);

            workingSocekt.BeginReceive(
                 connection.ConnectionBuffer, 0, HttpSession.BUFFER_SIZE, 0,
                 new AsyncCallback(connection.SessionHanlder.ReadCallback), connection);
        }
    }
}
