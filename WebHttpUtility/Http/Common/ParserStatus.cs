using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Http.Common
{
    [Flags]
    public enum ParserStatus : byte
    {
        MOVE_NEXT = 0xe0,
        NEW_BUFFER = 0x01,
        SAME_BUFFER = 0x02,
        MOVE_NEXT_NEW_BUFFER = MOVE_NEXT | NEW_BUFFER,
        MOVE_NEXT_SAME_BUFFER = MOVE_NEXT | SAME_BUFFER,
        EOF = 0xfd,
        REQUIRE_MORE = 0xfe,
        ERROR = 0xff
    }
}
