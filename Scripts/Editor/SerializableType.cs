using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RequestForMirror.Editor
{
    [Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {
        [SerializeField] private string typeName;
        public Type SerializedType;

        public void OnBeforeSerialize()
        {
            typeName = SerializedType?.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(typeName))
            {
                SerializedType = null;
                return;
            }

            SerializedType = Type.GetType(typeName);
        }

        public static implicit operator SerializableType(Type type)
        {
            return new SerializableType
            {
                SerializedType = type
            };
        }

        public static explicit operator Type(SerializableType serializableType)
        {
            return serializableType.SerializedType;
        }

        public static SerializableType[] ArrayFromTypes(Type[] types)
        {
            if (types == null) return null;
            var arr = new SerializableType[types.Length];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = (SerializableType)types[i];
            }

            return arr;
        }

        public static SerializableType[] ArrayFromListTypes(List<Type> types)
        {
            if (types == null) return null;
            var arr = new SerializableType[types.Count];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = (SerializableType)types[i];
            }

            return arr;
        }

        public static SerializableType[] ArrayFromIEnumerable(IEnumerable<Type> types)
        {
            return ArrayFromTypes(types.ToArray());
        }
    }
}