using HttpServer.Common;
using HttpServer.Http.Common;
using HttpServer.Http.Common.WebIO;
using HttpServer.Http.Request.Parser;
using HttpServer.Http.Session;
using HttpServer.Server;
using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using CookieCollection = HttpServer.Http.Common.Cookies.CookieCollection;
using HttpVersion = HttpServer.Common.HttpVersion;

namespace HttpServer.Http.Request
{
    public class HttpRequest
    {
        private IPEndPoint endPoint;


        public HttpMethod Method { get; }
        public HttpVersion ProtocolVersion { get; }
        public string TargetURI { get; }
        public string QueryString { get; } = string.Empty;
        public HttpSession Session { get; private set; }
        public CookieCollection RequestCookie { get; }
        public IPAddress RemoteClientAddress { get => endPoint?.Address; }
        public WebDataStream RequestContentStream { get; set; }
        public int RemoteClientPort
        {
            get {
                if (endPoint == null) return -1;
                return endPoint.Port;
            }
        }


        private Dictionary<string, string> headerFields;
        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        protected HttpRequest() {

        }

        internal HttpRequest(HttpHeader header, HttpConnection connection)
        {
            Method = (HttpMethod)Enum.Parse(typeof(HttpMethod), header.Method);
            ProtocolVersion = HttpVersion.Parse(header.Protocol);
            headerFields = header.HeaderFields;

            RequestCookie = new CookieCollection(GetHeaderField(HeaderFields.Cookie));

            string[] url_parts = header.TargetURL.Split('?');
            if (url_parts.Length == 2) {
                QueryString = url_parts[1];
                foreach (var item in URLParameterParser.GetURLParameter(url_parts[1])) {
                    parameters.Add(item.Key, item.Value);
                }
            }
            TargetURI = URLEncodeHelper.URLDecode(url_parts[0]);

            endPoint = connection.ActiveSocket.RemoteEndPoint as IPEndPoint;
            GetSession(connection.ServerContext);
        }

        internal void AddParameter(string paramName, string paramVal) {
            if (!parameters.ContainsKey(paramName)) {
                parameters.Add(paramName, paramVal);
            }
        }

        public string GetHeaderField(string field)
        {
            if (headerFields.ContainsKey(field))
            {
                return headerFields[field];
            }
            return string.Empty;
        }

        public string GetParameter(string paramName) {
            if (parameters.ContainsKey(paramName)) {
                return parameters[paramName];
            }
            return null;
        }

        public bool HasHeaderField(string field) {
            return headerFields.ContainsKey(field);
        }

        public T GetHeaderFieldAs<T>(string field)
        {
            if (headerFields.ContainsKey(field))
            {
                return (T)Convert.ChangeType(headerFields[field], typeof(T));
            }
            return default;
        }

        private void GetSession(HttpServerContext context) {
            string sessionID = GetHeaderField("Cookie_SessionField").Trim();
            Guid sid = Guid.Empty;
            if (!Guid.TryParse(sessionID, out sid)) {
                Session = context.SessionPool.GetSessionFromPool(Guid.Empty);
            }
            else {
                Session = context.SessionPool.GetSessionFromPool(sid);
            }
        }
    }
}
