using TwistCore;

namespace RequestForMirror
{
    public enum RequestSerializerType
    {
        JsonUtility,
        MirrorBuiltIn
    }

    public enum NetworkTransportMethod
    {
        NetworkMessages,
        HighLevelCommands
    }

    public class RequestSettings : SettingsAsset
    {
        public RequestSerializerType serializationMethod;
        public NetworkTransportMethod transportMethod;

        public override string GetEditorWindowTitle()
        {
            return "Request for Mirror";
        }

        public override string GetPackageName()
        {
            return "com.twistapps.request-for-mirror";
        }
    }
}