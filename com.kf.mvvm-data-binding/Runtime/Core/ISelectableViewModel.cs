using System;

namespace MVVMDatabinding
{
    public interface ISelectableViewModel
    {
        bool IsSelected { get; set; }
        int Index { get; set; }
        event Action<bool, int> SelectionChanged;
    }
}
