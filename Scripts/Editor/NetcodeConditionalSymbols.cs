using TwistCore.Editor;

namespace RequestForMirror.Editor
{
    [PackageName(PackageName)]
    public class NetcodeConditionalSymbols : ConditionalDefineSymbols
    {
        public const string UNITY_NETCODE = "UNITY_NETCODE";
        public const string PackageName = "com.unity.netcode.gameobjects";

        public override string GetSymbols()
        {
            return UNITY_NETCODE;
        }

        public override bool ShouldSetDefines()
        {
            return UPMCollection.GetFromAllPackages(PackageName) != null;
        }
    }
}