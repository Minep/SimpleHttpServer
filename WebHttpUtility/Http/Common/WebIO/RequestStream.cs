using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common.WebIO
{
    public class RequestStream : SimplePayloadStream
    {
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override void Write(string str, params object[] args) {
            throw new InvalidOperationException();
        }

        public override void Write(byte byt) {
            throw new InvalidOperationException();
        }

        public override void WriteLine(string str, params object[] args) {
            throw new InvalidOperationException();
        }

        public override void WriteRaw(byte[] buffer, int offset, int length) {
            throw new InvalidOperationException();
        }
    }
}
