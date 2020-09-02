using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Utils
{
    public class URLEncodeHelper
    {
        const int START = 0, HEX1 = 1, HEX2 = 2, ERROR = -1;
        static readonly char[] HexLookUpTable = new char[]
        {
            '0','1','2','3','4','5','6','7','8','9',
            'A','B','C','D','E','F'
        };
        public static string URLDecode(string url) {
            StringBuilder stringBuilder = new StringBuilder();
            int currentState = START;
            using(MemoryStream ms = new MemoryStream()) {
                int ptr = 0;
                byte b = 0;
                char chr;
                while (ptr < url.Length && currentState != ERROR) {
                    chr = url[ptr];
                    switch (currentState) {
                        case START:
                            if (chr == '%') currentState = HEX1;
                            else {
                                if (ms.Length > 0) {
                                    stringBuilder.Append(Encoding.UTF8.GetString(ms.ToArray()));
                                    ms.Position = 0;
                                    ms.SetLength(0);
                                }
                                stringBuilder.Append(chr);
                            }
                            break;
                        case HEX1:
                            if (isHex(chr)) {
                                b = (byte)(b | (HexDigitToByte(chr) << 4));
                                currentState = HEX2;
                            }
                            else currentState = ERROR;
                            break;
                        case HEX2:
                            if (isHex(chr)) {
                                b = (byte)(b | (HexDigitToByte(chr)));
                                ms.WriteByte(b);
                                b = 0;
                                currentState = START;
                            }
                            else currentState = ERROR;
                            break;
                        default:
                            break;
                    }
                    ptr++;
                }
                if (currentState == ERROR) return url;
                if (ms.Length > 0) {
                    stringBuilder.Append(Encoding.UTF8.GetString(ms.ToArray()));
                    ms.Position = 0;
                    ms.SetLength(0);
                }
                return stringBuilder.ToString();
            }
        }

        public static string URLEncode(string url) {
            StringBuilder newUrl = new StringBuilder();
            StringBuilder invalidChars = new StringBuilder();
            int ptr = 0;
            byte[] buffer;
            char chr;
            while (ptr < url.Length) {
                chr = url[ptr];
                if (chr <= byte.MaxValue) {
                    if (invalidChars.Length > 0) {
                        buffer = Encoding.UTF8.GetBytes(invalidChars.ToString());
                        foreach (byte item in buffer) {
                            newUrl.AppendFormat("%{0}{1}",
                            HexLookUpTable[(item & 0xf0) >> 4],
                            HexLookUpTable[item & 0x0f]);
                        }
                        invalidChars.Clear();
                    }
                    if (('a' > chr || chr > 'z') && ('A' > chr || chr > 'Z') && chr != '_' &&
                        ('0' > chr || chr > '9')) {
                        newUrl.AppendFormat("%{0}{1}",
                            HexLookUpTable[(chr & 0xf0) >> 4],
                            HexLookUpTable[chr & 0x0f]);
                    }
                    else newUrl.Append(chr);
                }
                else {
                    invalidChars.Append(chr);
                }
                ptr++;
            }
            if (invalidChars.Length > 0) {
                buffer = Encoding.UTF8.GetBytes(invalidChars.ToString());
                foreach (byte item in buffer) {
                    newUrl.AppendFormat("%{0}{1}",
                    HexLookUpTable[(item & 0xf0) >> 4],
                    HexLookUpTable[item & 0x0f]);
                }
            }
            return newUrl.ToString();
        }

        private static byte HexDigitToByte(char hdigit) {
            hdigit = char.ToLower(hdigit);
            if ('0' <= hdigit && hdigit <= '9') return (byte)(hdigit - 48);
            else return (byte)(hdigit - 87);
        }

        private static bool isHex(char currentChar) {
            if (char.IsDigit(currentChar)
                || 'A' <= currentChar && currentChar <= 'F'
                || 'a' <= currentChar && currentChar <= 'f') {
                return true;
            }
            return false;
        }
    }
}
