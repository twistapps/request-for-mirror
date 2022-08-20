﻿using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RequestForMirror.Editor.CodeGen
{
    public class CodeGenCsFileProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets, 
            string[] deletedAssets, 
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (deletedAssets.Length < 1) return;
            var settings = EditorUtils.LoadSettings<CodeGenSettings>();
            var generatedFileNames = settings.generatedFiles.Select(Path.GetFileName).ToArray();
            
            foreach (var deletedAsset in deletedAssets)
            {
                var deletedFileName = Path.GetFileName(deletedAsset);
                if (!generatedFileNames.Contains(deletedFileName)) continue;
                
                var generatedAsset = new ScriptAsset(CodeGen.GeneratedFolder, deletedFileName);
                if (generatedAsset.Delete())
                {
                    settings.generatedFiles.RemoveAll(asset => asset == generatedAsset.CsPath);
                    //if (settings.debugMode)
                        Debug.Log($"Deleting autogenerated file: {generatedAsset.CsPath} (initiated by user)");
                }
                
                #if MODULA //regenerate RequestManager in case it's a module that has been deleted.
                RequestManagerGenerator.GenerateScripts(generatedAsset.Classname);
                #endif
            }
        }
        
        


        // protected void OnPreprocessAsset()
        // {
        //     Debug.Log("CodeGen Asset Postprocessor has been loaded.");
        //     var watchedFiles = CodeGen.LoadSettingsAsset().generatedFiles.Select(Path.GetFileName);
        //     
        //     Debug.Log("Watched Files: " + string.Join(", ", watchedFiles));
        //     Debug.Log("Asset Path: " + assetPath);
        //     
        //     if (!watchedFiles.Contains(Path.GetFileName(assetPath))) return;
        //     
        //     if (assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        //     {
        //         var classname = Path.GetFileNameWithoutExtension(assetPath);
        //         CodeGen.AddPartialModifierToClassDefinition(classname);
        //     }
        // }
    }
}