using HttpServer.Http;
using HttpServer.Http.Response;
using HttpServer.Server;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace HttpServer.Common
{
    public class HttpSession : IComparable, IDisposable
    {
        public const int BUFFER_SIZE = 1024;
        public int ActiveAcc { get; private set; }
        public DateTime CreatedTimestamp { get; private set; }
        public bool IsShutDown { get; private set; }
        public Socket ActiveSocket { get; }
        public byte[] ConnectionBuffer = new byte[BUFFER_SIZE];
        public HttpResponse Response { get; }
        public HttpServerContext ServerContext { get; }
        public HttpHandler SessionHanlder { get; }

        public HttpSession(Socket activeSocket, HttpServerContext ServerContext, HttpHandler SessionHanlder) {
            ActiveSocket = activeSocket;
            CreatedTimestamp = DateTime.Now;

            this.ServerContext = ServerContext;
            this.SessionHanlder = SessionHanlder;
            Response = new HttpResponse(this, ServerContext.ServerConfig);
        }

        public void MarkActiveOnce() {
            ActiveAcc++;
        }

        public void ResetAccumulator() {
            ActiveAcc = 0;
        }

        public void NaturalClose() {
            if (IsShutDown) return;
            IsShutDown = true;
            ActiveSocket.Close();
            Response.Dispose();
        }

        private double CalculatePriority() {
            if (IsShutDown) return -1;
            double P = ActiveAcc / (DateTime.Now - CreatedTimestamp).TotalSeconds * 10;
            ActiveAcc = 0;
            return P;
        }

        public int CompareTo(object obj) {
            return CalculatePriority().CompareTo(obj);
        }

        public void Dispose() {
            if (!IsShutDown) {
                ActiveSocket.Close();
                IsShutDown = true;
                Response.Dispose();
            }
        }
    }
}
