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

        private bool bindingDone = false;

        private void OnEnable()
        {
            if (bindingDone)
            {
                foreach (IBinder binder in binders)
                {
                    binder.OnEnable();
                }
            }
        }

        private void Start()
        {
            foreach (IBinder binder in binders)
            {
                binder.Bind(gameObject);
            }
            bindingDone = true;
        }

        private void OnDisable()
        {
            foreach (IBinder binder in binders)
            {
                binder.OnDisable();
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (binders != null)
            {
                foreach (var binder in binders)
                {
                    binder.OnValidate(gameObject);
                }
            }
        }
#endif
    }
}