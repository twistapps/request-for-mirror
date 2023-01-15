using System;

namespace RequestForMirror
{
    public interface IRequest
    {
        Type ResponseType { get; }

        // Status Ok { get; }
        // Status Error { get; }
        bool IsAwaitingResponse(int requestId);
    }
}