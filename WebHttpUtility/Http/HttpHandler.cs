using HttpServer.Common;
using HttpServer.Http.Common;
using HttpServer.Http.Common.WebIO;
using HttpServer.Http.Request;
using HttpServer.Http.Request.Parser;
using System;
using System.Net;
using System.Net.Sockets;

namespace HttpServer.Http
{
    enum HeaderStatus
    {
        WITH_CONTENT,
        NO_CONTENT,
        CHUNKED,
        ERROR,
        CONTINUE
    }
    public class HttpHandler : IDisposable
    {
        const int HTTP_HEADER = 0, CONTENT = 1, ACCEPT = 2;

        int currentState = HTTP_HEADER;
        int maxContentLength = 0;

        HeaderParser headerParser = new HeaderParser();
        ContentParser contentParser = new ContentParser();

        HttpRequest CurrentRequest;

        public HttpHandler() {

        }

        public void Dispose() {
            headerParser.Dispose();
            contentParser.Dispose();
        }

        public void ReadCallback(IAsyncResult result)
        {
            HttpConnection conn = (HttpConnection)result.AsyncState;
            conn.MarkActiveOnce();
            try {
                int byteReceived = conn.ActiveSocket.EndReceive(result);

                // Remote Client Close Connection
                if (byteReceived == 0) {
                    conn.NaturalClose();
                    return;
                }

                ParserStatus status = ParserStatus.MOVE_NEXT_SAME_BUFFER;

                do {
                    switch (currentState) {
                        case HTTP_HEADER:
                            status = headerParser.ProcessBuffer(conn.ConnectionBuffer, byteReceived);
                            if (status == ParserStatus.ERROR) {
                                conn.Response.SendError(HttpStatusCode.BadRequest);
                                return;
                            }
                            else if (status.HasFlag(ParserStatus.MOVE_NEXT)) {
                                HeaderStatus header = CreateAndCheckHeader(headerParser.Content, conn, out maxContentLength);
                                if (header == HeaderStatus.ERROR) {
                                    conn.Response.SendError(HttpStatusCode.BadRequest);
                                }
                                else if (header == HeaderStatus.NO_CONTENT) {
                                    currentState = ACCEPT;
                                }
                                else if (header == HeaderStatus.CONTINUE) {
                                    //TODO Add 100/417 response
                                }
                                else currentState = CONTENT;

                                contentParser.BufferOffset = headerParser.BufferOffset;
                            }
                            conn.ActiveSocket.BeginReceive(
                                    conn.ConnectionBuffer, 0,
                                    HttpConnection.BUFFER_SIZE, 0,
                                    new AsyncCallback(ReadCallback), conn);
                            break;
                        case CONTENT:
                            if (maxContentLength > conn.ServerContext.ServerConfig.MaxContentSizeByte) {
                                conn.Response.SendError(HttpStatusCode.RequestEntityTooLarge);
                            }

                            // IMPORTANT Identify chunked data and try to process it.
                            // =================================================
                            // The illustration of such process.
                            // We will handle it to a IChunkedRequestStream (yet to implement)
                            // This will have a built in chunk parse state machine, invoking each time
                            // when user calls method IChunkedRequestStream::ReadData.
                            //
                            // This connection will be blocked until all chunked data to be read
                            // The control will be moved to the IChunkedRequestStream, this current thread
                            // will be blocked according to "chunk parse state machine" return value:
                            //      1. ParserStatus.REQUIRE_MORE:
                            //          Thread resume, BeginReceive will be invoke to get more data.
                            //      2. ParserStatus.MOVE_NEXT:
                            //          Thread resume, Goto ACCEPT state, wait for next request.
                            //      3. ParserStatus.ERROR:
                            //          Thread resume, Send BAD_REQUEST, close the current socket.

                            status = contentParser.ProcessBuffer(conn.ConnectionBuffer, byteReceived, maxContentLength);
                            if (status.HasFlag(ParserStatus.MOVE_NEXT)) {
                                currentState = ACCEPT;
                            }

                            // Put the socket on recieving mode for further incoming requests
                            conn.ActiveSocket.BeginReceive(
                                    conn.ConnectionBuffer, 0,
                                    HttpConnection.BUFFER_SIZE, 0,
                                    new AsyncCallback(ReadCallback), conn);
                            break;
                        case ACCEPT:
                            currentState = HTTP_HEADER;
                            maxContentLength = 0;
                            headerParser.ResetParser();
                            contentParser.ResetParser();

                            CurrentRequest.RequestContentStream = new RequestStream();
                            CurrentRequest.RequestContentStream.CopyFrom(contentParser.Content);

                            conn.ServerContext.RequestDispatcher.Dispatch(
                                CurrentRequest,
                                new Response.HttpResponse(conn));
                            break;
                    }
                }
                while (status.HasFlag(ParserStatus.SAME_BUFFER) || currentState == ACCEPT);
            }
            catch(SocketException se) {
                //TODO May need logger here
                Console.WriteLine("{0}\r\n{1}", se.Message, se.StackTrace);
                conn.NaturalClose();
            }
            catch(Exception ex) {
                Console.WriteLine("{0}\r\n{1}", ex.Message, ex.StackTrace) ;
                conn.Response.SendError(HttpStatusCode.InternalServerError);
            }
        }

        private HeaderStatus CreateAndCheckHeader(HttpHeader header, HttpConnection conn, out int ContentLength) {            
            ContentLength = 0;
            if (header == null) return HeaderStatus.ERROR;
            CurrentRequest = new HttpRequest(header, conn);
            if (CurrentRequest.HasHeaderField(HeaderFields.Expect)) {
                if (!CurrentRequest.GetHeaderField(HeaderFields.Expect).Equals(HeaderFields.ExpectVal)) {
                    return HeaderStatus.ERROR;
                }
                return HeaderStatus.CONTINUE;
            }
            if (CurrentRequest.GetHeaderField(HeaderFields.TransferEncoding).Contains("chunked")) {
                return HeaderStatus.CHUNKED;
            }
            if (!CurrentRequest.HasHeaderField(HeaderFields.ContenLength)) {
                return HeaderStatus.NO_CONTENT;
            }
            else {
                ContentLength = CurrentRequest.GetHeaderFieldAs<int>(HeaderFields.ContenLength);
                return HeaderStatus.WITH_CONTENT;
            }
        }
    }
}
