using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameStart.Skills
{
    [RequireComponent(typeof(Collider))]
    public class SkillGatedTrigger : MonoBehaviour
    {
        [SerializeField] private List<SkillRequirement> requirements = new List<SkillRequirement>();
        [SerializeField] private UnityEvent onAccessGranted;
        [SerializeField] private UnityEvent onAccessDenied;

        public IReadOnlyList<SkillRequirement> Requirements => requirements;

        public bool CanAccess(PlayerSkills skills) => skills != null && skills.MeetsRequirements(requirements);

        private void OnTriggerEnter(Collider other)
        {
            var skills = other.GetComponent<PlayerSkills>();
            if (skills == null)
            {
                return;
            }

            if (CanAccess(skills))
            {
                onAccessGranted?.Invoke();
            }
            else
            {
                onAccessDenied?.Invoke();
            }
        }
    }
}
