using System.IO;
using System.Linq;
using TwistCore;
using TwistCore.Editor.CodeGen;
using UnityEditor.Callbacks;
#if MODULA
using Modula;
using TwistCore.Editor;
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
                #if MODULA && REQUESTIFY_ENABLED
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
        #if MODULA && REQUESTIFY_ENABLED
        public static void GenerateScripts(params string[] ignored)
        {
            var builder = new CodeGenTemplateBuilder();

            var types = EditorUtils.GetDerivedTypesExcludingSelf<IModule>().Where(type =>
                typeof(IRequest).IsAssignableFrom(type) &&
                !type.Name.Contains('`') &&
                !type.IsAbstract &&
                !ignored.Contains(type.Name)
            );

            // var types = CodeGen.GetTypes()
            //     //filter modules only
            //     .Where(type => typeof(IModule).IsAssignableFrom(type) && !ignored.Contains(type.Name));

            builder.AppendLine("#if MODULA");

            builder.AppendLine("#if MIRROR");
            builder.Using("Mirror");
            builder.AppendLine("#elif UNITY_NETCODE");
            builder.Using("Unity.Netcode");
            builder.AppendLine("#endif");
            builder.Using("Modula");
            builder.Using("Modula.Common");
            builder.Using("UnityEngine");
            builder.Using("RequestForMirror");

            builder.EmptyLines(2);

            builder.Class(Scope.Public, RequestManagerClassname, typeof(RequestManagerBase));
            builder.AppendLine(
                "public override TypedList<IModule> AvailableModules { get; } = new TypedList<IModule>()");
            foreach (var type in types) builder.AppendLine(".Add<$>()", type.Name);

            builder.Append(";");

            builder.EmptyLines(1);
            // foreach (var type in types)
            // {
            //     var name = type.Name;
            //     builder.AppendLine($"public static {name} {name} => {SingletonInstanceName}.GetModule<{name}>();");
            // }

            /*
             *  public TRequest Req<TRequest>() where TRequest : IRequest, IModule
                {
                    var module = GetModule<TRequest>();
                    return module;
                }
             * 
             */

            builder.AppendLine("public static TRequest Req<TRequest>() where TRequest : IRequest, IModule");
            builder.OpenCurly();
            builder.AppendLine("return Instance.GetModule<TRequest>();");
            builder.CloseCurly();

            builder.EmptyLines(1);
            AddSingleton(builder);
            builder.EmptyLines(1);

            var lines = builder.GetVariableLines(CodeGenTemplateBuilder.CLASS_INNER).ToList();
            var awakeLine = lines.FindIndex(line => line.Contains("void Awake()"));
            lines[awakeLine] = "    protected override void Awake()";
            //lines.Insert(awakeLine + 2,
            //    "        Receiver.GlobalRequestManager = GetComponent<NetworkIdentity>();");

            builder.SetClassInner(lines.ToArray());
            builder.ReplaceClassInnerVar();

            builder.Endfile();
            builder.AppendLine("#endif");
            var outputPath = Path.ChangeExtension(RequestManagerPath, ".cs");
            builder.SaveToCsFile(outputPath);
        }
        #endif
        private static void AddSingleton(CodeGenTemplateBuilder builder)
        {
            builder.AppendLine("#region Singleton");
            builder.ClassInnerToken();
            builder.AppendLine("#endregion");

            builder.StartInner();
            builder.SetVariable("INSTANCE", SingletonInstanceName);
            builder.GenerateFromTemplate(
                Path.Combine(CodeGenDefinitions.TemplatesFolder, "RequestManagerSingleton.txt"), false);
            builder.EndInner();
        }
    }
}