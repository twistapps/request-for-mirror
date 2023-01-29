using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        #if REQUESTIFY_ENABLED
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
        #else
        [DidReloadScripts(2)]
        private static void RunCleanup()
        {
            Cleanup();
        }
        #endif

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
            //@params.Add("NetworkConnectionToClient sender = null");

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("#if MIRROR");
            sb.AppendLine("NetworkConnectionToClient sender = null");
            sb.AppendLine("#elif UNITY_NETCODE");
            sb.AppendLine("ServerRpcParams sender = default");
            sb.AppendLine("#endif");
            sb.AppendLine();

            @params.Add(sb.ToString());

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
            builder.AppendLine("#if MIRROR");
            builder.AppendLine("[Command(requiresAuthority = false)]");

            builder.AppendLine("#elif UNITY_NETCODE");
            builder.AppendLine("[ServerRpc(RequireOwnership = false)]");

            builder.AppendLine("#endif");

            builder.AppendLine($"public void CmdHandleRequest_{request.Name}_ServerRpc({string.Join(", ", @params)})");
            builder.OpenCurly();
            builder.AppendLine($"PushRequestOnServer(typeof({request.Name}), {string.Join(", ", paramNames)});");
            builder.CloseCurly();
        }

        private static void AddResponse(CodeGenBuilder builder, Type responseType)
        {
            builder.AppendLine("#if MIRROR");

            builder.AppendLine("[TargetRpc]");
            builder.AppendLine("public void TargetReceiveResponse$(NetworkConnection target, " +
                               "int requestId, Status status, $ response)",
                RequestSettings.CurrentSerializer, responseType.Name);
            builder.OpenCurly();
            builder.AppendLine("PushResponseOnClient(target, requestId, status, response);");
            builder.CloseCurly();

            builder.AppendLine("#elif UNITY_NETCODE");

            builder.AppendLine("[ClientRpc]");
            builder.AppendLine("public void TargetReceiveResponse$(" +
                               "int requestId, Status status, $ response, " +
                               "ClientRpcParams target = default" +
                               ")",
                RequestSettings.CurrentSerializer, responseType.Name);
            builder.OpenCurly();
            builder.AppendLine("PushResponseOnClient(target, requestId, status, response);");
            builder.CloseCurly();

            builder.AppendLine("#endif");
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


            //////////////////////////////////////
            //////////////GENERATED//////////////
            /////////////////////////////////////
            protected bool Equals(RequestMeta other)
            {
                return Name == other.Name && Equals(requestTypes, other.requestTypes) &&
                       Equals(responseType, other.responseType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((RequestMeta)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Name != null ? Name.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (requestTypes != null ? requestTypes.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (responseType != null ? responseType.GetHashCode() : 0);
                    return hashCode;
                }
            }
            //////////////////////////////////////
        }
    }
}