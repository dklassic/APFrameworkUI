using UnityEngine;

public class LocateCanvasCamera : MonoBehaviour
{
    public string CameraName = string.Empty;
    void Start()
    {
        GameObject go = GameObject.Find(CameraName);
        if (go != null)
            GetComponent<Canvas>().worldCamera = go.GetComponent<Camera>();
    }
}
