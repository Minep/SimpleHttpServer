using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Utils
{
    public class BasicUtils
    {
        public static bool IsHex(char currentChar) {
            if (char.IsDigit(currentChar)
                || 'A' <= currentChar && currentChar <= 'F'
                || 'a' <= currentChar && currentChar <= 'f') {
                return true;
            }
            return false;
        }
    }
}
