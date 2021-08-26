using MRTKExtensions.QRCodes;
using UnityEngine;

public class JetController : MonoBehaviour
{
    [SerializeField]
    private QRTrackerController trackerController;

    private void Start()
    {
        trackerController.PositionSet += PoseFound;
    }

    private void PoseFound(object sender, Pose pose)
    {
        var childObj = transform.GetChild(0);

        Vector3 rot = pose.rotation.eulerAngles;
        rot = new Vector3(rot.x , rot.y +180, rot.z);
        pose.rotation = Quaternion.Euler(rot);

        childObj.SetPositionAndRotation(pose.position, pose.rotation);
        childObj.gameObject.SetActive(true);
    }
}