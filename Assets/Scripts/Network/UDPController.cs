﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class UDPController : MonoBehaviour
{
    private NetworkSettings networkSettings;
    public SceneController sceneController;


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

            Vector3 Pos = this.sceneController.mirrorObj.transform.TransformPoint(createMessage.position);
            Vector3 forward = this.sceneController.mirrorObj.transform.TransformDirection(createMessage.forward);
            Vector3 Upward = this.sceneController.mirrorObj.transform.TransformDirection(createMessage.upward);

            GameObject gObj = (GameObject)Resources.Load("sharedPrefabs/"+createMessage.prefabName, typeof(GameObject));
            Instantiate(gObj, Pos, Quaternion.LookRotation(forward,Upward));
            Debug.Log("created at: "+ createMessage.position);


            this.sceneController.sharedCount++; // need to remember that some messages may arrive OUT OR ORDER so CHECK THIS
            this.sceneController.sharedObjMap.Add(createMessage.id, gObj.GetComponent<SharedObject>()); 
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
        }
        

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
        createMsg.forward = upward;
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
        jm.action = JsonMessage.Type.CREATE;
        ManipulateMessage manipulateMsg = new ManipulateMessage();
        manipulateMsg.id = id;
        manipulateMsg.position = position;
        manipulateMsg.forward = forward;
        manipulateMsg.forward = upward;
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