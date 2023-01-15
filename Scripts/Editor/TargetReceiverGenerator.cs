using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwistCore;
using TwistCore.Editor.CodeGen;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;
#if MODULA
#endif

namespace RequestForMirror.Editor
{
    public static class TargetReceiverGenerator
    {
        private const string TargetReceiverClassname = nameof(TargetReceiver);

        private static readonly string TargetReceiverPath =
            Path.Combine(CodeGenDefinitions.GeneratedFolder, TargetReceiverClassname);

        private static bool didRegisterEvents;

        private static RequestSettings Settings => SettingsUtility.Load<RequestSettings>();

        [DidReloadScripts(-1)]
        private static void OnScriptsReload()
        {
            if (didRegisterEvents) return;
            CodeGen.OnBeforeCsFileGeneration += OnBeforeCsFileGeneration;
            didRegisterEvents = true;
            //Action<CodeGenTemplateBuilder,Type> onBeforeCsFileGeneration = OnBeforeCsFileGeneration;
            //CodeGen.OnBeforeCsFileGeneration.GetInvocationList().Contains(onBeforeCsFileGeneration)
        }

        private static void ClonePackagedFileToAssetsFolder(string typeName)
        {
            var guids = AssetDatabase.FindAssets(typeName, new[] { "Packages" });
            var scriptFilePaths = guids.Select(AssetDatabase.GUIDToAssetPath);
            var classInner = CodeGenBuilder.WrapInSeparator(CodeGenTemplateBuilder.ClassInnerSlug);
            foreach (var path in scriptFilePaths)
            {
                var modified = false;
                var lines = File.ReadAllLines(path);
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (line == string.Empty) continue;
                    // var isClassDefinition = line.Contains("class " + typeName);
                    // if (!isClassDefinition) continue;

                    if (line.Replace(" ", string.Empty)
                        .StartsWith("//" + classInner))
                    {
                        lines[i] = classInner;
                        modified = true;
                    }
                }

                if (!modified) continue;
                var classname = nameof(TargetReceiver).Split('`').FirstOrDefault();
                var outputTemplatePath = Path.Combine(CodeGenDefinitions.TemplatesFolder, classname + ".txt");
                if (File.Exists(outputTemplatePath)) File.Delete(outputTemplatePath);
                File.WriteAllLines(outputTemplatePath, lines);
                return;
            }
        }

        [ExecutionOrder(0)]
        private static void OnBeforeCsFileGeneration(CodeGenTemplateBuilder builder, Type type)
        {
            if (!typeof(TargetReceiver).IsAssignableFrom(type)) return;
            Debug.Log(type.Name + "<<<<<<<<<<<<<<<<<");
            ClonePackagedFileToAssetsFolder(type.Name);

            var requests = Object.FindObjectsOfType<MonoBehaviour>().OfType<IRequest>().ToArray();
            var responseTypes = new HashSet<Type>(requests.Select(req => req.ResponseType));

            builder.StartInner();
            foreach (var responseType in responseTypes)
            {
                builder.AppendLine("[TargetRpc]");
                builder.AppendLine(
                    "protected void TargetReceiveResponse$(NetworkConnection target, int requestId, Status status, $ response)",
                    "$SERIALIZER$", responseType.Name);
                builder.OpenCurly();
                builder.AppendLine("PushResponseOnClient(target, requestId, status, response);");
                builder.CloseCurly();

                //builder.Endfile();
            }

            builder.EndInner();
        }

        private static void Cleanup()
        {
            var outputPaths = new[]
            {
                Path.ChangeExtension(TargetReceiverPath, ".cs"),
                Path.ChangeExtension(TargetReceiverPath, ".cs.meta")
            };

            foreach (var path in outputPaths)
                if (File.Exists(path))
                    File.Delete(path);
        }
    }
}