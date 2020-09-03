using HttpServer.Common;
using HttpServer.Http.Common;
using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Request.Parser
{
    public class ChunkParser : IMessageParser<byte[]>
    {
        const int START = 0, SIZE_CR = 1, ERROR = -1,
                  ENTITY = 3, ENT_CR = 4, ENT_LF = 7, 
                  FINAL_CR = 5, FINAL_LF = 8, ACCEPT = 6;

        MemoryStream memoryStream = new MemoryStream();
        StringBuilder hexBuilder = new StringBuilder();
        int currentState = START, chunkSize = 0, accumulate = 0;

        public int BufferOffset { get; set; }

        public byte[] Content => memoryStream.ToArray();

        public void Dispose() {
            memoryStream.Dispose();
        }

        public ParserStatus ProcessBuffer(byte[] b, int dataLength, int maxSize = 0) {
            int ptr = BufferOffset;

            while (ptr < dataLength && currentState != ERROR && currentState != ACCEPT) {
                char chr = (char)b[ptr];
                switch (currentState) {
                    case START:
                        if (BasicUtils.IsHex(chr)) {
                            hexBuilder.Append(chr);
                        }
                        else if (chr == '\r') currentState = SIZE_CR;
                        else currentState = ERROR;
                        break;
                    case SIZE_CR:
                        chunkSize = int.Parse(hexBuilder.ToString(), System.Globalization.NumberStyles.HexNumber);
                        hexBuilder.Clear();
                        if (chr == '\n') {
                            currentState = chunkSize == 0 ? FINAL_CR : ENTITY;
                        }
                        else currentState = ERROR;
                        break;
                    case ENTITY:
                        memoryStream.WriteByte(b[ptr]);
                        chunkSize--;
                        accumulate++;
                        if (chunkSize == 0) {
                            currentState = ENT_CR;
                        }
                        if (accumulate >= maxSize && maxSize > 0) {
                            accumulate = 0;
                            if (ptr + 1 >= dataLength) {
                                BufferOffset = 0;
                                return ParserStatus.MOVE_NEXT_NEW_BUFFER;
                            }
                            else {
                                BufferOffset = ptr;
                                return ParserStatus.MOVE_NEXT_SAME_BUFFER;
                            }
                        }
                        break;
                    case ENT_CR:
                        if (chr == '\r') currentState = ENT_LF;
                        else currentState = ERROR;
                        break;
                    case ENT_LF:
                        if (chr == '\n') currentState = START;
                        else currentState = ERROR;
                        break;
                    case FINAL_CR:
                        if (chr == '\r') currentState = FINAL_LF;
                        else currentState = ERROR;
                        break;
                    case FINAL_LF:
                        if (chr == '\n') currentState = ACCEPT;
                        else currentState = ERROR;
                        break;
                    default:
                        break;
                }
            }
            if(currentState == ACCEPT) {
                return ParserStatus.EOF;
            }
            if (currentState == ERROR) return ParserStatus.ERROR;
            return ParserStatus.REQUIRE_MORE;
        }

        public void ResetParser() {
            memoryStream.Position = 0;
            memoryStream.SetLength(0);
        }
    }
}
