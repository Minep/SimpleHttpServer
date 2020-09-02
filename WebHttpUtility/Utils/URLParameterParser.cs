using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Utils
{
    public class URLParameterParser
    {
        const int KEY = 0, VALUE = 1, HEX_LAST = 3, HEX_START = 2;
        readonly static int[,] TransitionTable = new int[,]
        {
          //Key Val = %  & HEX
           { 0 , -1,1,-1,-1,0},   //KEY
           { 1 , 1 ,1,2 ,0 ,1},   //VALUE
           { -1, -1,-1,-1,-1,3},  //HEX1
           { -1, -1,-1,-1,-1,1}   //HEX2
        };

        public static IEnumerable<KeyValuePair<string, string>> GetURLParameter(string paramPart) {
            int currentState = 0;
            int ptr = 0;
            StringBuilder key = new StringBuilder();
            StringBuilder value = new StringBuilder();
            MemoryStream memoryStream = new MemoryStream();

            byte currentByte = 0;

            while (ptr < paramPart.Length && currentState != -1) {
                char chr = paramPart[ptr];
                int next = TransitionTable[currentState, getNextState(chr)];

                if (next == KEY && currentState != KEY) {
                    yield return new KeyValuePair<string, string>(key.ToString(), value.ToString());
                    key.Clear();
                    value.Clear();
                }
                else if (next == KEY && currentState == KEY) {
                    key.Append(chr);
                }
                else if (next == VALUE && currentState == VALUE) {
                    if (memoryStream.Length != 0) {
                        value.Append(Encoding.UTF8.GetString(memoryStream.ToArray()));
                        memoryStream.Position = 0;
                        memoryStream.SetLength(0);
                    }
                    value.Append(chr);
                }
                else if (currentState == HEX_START) {
                    currentByte = (byte)(currentByte | HexDigitToByte(chr) << 4);
                }
                else if (currentState == HEX_LAST) {
                    currentByte = (byte)(currentByte | HexDigitToByte(chr));
                    memoryStream.WriteByte(currentByte);
                    currentByte = 0;
                }

                currentState = next;
                ptr++;
            }

            if (currentState == VALUE) {
                if (memoryStream.Length != 0) {
                    value.Append(Encoding.UTF8.GetString(memoryStream.ToArray()));
                    memoryStream.Position = 0;
                    memoryStream.SetLength(0);
                }
                yield return new KeyValuePair<string, string>(key.ToString(), value.ToString());
            }
            else {
                memoryStream.Dispose();
                throw new FormatException("Invalid parameter form");
            }
            memoryStream.Dispose();
        }

        private static byte HexDigitToByte(char hdigit) {
            hdigit = char.ToLower(hdigit);
            if ('0' <= hdigit && hdigit <= '9') return (byte)(hdigit - 48);
            else return (byte)(hdigit - 87);
        }

        private static int getNextState(char currentChar) {
            if (char.IsDigit(currentChar)
                || 'A' <= currentChar && currentChar <= 'F'
                || 'a' <= currentChar && currentChar <= 'f') {
                return 5;
            }
            if (char.IsLetterOrDigit(currentChar) || currentChar == '_') {
                return 0;
            }
            if (currentChar == '=') return 2;
            if (currentChar == '%') return 3;
            if (currentChar == '&') return 4;
            return 1;
        }
    }
}
