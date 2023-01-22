using TwistCore;

namespace RequestForMirror
{
    public class RequestSettings : SettingsAsset
    {
        public Serializer serializationMethod;
        public TransportMethod transportMethod;
        public LogLevel logLevel;
        public bool cacheMethodInfo = true;

        public static string CurrentSerializer => SettingsUtility.Load<RequestSettings>().serializationMethod switch
        {
            Serializer.JsonUtility => "Json",
            Serializer.MirrorBuiltIn => "MirrorWeaver",
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