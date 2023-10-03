using MVVMDatabinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionViewModel : BaseViewModel
{
    [SerializeField]
    private string optionNameString = "option";

    [SerializeField]
    private DataResolver lockStateData = null;

    private bool optionLocked = false;
    [BindableData(0)]
    public bool OptionLocked
    {
        get => optionLocked;
        set
        {
            if (optionLocked != value)
            {
                optionLocked = value;
                OnPropertyChanged();
            }
        }
    }

    private string optionName;
    [BindableData(1)]
    public string OptionName
    {
        get => optionName;
        set
        {
            if (optionName != value)
            {
                optionName = value;
                OnPropertyChanged();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        OptionName = optionNameString;   

        if (lockStateData != null)
        {
            lockStateData.DataUpdated += OnLockStateDataChanged;
            lockStateData.Subscribe(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (lockStateData != null)
        {
            lockStateData.Unsubscribe();
            lockStateData.DataUpdated -= OnLockStateDataChanged;
        }
    }

    private void OnLockStateDataChanged()
    {
        if (lockStateData.TryGetData<bool>(out bool value))
        {
            OptionLocked = value;
        }
    }
}
