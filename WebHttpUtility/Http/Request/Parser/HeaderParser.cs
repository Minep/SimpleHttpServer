using HttpServer.Http.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Http.Request.Parser
{
    public class HeaderParser : IMessageParser<HttpHeader>
    {
        const int OTHER = 1, FIRST_CRLF = 2, SECOND_CRLF = 3, ACCEPT = 4;
        const int SL_SP1 = 5, SL_SP2 = 6, SL_SP3 = 7;
        const int HF_KEY = 8, HF_VAL = 9;

        StringBuilder builder = new StringBuilder();
        StringBuilder builder2 = new StringBuilder();

        HttpHeader httpHeader = new HttpHeader();
        int currentState = OTHER;


        public int BufferOffset { get; set; } = 0;
        public HttpHeader Content => httpHeader;

        private int LineCounter = 0;

        /// <summary>
        /// Process raw message
        /// </summary>
        /// <returns>Require more</returns>
        public ParserStatus ProcessBuffer(byte[] b, int dataLength, int maxSize = -1)
        {
            // Construct a Turing Machine with two read header which are
            //      1. Current Lookahead
            //      2. Next Lookahead
            // to recognized a request header.

            // UPDATE:
            // Put some lexical process into our TM, so we can PARSE
            // the header (retrieve structured startline and header-fields) in O(n) time.
            // A Little bit messy but efficient.
            int i = BufferOffset;
            while (i < dataLength && currentState != ACCEPT)
            {
                char Current = (char)b[i];
                char NextLH = i + 1 >= dataLength ? '\0' : (char)b[i + 1];
                switch (currentState)
                {
                    case OTHER:
                        if (Current == '\r' && (NextLH == '\n' || NextLH == '\0'))
                            currentState = FIRST_CRLF;
                        else if (Current == '\n')
                            return ParserStatus.ERROR;
                        else if (LineCounter == 0) {
                            i--;    // We don't want to forward the pointer
                            currentState = SL_SP1;
                        }
                        else {
                            i--;    // We don't want to forward the pointer
                            currentState = HF_KEY;
                        }
                        break;
                    case FIRST_CRLF:
                        if (Current == '\n' && (NextLH == '\r' || NextLH == '\0'))
                            currentState = SECOND_CRLF;
                        else {
                            LineCounter++;
                            currentState = OTHER;
                        }
                        builder.Append(Current);
                        break;
                    case SECOND_CRLF:
                        if (Current == '\r' && (NextLH == '\n' || NextLH == '\0'))
                            currentState = ACCEPT;
                        break;
                    case SL_SP1:
                    case SL_SP2:
                    case SL_SP3:
                        if (Current == ' ') {
                            if(currentState == SL_SP1) {
                                httpHeader.Method = builder.ToString();
                                currentState = SL_SP2;
                            }
                            else if (currentState == SL_SP2) {
                                httpHeader.TargetURL = builder.ToString();
                                currentState = SL_SP3;
                            }
                            builder.Clear();
                        }
                        else if (currentState == SL_SP3 
                                && Current == '\r' 
                                && (NextLH == '\n' || NextLH == '\0')) {
                            httpHeader.Protocol = builder.ToString();
                            builder.Clear();
                            currentState = FIRST_CRLF;
                        }
                        else if (Current != '\r' && Current != '\n') builder.Append(Current);
                        else return ParserStatus.ERROR;
                        break;
                    case HF_KEY:
                    case HF_VAL:
                        if (Current == ':') {
                            currentState = HF_VAL;
                        }
                        else if (currentState == HF_VAL
                                && Current == '\r'
                                && (NextLH == '\n' || NextLH == '\0')) {
                            httpHeader.HeaderFields.Add(builder.ToString(), builder2.ToString());
                            builder.Clear();
                            builder2.Clear();
                            currentState = FIRST_CRLF;
                        }
                        else if (Current != '\r' && Current != '\n') {
                            if (currentState == HF_KEY) {
                                builder.Append(Current);
                            }
                            else {
                                builder2.Append(Current);
                            }
                        }
                        else return ParserStatus.ERROR;
                        break;
                    default:
                        return ParserStatus.ERROR;
                }
                i++;
            }
            if (currentState != ACCEPT)
            {
                // Pointer reach the end of buffer, but our TM neither accept nor reject.
                // which means, we need new buffer in order to allow our TM to make decision.
                //builder.Append((char)b[b.Length - 1]);
                return ParserStatus.REQUIRE_MORE;
            }
            else
            {
                if (i >= dataLength - 1)
                {
                    // Our TM accept at the end of current buffer.
                    BufferOffset = 0;
                    return ParserStatus.MOVE_NEXT_NEW_BUFFER;
                }
                else
                {
                    // Our TM accept at somewhere within current buffer.
                    BufferOffset = i + 1;   //Skip next look-ahead
                    return ParserStatus.MOVE_NEXT_SAME_BUFFER;
                }
            }
        }

        public void ResetParser()
        {
            BufferOffset = 0;
            LineCounter = 0;
            httpHeader = new HttpHeader();
            currentState = OTHER;
            builder.Clear();
            builder2.Clear();
        }

        public void Dispose() {
            
        }
    }
}
