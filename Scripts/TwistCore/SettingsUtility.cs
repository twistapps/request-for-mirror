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
        private static readonly Dictionary<Type, SettingsAsset> SettingsAssets = new Dictionary<Type, SettingsAsset>();
        private static string TwistappsFolder => Path.Combine("Assets", "TwistApps", "Resources", "Settings");
        public static T Load<T>() where T : SettingsAsset
        {
            var settingsType = typeof(T);
            T asset;
            if (SettingsAssets.ContainsKey(settingsType))
            {
                asset = (T)SettingsAssets[settingsType];
                if (asset != null)
                    return asset;

                SettingsAssets.Remove(settingsType);
            }
            
            //var settingsPath = Path.Combine(TwistappsFolder, settingsType.Name) + ".asset";
            //asset = (T)AssetDatabase.LoadAssetAtPath(settingsPath, settingsType);
            asset = Resources.Load<T>(Path.Combine("Settings", settingsType.Name));
            if (asset != null)
            {
                SettingsAssets.Add(settingsType, asset);
                return asset;
            }
            
#if UNITY_EDITOR
            
            //if settings file not found at desired location
            var settingsPath = Path.Combine(TwistappsFolder, settingsType.Name) + ".asset";
            asset = ScriptableObject.CreateInstance<T>();
            Directory.CreateDirectory(TwistappsFolder);
            AssetDatabase.CreateAsset(asset, settingsPath);
            AssetDatabase.SaveAssets();

            SettingsAssets.Add(settingsType, asset);
            return asset;
            
#else
            Debug.LogError($"Settings file {typeof(T).Name} not found in Resources!");
            return null;
#endif
        }
    }
}