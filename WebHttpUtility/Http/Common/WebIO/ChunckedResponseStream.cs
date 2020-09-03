using HttpServer.Common;
using HttpServer.Http.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common.WebIO
{
    public class ChunckedResponseStream : ChunkedStream
    {
        HttpConnection connection;
        HttpResponse response;

        readonly byte[] CRLF = new byte[] { 0x0D, 0x0A };
        readonly byte[] END_TRANSFER = Encoding.ASCII.GetBytes("0\r\n\r\n");

        public Action<int> ChunckTransferedCallback { get; set; }

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public ChunckedResponseStream(HttpConnection connection, HttpResponse response) {
            this.connection = connection;
            this.response = response;
        }

        public override void EndTransfer() {
            if (connection.IsShutDown) return;
            connection.ActiveSocket.BeginSend(END_TRANSFER, 0, END_TRANSFER.Length, 0,
                new AsyncCallback(OtherCallback), connection);
        }

        public override void BeginTransfer() {
            if (connection.IsShutDown) return;
            response.CompleteResponseHeader();
            response.SetHeader(HeaderFields.TransferEncoding, "chunked");
            byte[] header = Encoding.ASCII.GetBytes(response.GetValidHttpHeader());
            connection.ActiveSocket.BeginSend(header, 0, header.Length, 0,
                new AsyncCallback(OtherCallback), connection);
        }

        public override int TransferChunk(byte[] buffer, int offset, int length) {
            if (connection.IsShutDown) return 0;
            byte[] chunckHeader = Encoding.ASCII.GetBytes($"{length:X}\r\n");
            byte[] newBuffer = new byte[length + chunckHeader.Length + CRLF.Length];
            Array.Copy(chunckHeader, 0, newBuffer, 0, chunckHeader.Length);
            Array.Copy(buffer, offset, newBuffer, chunckHeader.Length, length);
            Array.Copy(CRLF, 0, newBuffer, chunckHeader.Length + length, CRLF.Length);

            connection.ActiveSocket.BeginSend(newBuffer, 0, newBuffer.Length, 0,
                new AsyncCallback(ChunckCallback), connection);
            return newBuffer.Length;
        }

        private void OtherCallback(IAsyncResult result) {
            HttpConnection conn = (HttpConnection)result.AsyncState;
            if (!conn.IsShutDown)
                conn.ActiveSocket.EndSend(result);
        }

        private void ChunckCallback(IAsyncResult result) {
            HttpConnection conn = (HttpConnection)result.AsyncState;
            if (conn.IsShutDown) return;
            int byteRecieve = conn.ActiveSocket.EndSend(result);
            ChunckTransferedCallback?.Invoke(byteRecieve);
        }

        public override void Dispose() {

        }


        public override void CopyFrom(Stream stream) {
            if (connection.IsShutDown) return;
            long length = stream.Length;
            int unit = connection.ServerContext.ServerConfig.MaximumChunkedUnitSize;
            BeginTransfer();
            while (length > 0) {
                int size = length - unit >= 0 ? unit : (int)length;
                byte[] buffer = new byte[size];
                stream.Read(buffer, 0, size);
                if (TransferChunk(buffer, 0, size) == 0) {
                    throw new IOException("Connection to remote user agent is aborted.");
                }
                length -= unit;
            }
            EndTransfer();
        }

        public override void CopyTo(Stream stream) {
            throw new NotSupportedException("This is not writtable");
        }
    }
}
