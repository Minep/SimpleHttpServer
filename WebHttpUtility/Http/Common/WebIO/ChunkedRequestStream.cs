using HttpServer.Common;
using HttpServer.Http.Request.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpServer.Http.Common.WebIO
{
    public class ChunkedRequestStream : ChunkedStream
    {
        class AsyncState
        {
            public HttpConnection Connection { get; }
            public object Appendix { get; }
            public byte[] BufferPtr { get; }
            public int DesireSize { get; }
            public int Offset { get; }
            public Action<ReadResult> Callback { get; }

            public AsyncState(HttpConnection connection, object appendix, 
                            byte[] bufPtr, int DesireSize, int offset, Action<ReadResult> Callback) {
                Connection = connection;
                Appendix = appendix;
                BufferPtr = bufPtr;
                this.DesireSize = DesireSize;
                Offset = offset;
                this.Callback = Callback;
            }
        }


        public override bool CanRead => true;

        public override bool CanWrite => false;

        private HttpConnection connection;

        private ChunkParser parser;
        private ParserStatus lastState;
        private int StartPtr = 0;

        internal ChunkedRequestStream(HttpConnection connection, ParserStatus previousStatus, int ptr) {
            lastState = previousStatus;
            StartPtr = ptr;
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public override void CopyFrom(Stream stream) {
            throw new NotSupportedException("This stream is not writtable");
        }

        public override void CopyTo(Stream stream) {
            
        }

        public override void ReceiveChunk(byte[] buffer, int offset, int length) {
            if (lastState == ParserStatus.EOF) {
                throw new EndOfStreamException();
            }
            if (offset + length >= buffer.Length) {
                throw new IndexOutOfRangeException();
            }
            if (lastState == ParserStatus.MOVE_NEXT_SAME_BUFFER) {
                // We have unprocess data in previous buffer.
                parser.BufferOffset = StartPtr;
                lastState = parser.ProcessBuffer(connection.ConnectionBuffer, HttpConnection.BUFFER_SIZE, length);
                StartPtr = parser.BufferOffset;

                if (lastState == ParserStatus.MOVE_NEXT) {
                    byte[] result = parser.Content;
                    Array.Copy(result, 0, buffer, offset, result.Length);
                    parser.ResetParser();
                }
            }
            else {
                lastState = ParserStatus.REQUIRE_MORE;
                int size = 0;
                while(lastState == ParserStatus.REQUIRE_MORE) {
                    size = connection.ActiveSocket.Receive(connection.ConnectionBuffer);
                    lastState = parser.ProcessBuffer(connection.ConnectionBuffer, size, length);
                }
                if(lastState.HasFlag(ParserStatus.MOVE_NEXT) || lastState == ParserStatus.EOF) {
                    Array.Copy(parser.Content, 0, buffer, offset, length);
                    if(lastState == ParserStatus.EOF) {
                        // Put connection back to normal listening mode
                        connection.ActiveSocket.BeginReceive(
                                connection.ConnectionBuffer, 0, HttpConnection.BUFFER_SIZE, 0,
                                new AsyncCallback(connection.ConnectionHandler.ReadCallback),
                                connection);
                    }
                }
                else if(lastState == ParserStatus.ERROR) {
                    connection.Response.SendError(System.Net.HttpStatusCode.NotImplemented);
                    throw new InvalidDataException();
                }

                parser.ResetParser();
            }
        }

        public override void ReceiveChunkAsync(byte[] buffer, int offset, int length, Action<ReadResult> callback, object stateObject) {
            if (lastState == ParserStatus.EOF) {
                throw new EndOfStreamException();
            }
            if (offset + length >= buffer.Length) {
                throw new IndexOutOfRangeException();
            }
            if (lastState == ParserStatus.MOVE_NEXT_SAME_BUFFER) {
                // We have unprocess data in previous buffer.
                parser.BufferOffset = StartPtr;
                lastState = parser.ProcessBuffer(connection.ConnectionBuffer, HttpConnection.BUFFER_SIZE, length);
                StartPtr = parser.BufferOffset;

                if(lastState == ParserStatus.MOVE_NEXT || lastState == ParserStatus.EOF) {
                    byte[] result = parser.Content;
                    Array.Copy(result, 0, buffer, offset, result.Length);
                    callback.Invoke(new ReadResult(buffer, result.Length, stateObject));
                    parser.ResetParser();
                }
                if(lastState == ParserStatus.EOF) {
                    // Put connection back to normal listening mode
                    connection.ActiveSocket.BeginReceive(
                            connection.ConnectionBuffer, 0, HttpConnection.BUFFER_SIZE, 0,
                            new AsyncCallback(connection.ConnectionHandler.ReadCallback),
                            connection);
                }
            }
            else if(lastState == ParserStatus.ERROR) {
                connection.Response.SendError(System.Net.HttpStatusCode.NotImplemented);
                callback.Invoke(new ReadResult(buffer, -1, stateObject));
            }
            else {
                connection.ActiveSocket.BeginReceive(
                        connection.ConnectionBuffer, 0, HttpConnection.BUFFER_SIZE, 0,
                        new AsyncCallback(AsyncCallback),
                        new AsyncState(connection, stateObject, buffer, length, offset, callback)
                        );
            }
        }

        private void AsyncCallback(IAsyncResult result) {
            AsyncState state = (AsyncState)result.AsyncState;
            int size = state.Connection.ActiveSocket.EndReceive(result);
            lastState = parser.ProcessBuffer(
                state.BufferPtr,
                size,
                state.DesireSize);
            if(lastState == ParserStatus.REQUIRE_MORE) {
                connection.ActiveSocket.BeginReceive(
                        connection.ConnectionBuffer, 0, HttpConnection.BUFFER_SIZE, 0,
                        new AsyncCallback(AsyncCallback),
                        state
                        );
            }
            else if(lastState == ParserStatus.ERROR) {
                connection.Response.SendError(System.Net.HttpStatusCode.NotImplemented);
                state.Callback.Invoke(
                    new ReadResult(state.BufferPtr, -1, state.Appendix));
            }
            else if(lastState == ParserStatus.EOF) {
                // Put connection back to normal listening mode
                connection.ActiveSocket.BeginReceive(
                        connection.ConnectionBuffer, 0, HttpConnection.BUFFER_SIZE, 0,
                        new AsyncCallback(connection.ConnectionHandler.ReadCallback),
                        connection);
            }
            else {
                byte[] res = parser.Content;
                Array.Copy(res, 0, state.BufferPtr, state.Offset, res.Length);
                state.Callback.Invoke(new ReadResult(state.BufferPtr, res.Length, state.Appendix));
                parser.ResetParser();
            }
        }

        public override void Dispose() {
            parser.Dispose();
        }
    }
}
