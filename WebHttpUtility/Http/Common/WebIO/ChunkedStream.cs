using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common.WebIO
{
    public abstract class ChunkedStream : WebDataStream
    {
        public struct ReadResult
        {
            public byte[] BufferPtr { get; }
            public int ActualReadByte { get; }
            public object Appendix { get; }

            public ReadResult(byte[] bufferPtr, int actualReadByte, object Appendix) {
                BufferPtr = bufferPtr;
                ActualReadByte = actualReadByte;
                this.Appendix = Appendix;
            }
        }
        public ChunkedStream() : base(true) {

        }
        public override long Length => 0;

        public override byte[] ContentToByteArray() {
            throw new InvalidOperationException();
        }

        public override void Flush() {
            
        }

        public virtual void BeginTransfer() {

        }

        public virtual int TransferChunk(byte[] data, int offset, int length) {
            return 0;
        }

        public virtual void EndTransfer() {

        }

        /// <summary>
        /// Recieve a chunk from chunked stream
        /// </summary>
        /// <param name="buffer">Buffer to receive the data</param>
        /// <param name="offset">Offset in buffer</param>
        /// <param name="length">Maximum bytes to be receive. Must not exceed buffer length</param>
        /// <param name="callback">Callback for this async method</param>
        /// <param name="stateObject">object will be pass through to the callback</param>
        /// <returns>Actual byte it recieve</returns>
        public virtual void ReceiveChunkAsync(byte[] buffer, int offset, int length, Action<ReadResult> callback, object stateObject) {
            
        }

        public virtual void ReceiveChunk(byte[] buffer, int offset, int length) {

        }
    }
}
