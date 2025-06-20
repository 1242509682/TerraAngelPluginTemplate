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
    public int UseTime { get; set; }
    public int UseAnimation { get; set; }
    public int UseStyle { get; set; }
    public int Ammo { get; set; }
    public int bait { get; set; }
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
    public bool Melee { get; set; }
    public bool Magic { get; set; }
    public bool Ranged { get; set; }
    public bool Summon { get; set; }
    public int Value { get; set; }

    public static ItemData FromItem(Item item)
    {
        return new ItemData
        {
            Name = item.Name,
            Type = item.type,
            Damage = item.damage,
            Defense = item.defense,
            Stack = item.stack,
            Prefix = (byte)item.prefix,
            Crit = item.crit,
            KnockBack = item.knockBack,
            UseTime = item.useTime,
            UseAnimation = item.useAnimation,
            UseStyle = item.useStyle,
            Ammo = item.ammo,
            HealLife = item.healLife, // 物品使用时回复的生命值
            HealMana = item.healMana, // 物品使用时回复的魔法值
            bait = item.bait,
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
            Value = item.value
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
        item.useTime = UseTime;
        item.useAnimation = UseAnimation;
        item.useStyle = UseStyle;
        item.ammo = Ammo;
        item.bait = bait;
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
        item.value = Value;
    }
}