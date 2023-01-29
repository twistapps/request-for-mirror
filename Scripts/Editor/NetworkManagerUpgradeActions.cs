#if MIRROR
using System;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using Object = UnityEngine.Object;

namespace RequestForMirror
{
    public static class NetworkManagerUpgradeActions
    {
        [MenuItem("Tools/Twist Apps/Commands/Upgrade Network Managers in scene")]
        public static void UpgradeNetManagersInScene()
        {
            foreach (var networkManager in Object.FindObjectsOfType<NetworkManager>())
            {
                var type = networkManager.GetType();
                if (type.IsSubclassOf(typeof(NetworkManager))) continue;

                var fieldInfos = type.GetFields();
                var fields = new Dictionary<string, object>();
                if (fields == null) throw new ArgumentNullException(nameof(fields));
                foreach (var fieldInfo in fieldInfos) fields[fieldInfo.Name] = fieldInfo.GetValue(networkManager);

                var gameObject = networkManager.gameObject;
                Undo.DestroyObjectImmediate(networkManager);
                var newNetworkManager = Undo.AddComponent<NetworkManagerWithEvents>(gameObject);
                foreach (var fieldInfo in newNetworkManager.GetType().GetFields())
                {
                    if (!fields.ContainsKey(fieldInfo.Name)) continue;
                    fieldInfo.SetValue(newNetworkManager, fields[fieldInfo.Name]);
                }
            }
        }

        public static void UpgradeInheritanceInCodebase()
        {
            //todo: replace ": NetworkManager" with ": NetworkManagerWithEvents" in every cs file;
            //show confirm dialog because this is indiscriminate method.
        }
    }
}
#endif