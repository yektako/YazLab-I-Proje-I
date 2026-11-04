using UnityEngine;

public class ActuatorController : MonoBehaviour
{
    public ActuatorState state = ActuatorState.Off;
    private Transform wheelR;
    private Transform wheelL;
    private Vector3 wheelLCenter;
    private Vector3 wheelRCenter;
    void Start()
    {
        wheelR = transform.Find("actuator/actuator wheel R");
        wheelL = transform.Find("actuator/actuator wheel L");
        wheelLCenter = wheelL.GetComponent<BoxCollider>().bounds.center;
        wheelRCenter = wheelR.GetComponent<BoxCollider>().bounds.center;
    }
    void Update()
    {
        if (GetEnabled() && gameObject.activeInHierarchy)
        {
            wheelL.RotateAround(wheelLCenter, Vector3.up, 180 * Time.deltaTime);
            wheelR.RotateAround(wheelRCenter, Vector3.right, 180 * Time.deltaTime);
        }
    }
    public void TurnOn()
    {
        state = ActuatorState.On;
    }
    public void TurnOff()
    {
        state = ActuatorState.Off;
    }
    public void SetEnabled(bool turnedOn)
    {
        if (turnedOn) state = ActuatorState.On;
        else state = ActuatorState.Off;
    }
    public bool GetEnabled()
    {
        return (state == ActuatorState.On);
    }
    public void DetectorTriggerEnter(Collider other)
    {
        if (state == ActuatorState.Locked && (other.CompareTag("NPC") || other.CompareTag("Player"))) TurnOn();
    }
    public void DetectorTriggerExit(Collider other)
    {
    }
    
    public enum ActuatorState
    {
        Off,
        On,
        Locked
    }
}
