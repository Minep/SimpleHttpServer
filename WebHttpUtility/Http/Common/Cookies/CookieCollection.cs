using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Http.Common.Cookies
{
    public class CookieCollection
    {
        private Dictionary<string, Cookie> Cookies = new Dictionary<string, Cookie>();

        public int Count { get => Cookies.Count; }

        public CookieCollection(string CookieString) {
            if (CookieString.Length == 0) {
                return;
            }

            foreach (var item in CookieParser.ParseCookie(CookieString)) {
                Cookies.Add(item.Key, new Cookie(item.Key, item.Value));
            }
        }

        public CookieCollection() {

        }

        public void SetCookie(string cookie_name, Cookie value) {
            if (Cookies.ContainsKey(cookie_name)) {
                Cookies[cookie_name] = value;
            }
            else {
                Cookies.Add(cookie_name, value);
            }
        }

        public Cookie GetCookie(string cookie_name) {
            if (Cookies.ContainsKey(cookie_name)) {
                return Cookies[cookie_name];
            }
            return null;
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.AppendJoin(';', Cookies.Values);
            return builder.ToString();
        }

        public void ClearAll() {
            Cookies.Clear();
        }
    }
}
