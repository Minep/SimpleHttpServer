using HttpServer.Common;
using HttpServer.Http.Common;
using HttpServer.Http.Request;
using HttpServer.Http.Request.Parser;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServer.Http
{
    enum HeaderStatus
    {
        WITH_CONTENT,
        NO_CONTENT,
        ERROR,
        CONTINUE
    }
    public class HttpHandler
    {
        const int HTTP_HEADER = 0, CONTENT = 1, ACCEPT = 2;

        int currentState = HTTP_HEADER;
        int maxContentLength = 0;

        HeaderParser headerParser = new HeaderParser();
        ContentParser contentParser = new ContentParser();

        HttpRequest CurrentRequest;

        public HttpHandler() {

        }

        public void ReadCallback(IAsyncResult result)
        {
            HttpSession session = (HttpSession)result.AsyncState;
            session.MarkActiveOnce();
            try {
                int byteReceived = session.ActiveSocket.EndReceive(result);

                // Remote Client Close Connection
                if (byteReceived == 0) {
                    session.NaturalClose();
                    return;
                }

                ParserStatus status = ParserStatus.MOVE_NEXT_SAME_BUFFER;

                do {
                    switch (currentState) {
                        case HTTP_HEADER:
                            status = headerParser.ProcessBuffer(session.ConnectionBuffer, byteReceived);
                            if (status == ParserStatus.ERROR) {
                                session.Response.SendError(HttpStatusCode.BadRequest);
                                return;
                            }
                            else if (status.HasFlag(ParserStatus.MOVE_NEXT)) {
                                HeaderStatus header = CreateAndCheckHeader(headerParser.Content, session, out maxContentLength);
                                if (header == HeaderStatus.ERROR) {
                                    session.Response.SendError(HttpStatusCode.BadRequest);
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
                            session.ActiveSocket.BeginReceive(
                                    session.ConnectionBuffer, 0,
                                    HttpSession.BUFFER_SIZE, 0,
                                    new AsyncCallback(ReadCallback), session);
                            break;
                        case CONTENT:
                            if (maxContentLength > session.ServerContext.ServerConfig.MaxContentSizeByte) {
                                session.Response.SendError(HttpStatusCode.RequestEntityTooLarge);
                            }
                            status = contentParser.ProcessBuffer(session.ConnectionBuffer, byteReceived, maxContentLength);
                            if (status.HasFlag(ParserStatus.MOVE_NEXT)) {
                                currentState = ACCEPT;
                            }

                            // Put the socket on recieving mode for further incoming requests
                            session.ActiveSocket.BeginReceive(
                                    session.ConnectionBuffer, 0,
                                    HttpSession.BUFFER_SIZE, 0,
                                    new AsyncCallback(ReadCallback), session);
                            break;
                        case ACCEPT:
                            currentState = HTTP_HEADER;
                            maxContentLength = 0;
                            headerParser.ResetParser();
                            contentParser.ResetParser();
                            CurrentRequest.CopyStreamFrom(contentParser.Content);
                            session.ServerContext.RequestDispatcher.Dispatch(
                                CurrentRequest,
                                new Response.HttpResponse(
                                    session, session.ServerContext.ServerConfig)
                                );
                            break;
                    }
                }
                while (status.HasFlag(ParserStatus.SAME_BUFFER) || currentState == ACCEPT);
            }
            catch(SocketException se) {
                //TODO May need logger here
                session.NaturalClose();
            }
            catch(Exception ex) {
                session.Response.SendError(HttpStatusCode.InternalServerError);
            }
        }

        private HeaderStatus CreateAndCheckHeader(HttpHeader header, HttpSession session, out int ContentLength) {            
            ContentLength = 0;
            if (header == null) return HeaderStatus.ERROR;
            CurrentRequest = new HttpRequest(header, session);
            if (CurrentRequest.HasHeaderField(HeaderFields.Expect)) {
                if (!CurrentRequest.GetHeaderField(HeaderFields.Expect).Equals(HeaderFields.ExpectVal)) {
                    return HeaderStatus.ERROR;
                }
                return HeaderStatus.CONTINUE;
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
