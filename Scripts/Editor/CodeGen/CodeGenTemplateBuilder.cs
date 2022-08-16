using System.Collections.Generic;
using System.IO;

namespace RequestForMirror.Editor.CodeGen
{
    public class CodeGenTemplateBuilder : CodeGenBuilder
    {
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

        public void GenerateFromTemplate(string templatePath)
        {
            var text = File.ReadAllText(templatePath);

            var parts = text.Split(SeparatorSymbol);
            for (var i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 1) parts[i] = _valuesToReplace[parts[i]];

                Append(parts[i]);
            }
        }

        public void MoveCursorToVariable(string variableName)
        {
            var generated = stringBuilder.ToString();
        }
    }
}