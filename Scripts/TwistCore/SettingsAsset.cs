using UnityEngine;

namespace RequestForMirror
{
    public abstract class SettingsAsset : ScriptableObject
    {
        public abstract string GetEditorWindowTitle();
    }
}