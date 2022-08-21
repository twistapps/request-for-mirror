using System;
using System.IO;
using System.Text;
using RequestForMirror.Utils;
using UnityEditor;

namespace RequestForMirror.Editor.CodeGen
{
    public enum Scope
    {
        Public,
        Protected,
        Private
    }

    public class CodeGenBuilder
    {
        protected const char SeparatorSymbol = '$';
        private const char Indent = ' ';
        private const int IndentAmountPerStep = 4;
        protected readonly StringBuilder stringBuilder;
        private int _indent;

        public CodeGenBuilder()
        {
            stringBuilder = new StringBuilder();
        }

        public void Using(string name)
        {
            AppendLine("using $;", name);
        }

        public void Class(Scope scope, string name, Type parentClass = null, bool _abstract = false,
            bool partial = false)
        {
            switch (scope)
            {
                case Scope.Public:
                    Append("public");
                    break;
                case Scope.Protected:
                    Append("protected");
                    break;
                case Scope.Private:
                    Append("private");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }

            Space();
            if (_abstract) Append("abstract ");
            if (partial) Append("partial ");
            Append("class $", name);
            if (parentClass != null) Append(" : $", parentClass.Name);
            OpenCurly();
        }

        public CodeGenBuilder Append(string line, params string[] lineInsertions)
        {
            var parts = line.Split(SeparatorSymbol);

            var counter = 0;
            foreach (var (text, insertion) in parts.ToTuplesWith(lineInsertions, true))
            {
                stringBuilder.Append(text + insertion);
                counter++;
            }

            for (var i = counter; i < parts.Length; i++) stringBuilder.Append(parts[i]);

            return this;
        }

        private string GetIndent()
        {
            return new string(Indent, _indent * IndentAmountPerStep);
        }

        public CodeGenBuilder AppendLine(string line, params string[] lineInsertions)
        {
            stringBuilder.AppendLine();
            stringBuilder.Append(GetIndent());
            Append(line, lineInsertions);
            return this;
        }

        public CodeGenBuilder AppendLine(bool ignoreIndent = false)
        {
            stringBuilder.AppendLine();
            if (!ignoreIndent) stringBuilder.Append(GetIndent());
            return this;
        }

        public void EmptyLines(int lines)
        {
            for (var i = 0; i < lines; i++) AppendLine();
        }

        private void Space(int amount = 1)
        {
            for (var i = 0; i < amount; i++) Append(" ");
        }

        public void OpenCurly()
        {
            AppendLine("{");
            _indent++;
        }

        public void CloseCurly()
        {
            _indent--;
            AppendLine("}");
        }

        //public abstract void Build();

        public void Endfile()
        {
            while (_indent > 0) CloseCurly();
        }

        public void SaveToCsFile(string path, bool keepBuilderDirty = false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, stringBuilder.ToString());
            AssetDatabase.Refresh();
            if (!keepBuilderDirty) Clear();
        }

        public void Clear()
        {
            stringBuilder.Clear();
            _indent = 0;
        }
    }
}