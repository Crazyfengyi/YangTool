using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AttributeBuff one = ScriptableObject.CreateInstance<AttributeBuff>();
        one.Init(10);
    }

}
