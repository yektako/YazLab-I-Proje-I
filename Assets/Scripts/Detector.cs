using UnityEngine;

public class Detector : MonoBehaviour
{
    private IDetectingController controller;
    void Start()
    {
        controller = GetComponentInParent<IDetectingController>();
    }
    void OnTriggerEnter(Collider other)
    {
        controller.DetectorTriggerEnter(other);
    }
    void OnTriggerExit(Collider other)
    {
        controller.DetectorTriggerExit(other);
    }
}
