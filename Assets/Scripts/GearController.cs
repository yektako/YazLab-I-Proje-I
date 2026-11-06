using UnityEngine;

public class GearController : MonoBehaviour, IResettable
{
    public bool isSpinning = true;
    public bool isCollectible = false;
    public float rotationSpeed = 90f;
    [Range(-1, 1)]
    public int directionSign = 1;
    private bool isCollected = false;
    private bool startSpinningState;
    private Transform gearBase;
    private Transform gearAxle;
    private Transform gear;
    private MeshRenderer gearRenderer;
    void Start()
    {
        startSpinningState = isSpinning;
        gearBase = transform.Find("gear/gear base");
        gearAxle = transform.Find("gear/gear axle");
        gear = transform.Find("gear/gear");
        gearRenderer = gear.GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (isSpinning && !isCollected)
        {
            gearAxle.transform.Rotate(rotationSpeed * directionSign * Time.deltaTime * Vector3.forward);
            gear.transform.Rotate(rotationSpeed * directionSign * Time.deltaTime * Vector3.forward);
        }
    }

    public void Collect()
    {
        isSpinning = false;
        isCollected = true;
        gearRenderer.enabled = false;
    }

    public bool IsCollectible()
    {
        return isCollectible && !isCollected;
    }

    public void ResetToStart()
    {
        isSpinning = startSpinningState;
        isCollected = false;
        gearRenderer.enabled = true;
    }
}
