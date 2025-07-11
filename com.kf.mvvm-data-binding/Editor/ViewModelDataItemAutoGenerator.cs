using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ViewModelDataItemAutoGenerator
{
    static ViewModelDataItemAutoGenerator()
    {
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }

    private static void OnAfterAssemblyReload()
    {
        MVVMDatabinding.Editor.ViewModelDataItemGenerator.Generate();
        // After codegen, update data records for all ViewModels in the scene and assets
        var viewModelType = typeof(MVVMDatabinding.BaseViewModel);
        // Find all loaded ViewModel instances in open scenes
        foreach (var vm in Object.FindObjectsOfType(viewModelType, true))
        {
            MVVMDatabinding.BaseViewModel baseVm = vm as MVVMDatabinding.BaseViewModel;
            baseVm.UpdateRecord();
            // var method = viewModelType.GetMethod("UpdateRecord", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            // method?.Invoke(vm, null);
        }
        // Optionally, update records for ViewModel assets (ScriptableObjects, prefabs, etc.)
        // This can be extended as needed.
    }
}
