using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cameraFirstPerson;
        [SerializeField] private CinemachineCamera cameraThirdPerson;
        [SerializeField] private bool isFirstPerson = false;
        [SerializeField] private float moveSpeed = 4;
        [SerializeField] private float rotateSpeed = 200;
        [SerializeField] private float moveInertia = 2f;
        [SerializeField] private float rotateInertia = 4f;
        [SerializeField] private float gravity = -6f;
        private CharacterController controller;
        private PlayerInputActions inputActions;
        private PlayerInputActions.PlayerActions playerActions;
        private Transform robotHead;
        private Transform robotLargeWheels;
        private Transform robotSmallWheelsBack;
        private Transform robotSmallWheelsFront;
        private MeshRenderer robotTracksRenderer;
        private MaterialPropertyBlock robotTracksProperty;
        private readonly int robotTracksShaderID = Shader.PropertyToID("_BaseMap_ST");
        private float robotTracksScroll = 0;
        private Vector2 momentum = Vector2.zero;
        private TeleporterController checkpointTeleporter;
        private void OnEnable()
        {
            controller = GetComponent<CharacterController>();
            
            inputActions = new PlayerInputActions();
            playerActions = inputActions.Player;
            playerActions.Enable();

            robotHead = transform.Find("robot/robot head");
            robotLargeWheels = transform.Find("robot/robot large wheels");
            robotSmallWheelsBack = transform.Find("robot/robot small wheels back");
            robotSmallWheelsFront = transform.Find("robot/robot small wheels front");
            robotTracksRenderer = transform.Find("robot/robot tracks").GetComponent<MeshRenderer>();
            robotTracksProperty = new MaterialPropertyBlock();

            if (isFirstPerson) ChangeView();
        }
        void Start()
        {
            
        }
        
        void Update()
        {
            Vector2 move = playerActions.Move.ReadValue<Vector2>();
            if (move != Vector2.zero || momentum != Vector2.zero)
            {
                ApplyMomentumThenMove(move);
            }
            ApplyGravity();

            if (playerActions.ChangeView.WasPressedThisFrame()) ChangeView();
        }
        private void MovePlayer(Vector2 move)
        {
            transform.Rotate(move.x * rotateSpeed * Time.deltaTime * Vector3.up);
            //transform.Translate(move.y * moveSpeed * Vector3.forward * Time.deltaTime);
            controller.Move(move.y * moveSpeed * Time.deltaTime * transform.forward);
            MoveVisuals(move);
        }
        private void MoveVisuals(Vector2 move)
        {
            Vector3 wheelsRotation = move.magnitude * Mathf.Sign(move.y) * rotateSpeed * Time.deltaTime * Vector3.left;
            robotLargeWheels.Rotate(wheelsRotation);
            robotSmallWheelsBack.Rotate(wheelsRotation);
            robotSmallWheelsFront.Rotate(wheelsRotation);

            if (move.magnitude > 0.05f)
            {
                robotTracksRenderer.GetPropertyBlock(robotTracksProperty);
                robotTracksScroll += (1f % (5 * move.magnitude * Time.deltaTime)) * -Mathf.Sign(move.y);
                robotTracksProperty.SetVector(robotTracksShaderID,new Vector4(1f,1f,0,robotTracksScroll));
                robotTracksRenderer.SetPropertyBlock(robotTracksProperty);
            }
        }
        private void ApplyMomentumThenMove(Vector2 move)
        {
            if (Mathf.Abs(move.x) == 1) {momentum.x = move.x;}
            else
            {
                if (Mathf.Abs(momentum.x) > 0) momentum.x -= rotateInertia * Mathf.Sign(momentum.x) * Time.deltaTime;
                if (Mathf.Abs(momentum.x) < 0.1f) momentum.x = 0;
            }
            if (Mathf.Abs(move.y) == 1) {momentum.y = move.y;}
            else
            {
                if (Mathf.Abs(momentum.y) > 0) momentum.y -= moveInertia * Mathf.Sign(momentum.y) * Time.deltaTime;
                if (Mathf.Abs(momentum.y) < 0.1f) momentum.y = 0;
            }
            MovePlayer(momentum);
        }
        private void ApplyGravity()
        {
            controller.Move(gravity * Vector3.up * Time.deltaTime);
            if (transform.position.y < -20) UseCheckpoint();
        }
        private void ChangeView()
        {
            if (cameraFirstPerson.Priority > cameraThirdPerson.Priority)
            {
                cameraThirdPerson.Priority = 20;
                cameraFirstPerson.Priority = 10;
                robotHead.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            else
            {
                cameraFirstPerson.Priority = 20;
                cameraThirdPerson.Priority = 10;
                robotHead.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
        public void SetCheckpoint(TeleporterController teleporter)
        {
            checkpointTeleporter = teleporter;
        }
        public void UseCheckpoint()
        {
            if (checkpointTeleporter != null)
            {
                controller.enabled = false;
                checkpointTeleporter.IgnorePlayer();
                transform.position = checkpointTeleporter.GetExitPosition();
                controller.enabled = true;
            }
            else
            {
                controller.enabled = false;
                transform.position = Vector3.zero;
                controller.enabled = true;
            }
        }
    }
}
