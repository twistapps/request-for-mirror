using TwistCore;

namespace RequestForMirror
{
    public class CsFinderSettings : SettingsAsset
    {
        public override string GetEditorWindowTitle()
        {
            return "Find CS File by Type";
        }

        public override string GetPackageName()
        {
            return "CS Finder";
        }
    }
}