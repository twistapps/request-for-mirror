// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using TwistCore.Editor;
// using TwistCore.Editor.CodeGen;
// using UnityEditor;
// using UnityEditor.Callbacks;
// using UnityEngine;
//
// namespace RequestForMirror.Editor
// {
//     public class TargetReceiverGenerator : ScriptableSingleton<TargetReceiverGenerator>
//     {
//         private const string TargetReceiverClassname = "TargetReceiver";
//
//         [SerializeField] private HashSet<Type> responseTypesCached;
//
//         [SerializeField] private List<RequestMeta> requests = new List<RequestMeta>();
//
//
//         private static string TargetReceiverPath =>
//             Path.Combine(CodeGenDefinitions.GeneratedFolder, TargetReceiverClassname);
//
//         [DidReloadScripts(2)]
//         private static void OnScriptsReload()
//         {
//             var responseTypes = GetAllResponseTypes();
//             if (instance.responseTypesCached == null && responseTypes.Count == 0)
//             {
//                 Cleanup();
//                 return;
//             }
//
//             if (instance.responseTypesCached != null && !instance.ShouldGenerateReceiver(responseTypes)) return;
//             instance.responseTypesCached = responseTypes;
//
//             var builder = new CodeGenTemplateBuilder();
//             var template = CodeGen.FindTxtTemplate(typeof(Receiver));
//
//             instance.SetInner(builder);
//             builder.SetVariable("CLASSNAME", TargetReceiverClassname);
//             builder.GenerateFromTemplate(template);
//             builder.SaveToCsFile(TargetReceiverPath);
//         }
//
//         private static void Cleanup()
//         {
//             var outputPaths = new[]
//             {
//                 Path.ChangeExtension(TargetReceiverPath, ".cs"),
//                 Path.ChangeExtension(TargetReceiverPath, ".cs.meta")
//             };
//
//             foreach (var path in outputPaths)
//                 if (File.Exists(path))
//                     File.Delete(path);
//         }
//
//         private bool ShouldGenerateReceiver(IReadOnlyCollection<Type> currentResponseTypes)
//         {
//             if (!File.Exists(Path.ChangeExtension(TargetReceiverPath, ".cs"))) return true;
//             return currentResponseTypes.All(responseTypesCached.Contains) &&
//                    currentResponseTypes.Count == responseTypesCached.Count;
//         }
//
//         private static HashSet<Type> GetAllResponseTypes()
//         {
//             var requestTypes = EditorUtils.GetDerivedTypesExcludingSelf<IRequest>().Where(t => !t.IsAbstract);
//             var responseTypes = new List<Type>();
//             foreach (var requestType in requestTypes)
//             {
//                 responseTypes.AddRange(GetResponseTypesOfRequest(requestType));
//             }
//
//             return new HashSet<Type>(responseTypes);
//         }
//
//         private static List<Type> GetResponseTypesOfRequest(Type requestType)
//         {
//             var responseTypes = new List<Type>();
//             var genericArgs = requestType.BaseType!.GenericTypeArguments;
//             if (genericArgs == null || genericArgs.Length < 1) return responseTypes;
//
//             if (genericArgs.Length == 1) responseTypes.Add(genericArgs.First());
//             if (genericArgs.Length > 1) responseTypes.AddRange(genericArgs.Skip(1));
//
//             return responseTypes;
//         }
//
//         private static RequestMeta[] GetAllRequests()
//         {
//             var requestTypes = EditorUtils.GetDerivedTypesExcludingSelf<IRequest>().Where(t => !t.IsAbstract);
//
//             var list = new List<RequestMeta>();
//             foreach (var type in requestTypes)
//             {
//                 var genericArgs = type.BaseType.GenericTypeArguments;
//                 if (genericArgs != null && genericArgs.Length >= 1) 
//                     list.Add(new RequestMeta
//                     {
//                         Name = type.Name, 
//                         RequestData = genericArgs.Length > 1 ? genericArgs[0] : null, 
//                         ResponseData = SerializableType.ArrayFromListTypes(GetResponseTypesOfRequest(type))
//                     });
//             }
//
//             return list.ToArray();
//         }
//
//         private void SetInner(CodeGenTemplateBuilder builder)
//         {
//             builder.StartInner();
//
//             foreach (var responseType in responseTypesCached)
//             {
//                 builder.AppendLine("[TargetRpc]");
//                 builder.AppendLine(
//                     "public void TargetReceiveResponse$(NetworkConnection target, int requestId, Status status, $ response)",
//                     RequestSettings.CurrentSerializer, responseType.Name);
//                 builder.OpenCurly();
//                 builder.AppendLine("PushResponseOnClient(target, requestId, status, response);");
//                 builder.CloseCurly();
//             }
//
//             builder.EndInner();
//         }
//
//         [Serializable]
//         private struct RequestMeta
//         {
//             public string Name;
//             public SerializableType RequestData;
//             public SerializableType[] ResponseData;
//         }
//     }
// }

