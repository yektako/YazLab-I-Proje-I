using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private PanelRenderer gameUI;
    private List<Transform> levels;
    private Label labelBeaconEnable;
    private Label labelBeaconDisable;
    void Start()
    {
        var gameObjects = gameObject.scene.GetRootGameObjects();
        levels = new List<Transform>();
        foreach (var gameObject in gameObjects)
        {
            if (gameObject.CompareTag("Level")) levels.Add(gameObject.transform);
        }
        gameUI.RegisterUIReloadCallback(OnUIReload);
    }

    void Update()
    {
        if (labelBeaconEnable != null && labelBeaconDisable != null)
        {
            if (labelBeaconEnable.visible) labelBeaconEnable.visible = false;
            if (labelBeaconDisable.visible) labelBeaconDisable.visible = false;
        }

        var activeCamera = cinemachineBrain.ActiveVirtualCamera;
        Component cameraComponent = activeCamera as Component;
        Vector3 origin = cameraComponent.transform.position;
        Vector3 direction = cameraComponent.transform.forward;
        RaycastHit[] hits = Physics.RaycastAll(origin,direction);
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Beacon"))
            {
                var beacon = hit.collider.GetComponent<BeaconController>();
                bool isSelected = beacon.GetSelected();
                if (labelBeaconEnable != null && labelBeaconDisable != null)
                {
                    if (isSelected) labelBeaconDisable.visible = true;
                    else labelBeaconEnable.visible = true;
                }
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    TurnOffAllBeacons();
                    if (!isSelected) beacon.Select();
                }
            }
        }
    }

    private void TurnOffAllBeacons()
    {
        
        foreach (var level in levels)
        {
            foreach (Transform child in level)
            {
                var gameObject = child.gameObject;
                if (gameObject.CompareTag("Beacon"))
                {
                    gameObject.GetComponent<BeaconController>().Deselect();
                }
            }
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        labelBeaconEnable = root.Q<Label>("BeaconEnable");
        labelBeaconDisable = root.Q<Label>("BeaconDisable");
    }
}
