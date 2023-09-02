using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class ViewModelDataSource : BaseDataSource
    {
        /// <summary>
        /// Use this call to initialize at runtime
        /// </summary>
        /// <param name="viewModel"></param>
        internal void InitializeFromViewModel(BaseViewModel viewModel)
        {
            bool isIdModifiedAtRuntime = !viewModel.IsGlobalSource;
            string viewModelName = viewModel.GetType().ToString();
            if (isIdModifiedAtRuntime)
            {
                viewModelName = ResolveNameWithRuntimeId(viewModelName, viewModel.GetInstanceID());
            }

            Initialize(viewModelName, isIdModifiedAtRuntime);
        }

    }
}