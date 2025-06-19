// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MVVMDatabinding
{
    [CustomEditor(typeof(BaseViewModel), true)]
    public class BaseViewModelEditor : UnityEditor.Editor
    {
        private BaseViewModel instance = null;

        private void OnEnable()
        {
            instance = target as BaseViewModel;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            // add a button (contents depend on situation)
            if (IsPrefabInstance())
            {
                EditorGUILayout.HelpBox("Please open prefab to update data record", MessageType.Warning);

                if (GUILayout.Button("Open Prefab", GUILayout.Height(30)))
                {
                    string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance);
                    PrefabStageUtility.OpenPrefab(path);
                }
            }
            else
            {
                // show update data record button
                if (GUILayout.Button("Update Data Record", GUILayout.Height(30)))
                {
                    instance.UpdateRecord();
                }
            }
        }

        private bool IsPrefabInstance()
        {
            return PrefabUtility.IsPartOfPrefabInstance(instance);
        }

    }
}