namespace GameStart.Crafting
{
    [System.Serializable]
    public struct ResourceCost
    {
        public string ResourceName;
        public int Amount;

        public ResourceCost(string resourceName, int amount)
        {
            ResourceName = resourceName;
            Amount = amount;
        }
    }
}
