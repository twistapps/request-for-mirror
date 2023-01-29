using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Unity.Netcode;

namespace RequestForMirror
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Status : INetworkSerializable
    {
        public bool RequestFailed;
        [UsedImplicitly] public ushort Code;
        public string Message;
        public bool IsBroadcast;
        public ushort requestType; //if sending to all

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

        public Status BroadcastResponse()
        {
            IsBroadcast = true;
            return this;
        }

        public static implicit operator Status(ushort code)
        {
            return new Status(code);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref RequestFailed);
            if (RequestFailed)
            {
                serializer.SerializeValue(ref Message);
                serializer.SerializeValue(ref Code);
            }
            serializer.SerializeValue(ref IsBroadcast);
            if (IsBroadcast) serializer.SerializeValue(ref requestType);
        }
    }
}