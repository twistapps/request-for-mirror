using TwistCore;
using TwistCore.CodeGen;
using TwistCore.CodeGen.Editor;
using TwistCore.Editor;
using UnityEditor;

namespace RequestForMirror
{
    public class CsFinderSettingsWindow : PackageSettingsWindow<CsFinderSettings>
    {
        private static string _searchYield;

        protected override void DrawGUI()
        {
            BeginSection("CS Finder Settings");
            Checkbox("Auto Generate Scripts On Compile",
                ref SettingsUtility.Load<CodeGenSettings>().autoGenerateOnCompile);
            Checkbox("Debug Mode", ref SettingsUtility.Load<CodeGenSettings>().debugMode);
            EndSection();

            CallToAction(
                $"Generate all files marked with {nameof(IMarkedForCodeGen)}:",
                new Button("Generate CS", () => CodeGen.GenerateScripts(true)));

            BeginSection("Create codegen-supported script:", true);
            InputField("Classname");
            InputField("SomeOtherField");
            HorizontalButtons(new Button("Create"));
            EndSection();


            BeginSection("Manual");
            InputField("Type name:", ref _searchYield);
        }

        [MenuItem("Tools/Twist Apps/CsFinder Settings")]
        public static void OnMenuItemClick()
        {
            ShowWindow();
        }
    }
}