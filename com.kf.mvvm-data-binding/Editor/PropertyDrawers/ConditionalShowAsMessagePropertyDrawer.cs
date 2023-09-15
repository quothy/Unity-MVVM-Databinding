// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding
{
    [CustomPropertyDrawer(typeof(ConditionalShowAsMessageAttribute))]
    public class ConditionalShowAsMessagePropertyDrawer : PropertyDrawer
    {
        private ConditionalShowAsMessageAttribute showAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                return 2 * EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return 0;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            showAttribute = attribute as ConditionalShowAsMessageAttribute;

            MessageType message = GetMessageType(showAttribute.Type);

            EditorGUI.HelpBox(position, property.stringValue, message);
        }

        private MessageType GetMessageType(ConditionalShowAsMessageAttribute.MessageType type)
        {
            switch(type)
            {
                case ConditionalShowAsMessageAttribute.MessageType.Info:
                    {
                        return MessageType.Info;
                    }
                case ConditionalShowAsMessageAttribute.MessageType.Warning:
                    {
                        return MessageType.Warning;
                    }
                case ConditionalShowAsMessageAttribute.MessageType.Error:
                    {
                        return MessageType.Error;
                    }
            }

            return MessageType.None;
        }


        private bool IsVisible(SerializedProperty property)
        {
            showAttribute = attribute as ConditionalShowAsMessageAttribute;

            bool comparisonValue = false;

            switch (showAttribute.ComparisonType)
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
            if (showAttribute.Condition == ConditionResultType.Never)
            {
                return false;
            }

            bool comparisonResult = false;

            if (ReflectionUtils.TryFindSerializedProperty(showAttribute.ComparisonPropertyName, property, out SerializedProperty targetProperty))
            {
                if (showAttribute.ComparisonType == ConditionComparisonType.Bool)
                {
                    comparisonResult = CompareBool(targetProperty.boolValue);
                }
                else if (showAttribute.ComparisonType == ConditionComparisonType.Enum)
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

                if (ReflectionUtils.TryFindPropertyGetter<T>(showAttribute.ComparisonPropertyName, refType, managedRef, out Func<T> getter))
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

                        if (ReflectionUtils.TryFindPropertyGetter<T>(showAttribute.ComparisonPropertyName, refType, managedRef, out Func<T> getter))
                        {
                            comparisonResult = EvaluateValue<T>(getter());
                        }
                    }
                }
                else
                {
                    if (ReflectionUtils.TryFindPropertyGetter<T>(showAttribute.ComparisonPropertyName, property.serializedObject.GetType(), property.serializedObject, out Func<T> getter))
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
            switch (showAttribute.ComparisonType)
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
            return (targetValue && showAttribute.Condition == ConditionResultType.ShowIfEquals) ||
                                (!targetValue && showAttribute.Condition == ConditionResultType.ShowIfNotEquals);
        }

        private bool CompareInt(int intValue)
        {
            bool result = false;
            switch (showAttribute.Condition)
            {
                case ConditionResultType.ShowIfEquals:
                    {
                        result = intValue == showAttribute.TargetIntValue;
                    }
                    break;
                case ConditionResultType.ShowIfNotEquals:
                    {
                        result = intValue != showAttribute.TargetIntValue;
                    }
                    break;
            }

            return result;
        }
    }
}