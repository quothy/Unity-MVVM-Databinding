using PlasticGui.Help;
using System;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVMDatabinding
{
    [CustomPropertyDrawer(typeof(ConditionalVisibilityAttribute))]
    public class ConditionalVisibilityPropertyDrawer : PropertyDrawer
    {
        private ConditionalVisibilityAttribute visibilityAttribute;
        private float propertyHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                propertyHeight = base.GetPropertyHeight(property, label);

            }
            else
            {
                propertyHeight = 0;
            }
            return propertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            visibilityAttribute = attribute as ConditionalVisibilityAttribute;

            bool comparisonValue = false;

            switch (visibilityAttribute.ComparisonType)
            {
                case ConditionComparisonType.Bool:
                    {
                        comparisonValue = EvaluateComparison<bool>(property);
                    }
                    break;

                case ConditionComparisonType.Enum:
                    {
                        comparisonValue = EvaluateComparison<Enum>(property);
                    }
                    break;
            }


            if (comparisonValue)
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        private bool IsVisible(SerializedProperty property)
        {
            visibilityAttribute = attribute as ConditionalVisibilityAttribute;

            bool comparisonValue = false;

            switch (visibilityAttribute.ComparisonType)
            {
                case ConditionComparisonType.Bool:
                    {
                        comparisonValue = EvaluateComparison<bool>(property);
                    }
                    break;

                case ConditionComparisonType.Enum:
                    {
                        comparisonValue = EvaluateComparison<Enum>(property);
                    }
                    break;
            }

            return comparisonValue;
        }

        private bool GetConditionValue(SerializedProperty property)
        {
            bool comparisonValue = false;

            // early out if this element should never be enabled
            if (visibilityAttribute.Condition == ConditionResultType.Never)
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

        private bool EvaluateComparison<T>(SerializedProperty property)
        {
            // early out if this element should never be enabled
            if (visibilityAttribute.Condition == ConditionResultType.Never)
            {
                return false;
            }

            bool comparisonResult = false;

            if (ReflectionUtils.TryFindSerializedProperty(visibilityAttribute.ConditionPropertyName, property, out SerializedProperty targetProperty))
            {
                if (visibilityAttribute.ComparisonType == ConditionComparisonType.Bool)
                {
                    comparisonResult = CompareBool(targetProperty.boolValue);
                }
                else if (visibilityAttribute.ComparisonType == ConditionComparisonType.Enum)
                {
                    int enumValueIdx = targetProperty.enumValueIndex;
                    // HACK: this is suuuuuuper fragile because it assumes the enum int value equals its index in the list. This really needs to
                    // be made more robust later to handle custom enum values/flagged enums.
                    var intValue = targetProperty.enumValueIndex;
                    comparisonResult = CompareInt(intValue);
                }
            }
            else if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                var managedRef = property.managedReferenceValue;
                Type refType = managedRef.GetType();

                if (ReflectionUtils.TryFindPropertyGetter<T>(visibilityAttribute.ConditionPropertyName, refType, managedRef, out Func<T> getter))
                {
                    comparisonResult = EvaluateValue<T>(getter());
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

                        if (ReflectionUtils.TryFindPropertyGetter<T>(visibilityAttribute.ConditionPropertyName, refType, managedRef, out Func<T> getter))
                        {
                            comparisonResult = EvaluateValue<T>(getter());
                        }
                    }
                }
                else
                {
                    if (ReflectionUtils.TryFindPropertyGetter<T>(visibilityAttribute.ConditionPropertyName, property.serializedObject.GetType(), property.serializedObject, out Func<T> getter))
                    {
                        comparisonResult = EvaluateValue<T>(getter());
                    }
                }
            }

            return comparisonResult;

        }

        private bool EvaluateValue<T>(T value)
        {
            bool result = false;
            switch (visibilityAttribute.ComparisonType)
            {
                case ConditionComparisonType.Bool:
                    {
                        result = CompareBool((bool)Convert.ChangeType(value, typeof(bool)));
                    }
                    break;
                case ConditionComparisonType.Enum:
                    {
                        Enum enumVal = (Enum)Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
                        int enumInt = (int)Enum.Parse(enumVal.GetType(), enumVal.ToString());
                        result = CompareInt(enumInt);
                    }
                    break;
            }

            return result;
        }

        private bool CompareBool(bool targetValue)
        {
            return (targetValue && visibilityAttribute.Condition == ConditionResultType.ShowIfEquals) ||
                                (!targetValue && visibilityAttribute.Condition == ConditionResultType.ShowIfNotEquals);
        }

        private bool CompareInt(int intValue)
        {
            bool result = false;
            switch (visibilityAttribute.Condition)
            {
                case ConditionResultType.ShowIfEquals:
                    {
                        result = intValue == visibilityAttribute.TargetIntValue;
                    }
                    break;
                case ConditionResultType.ShowIfNotEquals:
                    {
                        result = intValue != visibilityAttribute.TargetIntValue;
                    }
                    break;
            }

            return result;
        }
    }
}