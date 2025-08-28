// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework.Internal;
using NUnit.Framework;
using UnityEditorInternal.VersionControl;

namespace MVVMDatabinding
{
    public static class ReflectionUtils
    {
        private static readonly Type[] zeroParams = new Type[0];

        public static bool IsNestedProperty(string propertyPath)
        {
            return propertyPath.LastIndexOf(".") != -1;
        }

        public static bool IsNestedProperty(SerializedProperty property)
        {
            return IsNestedProperty(property.propertyPath);
        }

        public static bool IsPropertyAListItem(string propertyPath)
        {
            return propertyPath.LastIndexOf("]") != -1;
        }

        public static bool IsPropertyAListItem(SerializedProperty property)
        {
            return IsPropertyAListItem(property.propertyPath);
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

        public static bool TrySetValue<FieldType>(string name, Type targetType, object targetObject, FieldType value)
        {
            // first try properties
            if (TrySetPropertyValue<FieldType>(name, targetType, targetObject, value))
            {
                return true;
            }

            //TODO : add field setting
            // then try fields 
            //if (TryGetField<FieldType>(name, targetType, targetObject, out value))
            //{
            //    return true;
            //}


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

        public static bool DigForValue<FieldType>(string propertyPath, Type targetType, object targetObject, out FieldType value)
        {
            propertyPath = propertyPath.Trim('.');
            if (!IsNestedProperty(propertyPath) && !IsPropertyAListItem(propertyPath))
            {
                return TryGetValue<FieldType>(propertyPath, targetType, targetObject, out value);
            }

            // first check for list items
            string[] listItemChildren = propertyPath.Split(".Array.data[");
            object target = targetObject;

            bool targetingListItemItself = false;
            for (int i = 0; i < listItemChildren.Length; i++)
            {
                int arrayElemIdx = listItemChildren[i].IndexOf("]");

                string pathToCheck = listItemChildren[i];
                string finalChildName = string.Empty;
                if (arrayElemIdx != -1)
                {
                    // remove any array element index values and trim any stray '.' from the ends
                    pathToCheck = listItemChildren[i].Substring(arrayElemIdx + 1, listItemChildren[i].Length - arrayElemIdx - 1).Trim('.');
                }

                if (i + 1 == listItemChildren.Length)
                {
                    // last item
                    // let's only grab up to the second to last child
                    int lastItemIdx = pathToCheck.IndexOf('.');
                    if (lastItemIdx != -1)
                    {
                        finalChildName = pathToCheck.Substring(lastItemIdx, pathToCheck.Length - lastItemIdx).Trim('.');
                        pathToCheck = pathToCheck.Substring(0, lastItemIdx);
                    }
                    else
                    {
                        finalChildName = pathToCheck;
                    }
                }

                if (finalChildName != pathToCheck && !GetChildAtEndOfPath<object>(pathToCheck, target.GetType(), target, out target))
                {
                    break;
                }

                if (!string.IsNullOrWhiteSpace(finalChildName))
                {
                    return TryGetValue<FieldType>(finalChildName, target.GetType(), target, out value);
                }
                else
                {
                    // check for element index in next child
                    if (i + 1 < listItemChildren.Length)
                    {
                        int elemIdxEnd = listItemChildren[i + 1].IndexOf("]");
                        int itemIndex = Convert.ToInt32(listItemChildren[i + 1].Substring(0, elemIdxEnd));
                        if (target is IEnumerable listTarget)
                        {
                            var iterator = listTarget.GetEnumerator();
                            for (int j = 0; j <= itemIndex; j++)
                            {
                                if (!iterator.MoveNext())
                                {
                                    break;
                                }
                            }

                            target = iterator.Current;
                            i++;
                            targetingListItemItself = (i == listItemChildren.Length - 1);
                        }
                    }
                }
            }

            value = targetingListItemItself ? (FieldType)target : default;
            return targetingListItemItself;
        }

        private static bool GetChildAtEndOfPath<FieldType>(string path, Type targetType, object targetObject, out FieldType value)
        {
            string[] children = path.Split('.');

            if (children.Length > 0)
            {
                object target = targetObject;
                for (int i = 0; i < children.Length; i++)
                {
                    // last element
                    if (i == children.Length - 1)
                    {
                        return TryGetValue<FieldType>(children[i], target.GetType(), target, out value);
                    }

                    if (IsPropertyAListItem(children[i]))
                    {
                        // TODO: extract index and get object
                        // might need to split on list items first to account for the .Array.data[xx] bit in the property path
                        //int bracketIdx = children[i].IndexOf('[');
                        //int closeBracketIdx = children[i].IndexOf(']');
                        //int indexInList = Convert.ToInt32(children[i].Substring(bracketIdx + 1, closeBracketIdx - bracketIdx));
                        //if (!TryGet)
                        value = default;
                        return false;
                    }
                    else
                    {
                        TryGetValue<object>(children[i], target.GetType(), target, out target);
                    }
                }
            }

            value = default;
            return false;
        }

        public static bool DigForValueAndSet<FieldType>(string propertyPath, Type targetType, object targetObject, FieldType value)
        {
            propertyPath = propertyPath.Trim('.');
            if (!IsNestedProperty(propertyPath) || !IsPropertyAListItem(propertyPath))
            {
                return TrySetValue<FieldType>(propertyPath, targetType, targetObject, value);
            }

            // first check for list items
            string[] listItemChildren = propertyPath.Split(".Array.data[");
            object target = targetObject;

            for (int i = 0; i < listItemChildren.Length; i++)
            {
                int arrayElemIdx = listItemChildren[i].IndexOf("]");

                string pathToCheck = listItemChildren[i];
                string finalChildName = string.Empty;
                if (arrayElemIdx != -1)
                {
                    // remove any array element index values and trim any stray '.' from the ends
                    pathToCheck = listItemChildren[i].Substring(arrayElemIdx + 1, listItemChildren[i].Length - arrayElemIdx - 1).Trim('.');
                }

                if (i + 1 == listItemChildren.Length)
                {
                    // last item
                    // let's only grab the second to last child
                    int lastItemIdx = pathToCheck.IndexOf('.');
                    if (lastItemIdx != -1)
                    {
                        finalChildName = pathToCheck.Substring(lastItemIdx, pathToCheck.Length - lastItemIdx).Trim('.');
                        pathToCheck = pathToCheck.Substring(0, lastItemIdx);
                    }
                    else
                    {
                        finalChildName = pathToCheck;
                    }
                }

                if (finalChildName != pathToCheck && !GetChildAtEndOfPath<object>(pathToCheck, target.GetType(), target, out target))
                {
                    break;
                }

                if (!string.IsNullOrWhiteSpace(finalChildName))
                {
                    return TrySetValue<FieldType>(finalChildName, target.GetType(), target, value);
                }
                else
                {
                    // check for element index in next child
                    if (i + 1 < listItemChildren.Length)
                    {
                        int elemIdxEnd = listItemChildren[i + 1].IndexOf("]");
                        int itemIndex = Convert.ToInt32(listItemChildren[i + 1].Substring(0, elemIdxEnd));
                        if (target is IEnumerable listTarget)
                        {
                            var iterator = listTarget.GetEnumerator();
                            for (int j = 0; j <= itemIndex; j++)
                            {
                                if (!iterator.MoveNext())
                                {
                                    break;
                                }
                            }

                            target = iterator.Current;
                        }
                    }
                }
            }

            value = default;
            return false;
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