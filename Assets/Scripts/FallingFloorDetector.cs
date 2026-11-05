using UnityEngine;

public class FallingFloorDetector : MonoBehaviour
{
    private FallingFloorController controller;
    void Start()
    {
        controller = GetComponentInParent<FallingFloorController>();
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
