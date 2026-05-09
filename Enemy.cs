internal class Enemy : Character
{
    public string Type { get; }
    public int XPReward { get; }
    public int GoldReward { get; }

    public Enemy(string type, string name, int hp, int atk, int def, int xpReward, int goldReward)
    {
        Type = type;
        Name = name;
        HP = hp;
        MaxHP = hp;
        ATK = atk;
        DEF = def;
        XPReward = xpReward;
        GoldReward = goldReward;
    }

    public override void ApplyDamage(int damage)
    {
        base.ApplyDamage(damage);
    }
}
