using UnityEngine;

public interface IDetectingController
{
    void DetectorTriggerEnter(Collider other);
    void DetectorTriggerExit(Collider other);
}
