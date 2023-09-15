using Codice.Client.BaseCommands.BranchExplorer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.GenericMenu;

namespace MVVMDatabinding
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DataBinder))]
    public class DataBinderEditor : Editor
    {
        // cache these globally and only clear them when scripts are recompiled
        private static Dictionary<string, Type> binderTypeLookup = new Dictionary<string, Type>();
        private static List<string> binderNames = new List<string>();

        private DataBinder dataBinder = null;
        private ReorderableList binderList = null;

        private int listItemOffset = 10;

        private void OnEnable()
        {
            dataBinder = target as DataBinder;

            DiscoverBinderTypes();
        }

        public override void OnInspectorGUI()
        {
            DrawBinders();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBinders()
        {
            if (binderList == null)
            {
                binderList = new ReorderableList(serializedObject, serializedObject.FindProperty("binders"), true, true, true, true);

                binderList.drawElementCallback += DrawBinderElement;
                binderList.elementHeightCallback = (int index) => EditorGUI.GetPropertyHeight(binderList.serializedProperty.GetArrayElementAtIndex(index));
                binderList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Binders");

                binderList.onAddCallback += CheckUniqueness;

                binderList.onAddDropdownCallback += OnShowAddBinderDropdown;
            }
            else
            {
                binderList.DoLayoutList();
                CheckUniqueness(binderList);
            }
        }

        private void DrawBinderElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.width < 0)
            {
                return;
            }

            rect.x += listItemOffset;
            rect.width -= listItemOffset;
            var binderProp = binderList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, binderProp, new GUIContent(binderProp.FindPropertyRelative("name").stringValue), true);
        }

        private void OnShowAddBinderDropdown(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();
            foreach (string typeName in binderNames)
            {
                menu.AddItem(new GUIContent(typeName), false,
                (object t) =>
                {
                    Undo.RecordObject(dataBinder, "Add binder");
                    IBinder newBinder = Activator.CreateInstance((Type)t) as IBinder;
                    dataBinder.AddBinder(newBinder);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(dataBinder);
                    serializedObject.ApplyModifiedProperties();
                },
                binderTypeLookup[typeName]);
            }
            menu.ShowAsContext();
        }
        
        private void DiscoverBinderTypes()
        {
            if (binderNames.Count == 0)
            {
                var types = TypeCache.GetTypesDerivedFrom<IBinder>();

                foreach (Type type in types)
                {
                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    binderNames.Add(type.Name);
                    binderTypeLookup[type.Name] = type;
                }
            }
        }

        private Dictionary<long, int> uniquenessCheckList = new Dictionary<long, int>();
        private Dictionary<int, long> deepCopyTargetLookup = new Dictionary<int, long>();
        private List<int> successfullyDeepCopied = new List<int>();
        private void CheckUniqueness(ReorderableList list)
        {
            successfullyDeepCopied.Clear();
            DiscoverBinderTypes();
            uniquenessCheckList.Clear();
            for(int i = 0; i < list.count; i++)
            {
                var prop = list.serializedProperty.GetArrayElementAtIndex(i);
                if (!uniquenessCheckList.TryGetValue(prop.managedReferenceId, out int idx))
                {
                    uniquenessCheckList[prop.managedReferenceId] = i;
                    continue;
                }

                string typeName = prop.managedReferenceFullTypename;
                int lastDotIdx = typeName.LastIndexOf('.');
                typeName = typeName.Substring(lastDotIdx + 1, typeName.Length - lastDotIdx - 1);

                // found a dupe, need to properly dupe it
                if (binderTypeLookup.TryGetValue(typeName, out var type))
                {
                    long originalRefId = prop.managedReferenceId;
                    IBinder newBinder = Activator.CreateInstance((Type)type) as IBinder;
                    dataBinder.ReplaceBinderAtIndex(newBinder, i);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(dataBinder);
                    serializedObject.ApplyModifiedProperties();

                    deepCopyTargetLookup[i] = prop.managedReferenceId;
                }
            }

            // do we need to force a refresh of the list's serializedproperty at all?
            foreach (var kvp in deepCopyTargetLookup)
            {
                //SerializedProperty newBinderProp = list.serializedProperty.GetArrayElementAtIndex(kvp.Key);
                // now copy over private property values from the existing reference that was duped
                if (uniquenessCheckList.TryGetValue(kvp.Value, out int originalIndex))
                {
                    // now we copy values manually from the original to the new unique duplicate
                    var duplicateProp = list.serializedProperty.GetArrayElementAtIndex(kvp.Key);
                    var originalProp = list.serializedProperty.GetArrayElementAtIndex(originalIndex);

                    if (duplicateProp.managedReferenceId == originalProp.managedReferenceId)
                    {
                        continue;
                    }

                    duplicateProp.isExpanded = originalProp.isExpanded;

                    var endProp = duplicateProp.GetEndProperty();
                    while (duplicateProp.NextVisible(true) && !SerializedProperty.EqualContents(duplicateProp, endProp))
                    {
                        originalProp.NextVisible(true);
                        CopySerializedPropertyData(originalProp, duplicateProp);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(dataBinder);
                    }
                    successfullyDeepCopied.Add(kvp.Key);
                }

                serializedObject.ApplyModifiedProperties();
            }

            foreach (int idx in successfullyDeepCopied)
            {
                deepCopyTargetLookup.Remove(idx);
            }
        }

        private static void CopySerializedPropertyData(SerializedProperty copyFrom, SerializedProperty copyTo)
        {
            switch (copyTo.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    {
                        copyTo.animationCurveValue = copyFrom.animationCurveValue;
                    }
                    break;
                case SerializedPropertyType.ArraySize:
                    {
                        copyTo.arraySize = copyFrom.arraySize;
                    }
                    break;
                case SerializedPropertyType.Boolean:
                    {
                        copyTo.boolValue = copyFrom.boolValue;
                    }
                    break;
                case SerializedPropertyType.Bounds:
                    {
                        copyTo.boundsValue = copyFrom.boundsValue;
                    }
                    break;
                case SerializedPropertyType.BoundsInt:
                    {
                        copyTo.boundsIntValue = copyFrom.boundsIntValue;
                    }
                    break;
                case SerializedPropertyType.Character:
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    {
                        copyTo.intValue = copyFrom.intValue;
                    }
                    break;
                case SerializedPropertyType.Color:
                    {
                        copyTo.colorValue = copyFrom.colorValue;
                    }
                    break;
                case SerializedPropertyType.Enum:
                    {
                        copyTo.enumValueFlag = copyFrom.enumValueFlag;
                        copyTo.enumValueIndex = copyFrom.enumValueIndex;
                    }
                    break;
                case SerializedPropertyType.ExposedReference:
                    {
                        copyTo.exposedReferenceValue = copyFrom.exposedReferenceValue;
                    }
                    break;
                case SerializedPropertyType.Float:
                    {
                        copyTo.floatValue = copyFrom.floatValue;
                    }
                    break;
                case SerializedPropertyType.ObjectReference:
                    {
                        copyTo.objectReferenceValue = copyFrom.objectReferenceValue;
                    }
                    break;
                case SerializedPropertyType.Quaternion:
                    {
                        copyTo.quaternionValue = copyFrom.quaternionValue;
                    }
                    break;
                case SerializedPropertyType.Rect:
                    {
                        copyTo.rectValue = copyFrom.rectValue;
                    }
                    break;
                case SerializedPropertyType.RectInt:
                    {
                        copyTo.rectIntValue = copyFrom.rectIntValue;
                    }
                    break;
                case SerializedPropertyType.String:
                    {
                        copyTo.stringValue = copyFrom.stringValue;
                    }
                    break;
                case SerializedPropertyType.Vector2:
                    {
                        copyTo.vector2Value = copyFrom.vector2Value;
                    }
                    break;
                case SerializedPropertyType.Vector2Int:
                    {
                        copyTo.vector2IntValue = copyFrom.vector2IntValue;
                    }
                    break;
                case SerializedPropertyType.Vector3:
                    {
                        copyTo.vector3Value = copyFrom.vector3Value;
                    }
                    break;
                case SerializedPropertyType.Vector3Int:
                    {
                        copyTo.vector3IntValue = copyFrom.vector3IntValue;
                    }
                    break;
                case SerializedPropertyType.Vector4:
                    {
                        copyTo.vector4Value = copyFrom.vector4Value;
                    }
                    break;
            }
        }

        [DidReloadScripts]
        private static void ClearBinderTypes()
        {
            binderTypeLookup.Clear();
            binderNames.Clear();
        }
    }
}