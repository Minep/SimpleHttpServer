using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Http.Common.Cookies
{
    public class Cookie
    {
        private string content = string.Empty;
        public DateTime? Expire { get; set; } = null;
        public TimeSpan? MaxAge { get; set; } = null;
        public bool? IsHttpOnly { get; set; } = null;
        public bool? IsSessionCookie { get; set; } = null;

        private string Name { get; set; } = string.Empty;
        private string Path { get; set; } = string.Empty;
        private string Domain { get; set; } = string.Empty;
        public string Content 
        {
            get => content;
            set {
                content = URLEncodeHelper.URLEncode(value);
            }
        }

        public Cookie(string name, string content) {
            Name = name;
            this.content = content;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}={1}", Name, Content);
            if(IsSessionCookie.HasValue && !IsSessionCookie.Value) {
                if (MaxAge.HasValue) {
                    sb.AppendFormat("; Max-Age={0}", MaxAge.Value.TotalSeconds);
                }
                else if (Expire.HasValue) {
                    sb.AppendFormat("; Expire={0}", Expire.Value.ToString("R"));
                }
            }
            if(IsHttpOnly.HasValue && IsHttpOnly.Value) {
                sb.Append("; HttpOnly");
            }
            if (!string.IsNullOrEmpty(Path)) sb.Append($"; Path={Path}");
            if (!string.IsNullOrEmpty(Domain)) sb.Append($"; Domain={Domain}");
            return sb.ToString();
        }
    }
}
