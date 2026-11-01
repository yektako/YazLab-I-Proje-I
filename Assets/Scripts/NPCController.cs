using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NavMeshAgent))]

    public class NPCController : MonoBehaviour
    {
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
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            AssignAnimationIDs();
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
                    var index = Random.Range(0, audioFootsteps.Length);
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