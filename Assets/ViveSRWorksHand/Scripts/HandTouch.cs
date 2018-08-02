using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTouch : MonoBehaviour
{
    public static int HandTouchLayer
    {
        get
        {
            int layer = LayerMask.NameToLayer("HandTouch");
            if (layer < 0)
            { Debug.LogError("please add a layer => HandTouch, and set 'HandTouch' to the object you want to touch!"); return 0; }
            return layer;
        }
    }

    float recoverTime;
    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
        recoverTime = 0.5f;
    }

    private void Awake()
    {
        gameObject.layer = HandTouchLayer;
    }

    private void Update()
    {
        recoverTime -= Time.deltaTime;
        if (recoverTime < 0)
            GetComponent<MeshRenderer>().material.color = Color.white;
    }
}
