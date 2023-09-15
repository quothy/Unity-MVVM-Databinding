using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVMDatabinding
{
    [CustomPropertyDrawer(typeof(ConditionalEnableAttribute))]
    public class ConditionalEnablePropertyDrawer : PropertyDrawer
    {
        private ConditionalEnableAttribute enableAttribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            enableAttribute = attribute as ConditionalEnableAttribute;

            bool comparisonValue = GetConditionValue(property);

            bool enabled = (comparisonValue && enableAttribute.Condition == ConditionalEnableAttribute.ConditionalEnableType.EnableIfTrue) ||
                                (!comparisonValue && enableAttribute.Condition == ConditionalEnableAttribute.ConditionalEnableType.DisableIfTrue);

            bool currentEnabled = GUI.enabled;
            GUI.enabled = currentEnabled && enabled;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = currentEnabled;
        }

        private bool GetConditionValue(SerializedProperty property)
        {
            bool comparisonValue = false;

            // early out if this element should never be enabled
            if (enableAttribute.Condition == ConditionalEnableAttribute.ConditionalEnableType.Never)
            {
                return comparisonValue;
            }

            if (ReflectionUtils.TryFindSerializedProperty(enableAttribute.ConditionPropertyName, property, out SerializedProperty targetProperty))
            {
                comparisonValue = targetProperty.boolValue;
            }
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var managedRef = property.managedReferenceValue;
                Type refType = managedRef.GetType();

                if (ReflectionUtils.TryFindPropertyGetter<bool>(enableAttribute.ConditionPropertyName, refType, managedRef, out Func<bool> getter))
                {
                    comparisonValue = getter();
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

                        if (ReflectionUtils.TryFindPropertyGetter<bool>(enableAttribute.ConditionPropertyName, refType, managedRef, out Func<bool> getter))
                        {
                            comparisonValue = getter();
                        }
                    }
                }
                else
                {
                    if (ReflectionUtils.TryFindPropertyGetter<bool>(enableAttribute.ConditionPropertyName, property.serializedObject.GetType(), property.serializedObject, out Func<bool> getter))
                    {
                        comparisonValue = getter();
                    }
                }
            }

            return comparisonValue;
        }
    }
}
