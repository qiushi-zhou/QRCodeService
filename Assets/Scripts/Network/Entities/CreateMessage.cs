using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMessage : AbsMessage
{
    public bool isBodyAnchored { get; set; } // determines if the following Pose values should be made a chile of the Main Camera.
    public string prefabName { get; set; }
    public int id { get; set; }
    public Vector3 position { get; set; }
    public Vector3 forward { get; set; }
    public Vector3 upward { get; set; }
}
