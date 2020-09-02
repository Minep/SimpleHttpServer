using HttpServer.Common.Interface;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common
{
    public class ResponseStream : IWebIO
    {
        protected MemoryStream streamContent = new MemoryStream();


        public bool CanRead => false;

        public bool CanWrite => true;

        public long Length => streamContent.Length;

        public long Position
        {
            get => streamContent.Position;
            set 
            {
                if(value<=0 || value >= Length) {
                    throw new IndexOutOfRangeException();
                }
                streamContent.Position = value;
            }
        }

        public virtual void Flush() {
            streamContent.Position = 0;
            streamContent.SetLength(0);
        }

        public int ReadRaw(byte[] buffer, int offset, int length) {
            throw new NotImplementedException();
        }

        public string ReadString(int length) {
            throw new NotImplementedException();
        }

        public byte[] ReadBase64Encoded(int length) {
            throw new NotImplementedException();
        }

        public int WriteRaw(byte[] buffer, int offset, int length) {
            streamContent.Write(buffer, offset, length);
            return length;
        }

        public void Write(string str, params object[] args) {
            streamContent.Write(Encoding.UTF8.GetBytes(string.Format(str, args)));
        }

        public void WriteLine(string str, params object[] args) {
            streamContent.Write(Encoding.UTF8.GetBytes(string.Format($"{str}\r\n", args)));
        }

        public void Write(byte byt) {
            streamContent.WriteByte(byt);
        }

        public void CopyStreamFrom(Stream stream) {
            stream.CopyTo(streamContent);
        }

        public byte[] ContentToByteArray() {
            return streamContent.ToArray();
        }

        public virtual void Dispose() {
            streamContent.Dispose();
        }
    }
}
