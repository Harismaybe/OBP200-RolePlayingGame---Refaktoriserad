using System.Text;

namespace OBP200_RolePlayingGame;

internal class Program
{
    private static Player player = null!;
    private static readonly List<Room> rooms = new();
    private static readonly List<Enemy> enemyTemplates = new();

    private static int currentRoomIndex = 0;
    private static readonly Random rng = new();

    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitEnemyTemplates();

        while (true)
        {
            ShowMainMenu();
            Console.Write("Välj: ");
            var choice = (Console.ReadLine() ?? "").Trim();

            if (choice == "1")
            {
                StartNewGame();
                RunGameLoop();
            }
            else if (choice == "2")
            {
                Console.WriteLine("Avslutar...");
                return;
            }
            else
            {
                Console.WriteLine("Ogiltigt val.");
            }

            Console.WriteLine();
        }
    }

    private static void ShowMainMenu()
    {
        Console.WriteLine("=== Text-RPG ===");
        Console.WriteLine("1. Nytt spel");
        Console.WriteLine("2. Avsluta");
    }

    private static void StartNewGame()
    {
        Console.Write("Ange namn: ");
        var name = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Namnlös";
        }

        Console.WriteLine("Välj klass: 1) Warrior  2) Mage  3) Rogue");
        Console.Write("Val: ");
        var classChoice = (Console.ReadLine() ?? "").Trim();

        string playerClass = "Warrior";
        int hp = 40;
        int atk = 7;
        int def = 5;
        int potions = 2;
        int gold = 15;

        switch (classChoice)
        {
            case "1":
                playerClass = "Warrior";
                hp = 40;
                atk = 7;
                def = 5;
                potions = 2;
                gold = 15;
                break;

            case "2":
                playerClass = "Mage";
                hp = 28;
                atk = 10;
                def = 2;
                potions = 2;
                gold = 15;
                break;

            case "3":
                playerClass = "Rogue";
                hp = 32;
                atk = 8;
                def = 3;
                potions = 3;
                gold = 20;
                break;
        }

        player = new Player(name, playerClass, hp, atk, def, potions, gold);
        player.AddItem("Wooden Sword");
        player.AddItem("Cloth Armor");

        rooms.Clear();
        rooms.Add(new Room("battle", "Skogsstig"));
        rooms.Add(new Room("treasure", "Gammal kista"));
        rooms.Add(new Room("shop", "Vandrande köpman"));
        rooms.Add(new Room("battle", "Grottans mynning"));
        rooms.Add(new Room("rest", "Lägereld"));
        rooms.Add(new Room("battle", "Grottans djup"));
        rooms.Add(new Room("boss", "Urdraken"));

        currentRoomIndex = 0;

        Console.WriteLine($"Välkommen, {name} the {playerClass}!");
        ShowStatus();
    }

    private static void RunGameLoop()
    {
        while (true)
        {
            var room = rooms[currentRoomIndex];
            Console.WriteLine($"--- Rum {currentRoomIndex + 1}/{rooms.Count}: {room.Label} ({room.Type}) ---");

            bool continueAdventure = EnterRoom(room.Type);

            if (IsPlayerDead())
            {
                Console.WriteLine("Du har stupat... Spelet över.");
                break;
            }

            if (!continueAdventure)
            {
                Console.WriteLine("Du lämnar äventyret för nu.");
                break;
            }

            currentRoomIndex++;

            if (currentRoomIndex >= rooms.Count)
            {
                Console.WriteLine();
                Console.WriteLine("Du har klarat äventyret!");
                break;
            }

            Console.WriteLine();
            Console.WriteLine("[C] Fortsätt     [Q] Avsluta till huvudmeny");
            Console.Write("Val: ");
            var post = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (post == "Q")
            {
                Console.WriteLine("Tillbaka till huvudmenyn.");
                break;
            }

            Console.WriteLine();
        }
    }

    private static bool EnterRoom(string type)
    {
        switch ((type ?? "battle").Trim())
        {
            case "battle":
                return DoBattle(isBoss: false);

            case "boss":
                return DoBattle(isBoss: true);

            case "treasure":
                return DoTreasure();

            case "shop":
                return DoShop();

            case "rest":
                return DoRest();

            default:
                Console.WriteLine("Du vandrar vidare...");
                return true;
        }
    }

    private static bool DoBattle(bool isBoss)
    {
        Enemy enemy = GenerateEnemy(isBoss);
        Console.WriteLine($"En {enemy.Name} dyker upp! (HP {enemy.HP}, ATK {enemy.ATK}, DEF {enemy.DEF})");

        while (!enemy.IsDead() && !IsPlayerDead())
        {
            Console.WriteLine();
            ShowStatus();
            Console.WriteLine($"Fiende: {enemy.Name} HP={enemy.HP}");
            Console.WriteLine("[A] Attack   [X] Special   [P] Dryck   [R] Fly");

            if (isBoss)
            {
                Console.WriteLine("(Du kan inte fly från en boss!)");
            }

            Console.Write("Val: ");
            var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (cmd == "A")
            {
                int damage = CalculatePlayerDamage(enemy.DEF);
                ApplyDamageToTarget(enemy, damage);
                Console.WriteLine($"Du slog {enemy.Name} för {damage} skada.");
            }
            else if (cmd == "X")
            {
                int special = UseClassSpecial(enemy.DEF, isBoss);
                ApplyDamageToTarget(enemy, special);
                Console.WriteLine($"Special! {enemy.Name} tar {special} skada.");
            }
            else if (cmd == "P")
            {
                UsePotion();
            }
            else if (cmd == "R" && !isBoss)
            {
                if (TryRunAway())
                {
                    Console.WriteLine("Du flydde!");
                    return true;
                }

                Console.WriteLine("Misslyckad flykt!");
            }
            else
            {
                Console.WriteLine("Du tvekar...");
            }

            if (enemy.IsDead())
            {
                break;
            }

            int enemyDamage = CalculateEnemyDamage(enemy.ATK);
            ApplyDamageToTarget(player, enemyDamage);
            Console.WriteLine($"{enemy.Name} anfaller och gör {enemyDamage} skada!");
        }

        if (IsPlayerDead())
        {
            return false;
        }

        player.AddXp(enemy.XPReward);
        MaybeLevelUp();

        player.AddGold(enemy.GoldReward);

        Console.WriteLine($"Seger! +{enemy.XPReward} XP, +{enemy.GoldReward} guld.");
        MaybeDropLoot(enemy.Name);

        return true;
    }

    private static void ApplyDamageToTarget(IDamageable target, int damage)
    {
        target.ApplyDamage(damage);
    }

    private static Enemy GenerateEnemy(bool isBoss)
    {
        if (isBoss)
        {
            return new Enemy("boss", "Urdraken", 55, 9, 4, 30, 50);
        }

        Enemy template = enemyTemplates[rng.Next(enemyTemplates.Count)];

        int hp = template.HP + rng.Next(-1, 3);
        int atk = template.ATK + rng.Next(0, 2);
        int def = template.DEF + rng.Next(0, 2);
        int xp = template.XPReward + rng.Next(0, 3);
        int gold = template.GoldReward + rng.Next(0, 3);

        return new Enemy(template.Type, template.Name, hp, atk, def, xp, gold);
    }

    private static void InitEnemyTemplates()
    {
        enemyTemplates.Clear();
        enemyTemplates.Add(new Enemy("beast", "Vildsvin", 18, 4, 1, 6, 4));
        enemyTemplates.Add(new Enemy("undead", "Skelett", 20, 5, 2, 7, 5));
        enemyTemplates.Add(new Enemy("bandit", "Bandit", 16, 6, 1, 8, 6));
        enemyTemplates.Add(new Enemy("slime", "Geléslem", 14, 3, 0, 5, 3));
    }

    private static int CalculatePlayerDamage(int enemyDef)
    {
        int baseDamage = Math.Max(1, player.ATK - (enemyDef / 2));
        int roll = rng.Next(0, 3);

        switch (player.Class.Trim())
        {
            case "Warrior":
                baseDamage += 1;
                break;

            case "Mage":
                baseDamage += 2;
                break;

            case "Rogue":
                baseDamage += rng.NextDouble() < 0.2 ? 4 : 0;
                break;
        }

        return Math.Max(1, baseDamage + roll);
    }

    private static int UseClassSpecial(int enemyDef, bool vsBoss)
    {
        int specialDamage = 0;

        if (player.Class == "Warrior")
        {
            Console.WriteLine("Warrior använder Heavy Strike!");
            specialDamage = Math.Max(2, player.ATK + 3 - enemyDef);
            ApplyDamageToTarget(player, 2);
        }
        else if (player.Class == "Mage")
        {
            if (player.Gold >= 3)
            {
                Console.WriteLine("Mage kastar Fireball!");
                player.SpendGold(3);
                specialDamage = Math.Max(3, player.ATK + 5 - (enemyDef / 2));
            }
            else
            {
                Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
                specialDamage = 0;
            }
        }
        else if (player.Class == "Rogue")
        {
            if (rng.NextDouble() < 0.5)
            {
                Console.WriteLine("Rogue utför en lyckad Backstab!");
                specialDamage = Math.Max(4, player.ATK + 6);
            }
            else
            {
                Console.WriteLine("Backstab misslyckades!");
                specialDamage = 1;
            }
        }

        if (vsBoss)
        {
            specialDamage = (int)Math.Round(specialDamage * 0.8);
        }

        return Math.Max(0, specialDamage);
    }

    private static int CalculateEnemyDamage(int enemyAtk)
    {
        int roll = rng.Next(0, 3);
        int damage = Math.Max(1, enemyAtk - (player.DEF / 2)) + roll;

        if (rng.NextDouble() < 0.1)
        {
            damage = Math.Max(1, damage - 2);
        }

        return damage;
    }

    private static void UsePotion()
    {
        if (!player.HasPotions())
        {
            Console.WriteLine("Du har inga drycker kvar.");
            return;
        }

        int healed = player.UsePotion();
        Console.WriteLine($"Du dricker en dryck och återfår {healed} HP.");
    }

    private static bool TryRunAway()
    {
        double chance = 0.25;

        if (player.Class == "Rogue")
        {
            chance = 0.5;
        }
        else if (player.Class == "Mage")
        {
            chance = 0.35;
        }

        return rng.NextDouble() < chance;
    }

    private static bool IsPlayerDead()
    {
        return player.IsDead();
    }

    private static void MaybeLevelUp()
    {
        int xp = player.XP;
        int level = player.Level;
        int nextThreshold = level == 1 ? 10 : (level == 2 ? 25 : (level == 3 ? 45 : level * 20));

        if (xp >= nextThreshold)
        {
            player.LevelUp();
            Console.WriteLine($"Du når nivå {player.Level}! Värden ökade och HP återställd.");
        }
    }

    private static void MaybeDropLoot(string enemyName)
    {
        if (rng.NextDouble() < 0.35)
        {
            string item = enemyName.Contains("Urdraken") ? "Dragon Scale" : "Minor Gem";
            player.AddItem(item);
            Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
        }
    }

    private static bool DoTreasure()
    {
        Console.WriteLine("Du hittar en gammal kista...");

        if (rng.NextDouble() < 0.5)
        {
            int gold = rng.Next(8, 15);
            player.AddGold(gold);
            Console.WriteLine($"Kistan innehåller {gold} guld!");
        }
        else
        {
            string[] items = { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            string found = items[rng.Next(items.Length)];
            player.AddItem(found);
            Console.WriteLine($"Du plockar upp: {found}");
        }

        return true;
    }

    private static bool DoShop()
    {
        Console.WriteLine("En vandrande köpman erbjuder sina varor:");

        while (true)
        {
            Console.WriteLine($"Guld: {player.Gold} | Drycker: {player.Potions}");
            Console.WriteLine("1) Köp dryck (10 guld)");
            Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
            Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
            Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
            Console.WriteLine("5) Lämna butiken");
            Console.Write("Val: ");
            var val = (Console.ReadLine() ?? "").Trim();

            if (val == "1")
            {
                TryBuy(10, () => player.AddPotion(), "Du köper en dryck.");
            }
            else if (val == "2")
            {
                TryBuy(25, () => player.IncreaseAttack(2), "Du köper ett bättre vapen.");
            }
            else if (val == "3")
            {
                TryBuy(25, () => player.IncreaseDefense(2), "Du köper bättre rustning.");
            }
            else if (val == "4")
            {
                SellMinorGems();
            }
            else if (val == "5")
            {
                Console.WriteLine("Du säger adjö till köpmannen.");
                break;
            }
            else
            {
                Console.WriteLine("Köpmannen förstår inte ditt val.");
            }
        }

        return true;
    }

    private static void TryBuy(int cost, Action apply, string successMessage)
    {
        if (player.CanAfford(cost))
        {
            player.SpendGold(cost);
            apply();
            Console.WriteLine(successMessage);
        }
        else
        {
            Console.WriteLine("Du har inte råd.");
        }
    }

    private static void SellMinorGems()
    {
        int count = player.SellAllItemsNamed("Minor Gem");

        if (count == 0)
        {
            Console.WriteLine("Inga 'Minor Gem' i väskan.");
            return;
        }

        int totalGold = count * 5;
        player.AddGold(totalGold);
        Console.WriteLine($"Du säljer {count} st Minor Gem för {totalGold} guld.");
    }

    private static bool DoRest()
    {
        Console.WriteLine("Du slår läger och vilar.");
        player.RestoreToMaxHealth();
        Console.WriteLine("HP återställt till max.");
        return true;
    }

    private static void ShowStatus()
    {
        Console.WriteLine(
            $"[{player.Name} | {player.Class}]  HP {player.HP}/{player.MaxHP}  ATK {player.ATK}  DEF {player.DEF}  LVL {player.Level}  XP {player.XP}  Guld {player.Gold}  Drycker {player.Potions}"
        );

        if (player.Inventory.Count > 0)
        {
            Console.WriteLine($"Väska: {string.Join(";", player.Inventory)}");
        }
    }
}
