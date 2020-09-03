using HttpServer.Common;
using HttpServer.Http.Common.WebIO;
using HttpServer.Server;
using HttpServer.Server.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using CookieCollection = HttpServer.Http.Common.Cookies.CookieCollection;

namespace HttpServer.Http.Response
{
    public sealed class HttpResponse : IDisposable
    {
        readonly ServerVersion serverVersion;

        private HttpServerContext serverContext;
        private HttpConnection agent;
        private HttpStatusCode statusCode = HttpStatusCode.OK;
        private Dictionary<string, string> header;

        bool ShouldTerminate = false;

        public CookieCollection ResponseCookie { get; }
        public string ContentType { get; set; } 
            = MIMETypeRegistry.DEFAULT_MIME;
        public string ContentCharset { get; set; } = string.Empty;

        public WebDataStream ResponseDataStream { get; set; }

        internal HttpResponse(HttpConnection agent) {
            this.agent = agent;
            this.serverContext = agent.ServerContext;
            
            serverVersion = serverContext.ServerConfig.SERVER_VERSION;
            header = new Dictionary<string, string>();
            ResponseCookie = new CookieCollection();
            ResponseDataStream = new ResponseStream();

            RestoreDefaulHeader();
        }

        public void SetStatus(HttpStatusCode statusCode) {
            this.statusCode = statusCode;
        }

        public void SetTermination() {
            SetHeader(HeaderFields.Connection, "close");
            ShouldTerminate = true;
        }

        public SimplePayloadStream CreateSimpleStreamWriter() {
            if (ResponseDataStream.IsChunkedStream) {
                ResponseDataStream.Dispose();
            }
            else {
                return ResponseDataStream as SimplePayloadStream;
            }
            return (ResponseDataStream = new ResponseStream()) as SimplePayloadStream;
        }

        public void SetHeader(string header_field, string content) {
            if (header.ContainsKey(header_field)) {
                header[header_field] = content;
            }
            else {
                header.Add(header_field, content);
            }
        }

        public string GetHeader(string header_field) {
            if (header.ContainsKey(header_field)) {
                return header[header_field];
            }
            return string.Empty;
        }

        public void SendError(HttpStatusCode errorCode) {
            if (agent.IsShutDown) return;
            RestoreDefaulHeader();
            statusCode = errorCode;
            SetTermination();
            // Discard all content in the buffer.
            ResponseDataStream.Flush();

            // Write header to client
            SendHeader();
        }

        public void Flush() {

            if (agent.IsShutDown) return;
            if (ResponseDataStream.IsChunkedStream) return;

            if (ResponseDataStream.Length > 0) {
                SetHeader(HeaderFields.ContenLength, ResponseDataStream.Length.ToString());
            }
            byte[] content = ResponseDataStream.ContentToByteArray();

            SendHeader();

            agent.ActiveSocket.BeginSend(content, 0, content.Length, 0,
                new AsyncCallback(ContentFlushOut), agent);

            ResponseDataStream.Flush();
        }

        public ChunkedStream GetChunckedOutputStream() {
            if (!ResponseDataStream.IsChunkedStream) {
                ResponseDataStream.Dispose();
            }
            else {
                return ResponseDataStream as ChunkedStream;
            }
            return (ResponseDataStream = new ChunckedResponseStream(agent, this)) as ChunkedStream;
        }

        internal void CompleteResponseHeader() {
            if (ResponseCookie.Count > 0) {
                SetHeader(HeaderFields.SetCookie, ResponseCookie.ToString());
            }
            if (ContentCharset.Length > 0) {
                SetHeader(HeaderFields.ContenType, $"{ContentType}; charset={ContentCharset}");
            }
            else {
                SetHeader(HeaderFields.ContenType, ContentType);
            }
            SetHeader(HeaderFields.Date, DateTime.Now.ToString("R"));
        }

        private void SendHeader() {
            CompleteResponseHeader();
            byte[] resp = Encoding.UTF8.GetBytes(GetValidHttpHeader());
            agent.MarkActiveOnce();

            agent.ActiveSocket.BeginSend(resp, 0, resp.Length, 0,
                new AsyncCallback(HeaderFlushOut), agent);
        }

        private void HeaderFlushOut(IAsyncResult result) {
            HttpConnection conn = (HttpConnection)result.AsyncState;
            conn.ActiveSocket.EndSend(result);
            if (ShouldTerminate && ResponseDataStream.Length == 0) {
                conn.NaturalClose();
            }
        }

        private void ContentFlushOut(IAsyncResult result) {
            HttpConnection conn = (HttpConnection)result.AsyncState;
            conn.ActiveSocket.EndSend(result);
            if (ShouldTerminate) {
                conn.NaturalClose();
            }
        }

        public string GetValidHttpHeader() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0} {1} {2}\r\n",
                HttpServer.Common.HttpVersion.SERVER_VERSION,
                (int)statusCode, statusCode.ToString());
            foreach(var item in header) {
                stringBuilder.AppendFormat("{0}:{1}\r\n", item.Key, item.Value);
            }
            stringBuilder.Append("\r\n");
            return stringBuilder.ToString();
        }

        public void Dispose() {
            ResponseDataStream.Dispose();
        }

        public void RestoreDefaulHeader() {
            header.Clear();
            header.Add(HeaderFields.Server, serverVersion.ToString());
            header.Add(HeaderFields.Connection, "keep-alive");
        }

    }
}
