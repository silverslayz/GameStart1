namespace GameStart.Combat
{
    public interface IDamageable
    {
        // The flavor defaults so existing damage sources (starvation, environmental)
        // don't have to care, while a weakness-exploiting swing can say so.
        void TakeDamage(float amount, DamageFlavor flavor = DamageFlavor.Normal);
    }
}
