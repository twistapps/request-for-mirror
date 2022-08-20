using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
//using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RequestForMirror.Editor
{
    public static class EditorUtils
    {
        //cache results of GetDerivedFrom() because it's a pretty expensive method
        private static readonly Dictionary<Type, Type[]> DerivativesDictionary = new Dictionary<Type, Type[]>();
        
        private static readonly Dictionary<Type, SettingsAsset> SettingsAssets = new Dictionary<Type, SettingsAsset>();
        private static string TwistappsFolder => Path.Combine("Assets", "TwistApps", "Resources", "Settings");
        
        public static Type[] GetDerivedFrom<T>(params Type[] ignored)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Type[] foundArr;
            if (DerivativesDictionary.ContainsKey(typeof(T)))
            {
                // return cached result if GetDerivedFrom() has already been invoked before
                foundArr = DerivativesDictionary[typeof(T)];
            }
            else
            {
                var found = from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                    from assemblyType in domainAssembly.GetTypes()
                    where typeof(T).IsAssignableFrom(assemblyType)
                    select assemblyType;

                foundArr = found as Type[] ?? found.ToArray();

                DerivativesDictionary.Add(typeof(T), foundArr);
            }

            if (ignored != null)
                foundArr = foundArr.Where(t => !ignored.Contains(t)).ToArray();

            stopwatch.Stop();
            // if (stopwatch.ElapsedMilliseconds > 0)
            //     Debug.Log($"GetDerivedFrom<{typeof(T).Name}>() took {stopwatch.ElapsedMilliseconds}ms to execute");
            return foundArr;
        }
        
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