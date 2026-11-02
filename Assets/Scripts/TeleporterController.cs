using Player;
using UnityEngine;

public class TeleporterController : MonoBehaviour
{
    [SerializeField] private TeleporterController targetTeleporter;
    public Color lightColor = new Color(0.5f,0.5f,0.5f,0.6f);
    private BeaconLightController lightController;
    [SerializeField] private int lightPriority = 0;
    private bool isPlayerIgnored = false;
    void Start()
    {
        lightController = GetComponentInChildren<BeaconLightController>();
        lightController.SetColor(lightColor);
        if (targetTeleporter != null)
        {
            SetExitTeleporter(targetTeleporter);
            lightController.TurnOn();
        }
        else
        {
            lightController.TurnOff();
        }
    }
    void Update()
    {
    }
    public Vector3 GetExitPosition()
    {
        Bounds bounds = GetComponentInChildren<MeshCollider>().bounds;
        return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
    }
    public void SetExitTeleporter(TeleporterController exitTeleporter)
    {
        targetTeleporter = exitTeleporter;
        if (lightPriority < exitTeleporter.lightPriority)
        {
            lightColor = exitTeleporter.lightColor;
            lightController.SetColor(lightColor);
        }
    }
    public void DetectorTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerIgnored && targetTeleporter != null)
        {
            targetTeleporter.IgnorePlayer();
            var characterController = other.GetComponent<CharacterController>();
            characterController.enabled = false;
            other.transform.position = targetTeleporter.GetExitPosition();
            characterController.enabled = true;
        }
        else if (isPlayerIgnored)
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.SetCheckpoint(this);
        }
    }
    public void DetectorTriggerExit(Collider other)
    {
        isPlayerIgnored = false;
        if (targetTeleporter != null) lightController.TurnOn();
    }
    public void IgnorePlayer()
    {
        isPlayerIgnored = true;
        lightController.TurnOff();
    }
}
