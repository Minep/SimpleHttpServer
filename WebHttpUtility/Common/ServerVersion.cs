using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Common
{
    public struct ServerVersion
    {
        public string ServerName;
        public string ProductCode;
        public int Major;
        public int Minor;

        public ServerVersion(string serverName, string productCode, int major, int minor) {
            ServerName = serverName;
            ProductCode = productCode;
            Major = major;
            Minor = minor;
        }

        public override string ToString() {
            return $"{ServerName}/{Major}.{Minor} ({ProductCode})";
        }
    }
}
