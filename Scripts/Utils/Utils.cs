using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RequestForMirror.Utils
{
    public static class Utils
    {
        public static Tuple<T1, T2>[] ToTuples<T1, T2>(T1[] arr1, T2[] arr2, bool suppressWarnings = false)
        {
            var count = Mathf.Min(arr1.Length, arr2.Length);
            if (!suppressWarnings && arr1.Length != arr2.Length)
                Debug.LogWarning("List length mismatch. Returned tuples are truncated.");

            var result = new Tuple<T1, T2>[count];
            for (var i = 0; i < count; i++) result[i] = Tuple.Create(arr1[i], arr2[i]);

            return result;
        }

        public static Tuple<T1, T2>[] ToTuplesWith<T1, T2>(this IEnumerable<T1> arr, IEnumerable<T2> other,
            bool suppressWarnings = false)
        {
            return ToTuples(arr.ToArray(), other.ToArray(), suppressWarnings);
        }
    }
}