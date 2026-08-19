using UnityEngine;

namespace GameStart.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimatorBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        /// <summary>
        /// Where the locomotion blend tree puts the walk and run clips. The Starter Assets
        /// controller blends Idle(0) - Walk_N(2) - Run_N(6) on "Speed", and those numbers
        /// are the metres per second the clips were authored for.
        /// </summary>
        [SerializeField] private float walkBlendPoint = 2f;
        [SerializeField] private float runBlendPoint = 6f;

        /// <summary>
        /// Bounds on how far playback is stretched to match travel. Below 1 the feet drag,
        /// far above it the character starts to look sped-up rather than fast.
        /// </summary>
        [SerializeField] private float minMotionSpeed = 0.5f;
        [SerializeField] private float maxMotionSpeed = 2.5f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
        private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");

        private PlayerController controller;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            float speed = controller.CurrentSpeed;
            float blend = ToBlendSpeed(speed);

            animator.SetFloat(SpeedHash, blend);
            animator.SetFloat(MotionSpeedHash, ToMotionSpeed(speed, blend));
            animator.SetBool(GroundedHash, controller.IsGrounded);
            animator.SetBool(JumpHash, controller.IsAscending);
            animator.SetBool(FreeFallHash, controller.IsFalling);
        }

        /// <summary>
        /// Maps real movement speed onto the blend tree's scale, so each gait plays the clip
        /// it should. Feeding raw m/s in meant walking at 4 landed midway between the walk
        /// and run clips - the character ran everywhere - while sprinting sat past the top
        /// threshold, where going faster changed nothing on screen.
        /// </summary>
        private float ToBlendSpeed(float speed)
        {
            float walk = controller.WalkSpeed;
            float sprint = controller.SprintSpeed;

            if (speed <= 0.01f)
            {
                return 0f;
            }

            if (speed <= walk)
            {
                return walk <= 0.01f ? walkBlendPoint : Mathf.Lerp(0f, walkBlendPoint, speed / walk);
            }

            float range = sprint - walk;
            float t = range <= 0.01f ? 1f : Mathf.Clamp01((speed - walk) / range);
            return Mathf.Lerp(walkBlendPoint, runBlendPoint, t);
        }

        /// <summary>
        /// Stretches playback so the feet keep up with the ground actually covered. The
        /// clips are authored for their blend point, so a gait moving faster than that has
        /// to play proportionally faster or it slides.
        /// </summary>
        private float ToMotionSpeed(float speed, float blend)
        {
            if (blend <= 0.01f)
            {
                return 1f;
            }

            return Mathf.Clamp(speed / blend, minMotionSpeed, maxMotionSpeed);
        }
    }
}
