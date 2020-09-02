using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Common
{
    public struct HttpVersion
    {
        public int Major;
        public int Minor;

        public HttpVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public override string ToString() {
            return $"HTTP/{Major}.{Minor}";
        }

        public static HttpVersion Parse(string verString)
        {
            string[] vers = verString.Split('/');
            if (vers.Length == 2 && vers[0].Equals("HTTP"))
            {
                string[] versionNum = vers[1].Split('.');
                int mjr = 0, min = 0;
                if (int.TryParse(versionNum[0], out mjr)
                    && int.TryParse(versionNum[1], out min))
                {
                    return new HttpVersion(mjr, min);
                }
            }
            throw new FormatException("The version number must be \"HTTP/{uint}.{uint}\"");
        }

        public static readonly HttpVersion SERVER_VERSION = new HttpVersion(1, 1);
    }
}
