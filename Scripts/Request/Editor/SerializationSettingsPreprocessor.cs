using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RequestForMirror.Editor.CodeGen;
using RequestForMirror.Utils;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RequestForMirror.Editor.Request
{
    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    public static class SerializationSettingsPreprocessor
    {
        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            CodeGen.CodeGen.OnBeforeCsFileGeneration += OnBeforeCsFileGeneration;
        }

        private static void SetGenericArgsToString(CodeGenTemplateBuilder builder)
        {
            var variables = new Dictionary<string, string>(builder.Variables);
            foreach (var variable in variables)
            {
                Debug.Log($"{variable.Key} -- {variable.Key.Contains(CodeGenTemplateBuilder.GenericArgumentSlug)}");
                if (variable.Key.Contains(CodeGenTemplateBuilder.GenericArgumentSlug))
                {
                    builder.SetVariable(variable.Key, "string");
                    Debug.Log($"Set {variable.Key} to 'string'");
                }
            }
        }

        private static void SetResponseVariable(CodeGenTemplateBuilder builder, Type type)
        {
            var variables = builder.Variables;
            var genericArguments = type.BaseType!.GetGenericArguments();
            var argumentCount = genericArguments.Length;
        
            var responseIndex = argumentCount > 1 ? $"_{argumentCount}" : "";
            var key = $"{CodeGenTemplateBuilder.BaseSlug}_{CodeGenTemplateBuilder.GenericArgumentSlug}";
            
            builder.SetVariable("RESPONSE_TYPE", variables[key + responseIndex]);
        }
        
        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        [SuppressMessage("ReSharper", "RedundantJumpStatement")]
        private static void OnBeforeCsFileGeneration(CodeGenTemplateBuilder builder, Type type)
        {
            Debug.Log("Preprocessing codegen file...");
            if (!typeof(IRequest).IsAssignableFrom(type)) return;
            Debug.Log("Assignability test passed");
            
            
            var settings = SettingsUtility.Load<RequestSettings>();
            var serializerInUse = settings.serializationMethod;

            Debug.Log(serializerInUse);
            
            if (serializerInUse == RequestSerializerType.JsonUtility)
            {
                builder.SetVariable("SERIALIZER", "Json");
                SetGenericArgsToString(builder);
            }
            if (serializerInUse == RequestSerializerType.MirrorBuiltIn)
            {
                builder.SetVariable("SERIALIZER", "MirrorWeaver");
            }
            SetResponseVariable(builder, type);
        }
    }
}