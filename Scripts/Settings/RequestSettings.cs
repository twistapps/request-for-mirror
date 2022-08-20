namespace RequestForMirror
{
    public enum RequestSerializerType
    {
        JsonUtility,
        MirrorBuiltIn
    }

    public class RequestSettings : SettingsAsset
    {
        public RequestSerializerType serializationMethod;

        public override string GetEditorWindowTitle()
        {
            return "Request for Mirror Settings";
        }
    }
}