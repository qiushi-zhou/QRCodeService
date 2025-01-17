﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class UDPController : MonoBehaviour
{
    private NetworkSettings networkSettings;
    public SceneController sceneController;

    public GameObject clip;

    public GameObject shared_braid;
    public GameObject shared_picture;
    public GameObject shared_braid_flipped;
    public GameObject shared_picture_flipped;

    private int clicks = 0;
    public Transform Camera;

#if !UNITY_EDITOR
    
    UDPSocket socket;
    
#endif

    private void Awake()
    {
        this.networkSettings = GameObject.Find("NetworkSettings").GetComponent<NetworkSettings>();
    }

    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
        this.socket = new UDPSocket(this.networkSettings);    
        this.socket.Listen();
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if !UNITY_EDITOR

        this.CheckAndReadMessage();

#endif
    }

#if !UNITY_EDITOR
    void CheckAndReadMessage()
    {
        if (this.socket.MsgAvailable())
        {
            MessageReceivedHandler(this.socket.ReceiveMsg());
        }
    }
#endif

    void MessageReceivedHandler(byte[] serializedMsg)
    {
        //deserialize and process serializedMsg
        JsonMessage jm = Serializer.Deserialize<JsonMessage>(serializedMsg);
        if(jm.message is CreateMessage)
        {
            Debug.Log("Create Message Received");

            CreateMessage createMessage = (CreateMessage)jm.message;

            //creates an object in relation to REAL SPACE
            Vector3 Pos = this.sceneController.mirrorObj.transform.TransformPoint(createMessage.position);
            Vector3 forward = this.sceneController.mirrorObj.transform.TransformDirection(createMessage.forward);
            Vector3 Upward = this.sceneController.mirrorObj.transform.TransformDirection(createMessage.upward);

            
            GameObject prefab = (GameObject)Resources.Load("sharedPrefabs/"+createMessage.prefabName, typeof(GameObject));
            GameObject gObj = Instantiate(prefab, Pos, Quaternion.LookRotation(forward,Upward));
            Debug.Log("created at: "+ createMessage.position);
            gObj.name = createMessage.prefabName;

            

            //update id of sharedObj
            gObj.GetComponent<SharedObject>().id = createMessage.id;
            gObj.GetComponent<SharedObject>().isBodyAnchored = createMessage.isBodyAnchored;

            if (!gObj.GetComponent<SharedObject>().isBodyAnchored)
            {
                //add hololens specific scripts
                gObj.AddComponent<Microsoft.MixedReality.Toolkit.UI.ConstraintManager>();
                gObj.AddComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionGrabbable>();
                gObj.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
                gObj.AddComponent<ManipulationUpdater>(); // do not add this to flippedObj

                gObj.GetComponent<ManipulationUpdater>().udpController = this;
                gObj.GetComponent<ManipulationUpdater>().mirrorObj = this.sceneController.mirrorObj;
            }
            


            this.sceneController.sharedCount++; // need to remember that some messages may arrive OUT OR ORDER so CHECK THIS
            this.sceneController.sharedObjMap.Add(createMessage.id, gObj.GetComponent<SharedObject>());


            //create a Mirrored object representing MIRROR SPACE
            Vector3 flippedPosition = createMessage.position;
            flippedPosition.y *= -1;
            flippedPosition = this.sceneController.mirrorObj.transform.TransformPoint(flippedPosition);
            //using same forward and up for now..  might need to flip these as well wrt to mirrorObj
            GameObject flippedObj = Instantiate(prefab, flippedPosition, Quaternion.LookRotation(forward, Upward));
            flippedObj.name = createMessage.prefabName + "Flipped";
            //flippedObj.GetComponent<MeshRenderer>().enabled = false;
            

            flippedObj.transform.localScale = new Vector3(flippedObj.transform.localScale.x * -1, flippedObj.transform.localScale.y, flippedObj.transform.localScale.z);

            //TODO
            //Quaternion objGlobalRot = gObj.transform.rotation;
            MeshFilter mirrorPlane = this.sceneController.mirrorObj.GetComponent<MeshFilter>();
            Vector3 mirrorNormal = mirrorPlane.transform.TransformDirection(mirrorPlane.mesh.normals[0]);
            //Quaternion mirrorQuat = new Quaternion(mirrorNormal.x, mirrorNormal.y, mirrorNormal.z, 0);

            Vector3 gObjForw = gObj.transform.forward;
            Vector3 mirrored = Vector3.Reflect(gObjForw, mirrorNormal);
            this.transform.rotation = Quaternion.LookRotation(mirrored, gObj.transform.up);

            //add hololens specific scripts to flipped obj
            flippedObj.AddComponent<Microsoft.MixedReality.Toolkit.UI.ConstraintManager>();
            flippedObj.AddComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionGrabbable>();
            flippedObj.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();

            //attach script to make flipped object mirror the original gObj and vice versa
            gObj.AddComponent<FlippedBehaviour>();
            flippedObj.AddComponent<FlippedBehaviour>();
            gObj.GetComponent<FlippedBehaviour>().flippedObj = flippedObj;
            flippedObj.GetComponent<FlippedBehaviour>().flippedObj = gObj;

            // assign flipped object to ManipulationUpdater
            gObj.GetComponent<ManipulationUpdater>().flippedObject = flippedObj;

            if (createMessage.prefabName == "shared_paperclip")
            {
                clip.transform.position = gObj.transform.position;
                clip.transform.rotation = gObj.transform.rotation;
                clip.transform.localScale = gObj.transform.localScale;
                gObj.SetActive(false);
                flippedObj.SetActive(false);
            }

            if (createMessage.prefabName == "shared_picture")
            {
                this.shared_picture = gObj;
                this.shared_picture_flipped = flippedObj;
                flippedObj.GetComponent<MeshRenderer>().enabled = false;
                //gObj.transform.position += new Vector3(0, 2, 0);
                //flippedObj.transform.position += new Vector3(0, 2, 0);
                this.SendManipulateMessage(
                gObj.GetComponent<SharedObject>().id,
                this.sceneController.mirrorObj.transform.InverseTransformPoint(gObj.transform.position),
                this.sceneController.mirrorObj.transform.InverseTransformDirection(gObj.transform.forward),
                this.sceneController.mirrorObj.transform.InverseTransformDirection(gObj.transform.up)
                );
            }

            if (createMessage.prefabName == "shared_braid")
            {
                this.shared_braid = gObj;
                this.shared_braid_flipped = flippedObj;
                //flippedObj.SetActive(false);
                //gObj.transform.position += new Vector3(0, 2, 0);
                //flippedObj.transform.position += new Vector3(0, 2, 0);
                this.SendManipulateMessage(
                gObj.GetComponent<SharedObject>().id,
                this.sceneController.mirrorObj.transform.InverseTransformPoint(gObj.transform.position),
                this.sceneController.mirrorObj.transform.InverseTransformDirection(gObj.transform.forward),
                this.sceneController.mirrorObj.transform.InverseTransformDirection(gObj.transform.up)
                );
            }
        }
        if (jm.message is ManipulateMessage)
        {
            Debug.Log("Manipulate Message Received");

            ManipulateMessage manipulateMessage = (ManipulateMessage)jm.message;
            SharedObject obj = this.sceneController.sharedObjMap[manipulateMessage.id];
            obj.transform.position = this.sceneController.mirrorObj.transform.TransformPoint(manipulateMessage.position);
            obj.transform.rotation = Quaternion.LookRotation(
                this.sceneController.mirrorObj.transform.TransformDirection(manipulateMessage.forward),
                this.sceneController.mirrorObj.transform.TransformDirection(manipulateMessage.upward)
                );
            GameObject flippedObj = obj.GetComponent<FlippedBehaviour>().flippedObj;
            flippedObj.transform.position = obj.transform.position;
            flippedObj.transform.rotation = obj.transform.rotation;
            if(manipulateMessage.id == shared_braid.GetComponent<SharedObject>().id)
            {
                shared_braid.SetActive(false);
                shared_braid_flipped.SetActive(false);
            }
        }
    }

    public void handleClick() { 
        if (clicks == 0)
        {
            bringBraid();
        }
        else if (clicks ==1)
        {
            bringPicture();
            clip.GetComponent<MeshRenderer>().material.color = Color.white;
        }
        clicks++;
    }

    public void bringBraid()
    {
        GameObject prefabMail1 = (GameObject)Resources.Load("sharedPrefabs/Mail1", typeof(GameObject));
        GameObject gObjMail1 = Instantiate(prefabMail1, new Vector3(Camera.position.x , Camera.position.y , Camera.position.z + 0.5f), Quaternion.LookRotation(Vector3.forward, Vector3.up));

        shared_braid.transform.position -= new Vector3(0, 2, 0);
        shared_braid_flipped.transform.position -= new Vector3(0, 2, 0);

        Vector3 rot = shared_braid_flipped.transform.rotation.eulerAngles;
        rot = new Vector3(rot.x, rot.y + 180, rot.z);
        shared_braid_flipped.transform.rotation = Quaternion.Euler(rot);

        this.SendManipulateMessage(
                shared_braid.GetComponent<SharedObject>().id,
                this.sceneController.mirrorObj.transform.InverseTransformPoint(shared_braid.transform.position),
                this.sceneController.mirrorObj.transform.InverseTransformDirection(shared_braid.transform.forward),
                this.sceneController.mirrorObj.transform.InverseTransformDirection(shared_braid.transform.up)
                );
        
    }

    public void bringPicture()
    {
        GameObject prefabMail2 = (GameObject)Resources.Load("sharedPrefabs/Mail2", typeof(GameObject));
        GameObject gObjMail2 = Instantiate(prefabMail2, new Vector3(Camera.position.x, Camera.position.y, Camera.position.z + 0.5f), Quaternion.LookRotation(Vector3.forward, Vector3.up));

        shared_picture.transform.position -= new Vector3(0, 2, 0);
        shared_picture_flipped.transform.position -= new Vector3(0, 2, 0);
        this.SendManipulateMessage(
                shared_picture.GetComponent<SharedObject>().id,
                this.sceneController.mirrorObj.transform.InverseTransformPoint(shared_picture_flipped.transform.position),
                this.sceneController.mirrorObj.transform.InverseTransformDirection(shared_picture_flipped.transform.forward),
                this.sceneController.mirrorObj.transform.InverseTransformDirection(shared_picture_flipped.transform.up)
                );
    }

    public void SendTest()
    {
#if !UNITY_EDITOR
        JsonMessage jm = new JsonMessage();
        jm.action = JsonMessage.Type.CREATE;
        jm.message = new CreateMessage();
        byte[] msg = Serializer.Serialize<JsonMessage>(jm);
        this.socket.SendMessage(msg);
#endif

    }

    /// <summary>
    /// Sends a message to create an object using prefab with Name = prefabName 
    /// the id of this obj = id
    /// the obj is created in position = position
    /// the obj has direction (blue axis) = forward
    /// the obj has upwards (green axis) = upwards
    /// </summary>
    /// <param name="prefabName"></param>
    /// <param name="id"></param>
    /// <param name="position"></param>
    /// <param name="forward"></param>
    /// <param name="upward"></param>
    public void SendCreateMessage(bool isBodyAnchored, string prefabName, int id, Vector3 position, Vector3 forward, Vector3 upward)
    {
#if !UNITY_EDITOR
        JsonMessage jm = new JsonMessage();
        jm.action = JsonMessage.Type.CREATE;
        CreateMessage createMsg = new CreateMessage();
        createMsg.isBodyAnchored = isBodyAnchored;
        createMsg.prefabName = prefabName;
        createMsg.id = id;
        createMsg.position = position;
        createMsg.forward = forward;
        createMsg.upward = upward;
        jm.message = createMsg;
        byte[] msg = Serializer.Serialize<JsonMessage>(jm);
        this.socket.SendMessage(msg);
#endif
    }

    /// <summary>
    /// Sends a message to update the position and direction of the object with id = id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="position"></param>
    /// <param name="forward"></param>
    /// <param name="upward"></param>
    public void SendManipulateMessage(int id, Vector3 position, Vector3 forward, Vector3 upward)
    {
#if !UNITY_EDITOR
        JsonMessage jm = new JsonMessage();
        jm.action = JsonMessage.Type.MANIPULATE;
        ManipulateMessage manipulateMsg = new ManipulateMessage();
        manipulateMsg.id = id;
        manipulateMsg.position = new Vector3(position.x, position.y, position.z);
        manipulateMsg.forward = forward;
        manipulateMsg.upward = upward;
        jm.message = manipulateMsg;
        byte[] msg = Serializer.Serialize<JsonMessage>(jm);
        this.socket.SendMessage(msg);
#endif
    }

    /// <summary>
    /// Sends a message to confirm calibration is done
    /// </summary>
    public void SendCalibrationDoneMessage()
    {
#if !UNITY_EDITOR
        JsonMessage jm = new JsonMessage();
        jm.action = JsonMessage.Type.CALIBRATION_DONE; 
        CalibrationDoneMessage cd = new CalibrationDoneMessage();
        jm.message = cd;
        byte[] msg = Serializer.Serialize<JsonMessage>(jm);
        this.socket.SendMessage(msg);
#endif
    }


    /*
    void MessageReceivedHandler(byte[] serializedMsg)
    {
        JsonMessage jm = Serializer.Deserialize<JsonMessage>(serializedMsg);
        if (jm.messageObject is TestEntity)
        {
            
        }
        if (jm.messageObject is PostItContent)
        {
            

            this.postItContents.Add((PostItContent)jm.messageObject);
        }
        if (jm.messageObject is PostItNumber)
        {
            if (this.postItContents.Count != ((PostItNumber)jm.messageObject).number)
            {
                StartCoroutine(this.sceneController.Quit("Received " + this.postItContents.Count + "/" + ((PostItNumber)jm.messageObject).number));
            }
        }
        if (jm.messageObject is PostItVal)
        {
            
        }
        if (jm.messageObject is PostItValList)
        {
           
        }
        if (jm.messageObject is ActionMap)
        {
            this.sceneController.ExecuteAction((ActionMap)jm.messageObject);
        }
        if (jm.messageObject is ActionMapList)
        {
            this.sceneController.ExecuteActions((ActionMapList)jm.messageObject);
        }
        if (jm.messageObject is WhiteBoardPostItMap)
        {
            this.sceneController.ClusterColorPostIt((WhiteBoardPostItMap)jm.messageObject);
        }
        if (jm.messageObject is WhiteBoardPostItMapList)
        {
            this.sceneController.ClusterColorPostIts((WhiteBoardPostItMapList)jm.messageObject);
        }
    }
    */


    /*
    /// <summary>
    /// Used to send the initial message to start the experience
    /// </summary>
    public void SendStartMsg()
    {
#if !UNITY_EDITOR
        JsonMessage jmStart = new JsonMessage();
        jmStart.messageType = "Start";
        jmStart.messageObject = null;
        byte[] msg = Serializer.Serialize<JsonMessage>(jmStart);
        this.listenSocket.SendMessage(msg);
#endif
    }

    /// <summary>
    /// Used to send the last message to end the experience and record the completion time
    /// </summary>
    public void SendEndMsg(float completionTime)
    {
#if !UNITY_EDITOR
        JsonMessage jm = new JsonMessage();
        jm.messageType = "Completed";

        CompletionTime ct = new CompletionTime();
        ct.completionTime = completionTime;
        
        jm.messageObject = ct;

        byte[] msg = Serializer.Serialize<JsonMessage>(jm);
        this.listenSocket.SendMessage(msg);
#endif
    }

    /// <summary>
    /// Sends a snapshot of the observation space.
    /// </summary>
    /// <param name="pList">List of all the variables related to the post it gameobjects</param>
    public void SendObsRL(PostItValList pList)
    {
#if !UNITY_EDITOR
        //sending the whole list does not seem to work as only a part of it is received on the script    

        foreach(PostItVal pVal in pList.values)
        {
            JsonMessage jm = new JsonMessage();
            jm.messageType = "PostItVal";
            jm.messageObject = pVal;
            byte[] msg = Serializer.Serialize<JsonMessage>(jm);
            this.listenSocket.SendMessage(msg);
        }


#endif
    }

    /// <summary>
    /// Sends a list of whiteboards with respectively attached post it notes
    /// </summary>
    /// <param name="wbMapList">list of (whiteboard id, postIt Id) representing the attached postIt to the whiteBoad</param>
    public void SendObsCluster(WhiteBoardPostItMapList wbMapList)
    {
#if !UNITY_EDITOR
        foreach(WhiteBoardPostItMap wbPostItMap in wbMapList.values)
        {
            JsonMessage jm = new JsonMessage();
            jm.messageType = "WhiteBoardPostItMap";
            jm.messageObject = wbPostItMap;
            byte[] msg = Serializer.Serialize<JsonMessage>(jm);
            this.listenSocket.SendMessage(msg);
        }

#endif
    }

    public List<PostItContent> GetPostItContents()
    {
        return this.postItContents;
    }

    public SceneController.METHOD getMethod()
    {
        return this.networkSettings.method;
    }
    */
}