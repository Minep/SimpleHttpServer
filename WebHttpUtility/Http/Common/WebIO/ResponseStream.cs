using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common.WebIO
{
    public class ResponseStream : SimplePayloadStream
    {
        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override int ReadRaw(byte[] buffer, int offset, int length) {
            throw new InvalidOperationException();
        }

        public override string ReadString(int length) {
            throw new InvalidOperationException();
        }
    }
}
