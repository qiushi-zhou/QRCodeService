using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulateMessage : AbsMessage
{
    public int id { get; set; }
    public Vector3 position { get; set; }
    public Vector3 forward { get; set; }
    public Vector3 upward { get; set; }
}
