using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Http.Request.Parser
{
    public class HttpHeader
    {
        public string Method { get; set; }
        public string TargetURL { get; set; }
        public string Protocol { get; set; }

        public Dictionary<string, string> HeaderFields { get; } = new Dictionary<string, string>();

#if DEBUG
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Method: {Method}");
            sb.AppendLine($"TargetURL: {TargetURL}");
            sb.AppendLine($"Protocol: {Protocol}");
            foreach (var item in HeaderFields) {
                sb.AppendLine($"{item.Key} = {item.Value}");
            }
            return sb.ToString();
        }
#endif
    }
}
