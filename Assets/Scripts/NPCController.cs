using System;
using Player;
using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NavMeshAgent))]

    public class NPCController : MonoBehaviour
    {
        public PlayerController player;
        private Animator animator;
        private CharacterController controller;
        private NavMeshAgent agent;
        private int animIDSpeed;
        private int animIDJump;
        private int animIDFreeFall;
        private int animIDGrounded;
        private int animIDMotionSpeed;
        public AudioClip audioLanding;
        public AudioClip[] audioFootsteps;
        [Range(0, 1)] public float audioVolume = 0.5f;
        public float raycastMaxDistance = 1.6f;
        public float gravity = -9f;
        private float acceleration;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            AssignAnimationIDs();
            acceleration = agent.acceleration;
        }
        private void Start()
        {
        }
        private void Update()
        {
            Move();
        }
        private void Move()
        {
            animator.SetFloat(animIDSpeed, agent.velocity.magnitude);
            animator.SetFloat(animIDMotionSpeed, 1f);

            if (agent.remainingDistance > 0) CheckHazards();
        }
        private void CheckHazards()
        {
            bool detected = false;
            Vector3 origin = transform.position + 0.7f*Vector3.up;
            detected |= CheckHazardVector(origin, transform.forward);

            Vector3 directionL = transform.forward - 0.3f*transform.right;
            detected |= CheckHazardVector(origin,directionL);
            
            Vector3 directionR = transform.forward + 0.3f*transform.right;
            detected |= CheckHazardVector(origin,directionR);

            Vector3 directionLL = transform.forward - 0.75f*transform.right;
            detected |= CheckHazardVector(origin,directionLL);
            
            Vector3 directionRR = transform.forward + 0.75f*transform.right;
            detected |= CheckHazardVector(origin,directionRR);

            if (detected)
            {
                agent.isStopped = true;
                agent.acceleration = 150;
            }
            else
            {
                if (agent.hasPath)
                {
                    agent.isStopped = false;
                    if (agent.acceleration > acceleration) agent.acceleration = acceleration;
                }
            }
            
        }
        private bool CheckHazardVector(Vector3 origin, Vector3 direction)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin,direction,raycastMaxDistance,~0,QueryTriggerInteraction.Collide);
            Debug.DrawRay(origin,direction*raycastMaxDistance);
            Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Hazard"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void UseCheckpoint()
        {
            if (player.HasCheckpoint())
            {
                TeleporterController teleporter = player.checkpointTeleporter;
                controller.enabled = false;
                agent.Warp(teleporter.GetExitPosition() - 0.7f*player.transform.forward);
                controller.enabled = true;
            }
            else
            {
                controller.enabled = false;
                agent.Warp(player.transform.position - 0.7f*player.transform.forward);
                controller.enabled = true;
            }
        }
        public void StopImmediately()
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        private void AssignAnimationIDs()
        {
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (audioFootsteps.Length > 0)
                {
                    var index = UnityEngine.Random.Range(0, audioFootsteps.Length);
                    AudioSource.PlayClipAtPoint(audioFootsteps[index], transform.TransformPoint(controller.center), audioVolume);
                }
            }
        }
        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(audioLanding, transform.TransformPoint(controller.center), audioVolume);
            }
        }
    }
}