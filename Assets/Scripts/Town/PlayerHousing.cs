using System;
using UnityEngine;

namespace GameStart.Town
{
    public class PlayerHousing : MonoBehaviour
    {
        public event Action<Vector3> HouseClaimed;

        public bool HasHouse { get; private set; }
        public Vector3 HouseLocation { get; private set; }

        public void ClaimHouse(Vector3 location)
        {
            if (HasHouse)
            {
                return;
            }

            HasHouse = true;
            HouseLocation = location;
            HouseClaimed?.Invoke(location);
        }
    }
}
