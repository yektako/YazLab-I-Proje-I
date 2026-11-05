using UnityEngine;

public class BeaconLightController : MonoBehaviour
{
    private MeshRenderer[] lightRenderers;
    private bool isTurnedOn = false;
    void Awake()
    {
        lightRenderers = GetComponentsInChildren<MeshRenderer>();
        UpdateRenderers();
    }
    void Update()
    {
        if (isTurnedOn) transform.Rotate(new Vector3(2.5f,0,1.5f));
    }
    public void TurnOn()
    {
        isTurnedOn = true;
        UpdateRenderers();
    }
    public void TurnOff()
    {
        isTurnedOn = false;
        UpdateRenderers();
    }
    public void SetStatus (bool isLightOn)
    {
        isTurnedOn = isLightOn;
        UpdateRenderers();
    }
    public void UpdateRenderers()
    {
        if (lightRenderers != null)
        {
            foreach (var renderer in lightRenderers)
            {
                renderer.enabled = isTurnedOn;
            }
        }
    }
    public void SetColor(Color color)
    {
        foreach (var renderer in lightRenderers)
        {
            renderer.material.color = color;
        }
    }
}
