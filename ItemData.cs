using Terraria;

namespace MyPlugin;

public class ItemData
{
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public int Damage { get; set; }
    public int Defense { get; set; }
    public int Stack { get; set; }
    public byte Prefix { get; set; }
    public int Crit { get; set; }
    public float KnockBack { get; set; }
    public int fishingPole { get; set; } // 钓鱼竿等级
    public int bait { get; set; } //鱼饵值
    public int pick { get; set; } // 镐力
    public int axe { get; set; } // 斧力
    public int hammer { get; set; } // 锤力
    public int createTile { get; set; } // 创建的方块类型
    public int createWall { get; set; } // 创建的墙类型
    public int Value { get; set; } // 价格
    public bool wornArmor { get; set; } // 是否为穿戴的护甲

    public int headSlot { get; set; }
    public int bodySlot { get; set; }
    public int legSlot { get; set; }

    public int UseTime { get; set; }
    public int UseAnimation { get; set; }
    public int UseStyle { get; set; }
    public int Ammo { get; set; }
    public int HealLife { get; set; } // 物品使用时回复的生命值
    public int HealMana { get; set; } // 物品使用时回复的魔法值
    public int UseAmmo { get; set; }
    public bool AutoReuse { get; set; }
    public bool UseTurn { get; set; }
    public bool Channel { get; set; }
    public bool NoMelee { get; set; }
    public bool NoUseGraphic { get; set; }
    public int Shoot { get; set; }
    public float ShootSpeed { get; set; }
    public bool Melee { get; set; } // 是否为近战武器
    public bool Magic { get; set; } // 是否为魔法武器
    public bool Ranged { get; set; } // 是否为远程武器
    public bool Summon { get; set; } // 是否为召唤武器
    public bool sentry { get; set; } // 是否为哨兵
    public bool accessory { get; set; } // 是否为饰品，0表示不是，1表示是饰品
    public bool consumable { get; set; } // 是否为消耗品
    public bool material { get; set; } // 是否为材料

    public static ItemData FromItem(Item item)
    {
        return new ItemData
        {
            Name = item.Name,
            Type = item.type,
            Damage = item.damage,
            Defense = item.defense,
            Stack = item.stack,
            Prefix = item.prefix,
            Crit = item.crit,
            KnockBack = item.knockBack,
            bait = item.bait, // 鱼饵值
            fishingPole = item.fishingPole, // 钓鱼竿等级
            pick = item.pick, // 镐力
            axe = item.axe, // 斧力
            hammer = item.hammer, // 锤力
            createTile = item.createTile, // 创建的方块类型
            createWall = item.createWall, // 创建的墙类型
            Value = item.value,
            UseTime = item.useTime,
            UseAnimation = item.useAnimation,
            UseStyle = item.useStyle,
            Ammo = item.ammo,
            HealLife = item.healLife, // 物品使用时回复的生命值
            HealMana = item.healMana, // 物品使用时回复的魔法值
            UseAmmo = item.useAmmo,
            AutoReuse = item.autoReuse,
            UseTurn = item.useTurn,
            Channel = item.channel,
            NoMelee = item.noMelee,
            NoUseGraphic = item.noUseGraphic,
            Shoot = item.shoot,
            ShootSpeed = item.shootSpeed,
            Melee = item.melee,
            Magic = item.magic,
            Summon = item.summon,
            Ranged = item.ranged,
            sentry = item.sentry, // 是否为哨兵
            accessory = item.accessory, // 是否为饰品
            consumable = item.consumable, // 是否为消耗品
            material = item.material, // 是否为材料
            headSlot = item.headSlot, // 头部装备栏
            bodySlot = item.bodySlot, // 身体装备栏
            legSlot = item.legSlot, // 腿部装备栏
            wornArmor = item.wornArmor, // 是否为穿戴的护甲
        };
    }

    public void ApplyTo(Item item)
    {
        // 确保只应用于相同类型的物品
        if (item.type != Type) return;
        
        item.damage = Damage;
        item.defense = Defense;
        item.stack = Stack;
        item.prefix = Prefix;
        item.crit = Crit;
        item.knockBack = KnockBack;
        item.bait = bait;
        item.fishingPole = fishingPole; // 钓鱼竿等级
        item.pick = pick; // 镐力
        item.axe = axe; // 斧力
        item.hammer = hammer; // 锤力
        item.createTile = createTile; // 创建的方块类型
        item.createWall = createWall; // 创建的墙类型
        item.value = Value;

        item.useTime = UseTime;
        item.useAnimation = UseAnimation;
        item.useStyle = UseStyle;
        item.ammo = Ammo;
        item.healLife = HealLife; // 物品使用时回复的生命值
        item.healMana = HealMana; // 物品使用时回复的魔法值
        item.useAmmo = UseAmmo;
        item.autoReuse = AutoReuse;
        item.useTurn = UseTurn;
        item.channel = Channel;
        item.noMelee = NoMelee;
        item.noUseGraphic = NoUseGraphic;
        item.shoot = Shoot;
        item.shootSpeed = ShootSpeed;

        item.melee = Melee;
        item.magic = Magic;
        item.ranged = Ranged;
        item.summon = Summon;
        item.sentry = sentry; // 是否为哨兵
        item.consumable = consumable; // 是否为消耗品
        item.material = material; // 是否为材料
        item.wornArmor = wornArmor; // 是否为穿戴的护甲
        item.accessory = accessory; // 是否为饰品
        item.headSlot = headSlot; // 头部装备栏
        item.bodySlot = bodySlot; // 身体装备栏
        item.legSlot = legSlot; // 腿部装备栏

    }
}