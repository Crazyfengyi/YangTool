﻿using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{

    public float lifetime;

    // Use this for initialization
    void Start()
    {
        Invoke("DestroyMe", lifetime);
    }

    void DestroyMe()
    {
        Destroy(gameObject);
    }
}
