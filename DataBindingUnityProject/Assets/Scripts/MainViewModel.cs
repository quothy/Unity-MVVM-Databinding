using MVVMDatabinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainViewModel : BaseGlobalViewModel
{
    private bool exampleBool = false;
    [BindableData(0)]
    public bool ExampleBool
    {
        get => exampleBool;
        set
        {
            if (exampleBool != value)
            {
                exampleBool = value;

            }
        }
    }

    private int exampleInt = 0;
    [BindableData(1)]
    public int ExampleInt
    {
        get => exampleInt;
        set
        {
            if (exampleInt != value)
            {
                exampleInt = value;
            }
        }
    }

    private float exampleFloat = 0;
    [BindableData(2)]
    public float ExampleFloat
    {
        get => exampleFloat;
        set
        {
            if (exampleFloat != value)
            {
                exampleFloat = value;
            }
        }
    }

    private string exampleString = string.Empty;
    [BindableData(3)]
    public string CounterString
    {
        get => exampleString;
        set
        {
            if (exampleString != value)
            {
                exampleString = value;
                OnPropertyChanged();
            }
        }
    }

    private bool optionsLocked = false;
    [BindableData(4)]
    public bool OptionsLocked
    {
        get => optionsLocked;
        set
        {
            if (optionsLocked != value)
            {
                optionsLocked = value;
                OnPropertyChanged();
            }
        }
    }

    [SerializeField]
    private bool testCheckbox = false;

    [SerializeField]
    private bool testEnable = false;

    [ConditionalVisibility(nameof(testCheckbox), ConditionResultType.ShowIfEquals)]
    [ConditionalEnable(nameof(testEnable), ConditionalEnableAttribute.ConditionalEnableType.EnableIfTrue)]
    [SerializeField]
    private string text = "test";

    private void Start()
    {
        CounterString = $"{ExampleInt}";
    }


    [BindableAction(10)]
    private void IncrementCounter()
    {
        ExampleInt++;
        CounterString = $"{ExampleInt}";
    }

    [BindableAction(11)]
    private void ToggleOptionsLock()
    {
        OptionsLocked = !OptionsLocked;
    }


    [ContextMenu("Increment counter")]
    public void Editor_IncrementCounter()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ExampleInt++;
        CounterString = $"{ExampleInt}";
    }
}
