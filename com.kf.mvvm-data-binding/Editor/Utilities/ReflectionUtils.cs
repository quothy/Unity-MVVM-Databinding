using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework.Internal;

namespace MVVMDatabinding
{
    public static class ReflectionUtils
    {
        private static readonly Type[] zeroParams = new Type[0];

        public static bool IsNestedProperty(SerializedProperty property)
        {
            return property.propertyPath.LastIndexOf(".") != -1;
        }

        public static bool IsPropertyAListItem(SerializedProperty property)
        {
            int lastIdx = property.propertyPath.LastIndexOf(".");
            return property.propertyPath[lastIdx - 1] == ']';
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

        public static bool TryGetValue<FieldType>(string name, Type targetType, object targetObject, out FieldType value)
        {
            // first try properties
            if (TryFindPropertyGetter<FieldType>(name, targetType, targetObject, out Func<FieldType> getterFunc))
            {
                value = getterFunc();
                return true;
            }

            // then try fields 
            if (TryGetField<FieldType>(name, targetType, targetObject, out value))
            {
                return true;
            }


            value = default;
            return false;
        }


        public static bool TryGetListItem(string listName, int listIndex, object targetObject, out object listItem)
        {
            listItem = null;
            if (TryGetValue<IEnumerable>(listName, targetObject.GetType(), targetObject, out IEnumerable list))
            {
                var iterator = list.GetEnumerator();
                int count = 0;
                for (int i = 0; i <= listIndex; i++)
                {
                    if (!iterator.MoveNext())
                    {
                        return false;
                    }
                }

                listItem = iterator.Current;
            }
            return listItem != null;
        }


        public static bool TryFindPropertyGetter<FieldType>(string propertyName, Type targetType, object targetObject, out Func<FieldType> getterFunc, bool checkChildFields = false)
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
                    return getterFunc != null;
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

        public static bool TryFindPropertyGetterInChild<FieldType>(string propertyName, string childName, Type targetType, object targetObject, out Func<FieldType> getterFunc)
        {
            FieldInfo childField = targetType.GetField(childName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (childField != null)
            {
                var childObject = childField.GetValue(targetObject);
                return TryFindPropertyGetter<FieldType>(propertyName, childField.FieldType, childObject, out getterFunc);
            }

            getterFunc = null;
            return false;
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


        public static bool TrySetPropertyInChild<FieldType>(string propertyName, string childName, Type targetType, object targetObject, FieldType valueToSet)
        {
            FieldInfo childField = targetType.GetField(childName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (childField != null)
            {
                var childObject = childField.GetValue(targetObject);
                return TrySetPropertyValue<FieldType>(propertyName, childField.FieldType, childObject, valueToSet);
            }

            return false;
        }

        public static bool TryGetField<FieldType>(string fieldName, Type targetType, object targetObject, out FieldType value)
        {
            Type typeToCheck = targetType;

            while (typeToCheck != null)
            {
                FieldInfo field = typeToCheck.GetField(fieldName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (field != null)
                {
                    value =  (FieldType)field.GetValue(targetObject);
                    return true;
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

            value = default;
            return false;
        }
    }
}