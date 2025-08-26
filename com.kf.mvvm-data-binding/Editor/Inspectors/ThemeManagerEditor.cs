// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [CustomEditor(typeof(ThemeManager), true)]
    public class ThemeManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                GUILayout.Space(10);

                // show update data record button
                if (ThemeManager.Instance.ActiveVariant != ThemeVariant.Light && GUILayout.Button("Apply Light Theme", GUILayout.Height(30)))
                {
                    // TODO:
                    ThemeManager.Instance.ChangeThemeVariant(ThemeVariant.Light);
                }
                else if (ThemeManager.Instance.ActiveVariant != ThemeVariant.Dark && GUILayout.Button("Apply Dark Theme", GUILayout.Height(30)))
                {
                    ThemeManager.Instance.ChangeThemeVariant(ThemeVariant.Dark);
                }
                else if (ThemeManager.Instance.ActiveVariant != ThemeVariant.HighContrast && GUILayout.Button("Apply High Contrast Theme", GUILayout.Height(30)))
                {
                    ThemeManager.Instance.ChangeThemeVariant(ThemeVariant.HighContrast);
                }
            }
        }
    }
}