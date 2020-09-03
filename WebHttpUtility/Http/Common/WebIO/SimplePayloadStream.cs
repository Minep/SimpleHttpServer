using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common.WebIO
{
    public abstract class SimplePayloadStream : WebDataStream
    {
        MemoryStream memoryStream = new MemoryStream();

        public SimplePayloadStream() : base(false) {
            memoryStream = new MemoryStream();
        }

        public override long Length => memoryStream.Length;
        public long Position
        {
            get => memoryStream.Position;
            set => memoryStream.Position = value;
        }
        public virtual int ReadRaw(byte[] buffer, int offset, int length) {
            return memoryStream.Read(buffer, offset, length);
        }
        public virtual string ReadString(int length) {
            byte[] b = new byte[length];
            if (memoryStream.Read(b, 0, length) > 0) {
                return Encoding.UTF8.GetString(b);
            }
            return string.Empty;
        }

        public virtual void WriteRaw(byte[] buffer, int offset, int length) {
            memoryStream.Write(buffer, 0, length);
        }
        public virtual void Write(string str, params object[] args) {
            byte[] b = Encoding.UTF8.GetBytes(string.Format(str, args));
            memoryStream.Write(b, 0, b.Length);
        }
        public virtual void WriteLine(string str, params object[] args) {
            byte[] b = Encoding.UTF8.GetBytes(string.Format($"{str}\r\n", args));
            memoryStream.Write(b, 0, b.Length);
        }
        public virtual void Write(byte byt) {
            memoryStream.WriteByte(byt);
        }

        public override byte[] ContentToByteArray() {
            return memoryStream.ToArray();
        }
        public override void Flush() {
            memoryStream.Position = 0;
            memoryStream.SetLength(0);
        }

        public override void CopyTo(Stream stream) {
            memoryStream.CopyTo(stream);
        }
        public override void CopyFrom(Stream stream) {
            stream.CopyTo(memoryStream);
        }

        public override void Dispose() {
            memoryStream.Dispose();
        }
    }
}
