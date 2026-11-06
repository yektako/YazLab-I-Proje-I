using System;
using System.Collections.Generic;
using Player;
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
    private PlayerController player;
    private List<Transform> levels;
    private Label labelBeaconEnable;
    private Label labelBeaconDisable;
    private Label labelActuatorEnable;
    private Label labelActuatorDisable;
    private Label labelActuatorLocked;
    private Label labelActuatorReset;
    private Label labelCollectGear;
    private Label labelCollectibleCount;
    private Image crosshairEmpty;
    private Image crosshairSelect;
    void Start()
    {
        player = gameObject.GetComponent<PlayerController>();
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
         && labelActuatorLocked != null && labelActuatorReset != null && labelCollectGear != null && crosshairEmpty != null
         && crosshairSelect != null)
        {
            if (labelBeaconEnable.visible) labelBeaconEnable.visible = false;
            if (labelBeaconDisable.visible) labelBeaconDisable.visible = false;
            if (labelActuatorEnable.visible) labelActuatorEnable.visible = false;
            if (labelActuatorDisable.visible) labelActuatorDisable.visible = false;
            if (labelActuatorLocked.visible) labelActuatorLocked.visible = false;
            if (labelActuatorReset.visible) labelActuatorReset.visible = false;
            if (labelCollectGear.visible) labelCollectGear.visible = false;
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
                if (CheckInteract())
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
                else if (actuator.state == ActuatorController.ActuatorState.Reset)
                {
                    if (labelActuatorReset != null) labelActuatorReset.visible = true;
                    ShowCrosshairSelect();
                    if (CheckInteract())
                    {
                        ResetLevel(actuator);
                    }
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
                    if (CheckInteract())
                    {
                        if (isTurnedOn) actuator.TurnOff();
                        else actuator.TurnOn();
                    }
                }
            }
            else if (hit.collider.CompareTag("Collectible"))
            {
                var gear = hit.collider.GetComponent<GearController>();
                if (gear.IsCollectible())
                {
                    if (labelCollectGear != null) labelCollectGear.visible = true;
                    ShowCrosshairSelect();
                    if (CheckInteract())
                    {
                        gear.Collect();
                        player.collectibleCount++;
                        labelCollectibleCount.SetText(player.collectibleCount);
                    }
                }
            }
        }
    }
    private bool CheckInteract()
    {
        return Mouse.current.leftButton.wasReleasedThisFrame || Keyboard.current.eKey.wasReleasedThisFrame;
    }
    public void ResetLevel(ActuatorController actuator)
    {
        Debug.Log("seviye sıfırlanıyor");
        Transform level = actuator.transform.parent;
        if (level == null) return;
        else if (level.CompareTag("Level"))
        {
            foreach (Transform levelObject in level)
            {
                if (levelObject.childCount > 0)
                {
                    foreach (Transform subObject in levelObject)
                    {
                        ResetObjectIfResettable(subObject);
                    }
                }
                else ResetObjectIfResettable(levelObject);
            }

            TurnOffAllBeacons();
            player.npc.StopImmediately();
            player.npc.UseCheckpoint();
        }
    }
    public void ResetObjectIfResettable(Transform transform)
    {
        IResettable controller = transform.GetComponent<IResettable>();
        if (controller != null) controller.ResetToStart();
    }
    private void TurnOffAllBeacons()
    {
        foreach (var level in levels)
        {
            foreach (Transform child in level)
            {
                if (child.CompareTag("Beacon"))
                {
                    child.GetComponent<BeaconController>().Deselect();
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
        labelActuatorReset = root.Q<Label>("ActuatorReset");
        labelCollectGear = root.Q<Label>("CollectGear");
        crosshairEmpty = root.Q<Image>("CrosshairEmpty");
        crosshairSelect = root.Q<Image>("CrosshairSelect");
        labelCollectibleCount = root.Q<Label>("CollectibleCount");
    }
}
