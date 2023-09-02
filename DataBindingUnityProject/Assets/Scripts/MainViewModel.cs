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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
