using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RequestForMirror.BloomTools;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace RequestForMirror.Editor.CodeGen
{
    public static class CodeGen
    {
        public static string GeneratedFolder => Path.Combine("Scripts", "Generated");
        private static string TemplatesFolder => Path.Combine("Packages","ScriptTemplates");
        private const string SettingsFilename = "CodeGenSettings";
        private const string DefaultTemplate = "CodeGenDefault";

        private static CodeGenSettings _settings;
        private static string HomeFolder => Path.Combine("Assets", "Scripts", "RequestForMirror");
        private static string SettingsFolder => Path.Combine(HomeFolder);

        private static string GetTxtPath(params string[] pathParts)
        {
            return Path.ChangeExtension(Path.Combine(pathParts), ".txt");
        }

        private static string GetTxtTemplatePathWithGenericSupport(string folder, Type type)
        {
            Debug.Assert(type.BaseType != null, "type.BaseType != null");
            var parts = type.BaseType.Name.Split('`');
            var parentClassName = parts.FirstOrDefault();
            var hasGenerics = int.TryParse(parts.LastOrDefault(), out var genericsAmount);

            //Debug.Log(hasGenerics.ToString() + genericsAmount);

            var genericSpecificTemplate =
                GetTxtPath(folder, $"{parentClassName}`{genericsAmount}");

            //Debug.Log(genericSpecificTemplate);

            var basicTemplateForClass =
                GetTxtPath(folder, parentClassName ?? DefaultTemplate);

            if (hasGenerics && genericsAmount > 0 && File.Exists(genericSpecificTemplate))
                return genericSpecificTemplate;
            return basicTemplateForClass;
        }

        // private static string GetTxtTemplatePath(string folder, Type type)
        // {
        //     var parentClassName = type.BaseType?.Name.Split('`').First();
        //     return Path.ChangeExtension(Path.Combine(folder, parentClassName ?? DefaultTemplate), ".txt");
        // }

        private static string GetOutputCsPath(string folder, Type type)
        {
            return Path.ChangeExtension(Path.Combine(folder, type.Name), ".cs");
        }

        public static CodeGenSettings LoadSettingsAsset()
        {
            var settingsPath = Path.ChangeExtension(Path.Combine(SettingsFolder, SettingsFilename), ".asset");
            _settings = (CodeGenSettings)AssetDatabase.LoadAssetAtPath(settingsPath, typeof(CodeGenSettings));

            if (_settings != null) return _settings;
            
            var asset = ScriptableObject.CreateInstance<CodeGenSettings>();
            Directory.CreateDirectory(SettingsFolder);
            AssetDatabase.CreateAsset(asset, settingsPath);
            AssetDatabase.SaveAssets();
            
            _settings = asset;
            return _settings;
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            LoadSettingsAsset();
            if (_settings.autoGenerateOnCompile)
                GenerateScripts();
        }

        public static IEnumerable<Type> GetTypes()
        {
            //string baseType = "Fetch`1";
            return Utils.GetDerivedFrom<IMarkedForCodeGen>(typeof(IMarkedForCodeGen))
                .Where(type => !type.Name.Contains('`') && !type.IsAbstract);
        }

        public static void GenerateScripts(bool forceRegenerateExisting = false)
        {
            var types = GetTypes();

            var scriptFolder = Path.Combine("Assets", GeneratedFolder);

            var builder = new CodeGenTemplateBuilder();

            foreach (var type in types)
            {
                var outputPath = GetOutputCsPath(scriptFolder, type);
                if (!forceRegenerateExisting && (_settings.generatedFiles?.Contains(outputPath) ?? false) &&
                    File.Exists(outputPath))
                {
                    if (_settings.debugMode)
                        UnityEngine.Debug.Log(
                            $"Skipping {outputPath} because it already has been generated previously.");
                    continue;
                }

                var templatePath = GetTxtTemplatePathWithGenericSupport(TemplatesFolder, type);
                builder.SetVariable("CLASSNAME", type.Name);
                builder.GenerateFromTemplate(templatePath);
                builder.SaveToCsFile(outputPath);

                if (!_settings.generatedFiles?.Contains(outputPath) ?? false)
                    _settings.generatedFiles.Add(outputPath);
            }

            CleanupFolder();
        }

        private static bool TypeIsMarkedWithInterface(string className)
        {
            return GetTypes().FirstOrDefault(type => type.Name == className) != null;
        }

        private static void CleanupFolder()
        {
            var scriptFolder = Path.Combine("Assets", GeneratedFolder);
            var files = Directory.GetFiles(scriptFolder, "*.cs");
            foreach (var file in files)
            {
                var className = Path.GetFileNameWithoutExtension(file);
                
                //if original class that was using IMarkedForCodeGen interface has been deleted,
                //remove the auto-generated code too
                if (!TypeIsMarkedWithInterface(className))
                {
                    File.Delete(file);
                    if (_settings.generatedFiles.Contains(file))
                        _settings.generatedFiles.RemoveAll(entry => entry == file);
                }
            }
        }
    }
}