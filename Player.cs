internal class Player : Character
{
    private readonly List<string> inventory = new();

    public string Class { get; private set; }
    public int Gold { get; private set; }
    public int XP { get; private set; }
    public int Level { get; private set; }
    public int Potions { get; private set; }

    public IReadOnlyList<string> Inventory => inventory;

    public Player(string name, string playerClass, int hp, int atk, int def, int potions, int gold)
    {
        Name = name;
        Class = playerClass;
        HP = hp;
        MaxHP = hp;
        ATK = atk;
        DEF = def;
        Potions = potions;
        Gold = gold;
        XP = 0;
        Level = 1;
    }

    public void AddGold(int amount)
    {
        Gold += Math.Max(0, amount);
    }

    public void AddXp(int amount)
    {
        XP += Math.Max(0, amount);
    }

    public bool CanAfford(int amount)
    {
        return Gold >= amount;
    }

    public void SpendGold(int amount)
    {
        if (amount > 0 && Gold >= amount)
        {
            Gold -= amount;
        }
    }

    public bool HasPotions()
    {
        return Potions > 0;
    }

    public int UsePotion()
    {
        if (Potions <= 0)
        {
            return 0;
        }

        Potions--;

        int hpBefore = HP;
        HP = Math.Min(MaxHP, HP + 12);

        return HP - hpBefore;
    }

    public void AddPotion()
    {
        Potions++;
    }

    public void IncreaseAttack(int amount)
    {
        ATK += amount;
    }

    public void IncreaseDefense(int amount)
    {
        DEF += amount;
    }

    public void RestoreToMaxHealth()
    {
        HP = MaxHP;
    }

    public void AddItem(string item)
    {
        if (!string.IsNullOrWhiteSpace(item))
        {
            inventory.Add(item);
        }
    }

    public int SellAllItemsNamed(string itemName)
    {
        int count = inventory.Count(item => item == itemName);

        if (count > 0)
        {
            inventory.RemoveAll(item => item == itemName);
        }

        return count;
    }

    public void LevelUp()
    {
        Level++;

        switch (Class)
        {
            case "Warrior":
                MaxHP += 6;
                ATK += 2;
                DEF += 2;
                break;

            case "Mage":
                MaxHP += 4;
                ATK += 4;
                DEF += 1;
                break;

            case "Rogue":
                MaxHP += 5;
                ATK += 3;
                DEF += 1;
                break;

            default:
                MaxHP += 4;
                ATK += 3;
                DEF += 1;
                break;
        }

        HP = MaxHP;
    }
}
