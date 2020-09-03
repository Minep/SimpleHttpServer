using HttpServer.Http.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Request.Parser
{
    public class ContentParser : IMessageParser<MemoryStream>
    {
        public int BufferOffset { get; set; }

        public MemoryStream Content { get; } = new MemoryStream();

        int size = 0;

        public ParserStatus ProcessBuffer(byte[] b, int dataLength, int maxSize = 0)
        {
            if (maxSize <= 0 || dataLength <= 0) {
                return ParserStatus.MOVE_NEXT_NEW_BUFFER;
            }

            if(size <= 0) {
                size = maxSize;
            }

            int size_offset = dataLength - BufferOffset;

            if (size_offset > size) {
                Content.Write(b, BufferOffset, size);
                BufferOffset = size_offset - size;
                size = 0;
                return ParserStatus.MOVE_NEXT_SAME_BUFFER;
            }
            else {
                Content.Write(b, BufferOffset, size_offset);
                size -= size_offset;
                BufferOffset = 0;
                return size == 0 ? ParserStatus.MOVE_NEXT_NEW_BUFFER : ParserStatus.REQUIRE_MORE;
            }
        }

        public void ResetParser()
        {
            Content.Seek(0, SeekOrigin.Begin);
            BufferOffset = 0;
        }

        public void Dispose() {
            Content.Dispose();
        }
    }
}
