using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class PaperClipBehaviour : MonoBehaviour
{
    ObjectManipulator objectManipulator;
    // Start is called before the first frame update
    void Start()
    {
        objectManipulator = this.gameObject.GetComponent<ObjectManipulator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
