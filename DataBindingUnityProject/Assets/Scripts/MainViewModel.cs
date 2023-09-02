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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
