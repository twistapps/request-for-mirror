using System;

namespace RequestForMirror
{
    [Serializable]
    public class Response<TRes>
    {
        public TRes payload;

        public void SetPayload(TRes data)
        {
            payload = data;
        }
    }
}