using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Common.Interface
{
    public interface IWebIO : IDisposable
    {
        bool CanRead { get; }
        bool CanWrite { get; }
        long Length { get; }
        long Position { get; set; }
        int ReadRaw(byte[] buffer, int offset, int length);
        string ReadString(int length);

        int WriteRaw(byte[] buffer, int offset, int length);
        void Write(string str, params object[] args);
        void WriteLine(string str, params object[] args);
        void Write(byte byt);

        byte[] ContentToByteArray();
        void Flush();
        void CopyStreamFrom(Stream stream);
    }
}
