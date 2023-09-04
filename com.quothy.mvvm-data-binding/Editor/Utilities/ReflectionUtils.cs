using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MVVMDatabinding
{
    public static class ReflectionUtils
    {
        private static readonly Type[] zeroParams = new Type[0];

        public static bool IsNestedProperty(SerializedProperty property)
        {
            return property.propertyPath.LastIndexOf(".") != -1;
        }

        public static bool TryGetParentProperty(SerializedProperty property, out SerializedProperty parent)
        {
            parent = null;

            int idx = property.propertyPath.LastIndexOf(".");
            string path = property.propertyPath.Substring(0, idx);

            parent = property.serializedObject.FindProperty(path);

            return parent != null;
        }

        public static bool TryFindSerializedProperty(string targetPropertyName, SerializedProperty property, out SerializedProperty targetProperty)
        {
            string targetPropertyPath = targetPropertyName;
            int idx = property.propertyPath.LastIndexOf('.');
            if (idx != -1)
            {
                // get last instance of "." in the propertyPath
                string path = property.propertyPath;
                if (idx != -1)
                {
                    path = path.Substring(0, idx);
                    targetPropertyPath = $"{path}.{targetPropertyPath}";
                }
            }

            targetProperty = property.serializedObject.FindProperty(targetPropertyPath);
            return targetProperty != null;
        }

        public static bool TryFindPropertyGetter<FieldType>(string propertyName, Type targetType, object targetObject, out Func<FieldType> getterFunc)
        {
            getterFunc = null;

            Type typeToCheck = targetType;

            while (typeToCheck != null)
            {
                PropertyInfo info = typeToCheck.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, typeof(FieldType), zeroParams, null);
                if (info != null && info.CanRead)
                {
                    var owner = info.GetGetMethod(true).IsStatic ? null : targetObject;
                    getterFunc = () => { return (FieldType)info.GetValue(targetObject); };
                    break;
                }
                else if (typeToCheck.BaseType != null)
                {
                    typeToCheck = typeToCheck.BaseType;
                }
                else
                {
                    break;
                }
            }

            return getterFunc != null;
        }
        public static bool TrySetPropertyValue<FieldType>(string propertyName, Type targetType, object targetObject, FieldType valueToSet)
        {
            bool success = false;

            Type typeToCheck = targetType;

            while (typeToCheck != null)
            {
                PropertyInfo info = typeToCheck.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, typeof(FieldType), zeroParams, null);
                if (info != null && info.CanWrite)
                {
                    var owner = info.GetSetMethod(true).IsStatic ? null : targetObject;
                    info.SetValue(targetObject, valueToSet);
                    success = true;
                    break;
                }
                else if (typeToCheck.BaseType != null)
                {
                    typeToCheck = typeToCheck.BaseType;
                }
                else
                {
                    break;
                }
            }

            return success;
        }
    }
}