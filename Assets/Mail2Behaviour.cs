﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mail2Behaviour : MonoBehaviour
{
    private GameObject udpController;
    public void InstantiateModel()
    {
        udpController = GameObject.Find("UDPController");

        udpController.GetComponent<UDPController>().bringPicture();

    }
}
