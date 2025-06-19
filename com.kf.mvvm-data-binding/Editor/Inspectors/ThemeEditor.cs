// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    [CustomEditor(typeof(Theme), true)]
    public class ThemeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying )
            {
                GUILayout.Space(10);

                // show update data record button
                if (GUILayout.Button("Load & Apply Theme", GUILayout.Height(30)))
                {
                    (target as Theme).Editor_LoadTheme();
                }
            }

        }
    }
}