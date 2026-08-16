using UnityEngine;
using GameStart.Interaction;
using GameStart.Economy;

namespace GameStart.Town
{
    public class HousePlot : MonoBehaviour, IInteractable
    {
        [SerializeField] private string unclaimedPrompt = "Claim House Plot";
        [SerializeField] private float repairAmount = 25f;
        [SerializeField] private int repairCostInGems = 2;

        private HouseCondition condition;

        public bool IsClaimed { get; private set; }

        public string InteractionPrompt
        {
            get
            {
                if (!IsClaimed)
                {
                    return unclaimedPrompt;
                }

                if (condition != null && condition.CurrentCondition < condition.MaxCondition)
                {
                    return $"Repair House ({repairCostInGems} gems)";
                }

                return "Your House (Well-Maintained)";
            }
        }

        public void Interact(GameObject interactor)
        {
            if (!IsClaimed)
            {
                var housing = interactor.GetComponent<PlayerHousing>();
                if (housing == null || housing.HasHouse)
                {
                    return;
                }

                IsClaimed = true;
                housing.ClaimHouse(transform.position);
                BuildPlaceholderHouse();
                return;
            }

            if (condition == null || condition.CurrentCondition >= condition.MaxCondition)
            {
                return;
            }

            var currency = interactor.GetComponent<PlayerCurrency>();
            if (currency == null || !currency.TrySpendGems(repairCostInGems))
            {
                return;
            }

            condition.Repair(repairAmount);
        }

        private void BuildPlaceholderHouse()
        {
            GameObject house = new GameObject("PlaceholderHouse");
            house.transform.SetParent(transform, false);
            house.transform.localPosition = Vector3.zero;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Walls";
            body.transform.SetParent(house.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(3f, 2f, 3f);
            SetColor(body, new Color(0.65f, 0.55f, 0.4f));

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(house.transform, false);
            roof.transform.localPosition = new Vector3(0f, 2.25f, 0f);
            roof.transform.localScale = new Vector3(3.4f, 0.5f, 3.4f);
            SetColor(roof, new Color(0.4f, 0.2f, 0.15f));

            condition = house.AddComponent<HouseCondition>();
            condition.SetWallsRenderer(body.GetComponent<Renderer>());
        }

        private void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            renderer.sharedMaterial.color = color;
        }
    }
}
