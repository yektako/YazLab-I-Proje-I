using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class FirstPerson : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private CinemachineCamera cameraFirstPerson;
    [SerializeField] private CinemachineCamera cameraThirdPerson;
    private bool isFirstPerson;
    private bool isPlayerHidden;
    void Start()
    {
        isFirstPerson = cameraFirstPerson.Priority > cameraThirdPerson.Priority;
        Renderer playerRenderer = playerObject.GetComponent<Renderer>();
        isPlayerHidden = playerRenderer.shadowCastingMode.Equals(UnityEngine.Rendering.ShadowCastingMode.On);
        if (isFirstPerson && !isPlayerHidden)
        {
            HidePlayer();
        }
    }

    void Update()
    {
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            isFirstPerson = !isFirstPerson;
            if (!isFirstPerson)
            {
                playerTransform.localRotation = Quaternion.Euler(0,0,0);
                ShowPlayer();
            }
        }
        if (isFirstPerson && isPlayerHidden)
        {
            float yAngle = cameraFirstPerson.transform.rotation.eulerAngles.y;
            playerTransform.rotation = Quaternion.Euler(0,yAngle,0);
        }
        if (isFirstPerson && !isPlayerHidden && !cameraThirdPerson.IsLive)
        {
            HidePlayer();
        }
    }

    private void HidePlayer()
    {
        Renderer playerRenderer = playerObject.GetComponent<Renderer>();
        playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        isPlayerHidden = true;
    }
    private void ShowPlayer()
    {
        Renderer playerRenderer = playerObject.GetComponent<Renderer>();
        playerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        isPlayerHidden = false;
    }
}
