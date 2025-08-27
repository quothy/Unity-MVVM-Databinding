// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding
{
    [CustomPropertyDrawer(typeof(ConditionalVisibilityAttribute))]
    public class ConditionalVisibilityPropertyDrawer : UnityEditor.PropertyDrawer
    {
        private ConditionalVisibilityAttribute visibilityAttribute;
        private float propertyHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
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

            if (IsVisible(property))
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

            if (visibilityAttribute.ConditionPropertyName.StartsWith("@"))
            {
                object target = property.serializedObject.targetObject;
                if (ReflectionUtils.IsNestedProperty(property))
                {
                    string nestedName = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.'));
                    if (ReflectionUtils.DigForValue<object>(nestedName, target.GetType(), target, out object nestedTarget))
                    {
                        target = nestedTarget;                        
                    }
                }
                return ConditionalExpressionEvaluator.Evaluate(visibilityAttribute.ConditionPropertyName.Substring(1), target);
            }

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

    public static class ConditionalExpressionEvaluator
    {
        public static bool Evaluate(string expr, object target)
        {
            var tokens = Tokenize(expr);
            int index = 0;
            return ParseOr(tokens, ref index, target);
        }

        private static List<string> Tokenize(string expr)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < expr.Length)
            {
                if (char.IsWhiteSpace(expr[i])) { i++; continue; }
                if (i + 1 < expr.Length)
                {
                    string two = expr.Substring(i, 2);
                    if (two == "==" || two == "!=" || two == "&&" || two == "||")
                    {
                        tokens.Add(two);
                        i += 2;
                        continue;
                    }
                }
                if (expr[i] == '(' || expr[i] == ')')
                {
                    tokens.Add(expr[i].ToString());
                    i++;
                    continue;
                }
                int start = i;
                while (i < expr.Length && (char.IsLetterOrDigit(expr[i]) || expr[i] == '.' || expr[i] == '_')) i++;
                if (start != i)
                {
                    tokens.Add(expr.Substring(start, i - start));
                    continue;
                }
                throw new Exception($"Unexpected character: {expr[i]}");
            }
            return tokens;
        }

        private static bool ParseOr(List<string> tokens, ref int index, object target)
        {
            bool left = ParseAnd(tokens, ref index, target);
            while (index < tokens.Count && tokens[index] == "||")
            {
                index++;
                bool right = ParseAnd(tokens, ref index, target);
                left = left || right;
            }
            return left;
        }

        private static bool ParseAnd(List<string> tokens, ref int index, object target)
        {
        bool left = ParseComparison(tokens, ref index, target);
        while (index < tokens.Count && tokens[index] == "&&")
        {
            index++;
            bool right = ParseComparison(tokens, ref index, target);
            left = left && right;
        }
        return left;
    }

        private static bool ParseComparison(List<string> tokens, ref int index, object target)
        {
            object left = ParseValue(tokens, ref index, target);
            if (index < tokens.Count && (tokens[index] == "==" || tokens[index] == "!="))
            {
                string op = tokens[index++];
                object right = ParseValue(tokens, ref index, target);
                bool eq = Equals(left, right);
                return op == "==" ? eq : !eq;
            }
            // If not a comparison, treat as bool
            if (left is bool b) return b;
            throw new Exception($"Expected boolean or comparison, got {left}");
        }

        private static object ParseValue(List<string> tokens, ref int index, object target)
        {
            string token = tokens[index];
            if (token == "(")
            {
                index++;
                var value = ParseOr(tokens, ref index, target);
                if (tokens[index] != ")") throw new Exception("Expected ')'");
                index++;
                return value;
            }
            if (token == "true" || token == "false")
            {
                index++;
                return bool.Parse(token);
            }
            // Support nested enum references: EffectData.ConversionTarget.Stacks
            if (token.Contains("."))
            {
                index++;
                var parts = token.Split('.');
                if (parts.Length < 2) throw new Exception($"Invalid enum value: {token}");
                // The last part is the enum value, the rest is the type path
                string enumValue = parts[parts.Length - 1];
                string enumTypeName = string.Join(".", parts, 0, parts.Length - 1);
                var enumType = FindEnumType(enumTypeName, target.GetType().Assembly);
                if (enumType == null) throw new Exception($"Enum type not found: {enumTypeName}");
                return Enum.Parse(enumType, enumValue);
            }
            // Identifier: property or field on target
            if (IsIdentifier(token))
            {
                index++;
                var prop = target.GetType().GetProperty(token, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null) return prop.GetValue(target);
                var field = target.GetType().GetField(token, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) return field.GetValue(target);
                throw new Exception($"Unknown identifier: {token}");
            }
            throw new Exception($"Unexpected token: {token}");
        }

        private static bool IsIdentifier(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            if (!char.IsLetter(token[0]) && token[0] != '_') return false;
            foreach (char c in token)
                if (!char.IsLetterOrDigit(c) && c != '_') return false;
            return true;
        }

        private static Type FindEnumType(string name, Assembly assembly)
        {
            // Try to find top-level enum first
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsEnum && type.Name == name)
                    return type;
            }
            // Try to find nested enums (e.g., EffectData.ConversionTarget)
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsEnum && type.FullName != null && (type.FullName.EndsWith("." + name) || type.FullName.Replace("+", ".").EndsWith("." + name)))
                    return type;
            }
            // Try to find by full path (e.g., EffectData.ConversionTarget)
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsEnum && (type.FullName?.EndsWith(name) == true || type.FullName?.Replace("+", ".").EndsWith(name) == true))
                    return type;
            }
            // Try to find by nested type name
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsEnum)
                {
                    string[] parts = name.Split('.');
                    if (type.Name == parts[parts.Length - 1]) return type;
                }
            }
            return null;
        }
    }
}