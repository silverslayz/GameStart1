namespace GameStart.Class
{
    public class InventorySlot
    {
        public GearItem Item;
        public int Count;

        public bool IsEmpty => Count <= 0;
    }
}
