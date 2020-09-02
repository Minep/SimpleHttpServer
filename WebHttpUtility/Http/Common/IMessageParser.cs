using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Http.Common
{
    public interface IMessageParser<T>
    {
        int BufferOffset { get; set; }
        T Content { get; }
        ParserStatus ProcessBuffer(byte[] b, int dataLength, int maxSize = 0);
        void ResetParser();
    }
}
