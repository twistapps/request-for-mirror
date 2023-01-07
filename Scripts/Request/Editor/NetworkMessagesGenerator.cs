using System;
using System.IO;
using TwistCore;
using TwistCore.CodeGen.Editor;
using UnityEditor.Callbacks;

namespace RequestForMirror.Editor
{
    public static class NetworkMessagesGenerator
    {
        private const string NetworkMessageTemplatesFolder = "NetworkMessages";
        
        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            CodeGen.ShouldGenerateCs += ShouldGenerateCs;
        }

        private static void GenerateCsNetworkMessages(Type type)
        {
            var builder = new CodeGenTemplateBuilder();
            //var template = Path.ChangeExtension(CodeGen.FindTxtTemplate(type), "NetworkMessage.txt");
            var outputPath = Path.Combine(CodeGenDefinitions.GeneratedFolder, NetworkMessageTemplatesFolder, type.Name + ".cs");
            
            builder.SetVariablesForType(type);
            //builder.GenerateFromTemplate(template);
            var camelCaseName = char.ToLower(type.Name[0]) + type.Name.Substring(1);
            
            builder.Class(Scope.Public, "Request", @static:true, partial:true);
            builder.AppendLine($"public static readonly {type.Name} {camelCaseName} = new {type.Name}();");
            builder.Endfile();
            builder.SaveToCsFile(outputPath);
        }
        
        private static bool ShouldGenerateCs(Type type)
        {
            if (!typeof(IRequest).IsAssignableFrom(type)) return true;
            var settings = SettingsUtility.Load<RequestSettings>();

            if (settings.transportMethod == NetworkTransportMethod.NetworkMessages)
                GenerateCsNetworkMessages(type);

            return settings.transportMethod switch
            {
                NetworkTransportMethod.HighLevelCommands => true,
                NetworkTransportMethod.NetworkMessages => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}