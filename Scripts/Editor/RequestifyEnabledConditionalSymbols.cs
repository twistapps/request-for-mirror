using TwistCore.Editor;

namespace RequestForMirror.Editor
{
    public class RequestifyEnabledConditionalSymbols : ConditionalDefineSymbols
    {
        public const string REQUESTIFY_ENABLED = "REQUESTIFY_ENABLED";
        public override string GetSymbols()
        {
            return REQUESTIFY_ENABLED;
        }

        public override bool ShouldSetDefines()
        {
            #if MIRROR
            return true;
            #elif UNITY_NETCODE
            return true;
            #else
            return false;
            #endif
        }
    }
}