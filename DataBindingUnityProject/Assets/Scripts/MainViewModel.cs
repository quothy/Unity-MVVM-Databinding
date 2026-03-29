using MVVMDatabinding;
using MVVMDatabinding.Theming;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MainViewModel : BaseGlobalViewModel
{
    private bool exampleBool = false;
    [BindableData(0, "This is a example boolean that you can bind to, but it's not hooked up to anything")]
    public bool ExampleBool
    {
        get => exampleBool;
        set
        {
            if (exampleBool != value)
            {
                exampleBool = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
    [BindableData(4, comment: "Whether or not all options should be considered locked")]
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

    private string editableString = "blah";
    [BindableData(5, comment: "This string is for editing")]
    public string EditableString
    {
        get => editableString;
        set
        {
            if (editableString != value)
            {
                editableString = value;
                OnPropertyChanged();
            }
        }
    }
    
    private bool darkMode = false;
    [BindableData(6, "Whether the UI is curerntly using the dark mode variant of the theme")]
    public bool InDarkMode
    {
        get => darkMode;
        set
        {
            if (darkMode != value)
            {
                darkMode = value;
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

    [BindableAction(12)]
    private void DecrementCounter()
    {
        ExampleInt--;
        CounterString = $"{ExampleInt}";
    }

    [BindableAction(13)]
    private void ChangeTheme()
    {
        if (ThemeManager.Instance.ActiveVariant == ThemeVariant.Light)
        {
            ThemeManager.Instance.ChangeThemeVariant(ThemeVariant.Dark);
            InDarkMode = true;
        }
        else
        {
            ThemeManager.Instance.ChangeThemeVariant(ThemeVariant.Light);      
            InDarkMode = false;      
        }
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
