// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
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
            if (IsVisible(property))
            {
                propertyHeight = base.GetPropertyHeight(property, label);
                if (property.hasVisibleChildren && property.isExpanded)
                {
                    SerializedProperty visibleChildrenIterator = property.Copy();
                    
                    while (visibleChildrenIterator.NextVisible(true) && property.depth < visibleChildrenIterator.depth)
                    {
                        propertyHeight += EditorGUI.GetPropertyHeight(visibleChildrenIterator, new GUIContent(visibleChildrenIterator.displayName), true);
                    }

                    propertyHeight += 8;
                }
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
                EditorGUI.PropertyField(position, property, label, property.isExpanded);
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

        private bool EvaluateComparison<T>(SerializedProperty property)
        {
            // early out if this element should never be enabled
            if (visibilityAttribute.Condition == ConditionResultType.Never)
            {
                return false;
            }

            bool comparisonResult = false;

            string conditionPath = visibilityAttribute.ConditionPropertyName;
            if (ReflectionUtils.IsNestedProperty(property.propertyPath))
            {
                int lastItemIdx = property.propertyPath.LastIndexOf('.');
                conditionPath = property.propertyPath.Substring(0, lastItemIdx + 1) + conditionPath;
            }

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
            else
            {
                object target = property.serializedObject.targetObject;
                if (ReflectionUtils.DigForValue<T>(conditionPath, target.GetType(), target, out T value))
                {
                    comparisonResult = EvaluateValue<T>(value);
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