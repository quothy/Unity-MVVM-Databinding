// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVVMDatabinding.Theming
{
    public class ThemeBinder : MonoBehaviour
    {
        [SerializeReference]
        private List<IThemeBinder> binders = null;

#if UNITY_EDITOR
        public List<IThemeBinder> Binders => binders;
#endif
        private void Awake()
        {
            foreach (IThemeBinder binder in binders)
            {
                binder.Bind();
            }
        }

        private void OnDestroy()
        {
            foreach (IThemeBinder binder in binders)
            {
                binder.Unbind();
            }
        }
        public void AddBinder(IThemeBinder binder)
        {
            if (binders == null)
            {
                binders = new List<IThemeBinder>();
            }
            binders.Add(binder);
        }

        public void ReplaceBinderAtIndex(IThemeBinder binder, int index)
        {
            binders[index] = binder;
        }
    }
}