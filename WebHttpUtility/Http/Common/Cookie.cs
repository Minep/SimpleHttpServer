using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Http.Common
{
    public class Cookie
    {
        private Dictionary<string, string> Cookies = new Dictionary<string, string>();

        public int Count { get => Cookies.Count; }

        public Cookie(string CookieString) {
            if (CookieString.Length == 0) {
                return;
            }

            foreach (var item in CookieParser.ParseCookie(CookieString)) {
                Cookies.Add(item.Key, item.Value);
            }
        }

        public Cookie() {

        }

        public void SetCookie(string cookie_name, string value) {
            if (Cookies.ContainsKey(cookie_name)) {
                Cookies[cookie_name] = value;
            }
            else {
                Cookies.Add(cookie_name, value);
            }
        }

        public string GetCookie(string cookie_name) {
            if (Cookies.ContainsKey(cookie_name)) {
                return Cookies[cookie_name];
            }
            return string.Empty;
        }

        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            foreach (var item in Cookies) {
                builder.Append($"{item.Key}={item.Value};");
            }
            return builder.ToString();
        }

        public void ClearAll() {
            Cookies.Clear();
        }
    }
}
