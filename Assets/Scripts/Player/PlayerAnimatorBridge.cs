using UnityEngine;

namespace GameStart.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimatorBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;

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

            animator.SetFloat(SpeedHash, controller.CurrentSpeed);
            animator.SetFloat(MotionSpeedHash, 1f);
            animator.SetBool(GroundedHash, controller.IsGrounded);
            animator.SetBool(JumpHash, controller.IsAscending);
            animator.SetBool(FreeFallHash, controller.IsFalling);
        }
    }
}
