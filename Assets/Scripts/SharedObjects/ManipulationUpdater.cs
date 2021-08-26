using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulationUpdater : MonoBehaviour
{

    SharedObject sObj;
    ObjectManipulator objectManipulator;
    public UDPController udpController;
    public GameObject mirrorObj;

    private bool isInteracted = false;

    private void Awake()
    {
        sObj = this.gameObject.GetComponent<SharedObject>();
        objectManipulator = this.gameObject.GetComponent<ObjectManipulator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        objectManipulator.OnManipulationStarted.AddListener(InteractStarted);
        objectManipulator.OnManipulationEnded.AddListener(InteractEnded);
    }

    private void InteractStarted(ManipulationEventData arg0)
    {
        this.isInteracted = true;
    }

    private void InteractEnded(ManipulationEventData arg0)
    {
        this.isInteracted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isInteracted)
        {
            this.udpController.SendManipulateMessage(
                this.sObj.id, 
                this.mirrorObj.transform.InverseTransformPoint(this.transform.position), 
                this.mirrorObj.transform.InverseTransformDirection(this.transform.forward), 
                this.mirrorObj.transform.InverseTransformDirection(this.transform.up)
                );
            //Debug.Log("SendManipulateMessage obj id: " + this.sObj.id);
        }
    }

}
