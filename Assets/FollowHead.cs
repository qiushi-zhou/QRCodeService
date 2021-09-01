using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowHead : MonoBehaviour
{
    public Transform camera;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = camera.position + new Vector3(0,0,0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
