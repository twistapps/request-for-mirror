using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RequestForMirror.Editor.CodeGen
{
    public class CodeGenTemplateBuilder : CodeGenBuilder
    {
        public const string BaseSlug = "BASE_";
        public const string GenericArgumentSlug = "GENERIC_ARGUMENT_";
        private readonly Dictionary<string, string> _valuesToReplace = new Dictionary<string, string>();

        //todo: move so called 'cursor' to position of a variable and ability to perform standard Builder's actions from that position,
        // then reset cursor
        private string _mem;

        public void SetVariable(string key, string replacementValue)
        {
            if (_valuesToReplace.ContainsKey(key))
                _valuesToReplace[key] = replacementValue;
            else
                _valuesToReplace.Add(key, replacementValue);
        }

        public void SetVariablesForType(Type type)
        {
            SetVariable("CLASSNAME", type.Name);
            FindAndSetGenericArguments(type);
            if (type.BaseType != null) FindAndSetGenericArguments(type.BaseType, BaseSlug);
        }

        private void FindAndSetGenericArguments(Type type, string prefix = "")
        {
            //default variables
            var genericArguments = type.GetGenericArguments();

            for (var i = 0; i < genericArguments.Length; i++)
            {
                var genericArgument = genericArguments[i];
                var variableName = (prefix + GenericArgumentSlug + (i + 1)).Replace("_1", "");
                if (EditorUtils.LoadSettings<CodeGenSettings>().debugMode)
                    Debug.Log($"Setting builder variable: ${variableName}$");
                SetVariable(variableName, genericArgument.Name);
            }
        }

        public void GenerateFromTemplate(string templatePath)
        {
            var text = File.ReadAllText(templatePath);

            var parts = text.Split(SeparatorSymbol);
            for (var i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 1)
                {
                    if (!_valuesToReplace.ContainsKey(parts[i]))
                    {
                        Debug.LogWarning(
                            $"CodeGen: variable ${parts[i]}$ is not set for template {Path.GetFileName(templatePath)}. " +
                            "It will be replaced with empty string");
                        parts[i] = string.Empty;
                        continue;
                    }

                    parts[i] = _valuesToReplace[parts[i]];
                }

                Append(parts[i]);
            }
        }

        public void MoveCursorToVariable(string variableName)
        {
            var generated = stringBuilder.ToString();
        }
    }
}