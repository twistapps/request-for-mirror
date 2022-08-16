using System.IO;
using System.Linq;
using Modula;
using Modula.Common;
using UnityEditor.Callbacks;

namespace RequestForMirror.Editor.CodeGen
{
    public static class RequestManagerGenerator
    {
        private const string ModularBehaviourClassname = "RequestManager";

        private static CodeGenSettings _settings;
        
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _settings = CodeGen.LoadSettingsAsset();
            if (_settings.autoGenerateOnCompile)
                GenerateScripts();
        }
        
        public static void GenerateScripts()
        {
            CodeGenBuilder builder = new CodeGenBuilder();
            var types = CodeGen.GetTypes()
                //filter modules only
                .Where(type => typeof(IModule).IsAssignableFrom(type));
            
            builder.Using("Modula");
            builder.Using("Modula.Common");
            
            builder.EmptyLines(2);
            
            builder.Class(Scope.Public, ModularBehaviourClassname, typeof(ModularBehaviour));
            builder.AppendLine("public override TypedList<IModule> AvailableModules { get; } = new TypedList<IModule>()");
            foreach (var type in types)
            {
                builder.AppendLine(".Add<$>()", type.Name);
            }
            builder.Append(";");
            builder.Endfile();
            
            var scriptFolder =CodeGen.GeneratedFolder;
            var outputPath = Path.ChangeExtension(Path.Combine(scriptFolder, ModularBehaviourClassname), ".cs");
            
            builder.SaveToCsFile(outputPath);
        }
    }
}