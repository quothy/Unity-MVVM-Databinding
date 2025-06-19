// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace MVVMDatabinding
{
    [CustomPropertyDrawer(typeof(DropdownSelectionAttribute))]
    public class DropdownSelectionPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private DropdownSelectionAttribute dropdownAttribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            dropdownAttribute = attribute as DropdownSelectionAttribute;

            EditorGUI.BeginChangeCheck();
            List<string> options = GetOptions(property);
            string selectedOption = GetSelectedOption(property);

            int idx = int.MinValue;
            int counter = 0;
            foreach (string option in options)
            {
                if (option == selectedOption)
                {
                    idx = counter;
                    break;
                }
                counter++;
            }

            if (idx == int.MinValue)
            {
                return;
            }

            int selectedIdx = EditorGUI.Popup(position, "Bound Item", idx, options.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Select data item");
                string option = options[selectedIdx];
                SetSelectedOption(property, option);
                PrefabUtility.RecordPrefabInstancePropertyModifications(property.serializedObject.targetObject);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private void ShowDropdown(SerializedProperty property)
        {
            List<string> options = GetOptions(property);

            GenericMenu menu = new GenericMenu();
            foreach (string option in options)
            {
                menu.AddItem(new GUIContent(option), false,
                (object t) =>
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Select data item");

                    SetSelectedOption(property, t as string);

                    PrefabUtility.RecordPrefabInstancePropertyModifications(property.serializedObject.targetObject);
                    property.serializedObject.ApplyModifiedProperties();
                },
                option);
            }
            menu.ShowAsContext();
        }

        private List<string> GetOptions(SerializedProperty property)
        {
            List<string> options = null;

            if (ReflectionUtils.TryFindSerializedProperty(dropdownAttribute.OptionsSourcePropertyName, property, out SerializedProperty selectedOptionProperty))
            {
                options = new List<string>(selectedOptionProperty.arraySize);
                for (int i = 0; i < selectedOptionProperty.arraySize; i++)
                {
                    options.Add(selectedOptionProperty.GetArrayElementAtIndex(i).stringValue);
                }
            }
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var managedRef = property.managedReferenceValue;
                Type refType = managedRef.GetType();

                if (ReflectionUtils.TryFindPropertyGetter<List<string>>(dropdownAttribute.OptionsSourcePropertyName, refType, managedRef, out Func<List<string>> getter))
                {
                    options = getter();
                }
            }
            else
            {
                if (ReflectionUtils.IsPropertyAListItem(property))
                {
                    //int brackIdx = property.propertyPath.LastIndexOf('[');
                    //int listNameIdxEnd = property.propertyPath.IndexOf(".Array");
                    //SerializedProperty listProp = property.serializedObject.FindProperty(property.propertyPath.Substring(0, listNameIdxEnd));

                    //int elementIdx = Convert.ToInt32(property.propertyPath.Substring(brackIdx + 1, 1));

                    //string listPropName = property.propertyPath.Substring(0, listNameIdxEnd);
                    //int endOfListSyntaxIndex = property.propertyPath.LastIndexOf("].");
                    //endOfListSyntaxIndex++;
                    //string afterListItemName = string.Empty;
                    //if (endOfListSyntaxIndex != -1)
                    //{
                    //    afterListItemName = endOfListSyntaxIndex + 1 >= property.propertyPath.Length ? string.Empty : property.propertyPath.Substring(endOfListSyntaxIndex + 1, property.propertyPath.Length - endOfListSyntaxIndex - 1);

                    //    int lastChildStartIdx = afterListItemName.LastIndexOf('.');
                    //    if (lastChildStartIdx > 0)
                    //    {
                    //        afterListItemName = afterListItemName.Substring(0, lastChildStartIdx);
                    //    }
                    //    else
                    //    {
                    //        afterListItemName = string.Empty;
                    //    }
                    //}

                    int lastItemIdx = property.propertyPath.LastIndexOf('.');
                    string path = property.propertyPath.Substring(0, lastItemIdx);

                    ReflectionUtils.DigForValue<List<string>>(path + "." + dropdownAttribute.OptionsSourcePropertyName, property.serializedObject.targetObject.GetType(), property.serializedObject.targetObject, out options);

                }
                else if (ReflectionUtils.IsNestedProperty(property) && ReflectionUtils.TryGetParentProperty(property, out SerializedProperty parent))
                {
                    if (parent.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var managedRef = parent.managedReferenceValue;
                        Type refType = managedRef.GetType();

                        if (ReflectionUtils.TryFindPropertyGetter<List<string>>(dropdownAttribute.OptionsSourcePropertyName, refType, managedRef, out Func<List<string>> getter))
                        {
                            options = getter();
                        }
                    }
                    else
                    {
                        if (ReflectionUtils.TryFindPropertyGetterInChild<List<string>>(dropdownAttribute.OptionsSourcePropertyName, parent.name, parent.serializedObject.targetObject.GetType(), parent.serializedObject.targetObject, out Func<List<string>> getter))
                        {
                            options = getter();
                        }
                    }
                }
                else
                {
                    if (ReflectionUtils.TryFindPropertyGetter<List<string>>(dropdownAttribute.OptionsSourcePropertyName, property.serializedObject.GetType(), property.serializedObject, out Func<List<string>> getter))
                    {
                        options = getter();
                    }
                }
            }

            return options;
        }

        private string GetSelectedOption(SerializedProperty property)
        {
            string selected = string.Empty;

            if (ReflectionUtils.TryFindSerializedProperty(dropdownAttribute.SelectedOptionPropertyName, property, out SerializedProperty selectedOptionProperty))
            {
                selected = selectedOptionProperty.stringValue;
            }
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var managedRef = property.managedReferenceValue;
                Type refType = managedRef.GetType();

                if (ReflectionUtils.TryFindPropertyGetter<string>(dropdownAttribute.SelectedOptionPropertyName, refType, managedRef, out Func<string> getter))
                {
                    selected = getter();
                }
            }
            else
            {
                if (ReflectionUtils.IsPropertyAListItem(property))
                {

                    int lastItemIdx = property.propertyPath.LastIndexOf('.');
                    string path = property.propertyPath.Substring(0, lastItemIdx);

                    ReflectionUtils.DigForValue<string>(path + "." + dropdownAttribute.SelectedOptionPropertyName, property.serializedObject.targetObject.GetType(), property.serializedObject.targetObject, out selected);
                }
                else if (ReflectionUtils.IsNestedProperty(property) && ReflectionUtils.TryGetParentProperty(property, out SerializedProperty parent))
                {
                    if (parent.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var managedRef = parent.managedReferenceValue;
                        Type refType = managedRef.GetType();

                        if (ReflectionUtils.TryFindPropertyGetter<string>(dropdownAttribute.SelectedOptionPropertyName, refType, managedRef, out Func<string> getter))
                        {
                            selected = getter();
                        }
                    }
                    else
                    {
                        if (ReflectionUtils.TryFindPropertyGetterInChild<string>(dropdownAttribute.SelectedOptionPropertyName, parent.name, parent.serializedObject.targetObject.GetType(), parent.serializedObject.targetObject, out Func<string> getter))
                        {
                            selected = getter();
                        }
                    }
                }
                else
                {
                    if (ReflectionUtils.TryFindPropertyGetter<string>(dropdownAttribute.SelectedOptionPropertyName, property.serializedObject.GetType(), property.serializedObject, out Func<string> getter))
                    {
                        selected = getter();
                    }
                }
            }

            return selected;
        }

        private void SetSelectedOption(SerializedProperty property, string option)
        {
            if (ReflectionUtils.TryFindSerializedProperty(dropdownAttribute.SelectedOptionPropertyName, property, out SerializedProperty selectedOptionProperty))
            {
                selectedOptionProperty.stringValue = option;
            }
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var managedRef = property.managedReferenceValue;
                Type refType = managedRef.GetType();

                ReflectionUtils.TrySetPropertyValue<string>(dropdownAttribute.SelectedOptionPropertyName, refType, managedRef, option);
            }
            else
            {
                if (ReflectionUtils.IsPropertyAListItem(property))
                {
                    int brackIdx = property.propertyPath.LastIndexOf('[');
                    int listNameIdxEnd = property.propertyPath.IndexOf(".Array");
                    SerializedProperty listProp = property.serializedObject.FindProperty(property.propertyPath.Substring(0, listNameIdxEnd));

                    int elementIdx = Convert.ToInt32(property.propertyPath.Substring(brackIdx + 1, 1));

                    string listPropName = property.propertyPath.Substring(0, listNameIdxEnd);
                    int endOfListSyntaxIndex = property.propertyPath.LastIndexOf("].");
                    endOfListSyntaxIndex++;
                    string afterListItemName = string.Empty;
                    if (endOfListSyntaxIndex != -1)
                    {
                        afterListItemName = endOfListSyntaxIndex + 1 >= property.propertyPath.Length ? string.Empty : property.propertyPath.Substring(endOfListSyntaxIndex + 1, property.propertyPath.Length - endOfListSyntaxIndex - 1);

                        int lastChildStartIdx = afterListItemName.LastIndexOf('.');
                        if (lastChildStartIdx > 0)
                        {
                            afterListItemName = afterListItemName.Substring(0, lastChildStartIdx);
                        }
                        else
                        {
                            afterListItemName = string.Empty;
                        }
                    }

                    int lastItemIdx = property.propertyPath.LastIndexOf('.');
                    string path = property.propertyPath.Substring(0, lastItemIdx);
                    ReflectionUtils.DigForValueAndSet<string>(path + "." + dropdownAttribute.SelectedOptionPropertyName, property.serializedObject.targetObject.GetType(), property.serializedObject.targetObject, option);

                    //if (ReflectionUtils.TryGetListItem(listPropName, elementIdx, property.serializedObject.targetObject, out object listItem))
                    //{
                    //    ReflectionUtils.DigForValueAndSet<string>(afterListItemName + "." + dropdownAttribute.SelectedOptionPropertyName, listItem.GetType(), listItem, option);
                    //}
                }
                else if (ReflectionUtils.IsNestedProperty(property) && ReflectionUtils.TryGetParentProperty(property, out SerializedProperty parent))
                {
                    if (parent.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var managedRef = parent.managedReferenceValue;
                        Type refType = managedRef.GetType();
                        ReflectionUtils.TrySetPropertyValue<string>(dropdownAttribute.SelectedOptionPropertyName, refType, managedRef, option);
                    }
                    else
                    {
                        ReflectionUtils.TrySetPropertyInChild<string>(dropdownAttribute.SelectedOptionPropertyName, parent.name, parent.serializedObject.targetObject.GetType(), parent.serializedObject.targetObject, option);
                    }
                }
                else
                {
                    ReflectionUtils.TrySetPropertyValue<string>(dropdownAttribute.SelectedOptionPropertyName, property.serializedObject.GetType(), property.serializedObject, option);
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
