using UnityEngine;
using UnityEngine.AI;

public class BeaconController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent npcAgent;
    [SerializeField] private bool isSelected = false;
    [SerializeField] private ActuatorController actuator;
    private BeaconLightController lightController;
    void Start()
    {
        lightController = GetComponentInChildren<BeaconLightController>();
        SetSelected(isSelected);
    }
    void Update()
    {
    }
    public bool GetSelected ()
    {
        return isSelected;
    }
    public void SetSelected (bool isSelected)
    {
        this.isSelected = isSelected;
        if (isSelected) Select();
        else Deselect();
    }
    public void Select ()
    {
        isSelected = true;
        lightController.TurnOn();
        if (actuator == null) npcAgent.SetDestination(gameObject.transform.position);
        else npcAgent.SetDestination(actuator.transform.position);
    }
    public void Deselect ()
    {
        isSelected = false;
        lightController.TurnOff();
        npcAgent.ResetPath();
    }
}
