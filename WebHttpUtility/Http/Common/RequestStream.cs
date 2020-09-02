using HttpServer.Common.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common
{
    public class RequestStream : IWebIO
    {
        MemoryStream memoryStream = new MemoryStream();
        public bool CanRead => true;

        public bool CanWrite => false;

        public long Length => memoryStream.Length;

        public long Position
        {
            get => memoryStream.Position;
            set {
                if (value <= 0 || value >= Length) {
                    throw new IndexOutOfRangeException();
                }
                memoryStream.Position = value;
            }
        }
        public byte[] ContentToByteArray() {
            return memoryStream.ToArray();
        }

        public void CopyStreamFrom(Stream stream) {
            stream.CopyTo(memoryStream);
        }

        public virtual void Dispose() {
            memoryStream.Dispose();
        }

        public void Flush() {
            throw new InvalidOperationException();
        }

        public int ReadRaw(byte[] buffer, int offset, int length) {
            return memoryStream.Read(buffer, offset, length);
        }

        public string ReadString(int length) {
            byte[] b = new byte[length];
            memoryStream.Read(b, 0, b.Length);
            return Encoding.UTF8.GetString(b);
        }

        public void Write(string str, params object[] args) {
            throw new InvalidOperationException();
        }

        public void Write(byte byt) {
            throw new InvalidOperationException();
        }

        public void WriteLine(string str, params object[] args) {
            throw new InvalidOperationException();
        }

        public int WriteRaw(byte[] buffer, int offset, int length) {
            throw new InvalidOperationException();
        }
    }
}
