using UnityEngine;
using UnityEngine.AI;

public class BeaconController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent npcAgent;
    [SerializeField] private bool isSelected = false;
    private BeaconLightController lightController;
    void Start()
    {
        lightController = GetComponentInChildren<BeaconLightController>();
        lightController.SetStatus(isSelected);
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
        npcAgent.SetDestination(gameObject.transform.position);
    }
    public void Deselect ()
    {
        isSelected = false;
        lightController.TurnOff();
        npcAgent.ResetPath();
    }
}