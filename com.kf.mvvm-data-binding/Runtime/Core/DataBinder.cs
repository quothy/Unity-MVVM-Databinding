// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding
{
    public class DataBinder : MonoBehaviour
    {
        [SerializeReference]
        private List<IBinder> binders = null;

        private void Awake()
        {
            foreach (IBinder binder in binders)
            {
                binder.Bind(gameObject);
            }
        }

        private void OnDestroy()
        {
            foreach (IBinder binder in binders)
            {
                binder.Unbind();
            }
        }

        public void AddBinder(IBinder binder)
        {
            if (binders == null)
            {
                binders = new List<IBinder>();
            }
            binders.Add(binder);
        }

        public void ReplaceBinderAtIndex(IBinder binder, int index)
        {
            binders[index] = binder;
        }
    }
}