using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform Camera;

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(Camera);
    }
}
