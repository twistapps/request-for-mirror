using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RequestForMirror.Utils
{
    public static class SettingsUtility
    {
        private static readonly Dictionary<Type, SettingsAsset> Settings = new Dictionary<Type, SettingsAsset>();
        private static string TwistappsFolder => Path.Combine("Assets", "TwistApps", "Resources", "Settings");
        public static T Load<T>() where T : SettingsAsset
        {
            var type = typeof(T);
            T asset;
            if (Settings.ContainsKey(type))
            {
                asset = (T)Settings[type];
                if (asset != null)
                    return asset;

                Settings.Remove(type);
            }
            
            //var settingsPath = Path.Combine(TwistappsFolder, settingsType.Name) + ".asset";
            //asset = (T)AssetDatabase.LoadAssetAtPath(settingsPath, settingsType);
            asset = Resources.Load<T>(Path.Combine("Settings", type.Name));
            if (asset != null)
            {
                Settings.Add(type, asset);
                return asset;
            }
            
#if UNITY_EDITOR
            
            //if settings file not found at desired location
            asset = ScriptableObject.CreateInstance<T>();
            var assetPath = Path.Combine(TwistappsFolder, type.Name) + ".asset";
            Directory.CreateDirectory(TwistappsFolder);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            Settings.Add(type, asset);
            return asset;
            
#else
            Debug.LogError($"Settings file {typeof(T).Name} not found in Resources!");
            return null;
#endif
        }
    }
}