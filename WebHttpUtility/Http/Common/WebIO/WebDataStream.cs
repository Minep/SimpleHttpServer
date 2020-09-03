using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common.WebIO
{
    public abstract class WebDataStream : IDisposable
    {
        public bool IsChunkedStream { get; }

        public WebDataStream(bool isChunkedStream) {
            IsChunkedStream = isChunkedStream;
        }

        public abstract void Dispose();
        public abstract void Flush();
        public abstract byte[] ContentToByteArray();

        /// <summary>
        /// Read all bytes in <paramref name="stream"/> and write to 
        /// <see cref="WebDataStream"/>
        /// </summary>
        /// <param name="stream">source stream</param>
        public abstract void CopyFrom(Stream stream);
        public abstract void CopyTo(Stream stream);

        public abstract bool CanRead { get; }
        public abstract bool CanWrite { get; }
        public abstract long Length { get; }
    }
}
