using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RequestForMirror.Editor.Setup
{
    public static class DefineSymbolsSetter
    {
        [InitializeOnLoadMethod]
        public static void AddDefineSymbols()
        {
            var buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;

            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
            var symbolsHashSet = new HashSet<string>(symbols.Split(';')) { "REQUEST_FOR_MIRROR" };

            var modifiedSymbols = string.Join(";", symbolsHashSet);
            if (symbols == modifiedSymbols) return;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, modifiedSymbols);
            Debug.Log("Adding 'REQUEST_FOR_MIRROR' to scripting defines...");
        }
    }
}