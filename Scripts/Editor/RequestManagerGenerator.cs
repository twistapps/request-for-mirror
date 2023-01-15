using System.IO;
using System.Linq;
using TwistCore;
using TwistCore.Editor.CodeGen;
using UnityEditor.Callbacks;
#if MODULA
using Modula;
#endif

namespace RequestForMirror.Editor
{
    public static class RequestManagerGenerator
    {
        private const string RequestManagerClassname = "RequestManager";
        private const string SingletonInstanceName = "Instance";

        private static readonly string RequestManagerPath =
            Path.Combine(CodeGenDefinitions.GeneratedFolder, "Modula", RequestManagerClassname);

        private static CodeGenSettings _settings;

        [DidReloadScripts(2)]
        private static void OnScriptsReloadedOrChanged()
        {
            _settings = SettingsUtility.Load<CodeGenSettings>();
            if (_settings.autoGenerateOnCompile)
#if MODULA
                GenerateScripts();
#else
                Cleanup();
#endif
        }

        private static void Cleanup()
        {
            var outputPaths = new[]
            {
                Path.ChangeExtension(RequestManagerPath, ".cs"),
                Path.ChangeExtension(RequestManagerPath, ".cs.meta")
            };

            foreach (var path in outputPaths)
                if (File.Exists(path))
                    File.Delete(path);
        }
#if MODULA
        public static void GenerateScripts(params string[] ignored)
        {
            var builder = new CodeGenTemplateBuilder();
            var types = CodeGen.GetTypes()
                //filter modules only
                .Where(type => typeof(IModule).IsAssignableFrom(type) && !ignored.Contains(type.Name)).ToArray();

            builder.AppendLine("#if MODULA");

            builder.Using("Mirror");
            builder.Using("Modula");
            builder.Using("Modula.Common");
            builder.Using("UnityEngine");

            builder.EmptyLines(2);

            builder.Class(Scope.Public, RequestManagerClassname, typeof(ModularBehaviour));
            builder.AppendLine(
                "public override TypedList<IModule> AvailableModules { get; } = new TypedList<IModule>()");
            foreach (var type in types) builder.AppendLine(".Add<$>()", type.Name);
            builder.Append(";");

            builder.AppendLine("#region Singleton");
            AddSingleton(builder);
            builder.AppendLine("#endregion");

            builder.EmptyLines(1);
            foreach (var type in types)
            {
                var name = type.Name;
                builder.AppendLine($"public static {name} {name} => {SingletonInstanceName}.GetModule<{name}>();");
            }

            builder.EmptyLines(1);
            builder.AppendLine("void Start()");
            builder.OpenCurly();
            builder.AppendLine("TargetReceiver.GlobalRequestManager = GetComponent<NetworkIdentity>();");
            builder.CloseCurly();

            builder.Endfile();
            builder.AppendLine("#endif");

            var outputPath = Path.ChangeExtension(RequestManagerPath, ".cs");

            builder.SaveToCsFile(outputPath);
        }
#endif
        private static void AddSingleton(CodeGenTemplateBuilder builder)
        {
            builder.AppendLine();
            builder.SetVariable("INSTANCE", SingletonInstanceName);
            builder.GenerateFromTemplate(
                Path.Combine(CodeGenDefinitions.TemplatesFolder, "RequestManagerSingleton.txt"));
        }
    }
}