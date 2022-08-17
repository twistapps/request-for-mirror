﻿
using System.IO;
using System.Linq;
using UnityEditor.Callbacks;

#if MODULA
using Modula;
using Modula.Common;
#endif

namespace RequestForMirror.Editor.CodeGen
{
    public static class RequestManagerGenerator
    {
        private const string ModularBehaviourClassname = "RequestManager";
        private const string SingletonInstanceName = "Instance";

        private static CodeGenSettings _settings;
        
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _settings = CodeGen.LoadSettingsAsset();
            if (_settings.autoGenerateOnCompile)
                #if MODULA
                GenerateScripts();
                #else
                Cleanup();
                #endif
        }

        private static void Cleanup()
        {
            var scriptFolder = CodeGen.GeneratedFolder;
            var outputPath = Path.ChangeExtension(Path.Combine(scriptFolder, ModularBehaviourClassname), ".cs");
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
        
        #if MODULA
        public static void GenerateScripts()
        {
            CodeGenTemplateBuilder builder = new CodeGenTemplateBuilder();
            var types = CodeGen.GetTypes()
                //filter modules only
                .Where(type => typeof(IModule).IsAssignableFrom(type)).ToArray();
            
            builder.Using("Modula");
            builder.Using("Modula.Common");
            builder.Using("UnityEngine");
            
            builder.EmptyLines(2);
            
            builder.Class(Scope.Public, ModularBehaviourClassname, typeof(ModularBehaviour));
            builder.AppendLine("public override TypedList<IModule> AvailableModules { get; } = new TypedList<IModule>()");
            foreach (var type in types)
            {
                builder.AppendLine(".Add<$>()", type.Name);
            }
            builder.Append(";");
            
            builder.EmptyLines(1);
            foreach (var type in types)
            {
                var name = type.Name;
                builder.AppendLine("public static $ $ => $.GetModule<$>();", name, name, SingletonInstanceName, name);
            }
            AddSingleton(builder);
            builder.Endfile();
            
            var scriptFolder = CodeGen.GeneratedFolder;
            var outputPath = Path.ChangeExtension(Path.Combine(scriptFolder, ModularBehaviourClassname), ".cs");
            
            builder.SaveToCsFile(outputPath);
        }
        #endif

        private static void AddSingleton(CodeGenTemplateBuilder builder)
        {
            builder.AppendLine();
            builder.SetVariable("INSTANCE", SingletonInstanceName);
            builder.GenerateFromTemplate(Path.ChangeExtension(Path.Combine(CodeGen.TemplatesFolder, "RequestManagerSingleton"), ".txt"));
        }
    }
}