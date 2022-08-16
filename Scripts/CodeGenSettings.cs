using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
namespace RequestForMirror
{
    public class CodeGenSettings : ScriptableObject
    {
        public bool autoGenerateOnCompile = true;
        public bool debugMode = false;
        public List<string> generatedFiles = new List<string>();
    }
}