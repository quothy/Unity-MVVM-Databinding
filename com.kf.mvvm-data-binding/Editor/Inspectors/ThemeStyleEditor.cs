using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [CustomEditor(typeof(ThemeStyle))]
    public class ThemeStyleEditor : Editor
    {
        private ThemeStyle themeStyle = null;
        private int valueListCount = 0;

        private void OnEnable()
        {
            themeStyle = serializedObject.targetObject as ThemeStyle;
            valueListCount = themeStyle.Values != null ? themeStyle.Values.Count : 0;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (themeStyle.Values?.Count != valueListCount)
            {
                CheckUniqueness();
                valueListCount = themeStyle != null && themeStyle.Values != null ? themeStyle.Values.Count : 0;

                if (themeStyle != null)
                {
                    // TODO: propagate template from style to style value
                    foreach (var value in themeStyle.Values)
                    {
                        value.Editor_SetTemplate(themeStyle.Template);
                    }
                }
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

            SerializedProperty listProp = serializedObject.FindProperty("values");

            if (listProp == null)
            {
                return;
            }

            for (int i = 0; i < themeStyle.Values.Count; i++)
            {
                var prop = listProp.GetArrayElementAtIndex(i);

                // get theme value prop
                SerializedProperty valueProp = prop.FindPropertyRelative("themeValue");

                if (!uniquenessCheckList.TryGetValue(valueProp.managedReferenceId, out int idx))
                {
                    uniquenessCheckList[valueProp.managedReferenceId] = i;
                    continue;
                }

                themeStyle.Values[i].ResetAndUpdateThemeValue();
                PrefabUtility.RecordPrefabInstancePropertyModifications(themeStyle);

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
