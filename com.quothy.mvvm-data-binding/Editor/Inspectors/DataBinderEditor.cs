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

                binderList.onAddDropdownCallback += OnShowAddBinderDropdown;
            }
            else
            {
                binderList.DoLayoutList();
            }
        }

        private void DrawBinderElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.x += listItemOffset;
            rect.width -= listItemOffset;
            EditorGUI.PropertyField(rect, binderList.serializedProperty.GetArrayElementAtIndex(index), true);
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

                /// To determine available types to bind to, we have to get current loaded assemblies and look for ones that 
                /// implement IBinder
                /// 
                //Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                //foreach (Assembly assembly in loadedAssemblies)
                //{
                //    foreach (Type type in assembly.GetTypes())
                //    {
                //        if (type != null && DoesTypeImplementInterface(type, typeof(IBinder)))
                //        {
                //            binderNames.Add(type.Name);
                //            binderTypeLookup[type.Name] = type;
                //        }
                //    }
                //}
            }
        }

        //private bool DoesTypeImplementInterface(Type typeToCheck, Type interfaceType)
        //{
        //    if (typeToCheck.IsAbstract || typeToCheck.IsInterface)
        //    {
        //        return false;
        //    }

        //    Type[] interfaces = typeToCheck.GetInterfaces();
        //    foreach (Type type in interfaces)
        //    {
        //        if (type == interfaceType)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        [DidReloadScripts]
        private static void ClearBinderTypes()
        {
            binderTypeLookup.Clear();
            binderNames.Clear();
        }
    }
}