using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RequestForMirror.Editor
{
    public static class EditorUtils
    {
        //cache results of GetDerivedFrom() because it's a pretty expensive method
        private static readonly Dictionary<Type, Type[]> DerivativesDictionary = new Dictionary<Type, Type[]>();
        
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

        public static Type[] GetDerivedTypesExcludingSelf<T>(params Type[] ignored)
        {
            return GetDerivedFrom<T>(ignored.Append(typeof(T)).ToArray());
        }
    }
}