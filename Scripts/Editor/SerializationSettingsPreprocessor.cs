#if REQUESTIFY_ENABLED
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TwistCore;
using TwistCore.Editor.CodeGen;
using UnityEditor.Callbacks;
using UnityEngine;

namespace RequestForMirror.Editor
{
    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    public static class SerializationSettingsPreprocessor
    {
        [DidReloadScripts(-10)]
        private static void OnScriptsReload()
        {
            CodeGen.OnBeforeCsFileGenerated += OnBeforeCsFileGeneration;
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

            if (argumentCount == 0) return;

            var responseIndex = argumentCount > 1 ? $"_{argumentCount}" : "";
            var key = $"{CodeGenTemplateBuilder.BaseSlug}_{CodeGenTemplateBuilder.GenericArgumentSlug}";

            builder.SetVariable("RESPONSE_TYPE", variables[key + responseIndex]);
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
        [SuppressMessage("ReSharper", "RedundantJumpStatement")]
        [ExecutionOrder(-1000)]
        private static void OnBeforeCsFileGeneration(CodeGenTemplateBuilder builder, Type type)
        {
            //Debug.Log("Preprocessing codegen file...");
            if (!typeof(IRequest).IsAssignableFrom(type) &&
                !typeof(Receiver).IsAssignableFrom(type)) return;
            //Debug.Log("Assignability test passed");


            var settings = SettingsUtility.Load<RequestSettings>();
            var serializerInUse = settings.serializationMethod;


            builder.SetVariable("SERIALIZER", RequestSettings.CurrentSerializer);
            if (serializerInUse == Serializer.JsonUtility) SetGenericArgsToString(builder);

            SetResponseVariable(builder, type);
        }
    }
}
#endif