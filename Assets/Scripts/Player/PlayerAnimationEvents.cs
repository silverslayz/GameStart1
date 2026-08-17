using UnityEngine;

namespace GameStart.Player
{
    // Receives animation events embedded in the Starter Assets character clips
    // (footstep/landing SFX cues). No audio hookup yet - present so the
    // Animator doesn't log "no receiver" warnings during playback.
    public class PlayerAnimationEvents : MonoBehaviour
    {
        public void OnFootstep(AnimationEvent animationEvent)
        {
        }

        public void OnLand(AnimationEvent animationEvent)
        {
        }
    }
}
