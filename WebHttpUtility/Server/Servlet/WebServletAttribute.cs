using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Server.Servlet
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class WebServletAttribute : Attribute
    {
        public WebServletAttribute(string name) {
            this.Name = name;

        }

        public string Name { get; }

        public string Path { get; set; } = string.Empty;

        internal string GetMappingRoute() {
            if (Path.EndsWith('/')) {
                Path = Path.Substring(0, Path.Length - 1);
            }
            return $"{Path}/{Name}";
        }
    }
}
