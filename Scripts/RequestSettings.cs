using TwistCore;

namespace RequestForMirror
{
    public class RequestSettings : SettingsAsset
    {
        public RequestSerializerType serializationMethod;
        public NetworkTransportMethod transportMethod;
        public LogLevel logLevel;
        public bool cacheMethodInfo = true;

        public static string CurrentSerializer => SettingsUtility.Load<RequestSettings>().serializationMethod switch
        {
            RequestSerializerType.JsonUtility => "Json",
            RequestSerializerType.MirrorBuiltIn => "MirrorWeaver",
            _ => "MirrorWeaver"
        };

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