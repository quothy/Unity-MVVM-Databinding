using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ThemeStyleApplier))]
    public class ThemeStyleApplierEditor : Editor
    {
        private Theme selectedTheme = null;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Update Visuals to Match Style", GUILayout.Height(30)))
                {
                    EditorGUIUtility.ShowObjectPicker<Theme>(selectedTheme, false, "t:Theme", 0);
                }

                if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorUpdated")
                {
                    selectedTheme = EditorGUIUtility.GetObjectPickerObject() as Theme;
                    if (selectedTheme != null)
                    {
                        Debug.Log("Updating visuals with selected theme");
                        // try to update the value
                        (target as ThemeStyleApplier).Editor_ForceUpdateVisuals(selectedTheme);
                    }
                }
            }
        }
    }
}