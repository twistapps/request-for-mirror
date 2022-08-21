using System.Collections.Generic;

//[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
namespace RequestForMirror
{
    public class CodeGenSettings : SettingsAsset
    {
        public bool autoGenerateOnCompile = true;
        public bool debugMode;
        public List<string> generatedFiles = new List<string>();

        public override string GetEditorWindowTitle()
        {
            return "CodeGen Settings";
        }
    }
}