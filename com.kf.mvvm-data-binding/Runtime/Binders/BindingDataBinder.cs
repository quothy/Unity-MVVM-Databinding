

using UnityEngine;

namespace MVVMDatabinding
{
    public class BindingDataBinder : BaseBinder
    {
        [SerializeField]
        private BaseLocalViewModel target = null;

        public override void Bind(GameObject bindingObject)
        {
            base.Bind(bindingObject);
            if (target != null)
            {
                target.SetBindingData(SourceId, itemId);
            }
        }

        public override void OnDataItemUpdate(IDataSource dataSource, int itemId) { }
    }
}