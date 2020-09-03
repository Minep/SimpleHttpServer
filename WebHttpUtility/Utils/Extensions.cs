using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Utils
{
    public static class Extensions
    {
        public static readonly DateTime EPOCH =
            new DateTime(1970, 1, 1, 0, 0, 0);
        public static long GetUnixTime(this DateTime dateTime) {
            return (long)(dateTime - EPOCH).TotalSeconds;
        }
    }
}
