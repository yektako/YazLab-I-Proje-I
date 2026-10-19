using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class ViewSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cameraFirstPerson;
    [SerializeField] private CinemachineCamera cameraThirdPerson;
    void Start()
    {
    }

    void Update()
    {
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (cameraFirstPerson.Priority > cameraThirdPerson.Priority)
            {
                cameraThirdPerson.Priority = 20;
                cameraFirstPerson.Priority = 10;
            }
            else
            {
                cameraFirstPerson.Priority = 20;
                cameraThirdPerson.Priority = 10;
            }
        }
    }
}
