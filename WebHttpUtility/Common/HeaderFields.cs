using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Common
{
    public static class HeaderFields
    {
        public const string ContenType = "Content-Type";
        public const string ContenLength = "Content-Length";
        public const string Expect = "Expect";
        public const string ExpectVal = "100-continue";
        public const string Server = "Server";
        public const string Date = "Date";
        public const string Allow = "Allow";
        public const string Cookie = "Cookie";
        public const string SetCookie = "Set-Cookie";
        public const string Connection = "Connection";
        public const string TransferEncoding = "Transfer-Encoding";
        public const string CacheControl = "Cache-Control";
        public const string CacheControl_NoCahe = "no-cache";

        public const string Cookie_SessionField = "LNAXSID";
    }
}
