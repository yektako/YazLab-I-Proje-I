using System;
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
    private Label labelActuatorEnable;
    private Label labelActuatorDisable;
    private Label labelActuatorLocked;
    private Image crosshairEmpty;
    private Image crosshairSelect;
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
        if (labelBeaconEnable != null && labelBeaconDisable != null && labelActuatorEnable != null && labelActuatorDisable != null
         && labelActuatorLocked != null && crosshairEmpty != null && crosshairSelect != null)
        {
            if (labelBeaconEnable.visible) labelBeaconEnable.visible = false;
            if (labelBeaconDisable.visible) labelBeaconDisable.visible = false;
            if (labelActuatorEnable.visible) labelActuatorEnable.visible = false;
            if (labelActuatorDisable.visible) labelActuatorDisable.visible = false;
            if (labelActuatorLocked.visible) labelActuatorLocked.visible = false;
            if (crosshairSelect.visible) crosshairSelect.visible = false;
            if (!crosshairEmpty.visible) crosshairEmpty.visible = true;
        }

        var activeCamera = cinemachineBrain.ActiveVirtualCamera;
        Component cameraComponent = activeCamera as Component;
        Vector3 origin = cameraComponent.transform.position;
        Vector3 direction = cameraComponent.transform.forward;
        RaycastHit[] hits = Physics.RaycastAll(origin,direction);
        Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        if (hits.Length > 0)
        {
            RaycastHit hit = hits[0];
            if (hits.Length > 1)
            {
                if (hit.collider.CompareTag("Detector")) hit = hits[1];
            }
            if (hit.collider.CompareTag("Beacon"))
            {
                var beacon = hit.collider.GetComponent<BeaconController>();
                bool isSelected = beacon.GetSelected();
                if (labelBeaconEnable != null && labelBeaconDisable != null)
                {
                    if (isSelected) labelBeaconDisable.visible = true;
                    else labelBeaconEnable.visible = true;
                }
                ShowCrosshairSelect();
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    if (!isSelected)
                    {
                        TurnOffAllBeacons();
                        beacon.Select();
                    }
                    else beacon.Deselect();
                }
            }
            else if (hit.collider.CompareTag("Actuator"))
            {
                var actuator = hit.collider.GetComponent<ActuatorController>();
                if (actuator.state == ActuatorController.ActuatorState.Locked)
                {
                    if (labelActuatorLocked != null) labelActuatorLocked.visible = true;
                }
                else
                {
                    bool isTurnedOn = actuator.GetEnabled();
                    if (labelActuatorEnable != null && labelActuatorDisable != null)
                    {
                        if (isTurnedOn) labelActuatorDisable.visible = true;
                        else labelActuatorEnable.visible = true;
                    }
                    ShowCrosshairSelect();
                    if (Mouse.current.leftButton.wasReleasedThisFrame)
                    {
                        if (isTurnedOn) actuator.TurnOff();
                        else actuator.TurnOn();
                    }
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

    private void ShowCrosshairSelect()
    {
        if (crosshairEmpty != null && crosshairSelect != null)
        {
            crosshairEmpty.visible = false;
            crosshairSelect.visible = true;
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        labelBeaconEnable = root.Q<Label>("BeaconEnable");
        labelBeaconDisable = root.Q<Label>("BeaconDisable");
        labelActuatorEnable = root.Q<Label>("ActuatorEnable");
        labelActuatorDisable = root.Q<Label>("ActuatorDisable");
        labelActuatorLocked = root.Q<Label>("ActuatorLocked");
        crosshairEmpty = root.Q<Image>("CrosshairEmpty");
        crosshairSelect = root.Q<Image>("CrosshairSelect");
    }
}
