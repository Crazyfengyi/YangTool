using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AttributeBuff one = ScriptableObject.CreateInstance<AttributeBuff>();
        one.Init(10);

        PooledObject<Test> pooledObject = new PooledObject<Test>();
        ObjectPool<Test> objectPool = new ObjectPool<Test>(null, null, null);
    }
}
