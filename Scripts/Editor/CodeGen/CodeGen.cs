﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RequestForMirror.BloomTools;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RequestForMirror.Editor.CodeGen
{
    public static class CodeGen
    {
        private const string PackageName = "com.twistapps.request-for-mirror";
        private const string DefaultTemplate = "CodeGenDefault";
        private const string SettingsFilename = "CodeGenSettings";

        private static CodeGenSettings _settings;

        private static string TwistappsFolder => Path.Combine("Assets", "TwistApps");
        public static string TemplatesFolder => Path.Combine("Packages", PackageName, "ScriptTemplates");
        
        private static string CodeGenFolder => Path.Combine(TwistappsFolder, "CodeGen");
        private static string AssetFolder => Path.Combine(TwistappsFolder, "RequestForMirror");
        
        public static string GeneratedFolder => Path.Combine(AssetFolder, "GeneratedScripts");

        
        private static string GetTxtPath(params string[] pathParts)
        {
            return Path.ChangeExtension(Path.Combine(pathParts), ".txt");
        }

        private static string FindTxtTemplate(Type type)
        {
            var parts = type.BaseType.Name.Split('`');
            var parentClassName = parts.FirstOrDefault();
            var hasGenericTypes = int.TryParse(parts.LastOrDefault(), out var genericArgsAmount);

            var genericSpecificTemplate =
                GetTxtPath(TemplatesFolder, $"{parentClassName}`{genericArgsAmount}");

            var basicTemplateForClass =
                GetTxtPath(TemplatesFolder, parentClassName ?? DefaultTemplate);

            if (hasGenericTypes && genericArgsAmount > 0 && File.Exists(genericSpecificTemplate))
                return genericSpecificTemplate;
            else
                return basicTemplateForClass;
        }

        private static string GetOutputCsPath(Type type)
        {
            return Path.ChangeExtension(Path.Combine(GeneratedFolder, type.Name), ".cs");
        }

        public static CodeGenSettings LoadSettingsAsset()
        {
            var settingsPath = Path.Combine(CodeGenFolder, SettingsFilename) + ".asset";
            _settings = (CodeGenSettings)AssetDatabase.LoadAssetAtPath(settingsPath, typeof(CodeGenSettings));

            if (_settings != null) return _settings;

            //if settings file not found at desired location
            var asset = ScriptableObject.CreateInstance<CodeGenSettings>();
            Directory.CreateDirectory(CodeGenFolder);
            AssetDatabase.CreateAsset(asset, settingsPath);
            AssetDatabase.SaveAssets();

            _settings = asset;
            return _settings;
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (LoadSettingsAsset().autoGenerateOnCompile)
                GenerateScripts();
        }

        /// <summary>
        /// Find types in assembly that should be complemented with generated code.
        /// </summary>
        /// <returns>Array of types derived from any abstract class implementing IMarkedForCodegen.</returns>
        public static IEnumerable<Type> GetTypes()
        {
            //string baseType = "Fetch`1";
            return Utils.GetDerivedFrom<IMarkedForCodeGen>(typeof(IMarkedForCodeGen))
                .Where(type => !type.Name.Contains('`') && !type.IsAbstract);
        }

        public static void AddPartialModifierToClassDefinition(string typeName)
        {
            var guids = AssetDatabase.FindAssets(typeName);
            var scriptFilePaths = guids.Select(AssetDatabase.GUIDToAssetPath);
            foreach (var path in scriptFilePaths)
            {
                var modified = false;
                var lines = File.ReadAllLines(path);
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line == string.Empty) continue;
                    var isClassDefinition = line.Contains("class " + typeName);
                    if (!isClassDefinition) continue;
                    if (line.Contains("partial")) break;

                    var insertionIndex = line.IndexOf("class", StringComparison.Ordinal);
                    lines[i] = line.Insert(insertionIndex, "partial ");
                    modified = true;
                    break;
                }

                if (modified)
                {
                    File.WriteAllLines(path, lines);
                    if (_settings.debugMode) Debug.Log($"Adding 'partial' modifier to {path}");
                }
            }
        }

        public static void GenerateScripts(bool forceRegenerateExisting = false)
        {
            var types = GetTypes();
            var builder = new CodeGenTemplateBuilder();

            foreach (var type in types)
            {
                var outputPath = GetOutputCsPath(type);

                var generatedFileIsRegistered = _settings.generatedFiles?.Contains(outputPath) ?? false;
                if (!forceRegenerateExisting 
                    && generatedFileIsRegistered 
                    && File.Exists(outputPath))
                {
                    if (_settings.debugMode)
                        Debug.Log($"Skipping {outputPath} because it has already been generated previously.");
                    continue;
                }

                var templatePath = FindTxtTemplate(type);
                builder.SetVariable("CLASSNAME", type.Name);
                builder.GenerateFromTemplate(templatePath);

                var autoRefresh = "kAutoRefresh";
                var autoRefreshState = EditorPrefs.GetInt(autoRefresh);
                EditorPrefs.SetInt(autoRefresh, 0);
                builder.SaveToCsFile(outputPath);
                AddPartialModifierToClassDefinition(type.Name);
                EditorPrefs.SetInt(autoRefresh, autoRefreshState);
                
                if (!generatedFileIsRegistered)
                    _settings.generatedFiles!.Add(outputPath);
            }

            CleanupFolder();
        }

        private static bool TypeIsMarkedWithInterface(string className)
        {
            return GetTypes().FirstOrDefault(type => type.Name == className) != null;
        }

        private static void CleanupFolder()
        {
            if (!Directory.Exists(GeneratedFolder)) return;
            var files = Directory.GetFiles(GeneratedFolder, "*.cs");
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