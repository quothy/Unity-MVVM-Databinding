using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding
{
    [CustomPropertyDrawer(typeof(ConditionalVisibilityAttribute))]
    public class ConditionalVisibilityPropertyDrawer : PropertyDrawer
    {
        private ConditionalVisibilityAttribute visibilityAttribute;
        private float propertyHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return propertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            visibilityAttribute = attribute as ConditionalVisibilityAttribute;

            bool comparisonValue = GetConditionValue(property);


            bool conditionMet = (comparisonValue && visibilityAttribute.Condition == ConditionalVisibilityAttribute.ConditionalVisibilityType.ShowIfTrue) ||
                                (!comparisonValue && visibilityAttribute.Condition == ConditionalVisibilityAttribute.ConditionalVisibilityType.HideIfTrue);

            propertyHeight = base.GetPropertyHeight(property, label);
            if (conditionMet)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                propertyHeight = 0;
            }
        }

        private bool GetConditionValue(SerializedProperty property)
        {
            bool comparisonValue = false;

            // early out if this element should never be enabled
            if (visibilityAttribute.Condition == ConditionalVisibilityAttribute.ConditionalVisibilityType.Never)
            {
                return comparisonValue;
            }

            if (ReflectionUtils.TryFindSerializedProperty(visibilityAttribute.ConditionPropertyName, property, out SerializedProperty targetProperty))
            {
                comparisonValue = targetProperty.boolValue;
            }
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var managedRef = property.managedReferenceValue;
                Type refType = managedRef.GetType();

                if (ReflectionUtils.TryFindPropertyGetter<bool>(visibilityAttribute.ConditionPropertyName, refType, managedRef, out Func<bool> getter))
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

                        if (ReflectionUtils.TryFindPropertyGetter<bool>(visibilityAttribute.ConditionPropertyName, refType, managedRef, out Func<bool> getter))
                        {
                            comparisonValue = getter();
                        }
                    }
                }
                else
                {
                    if (ReflectionUtils.TryFindPropertyGetter<bool>(visibilityAttribute.ConditionPropertyName, property.serializedObject.GetType(), property.serializedObject, out Func<bool> getter))
                    {
                        comparisonValue = getter();
                    }
                }
            }

            return comparisonValue;
        }
    }
}