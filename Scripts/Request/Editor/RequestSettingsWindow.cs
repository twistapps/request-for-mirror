using System.IO;
using TwistCore.CodeGen.Editor;
using TwistCore.Editor;
using TwistCore.Editor.UIComponents;
using TwistCore.PackageDevelopment.Editor;
using TwistCore.PackageRegistry.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace RequestForMirror.Editor
{
    public class RequestSettingsWindow : PackageSettingsWindow<RequestSettings>
    {
        // protected void OnGUI()
        // {
        //
        //     // CallToAction($"Generate all files marked with {nameof(IMarkedForCodeGen)}:", 
        //     //     new Button("Generate CS", () => { Debug.Log("Button Click"); }));
        //     //
        //     // BeginSection("Create codegen-supported script:", true);
        //     // InputField("Classname", ref Settings.testString);
        //     // //InputField("SomeOtherField", "SomeOtherDefaultText");
        //     // HorizontalButtons(new Button("Create", () => { Debug.Log("Button Click"); }),
        //     //     new Button("Create", () => { Debug.Log("Button Click"); }));
        //     // EndSection();
        // }

        protected override void DrawGUI()
        {
            AddSection("General", () =>
            {
                EnumPopup("Serialization Method", ref Settings.serializationMethod,
                    newValue => { CodeGen.GenerateScripts(true); });
                EnumPopup("Transport Method", ref Settings.transportMethod);
            });

            AddSection("Core Management", () =>
            {
                this.DrawCachedComponent("CoreUnpackWidget");
                ButtonLabel("Do a Barrel Roll", new Button("Barrel Roll", DoABarrelRoll));
            });
        }

        private static void DoABarrelRoll()
        {
            var dependencyName = "com.unity.editorcoroutines";
            var alias = UPMCollection.GetFromAllPackages(dependencyName).Alias();

            var files = Directory.GetFiles(Path.Combine("Packages", dependencyName), "*.asmdef",
                SearchOption.AllDirectories);
            Debug.Log(files[0]);
            Debug.Log(AssetDatabase.GUIDFromAssetPath(files[0]));

            foreach (var assembly in CompilationPipeline.GetAssemblies())
                //var newRef = assembly.assemblyReferences.ToList();
                //newRef.Add(null);
                //CompilationPipeline.
                if (assembly.name.ToLower().Contains(alias))
                {
                    // Debug.Log(AssetDatabase.FindAssets("t:Assembly", new[]
                    // {
                    //     Path.Combine("Packages", dependencyName)
                    // })?[0] ?? "0");

                    Debug.Log(assembly.outputPath);
                    Debug.Log(CompilationPipeline.GUIDToAssemblyDefinitionReferenceGUID(
                        AssetDatabase.AssetPathToGUID(assembly.outputPath)));
                    Debug.Log(AssetDatabase.GUIDToAssetPath("478a2357cc57436488a56e564b08d223"));
                }
        }

        [MenuItem("Tools/Twist Apps/Request for Mirror Settings")]
        public static void OnMenuItemClick()
        {
            ShowWindow();
        }
    }
}