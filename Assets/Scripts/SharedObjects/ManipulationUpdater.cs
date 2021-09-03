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
    public GameObject flippedObject;

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
        if (this.isInteracted || flippedObject.GetComponent<FlippedBehaviour>().isInteracted)
        {
            this.udpController.SendManipulateMessage(
                this.sObj.id, 
                this.mirrorObj.transform.InverseTransformPoint(this.transform.position), 
                this.mirrorObj.transform.InverseTransformDirection(this.transform.forward), 
                this.mirrorObj.transform.InverseTransformDirection(this.transform.up)
                );
            //Debug.Log("SendManipulateMessage obj id: " + this.sObj.id);
            
        }

        if (this.gameObject == this.udpController.shared_picture && transform.position.z > 0)
        {
            this.udpController.SendManipulateMessage(
                this.sObj.id,
                this.mirrorObj.transform.InverseTransformPoint(this.flippedObject.transform.position),
                this.mirrorObj.transform.InverseTransformDirection(this.flippedObject.transform.forward),
                this.mirrorObj.transform.InverseTransformDirection(this.flippedObject.transform.up)
                );
        }
    }

}
