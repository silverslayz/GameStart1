namespace GameStart.Combat
{
    /// <summary>
    /// Why a hit landed for what it did. Carried alongside the amount so feedback can
    /// distinguish an ordinary swing from one exploiting a discovered weakness - the
    /// final float alone can't say which, since both are just "some damage".
    /// </summary>
    public enum DamageFlavor
    {
        Normal,
        Weakness
    }
}
