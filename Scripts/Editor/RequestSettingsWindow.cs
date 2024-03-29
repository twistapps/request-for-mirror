﻿using System.IO;
using TwistCore.Editor;
using TwistCore.Editor.CodeGen;
using TwistCore.Editor.GuiWidgets;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace RequestForMirror.Editor
{
    public class RequestSettingsWindow : PackageSettingsWindow<RequestSettings>
    {
        protected override void DrawGUI()
        {
            AddSection("General", () =>
            {
                EnumPopup("Serialization Method", ref Settings.serializationMethod,
                    newValue => { CodeGen.GenerateScripts(true); });
                EnumPopup("Transport Method", ref Settings.transportMethod);
            });

            AddSection("Development", () => { EnumPopup("Log Level", ref Settings.logLevel); });

            AddSection("Performance", () => { Checkbox("Cache MethodInfo", ref Settings.cacheMethodInfo); });

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
                if (assembly.name.ToLower().Contains(alias))
                {
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