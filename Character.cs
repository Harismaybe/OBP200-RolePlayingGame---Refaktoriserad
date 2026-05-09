internal abstract class Character : IDamageable
{
    public string Name { get; protected set; } = string.Empty;
    public int HP { get; protected set; }
    public int MaxHP { get; protected set; }
    public int ATK { get; protected set; }
    public int DEF { get; protected set; }

    public virtual void ApplyDamage(int damage)
    {
        HP -= Math.Max(0, damage);
        if (HP < 0)
        {
            HP = 0;
        }
    }

    public bool IsDead()
    {
        return HP <= 0;
    }
}
