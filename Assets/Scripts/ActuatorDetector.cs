using UnityEngine;

public class ActuatorDetector : MonoBehaviour
{
    private ActuatorController controller;
    void Start()
    {
        controller = GetComponentInParent<ActuatorController>();
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
