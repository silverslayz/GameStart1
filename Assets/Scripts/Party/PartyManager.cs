using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameStart.Party
{
    public class PartyManager : MonoBehaviour
    {
        private readonly List<string> members = new List<string>();

        public event Action<string> MemberJoined;
        public event Action<string> MemberLeft;

        public IReadOnlyList<string> Members => members;
        public bool IsInParty(string memberName) => members.Contains(memberName);

        public void Join(string memberName)
        {
            if (string.IsNullOrEmpty(memberName) || members.Contains(memberName))
            {
                return;
            }

            members.Add(memberName);
            MemberJoined?.Invoke(memberName);
        }

        public void Leave(string memberName)
        {
            if (!members.Remove(memberName))
            {
                return;
            }

            MemberLeft?.Invoke(memberName);
        }
    }
}
