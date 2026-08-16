using System;
using UnityEngine;

namespace GameStart.Class
{
    public class PlayerClassSelection : MonoBehaviour
    {
        public event Action<PlayerClassType> ClassSelected;

        public PlayerClassType SelectedClass { get; private set; }
        public bool HasSelectedClass { get; private set; }

        public void SelectClass(PlayerClassType classType)
        {
            SelectedClass = classType;
            HasSelectedClass = true;
            ClassSelected?.Invoke(classType);
        }
    }
}
