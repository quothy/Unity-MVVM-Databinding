// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using MVVMDatabinding.Theming;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace MVVMDatabinding
{
    [CustomEditor(typeof(ThemeValueList))]
    public class ThemeValueListEditor : Editor
    {
        private ThemeValueList valueList = null;
        private int valueListCount = 0;

        private void OnEnable()
        {
            valueList = serializedObject.targetObject as ThemeValueList;
            valueListCount = valueList.Items != null ? valueList.Items.Count : 0;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (valueList.Items?.Count != valueListCount)
            {
                CheckUniqueness();
                valueListCount = valueList.Items != null ? valueList.Items.Count : 0;
            }
        }


        private Dictionary<long, int> uniquenessCheckList = new Dictionary<long, int>();
        private Dictionary<int, long> deepCopyTargetLookup = new Dictionary<int, long>();
        private List<int> successfullyDeepCopied = new List<int>();
        private void CheckUniqueness()
        {
            successfullyDeepCopied.Clear();
            DiscoverThemeValueTypes();
            uniquenessCheckList.Clear();

            SerializedProperty listProp = serializedObject.FindProperty("items");

            for (int i = 0; i < valueList.Items.Count; i++)
            {
                var prop = listProp.GetArrayElementAtIndex(i);

                // get theme value prop
                SerializedProperty valueProp = prop.FindPropertyRelative("themeValue");

                if (!uniquenessCheckList.TryGetValue(valueProp.managedReferenceId, out int idx))
                {
                    uniquenessCheckList[valueProp.managedReferenceId] = i;
                    continue;
                }

                valueList.Items[i].ResetAndUpdateThemeValue();
                PrefabUtility.RecordPrefabInstancePropertyModifications(valueList);

            }
        }

        private static Dictionary<string, Type> themeValueTypeLookup = new Dictionary<string, Type>();
        private static List<string> themeValueNames = new List<string>();
        private void DiscoverThemeValueTypes()
        {
            if (themeValueNames.Count == 0)
            {
                var types = TypeCache.GetTypesDerivedFrom<IThemeValue>();

                foreach (Type type in types)
                {
                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    themeValueNames.Add(type.Name);
                    themeValueTypeLookup[type.Name] = type;
                }
            }
        }
    }
}