using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlippedBehaviour : MonoBehaviour
{

    public GameObject flippedObj;

    private bool isInteracted = false;

    ObjectManipulator objectManipulator;

    GameObject mirrorObj;

    // Start is called before the first frame update
    void Start()
    {
        this.mirrorObj = GameObject.Find("SceneController").GetComponent<SceneController>().mirrorObj;
        objectManipulator = this.gameObject.GetComponent<ObjectManipulator>();

        objectManipulator.OnManipulationStarted.AddListener(InteractStarted);
        objectManipulator.OnManipulationEnded.AddListener(InteractEnded);
    }

    // Update is called once per frame
    void Update()
    {
        if (isInteracted)
        {
            this.flippedObj.GetComponent<FlippedBehaviour>().Mimic();
        }
    }

    private void InteractStarted(ManipulationEventData arg0)
    {
        this.isInteracted = true;
    }

    private void InteractEnded(ManipulationEventData arg0)
    {
        this.isInteracted = false;
    }

    public void Mimic()
    {
        Vector3 flippedLocalPos = this.mirrorObj.transform.InverseTransformPoint(this.flippedObj.transform.position);
        Vector3 updatedLocalPos = flippedLocalPos;
        updatedLocalPos.y *= -1;

        

        this.transform.position = this.mirrorObj.transform.TransformPoint(updatedLocalPos);

        this.transform.localScale = new Vector3(this.flippedObj.transform.localScale.x * -1, this.flippedObj.transform.localScale.y, this.flippedObj.transform.localScale.z);

        Vector3 rot = this.flippedObj.transform.rotation.eulerAngles;
        rot = new Vector3(rot.x , rot.y + 180, rot.z );
        this.transform.rotation = Quaternion.Euler(rot);
    }
}
