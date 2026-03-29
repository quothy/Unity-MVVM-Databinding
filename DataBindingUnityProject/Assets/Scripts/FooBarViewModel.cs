using UnityEngine;
using MVVMDatabinding;

public partial class FooBarViewModel : BaseLocalViewModel
{
    private bool isFooBar = false;
    [BindableData(0)]
    public bool IsFooBar
    {
      get => isFooBar;
      set { if (isFooBar != value) { isFooBar = value; OnPropertyChanged(); } }
    }

    private int fooCounter = 0;
    [BindableData(1)]
    public int FooCounter
    {
      get => fooCounter;
      set { if (fooCounter != value) { fooCounter = value; OnPropertyChanged(); CheckFooBar(); } }
    }

    private int barCounter = 0;
    [BindableData(2)]
    public int BarCounter
    {
      get => barCounter;
      set { if (barCounter != value) { barCounter = value; OnPropertyChanged(); CheckFooBar(); } }
    }

    [BindableAction(100)]
    private void IncrementFoo()
    {
        FooCounter++;
    }

    [BindableAction(101)]
    private void IncrementBar()
    {
        BarCounter++;
    }

    private void CheckFooBar()
    {
        IsFooBar = (FooCounter + BarCounter) % 5 == 0;
    }
}