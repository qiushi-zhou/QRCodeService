using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class PaperClipBehaviour : MonoBehaviour
{
    private GameObject udpController;
    public void InstantiateMails()
    {
        Debug.Log("touch started");

        GameObject prefabMail1 = (GameObject)Resources.Load("sharedPrefabs/Mail1", typeof(GameObject));
        GameObject prefabMail2 = (GameObject)Resources.Load("sharedPrefabs/Mail2", typeof(GameObject));

        GameObject gObjMail1 = Instantiate(prefabMail1, new Vector3(this.transform.position.x-0.3f, this.transform.position.y + 0.3f, this.transform.position.z + 0.3f), Quaternion.LookRotation(Vector3.forward, Vector3.up));
        GameObject gObjMail2 = Instantiate(prefabMail2, new Vector3(this.transform.position.x+0.3f, this.transform.position.y + 0.3f, this.transform.position.z + 0.3f), Quaternion.LookRotation(Vector3.forward, Vector3.up));

        udpController = GameObject.Find("UDPController");
        udpController.GetComponent<UDPController>().bringBraid();
        udpController.GetComponent<UDPController>().bringPicture();

        this.transform.gameObject.SetActive(false);
    }

}
