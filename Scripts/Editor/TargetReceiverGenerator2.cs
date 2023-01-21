using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwistCore.Editor;
using TwistCore.Editor.CodeGen;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RequestForMirror.Editor
{
    public class TargetReceiverGenerator2 : ScriptableSingleton<TargetReceiverGenerator2>
    {
        private const string GeneratedClassname = "TargetReceiver";
        [SerializeField] private RequestMeta[] registeredRequests;

        private static string OutputPath =>
            Path.Combine(
                CodeGenDefinitions.GeneratedFolder,
                GeneratedClassname);

        private static RequestMeta[] GetAllRequests()
        {
            var requestTypes = EditorUtils.GetDerivedTypesExcludingSelf<IRequest>().Where(t => !t.IsAbstract);

            var list = new List<RequestMeta>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var type in requestTypes)
            {
                var genericArgs = type.BaseType!.GenericTypeArguments;
                if (genericArgs == null || genericArgs.Length < 1) continue;
                var tReqs = genericArgs.Length > 1 ? genericArgs.Take(genericArgs.Length - 1).ToArray() : null;

                list.Add(new RequestMeta
                {
                    Name = type.Name,
                    requestTypes = SerializableType.ArrayFromTypes(tReqs),
                    responseType = genericArgs.Last()
                });
            }

            return list.ToArray();
        }

        [DidReloadScripts(2)]
        private static void GenerateTargetReceiver()
        {
            var allRequests = GetAllRequests();
            if (allRequests.Length == 0)
            {
                Cleanup();
                return;
            }

            if (!instance.ShouldGenerate(allRequests)) return;
            instance.registeredRequests = allRequests;

            var builder = new CodeGenTemplateBuilder();
            var template = CodeGen.FindTxtTemplate(typeof(Receiver));

            instance.SetClassInner(builder);
            builder.SetVariable("CLASSNAME", GeneratedClassname);
            builder.GenerateFromTemplate(template);
            builder.SaveToCsFile(OutputPath);
        }

        private bool ShouldGenerate(IEnumerable<RequestMeta> newRequests)
        {
            if (registeredRequests == null) return true;
            if (!File.Exists(Path.ChangeExtension(OutputPath, ".cs"))) return true;
            var requestMetas = newRequests as RequestMeta[] ?? newRequests.ToArray();
            return !registeredRequests.All(requestMetas.Contains) ||
                   registeredRequests.Length != requestMetas.Length;
        }

        private static void Cleanup()
        {
            var outputPaths = new[]
            {
                Path.ChangeExtension(OutputPath, ".cs"),
                Path.ChangeExtension(OutputPath, ".cs.meta")
            };

            foreach (var path in outputPaths)
                if (File.Exists(path))
                    File.Delete(path);
        }

        private void SetClassInner(CodeGenTemplateBuilder builder)
        {
            builder.StartInner();

            foreach (var request in registeredRequests) AddRequest(builder, request);

            var responseTypes = new HashSet<Type>(registeredRequests.Select(request => (Type)request.responseType));
            foreach (var responseType in responseTypes) AddResponse(builder, responseType);

            builder.EndInner();
        }

        private static void AddRequest(CodeGenBuilder builder, RequestMeta request)
        {
            var @params = request.requestTypes?.Select(t => t.SerializedType.Name + " request").ToList() ??
                          new List<string>();
            for (var i = 1; i < @params.Count; i++) @params[i] += i + 1; //TReq2 request2, TReq3 request3 etc.
            @params.Add("NetworkConnectionToClient sender = null");

            var paramNames = new List<string>();
            if (paramNames == null) throw new ArgumentNullException(nameof(paramNames));
            if (request.requestTypes != null)
                for (var i = 0; i < request.requestTypes.Length; i++)
                {
                    var param = "request";
                    if (i > 0) param += i + 1;
                    paramNames.Add(param);
                }

            paramNames.Add("sender");

            //builder.AppendLine("[Command]");
            builder.AppendLine("[Command(requiresAuthority = false)]");
            builder.AppendLine($"public void CmdHandleRequest_{request.Name}({string.Join(", ", @params)})");
            builder.OpenCurly();
            builder.AppendLine($"PushRequestOnServer(typeof({request.Name}), {string.Join(", ", paramNames)});");
            builder.CloseCurly();
        }

        private static void AddResponse(CodeGenBuilder builder, Type responseType)
        {
            builder.AppendLine("[TargetRpc]");
            builder.AppendLine("public void TargetReceiveResponse$(NetworkConnection target, " +
                               "int requestId, Status status, $ response)",
                RequestSettings.CurrentSerializer, responseType.Name);
            builder.OpenCurly();
            builder.AppendLine("PushResponseOnClient(target, requestId, status, response);");
            builder.CloseCurly();
        }

        [Serializable]
        private class RequestMeta
        {
            // ReSharper disable once InconsistentNaming
            public string Name;
            public SerializableType[] requestTypes;
            public SerializableType responseType;

            public static bool operator ==(RequestMeta meta1, RequestMeta meta2)
            {
                return meta1?.Name == meta2?.Name;
            }

            public static bool operator !=(RequestMeta meta1, RequestMeta meta2)
            {
                return meta1?.Name != meta2?.Name;
            }
        }
    }
}