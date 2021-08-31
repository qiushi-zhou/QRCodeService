using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlippedBehaviour : MonoBehaviour
{

    public GameObject flippedObj;

    public bool isInteracted = false;

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

        //mimic rotation
        // inspired from:
        // https://forum.unity.com/threads/how-to-mirror-a-euler-angle-or-rotation.90650/
        // http://www.euclideanspace.com/maths/geometry/affine/reflection/quaternion/index.htm
        //Quaternion flippedGlobalRot = this.flippedObj.transform.rotation;
        MeshFilter mirrorPlane = this.mirrorObj.GetComponent<MeshFilter>();
        Vector3 mirrorNormal = mirrorPlane.transform.TransformDirection(mirrorPlane.mesh.normals[0]);
        //Quaternion mirrorQuat = new Quaternion(mirrorNormal.x, mirrorNormal.y, mirrorNormal.z, 0);

        //this.transform.rotation = mirrorQuat * flippedGlobalRot * new Quaternion(-mirrorQuat.x, -mirrorQuat.y, -mirrorQuat.z, mirrorQuat.w);

        Vector3 forward = this.flippedObj.transform.forward;
        Vector3 upward = this.flippedObj.transform.up;
        Vector3 mirroredFor = Vector3.Reflect(forward, mirrorNormal);
        Vector3 mirroredUp = Vector3.Reflect(upward, mirrorNormal);
        this.transform.rotation = Quaternion.LookRotation(mirroredFor, mirroredUp);


        //Vector3 rot = this.flippedObj.transform.rotation.eulerAngles;
        //rot = new Vector3(rot.x, rot.y *-1, -1* rot.z);
        //this.transform.rotation = Quaternion.Euler(rot);
        //this.transform.rotation = this.flippedObj.transform.rotation;
    }
}
