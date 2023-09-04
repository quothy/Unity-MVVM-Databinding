using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;


namespace MVVMDatabinding
{
    [CustomPropertyDrawer(typeof(DropdownSelectionAttribute))]
    public class DropdownSelectionPropertyDrawer : PropertyDrawer
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
                if (ReflectionUtils.IsNestedProperty(property) && ReflectionUtils.TryGetParentProperty(property, out SerializedProperty parent))
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
                if (ReflectionUtils.IsNestedProperty(property) && ReflectionUtils.TryGetParentProperty(property, out SerializedProperty parent))
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
                if (ReflectionUtils.IsNestedProperty(property) && ReflectionUtils.TryGetParentProperty(property, out SerializedProperty parent))
                {
                    if (parent.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var managedRef = parent.managedReferenceValue;
                        Type refType = managedRef.GetType();
                        ReflectionUtils.TrySetPropertyValue<string>(dropdownAttribute.SelectedOptionPropertyName, refType, managedRef, option);
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
