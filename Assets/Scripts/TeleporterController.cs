using Player;
using UnityEngine;

public class TeleporterController : MonoBehaviour, IDetectingController
{
    [SerializeField] private TeleporterController targetTeleporter;
    public Color lightColor = new Color(0.5f,0.5f,0.5f,0.6f);
    private BeaconLightController lightController;
    [SerializeField] private int lightPriority = 0;
    public AudioClip audioTeleport;
    [Range(0, 1)] public float audioVolume = 0.5f;
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
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetCheckpoint(targetTeleporter);
            player.UseCheckpoint();
            var npc = player.npc;
            npc.StopImmediately();
            npc.UseCheckpoint();
        }
        else if (other.CompareTag("Player") && isPlayerIgnored)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.SetCheckpoint(this);
            AudioSource.PlayClipAtPoint(audioTeleport, GetExitPosition(), audioVolume);
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
