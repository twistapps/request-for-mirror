using UnityEditor;
using UnityEngine;

namespace RequestForMirror.Editor.Request
{
    public class RequestPackageSettingsWindow : PackageSettingsWindow<RequestSettings>
    {
        protected override void OnGUI()
        {
            base.OnGUI();

            BeginSection("General");
            EnumPopup("Serialization Method", ref Settings.serializationMethod,
                newValue => { CodeGen.CodeGen.GenerateScripts(true); });
            EndSection();
            
            
            WatchChangesAbove();

            // CallToAction($"Generate all files marked with {nameof(IMarkedForCodeGen)}:", 
            //     new Button("Generate CS", () => { Debug.Log("Button Click"); }));
            //
            // BeginSection("Create codegen-supported script:", true);
            // InputField("Classname", ref Settings.testString);
            // //InputField("SomeOtherField", "SomeOtherDefaultText");
            // HorizontalButtons(new Button("Create", () => { Debug.Log("Button Click"); }),
            //     new Button("Create", () => { Debug.Log("Button Click"); }));
            // EndSection();
        }

        [MenuItem("Tools/Twist Apps/Request for Mirror Settings")]
        private static void OnMenuItemClick()
        {
            ShowWindow();
        }
    }
}