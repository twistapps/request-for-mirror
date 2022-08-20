using System;
using RequestForMirror.Editor.CodeGen;
using UnityEditor.Callbacks;

namespace RequestForMirror.Editor.Request
{
    public static class SerializationSettingsPreprocessor
    {
        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            CodeGen.CodeGen.OnBeforeCsFileGeneration += OnBeforeCsFileGeneration;
        }

        private static void OnBeforeCsFileGeneration(CodeGenTemplateBuilder builder, Type type)
        {
            if (EditorUtils.LoadSettings<RequestSettings>().serializationMethod !=
                RequestSerializerType.JsonUtility) return;

            for (var i = 0; i < 4; i++)
            {
                var variableName =
                    (CodeGenTemplateBuilder.BaseSlug + CodeGenTemplateBuilder.GenericArgumentSlug + (i + 1)).Replace(
                        "_1", "");
                builder.SetVariable(variableName, "string");
            }
        }
    }
}