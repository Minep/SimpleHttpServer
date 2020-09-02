using HttpServer.Common;
using HttpServer.Http.Common;
using HttpServer.Http.Request.Parser;
using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HttpVersion = HttpServer.Common.HttpVersion;

namespace HttpServer.Http.Request
{
    public sealed class HttpRequest : RequestStream
    {
        private IPEndPoint endPoint;


        public HttpMethod Method { get; }
        public HttpVersion ProtocolVersion { get; }
        public string TargetURI { get; }
        public Common.Cookie RequestCookie { get; }
        public IPAddress RemoteClientAddress { get => endPoint?.Address; }
        public int RemoteClientPort
        {
            get {
                if (endPoint == null) return -1;
                return endPoint.Port;
            }
        }


        private Dictionary<string, string> headerFields;
        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        internal HttpRequest(HttpHeader header, HttpSession session)
        {
            Method = (HttpMethod)Enum.Parse(typeof(HttpMethod), header.Method);
            ProtocolVersion = HttpVersion.Parse(header.Protocol);
            headerFields = header.HeaderFields;

            RequestCookie = new Common.Cookie(GetHeaderField(HeaderFields.Cookie));

            string[] url_parts = header.TargetURL.Split('?');
            if (url_parts.Length == 2) {
                foreach (var item in URLParameterParser.GetURLParameter(url_parts[1])) {
                    parameters.Add(item.Key, item.Value);
                }
            }
            TargetURI = URLEncodeHelper.URLDecode(url_parts[0]);

            endPoint = session.ActiveSocket.RemoteEndPoint as IPEndPoint;
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
    }
}
