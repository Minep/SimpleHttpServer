using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Utils
{
    public class CookieParser
    {
        const int K = 0, V = 1, KH1 = 2, KH2 = 3, VH1 = 4, VH2 = 5, ERROR = -1;
        public static IEnumerable<KeyValuePair<string, string>> ParseCookie(string cookie) {
            StringBuilder keyBuilder = new StringBuilder();
            StringBuilder valBuilder = new StringBuilder();
            using (MemoryStream encodeConverter = new MemoryStream()) {
                int ptr = 0;
                byte hex = 0;
                char chr;
                int currentState = K;
                while (ptr < cookie.Length && currentState != ERROR) {
                    chr = cookie[ptr]; 
                    switch(currentState) {
                        case K:
                            if (encodeConverter.Length > 0) {
                                keyBuilder.Append(Encoding.UTF8.GetString(encodeConverter.ToArray()));
                                encodeConverter.Position = 0;
                                encodeConverter.SetLength(0);
                            }
                            if (chr == '=') currentState = V;
                            else if (chr == '%') currentState = KH1;
                            else keyBuilder.Append(chr);
                            break;
                        case V:
                            if (encodeConverter.Length > 0) {
                                valBuilder.Append(Encoding.UTF8.GetString(encodeConverter.ToArray()));
                                encodeConverter.Position = 0;
                                encodeConverter.SetLength(0);
                            }
                            if (chr == ';') {
                                yield return new KeyValuePair<string, string>(keyBuilder.ToString(), valBuilder.ToString());
                                keyBuilder.Clear();
                                valBuilder.Clear();
                                currentState = K;
                            }
                            else if (chr == '%') currentState = VH1;
                            else keyBuilder.Append(chr);
                            break;
                        case KH1:
                        case VH1:
                            if (isHex(chr)) {
                                hex = (byte)(hex | (HexDigitToByte(chr) << 4));
                                currentState = currentState == KH1 ? KH2 : VH2;
                            }
                            else currentState = ERROR;
                            break;
                        case KH2:
                        case VH2:
                            if (isHex(chr)) {
                                hex = (byte)(hex | (HexDigitToByte(chr)));
                                encodeConverter.WriteByte(hex);
                                hex = 0;
                                currentState = currentState == KH2 ? K : V;
                            }
                            else currentState = ERROR;
                            break;
                    }
                    ptr++;
                }
            }
            if(keyBuilder.Length!=0) {
                yield return new KeyValuePair<string, string>(keyBuilder.ToString(), valBuilder.ToString());
            }
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
