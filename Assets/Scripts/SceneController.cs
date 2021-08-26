using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public UDPController udpController;
    public GameObject mirrorObj = null;

    // keeps a list of all shared objs between the mirror and hololens
    public Dictionary<int, SharedObject> sharedObjMap;
    
    public int sharedCount = 0;

    float time = 0;

    // Start is called before the first frame update
    void Start()
    {
        this.sharedObjMap = new Dictionary<int, SharedObject>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    // allow creation of object only when a mirrorObj has been created here
    // i.e., after we have calibrated the mirror and holoLens
    public void SendCreateMsg(string prefabName, int id, SharedObject obj)
    {
        this.sharedCount++;
        obj.id = this.sharedCount;
        Vector3 pos = this.mirrorObj.transform.InverseTransformPoint(obj.transform.position);
        Vector3 forward = this.mirrorObj.transform.InverseTransformDirection(obj.transform.forward);
        Vector3 upward = this.mirrorObj.transform.InverseTransformDirection(obj.transform.up);
        this.udpController.SendCreateMessage(obj.isBodyAnchored, prefabName, this.sharedCount, pos, forward, upward);
        this.sharedObjMap.Add(obj.id, obj);
    }

    // Triggered by the speech command "Done"
    // used in GlobalSpeechHandler
    // locks the mirror clone in place
    public void CompleteCalibration()
    {
        this.mirrorObj.GetComponent<MeshCollider>().enabled = false;
        this.mirrorObj.GetComponent<MeshRenderer>().enabled = false;

        //send Calibration Done message
        this.udpController.SendCalibrationDoneMessage();
        //this.udpController.SendManipulateMessage(0, Vector3.zero, Vector3.zero, Vector3.zero);
    }

}
