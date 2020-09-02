using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Server.Servlet
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ServletPathPatternAttribute : Attribute
    {

        // This is a positional argument
        public ServletPathPatternAttribute(string positionalString) {
            this.PathPattern = positionalString;
        }

        public string PathPattern { get; }
    }
}
