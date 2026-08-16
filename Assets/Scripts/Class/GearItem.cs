namespace GameStart.Class
{
    [System.Serializable]
    public struct GearItem
    {
        public string Name;
        public float Weight;

        public GearItem(string name, float weight)
        {
            Name = name;
            Weight = weight;
        }
    }
}
