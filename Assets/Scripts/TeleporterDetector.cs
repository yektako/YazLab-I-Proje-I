using UnityEngine;

public class TeleporterDetector : MonoBehaviour
{
    private TeleporterController controller;
    void Start()
    {
        controller = GetComponentInParent<TeleporterController>();
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
