using UnityEngine;

namespace RequestForMirror.Editor.CodeGen
{
    /// <summary>
    ///     Generates example MonoBehaviour that prints "Hello World" to console on Start().
    /// </summary>
    public class ExampleBuilder : CodeGenBuilder
    {
        private const string HelloWorld = "\"Hello World\"";
        public const string ClassName = "HelloWorldApp";

        public void Build()
        {
            Using("Modula");
            Using("System");
            Using("UnityEngine");

            EmptyLines(2);

            Class(Scope.Public, ClassName, typeof(MonoBehaviour));
            AppendLine("public void Start()");
            OpenCurly();
            AppendLine("Debug.Log($);", HelloWorld);

            Endfile();
        }
    }
}