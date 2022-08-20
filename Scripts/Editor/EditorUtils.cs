using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RequestForMirror.Editor
{
    public static class EditorUtils
    {
        //cache results of GetDerivedFrom() because it's a pretty expensive method
        private static readonly Dictionary<Type, Type[]> DerivativesDictionary = new Dictionary<Type, Type[]>();

        private static readonly Dictionary<Type, object> SettingsAssets = new Dictionary<Type, object>();
        private static string TwistappsFolder => Path.Combine("Assets", "TwistApps");

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

        public static T LoadSettings<T>() where T : SettingsAsset
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

            var settingsPath = Path.Combine(TwistappsFolder, settingsType.Name) + ".asset";
            var settings = AssetDatabase.LoadAssetAtPath(settingsPath, settingsType);

            if ((T)settings != null)
            {
                SettingsAssets.Add(settingsType, settings);
                return (T)settings;
            }

            //if settings file not found at desired location
            asset = ScriptableObject.CreateInstance<T>();
            Directory.CreateDirectory(TwistappsFolder);
            AssetDatabase.CreateAsset(asset, settingsPath);
            AssetDatabase.SaveAssets();

            SettingsAssets.Add(settingsType, settings);
            return (T)settings;
        }
    }
}