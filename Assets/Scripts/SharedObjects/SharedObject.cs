using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedObject : MonoBehaviour
{
    public int id;
    public string prefabName;
    public bool isBodyAnchored = false;


    // Can send regular ManipulateMessages in the update loop here if this objects position has changed since the last X number of frames or time 
}
