using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace RequestForMirror
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Status
    {
        public readonly bool RequestFailed;
        [UsedImplicitly] public ushort Code;
        public string Message;

        public Status(bool ok, string message = null)
        {
            RequestFailed = !ok;
            if (ok) Code = 200;
            Message = message;
        }

        public Status(ushort code, string message = null)
        {
            Code = code;
            RequestFailed = code != 200;
            Message = message;
        }

        // Required by Mirror's serializer
        public Status()
        {
        }

        public Status SetMessage(string message)
        {
            Message = message;
            return this;
        }

        public static implicit operator Status(ushort code)
        {
            return new Status(code);
        }
    }
}