using HttpServer.Common;
using HttpServer.Http.Common;
using HttpServer.Server;
using HttpServer.Server.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Cookie = HttpServer.Http.Common.Cookie;

namespace HttpServer.Http.Response
{
    public sealed class HttpResponse : ResponseStream
    {
        readonly ServerVersion serverVersion;

        private HttpSession agent;
        private HttpStatusCode statusCode = HttpStatusCode.OK;
        private Dictionary<string, string> header;

        MemoryStream response = new MemoryStream();

        bool ShouldTerminate = false;

        public Cookie ResponseCookie { get; }
        public string ContentType { get; set; } = MIMETypeRegistry.DEFAULT_MIME;
        public string ContentCharset { get; set; } = string.Empty;

        internal HttpResponse(HttpSession agent, ServerConfig config) {
            this.agent = agent;
            serverVersion = config.SERVER_VERSION;
            header = new Dictionary<string, string>();
            ResponseCookie = new Cookie();

            RestoreDefaulHeader();
        }

        public void SetStatus(HttpStatusCode statusCode) {
            this.statusCode = statusCode;
        }

        public void SetTermination() {
            SetHeader(HeaderFields.Connection, "close");
            ShouldTerminate = true;
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
            base.Flush();
            
            // Write to client
            Flush();
        }

        public override void Flush() {

            if (agent.IsShutDown) return;

            CompleteResponseHeader();

            byte[] resp = Encoding.UTF8.GetBytes(BuildHeader());
            response.Write(resp);
            response.Write(ContentToByteArray());

            agent.MarkActiveOnce();
            agent.ActiveSocket.BeginSend(response.ToArray(), 0, (int)response.Length, 0, 
                new AsyncCallback(AsyncCallback), agent);

            base.Flush();
        }

        private void CompleteResponseHeader() {
            if (Length > 0) {
                SetHeader(HeaderFields.ContenLength, Length.ToString());
            }
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

        private void AsyncCallback(IAsyncResult result) {
            HttpSession conn = (HttpSession)result.AsyncState;
            conn.ActiveSocket.EndSend(result);
            if (ShouldTerminate) {
                conn.NaturalClose();
            }
        }

        private string BuildHeader() {
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

        public override void Dispose() {
            header.Clear();
            ResponseCookie.ClearAll();
            response.Dispose();
            base.Dispose();
        }

        public void RestoreDefaulHeader() {
            header.Clear();
            header.Add(HeaderFields.Server, serverVersion.ToString());
            header.Add(HeaderFields.Connection, "keep-alive");
        }

    }
}
