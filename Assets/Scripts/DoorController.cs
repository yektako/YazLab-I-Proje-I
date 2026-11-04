using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private ActuatorController actuator;
    public DoorState state = DoorState.Closed;
    public bool locked = false;
    public float speedMultiplier = 1;
    public enum DoorState
    {
        Closed,
        Open,
        Closing,
        Opening
    }
    private Transform doorLeft;
    private Transform doorRight;
    private Transform doorFrame;
    private Bounds bounds;
    void Awake()
    {
        doorLeft = transform.Find("metal door/metal door left");
        doorRight = transform.Find("metal door/metal door right");
        doorFrame = transform.Find("metal door/metal door frame");
        bounds = doorFrame.GetComponent<MeshCollider>().bounds;
    }
    void Update()
    {
        if (actuator != null)
        {
            if (state == DoorState.Open && !actuator.GetEnabled()) state = DoorState.Closing;
            else if (state == DoorState.Closed && actuator.GetEnabled()) state = DoorState.Opening;
        }
        if (state == DoorState.Opening)
        {
            if (doorLeft.transform.localPosition.x < bounds.extents.x/transform.localScale.x/3)
            {
                doorLeft.transform.localPosition += speedMultiplier * Time.deltaTime * Vector3.right;
                doorRight.transform.localPosition += speedMultiplier * Time.deltaTime * Vector3.left;
            }
            else
            {
                state = DoorState.Open;
                doorLeft.transform.localPosition = bounds.extents.x/transform.localScale.x/3 * Vector3.right;
                doorRight.transform.localPosition = bounds.extents.x/transform.localScale.x/3 * Vector3.left;
            }
        }
        else if (state == DoorState.Closing)
        {
            if (doorLeft.transform.localPosition.x > 0)
            {
                doorLeft.transform.localPosition -= speedMultiplier * Time.deltaTime * Vector3.right;
                doorRight.transform.localPosition -= speedMultiplier * Time.deltaTime * Vector3.left;
            }
            else
            {
                state = DoorState.Closed;
                doorLeft.transform.position = transform.position;
                doorRight.transform.position = transform.position;
            }
        }
    }
    public void Open()
    {
        if (state == DoorState.Closed) state = DoorState.Opening;
    }
    public void Close()
    {
        if (state == DoorState.Open) state = DoorState.Closing;
    }
}
