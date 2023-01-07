using System.Collections;
using System.Collections.Generic;
using TwistCore;
using UnityEngine;

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
