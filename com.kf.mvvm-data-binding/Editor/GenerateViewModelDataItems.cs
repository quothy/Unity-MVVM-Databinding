// Editor/GenerateViewModelDataItems.cs
// Place this script in an Editor folder (e.g., Assets/Editor)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MVVMDatabinding.Editor
{
    public static class ViewModelDataItemGenerator
    {
        [MenuItem("Tools/Generate ViewModel DataItems")]
        public static void Generate()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var viewModelType = typeof(MVVMDatabinding.BaseViewModel);

            foreach (var type in assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && viewModelType.IsAssignableFrom(t)))
            {
                // Only generate for types that are not the base class
                if (type == viewModelType) continue;
                if (!type.IsPublic) continue;
                // C# does not expose 'partial' via reflection; skip the PartialAttribute check

                var sb = new StringBuilder();
                // Collect all usings from the original file
                var usingSet = new HashSet<string> { "using System;", "using System.Collections.Generic;", "using MVVMDatabinding;" };
                var scriptFile = string.Empty;
                var classNamespace = string.Empty;
                string[] guids = AssetDatabase.FindAssets($"{type.Name} t:Script");
                foreach (var guid in guids)
                {
                    var candidate = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(candidate) == type.Name)
                    {
                        scriptFile = candidate;
                        break;
                    }
                }
                if (scriptFile == null)
                {
                    Debug.LogWarning($"[ViewModelDataItemGenerator] Could not find source file for {type.Name}, skipping codegen.");
                    continue;
                }
                var scriptLines = File.ReadAllLines(scriptFile);
                // Find the namespace of the original class
                classNamespace = type.Namespace; // Use reflection for namespace
                Debug.Log($"[ViewModelDataItemGenerator] Detected namespace for {type.Name}: '{classNamespace ?? "<global>"}'");

                // Instead of hardcoding namespace, use the detected one
                if (!string.IsNullOrEmpty(classNamespace))
                {
                    sb.AppendLine($"namespace {classNamespace}");
                    sb.AppendLine("{");
                    sb.AppendLine($"    public partial class {type.Name}");
                    sb.AppendLine("    {");
                }
                else
                {
                    sb.AppendLine($"public partial class {type.Name}");
                    sb.AppendLine("{");
                }

                // Fields for properties
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.GetCustomAttribute(typeof(BindableDataAttribute)) != null))
                {
                    sb.AppendLine($"        public DataItem<{prop.PropertyType.Name}> {prop.Name}DataItem;");
                }
                // Fields for methods
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttribute(typeof(BindableActionAttribute)) != null))
                {
                    sb.AppendLine($"        public DataItem<Action> {method.Name}ActionDataItem;");
                }

                // Method
                sb.AppendLine("        protected void InitializeGeneratedDataItems()");
                sb.AppendLine("        {");
                sb.AppendLine($"            global::UnityEngine.Debug.Log(\"[ViewModelDataItemGenerator] InitializeGeneratedDataItems called for {type.Name}\");");
                sb.AppendLine("            dataItemList = new List<IDataItem>();");

                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.GetCustomAttribute(typeof(BindableDataAttribute)) is BindableDataAttribute))
                {
                    var attr = (BindableDataAttribute)prop.GetCustomAttribute(typeof(BindableDataAttribute));
                    sb.AppendLine($"            {prop.Name}DataItem = new DataItem<{prop.PropertyType.Name}>();");
                    sb.AppendLine($"            {prop.Name}DataItem.Initialize({attr.DataItemId}, nameof({type.Name}.{prop.Name}), \"{attr.Comment}\");");
                    sb.AppendLine($"            {prop.Name}DataItem.valueGetter = () => this.{prop.Name};");
                    sb.AppendLine($"            {prop.Name}DataItem.valueSetter = v => this.{prop.Name} = v;");
                    sb.AppendLine($"            dataItemList.Add({prop.Name}DataItem);");
                }
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttribute(typeof(BindableActionAttribute)) is BindableActionAttribute))
                {
                    var attr = (BindableActionAttribute)method.GetCustomAttribute(typeof(BindableActionAttribute));
                    sb.AppendLine($"            {method.Name}ActionDataItem = new DataItem<Action>();");
                    sb.AppendLine($"            {method.Name}ActionDataItem.Initialize({attr.DataItemId}, nameof({type.Name}.{method.Name}), \"{attr.Comment}\");");
                    sb.AppendLine($"            {method.Name}ActionDataItem.valueGetter = () => (Action)this.{method.Name};");
                    sb.AppendLine($"            dataItemList.Add({method.Name}ActionDataItem);");
                }

                sb.AppendLine("        }");

                // Generate override for InitializeData
                sb.AppendLine("        public override void InitializeData()");
                sb.AppendLine("        {");
                sb.AppendLine("            InitializeGeneratedDataItems();");
                sb.AppendLine("            base.InitializeData();");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                if (!string.IsNullOrEmpty(classNamespace))
                {
                    sb.AppendLine("}");
                }

                // Check DataItem types for missing usings
                var dataItemTypes = new HashSet<Type>();
                foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.GetCustomAttribute(typeof(BindableDataAttribute)) != null))
                {
                    dataItemTypes.Add(prop.PropertyType);
                }
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttribute(typeof(BindableActionAttribute)) != null))
                {
                    dataItemTypes.Add(typeof(System.Action));
                }
                foreach (var t in dataItemTypes)
                {
                    var ns = t.Namespace;
                    if (!string.IsNullOrEmpty(ns) && !usingSet.Any(u => u.Contains(ns)))
                    {
                        usingSet.Add($"using {ns};");
                    }
                }

                // Always add the namespace for DataItem<> 
                var dataItemType = typeof(MVVMDatabinding.DataItem<>);
                var dataItemNamespace = dataItemType.Namespace;
                if (!string.IsNullOrEmpty(dataItemNamespace) && !usingSet.Any(u => u.Contains(dataItemNamespace)))
                {
                    usingSet.Add($"using {dataItemNamespace};");
                }

                // Write all usings to the generated file
                var scriptDir = Path.GetDirectoryName(scriptFile);
                var generatedDir = Path.Combine(scriptDir, "Generated");
                Directory.CreateDirectory(generatedDir);
                var path = Path.Combine(generatedDir, $"{type.Name}.DataItems.g.cs");

                sb.Insert(0, "// <auto-generated />\n");
                foreach (var u in usingSet)
                {
                    sb.Insert(0, u + "\n");
                }

                // Write to Generated folder local to the ViewModel source file
                File.WriteAllText(path, sb.ToString());
                Debug.Log($"[ViewModelDataItemGenerator] Generated: {path}");
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Generate ViewModel DataItems (Clean)")]
        public static void GenerateClean()
        {
            // Find all Generated folders under Assets
            var generatedDirs = Directory.GetDirectories("Assets", "Generated", SearchOption.AllDirectories);
            foreach (var dir in generatedDirs)
            {
                var files = Directory.GetFiles(dir, "*.DataItems.g.cs", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    File.Delete(file);
                    Debug.Log($"[ViewModelDataItemGenerator] Deleted generated file: {file}");
                }
            }
            AssetDatabase.Refresh();
            Generate();
        }
    }
}
