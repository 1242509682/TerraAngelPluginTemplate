using MonoMod.RuntimeDetour;
using Newtonsoft.Json;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Events;

namespace MyPlugin;

public class NPCEventSystem
{
    private static Hook? NpcUpdateHook; // 钩子
    public static event EventHandler<NPCUpdateEventArgs>? OnNPCUpdate; // NPC更新事件
    // 用于反射调用私有方法
    public static MethodInfo? OpenShopMethod;
    public static MethodInfo? HelpTextMethod;

    public static void Register()
    {
        // 获取NPC.UpdateNPC方法的反射信息
        MethodInfo UpdateNPC = typeof(NPC).GetMethod("UpdateNPC", BindingFlags.Instance | BindingFlags.Public)!;

        // 创建钩子
        NpcUpdateHook = new Hook(UpdateNPC, OnUpdateNPC);

        // 获取私有方法信息
        OpenShopMethod = typeof(Main).GetMethod("OpenShop", BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(int)],null);

        HelpTextMethod = typeof(Main).GetMethod("HelpText",BindingFlags.Static | BindingFlags.NonPublic);
    }

    public static void Dispose()
    {
        // 卸载钩子
        NpcUpdateHook?.Dispose();
        NpcUpdateHook = null;
        OnNPCUpdate = null;
        OpenShopMethod = null;
        HelpTextMethod = null;
    }

    // 钩住的UpdateNPC方法
    public static void OnUpdateNPC(Action<NPC, int> orig, NPC npc, int whoAmI)
    {
        orig(npc, whoAmI);  // 调用原始方法
        OnNPCUpdate?.Invoke(null, new NPCUpdateEventArgs(whoAmI, npc)); // 触发事件
    }
}

// 使用记录类型简化事件参数
public record NPCUpdateEventArgs(int whoAmI, NPC npc);

internal record NPCInfo(int ID, string Name, bool IsTownNPC);

#region 用于税收官随机奖励物品的类
public class RewardItem
{
    [JsonProperty("启用物品")]
    public bool Enabled { get; set; } = true;
    [JsonProperty("物品ID")]
    public int ItemID { get; set; } = Terraria.ID.ItemID.PlatinumCoin;
    [JsonProperty("物品数量")]
    public int Stack { get; set; } = 1;
    [JsonProperty("物品概率")]
    public int Chance { get; set; } = 100;
} 
#endregion

#region 自定义商店物品数据
public class CShopItemInfo
{
    [JsonProperty("物品ID")]
    public int id;
    [JsonProperty("物品前缀")]
    public int prefix;
    [JsonProperty("物品数量")]
    public int stack = 1;
    [JsonProperty("物品价格")]
    public int price = 1;
    [JsonProperty("物品进度")]
    public List<string> unlock = new List<string>();

    public CShopItemInfo Clone()
    {
        return new CShopItemInfo
        {
            id = this.id,
            prefix = this.prefix,
            stack = this.stack,
            price = this.price,
            unlock = new List<string>(this.unlock)
        };
    }
}
#endregion

#region 自定义商店物品类
public class ShopItem
{
    [JsonProperty("开启自定义商店")]
    public bool Enabled { get; set; } = true;
    [JsonProperty("替换原始物品")]
    public bool ReplaceOriginal { get; set; } = false;
    [JsonProperty("商店NPC名称")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("商店NPC类型")]
    public int NpcType { get; set; } = 0;
    [JsonProperty("自定义物品表")]
    public List<CShopItemInfo> item = new List<CShopItemInfo>();

    // 按解锁筛选方法
    public List<CShopItemInfo> FilterByUnlock(int plrIndex)
    {
        List<CShopItemInfo> list = new List<CShopItemInfo>();
        foreach (CShopItemInfo item in item)
        {
            bool flag = true;
            foreach (string item2 in item.unlock)
            {
                if (int.TryParse(item2, out var itemID))
                {
                    if (itemID > 0 && Main.player[plrIndex].inventory.Where((Item PItem) => PItem.netID == itemID).Count() == 0)
                    {
                        flag = false;
                    }
                    continue;
                }
                switch (item2)
                {
                    case "史莱姆王":
                    case "史王":
                        if (!NPC.downedSlimeKing)
                        {
                            flag = false;
                        }
                        break;
                    case "克眼":
                    case "克苏鲁之眼":
                        if (!NPC.downedBoss1)
                        {
                            flag = false;
                        }
                        break;
                    case "巨鹿":
                    case "鹿角怪":
                        if (!NPC.downedDeerclops)
                        {
                            flag = false;
                        }
                        break;
                    case "克脑":
                    case "世吞":
                    case "世界吞噬者":
                    case "克苏鲁之脑":
                    case "世界吞噬怪":
                        if (!NPC.downedBoss2)
                        {
                            flag = false;
                        }
                        break;
                    case "蜂王":
                        if (!NPC.downedQueenBee)
                        {
                            flag = false;
                        }
                        break;
                    case "骷髅王":
                        if (!NPC.downedBoss3)
                        {
                            flag = false;
                        }
                        break;
                    case "困难模式":
                    case "肉后":
                    case "血肉墙":
                        if (!Main.hardMode)
                        {
                            flag = false;
                        }
                        break;
                    case "毁灭者":
                        if (!NPC.downedMechBoss1)
                        {
                            flag = false;
                        }
                        break;
                    case "双子魔眼":
                        if (!NPC.downedMechBoss2)
                        {
                            flag = false;
                        }
                        break;
                    case "机械骷髅王":
                        if (!NPC.downedMechBoss3)
                        {
                            flag = false;
                        }
                        break;
                    case "世纪之花":
                    case "花后":
                    case "世花":
                        if (!NPC.downedPlantBoss)
                        {
                            flag = false;
                        }
                        break;
                    case "石后":
                    case "石巨人":
                        if (!NPC.downedGolemBoss)
                        {
                            flag = false;
                        }
                        break;
                    case "史后":
                    case "史莱姆皇后":
                        if (!NPC.downedQueenSlime)
                        {
                            flag = false;
                        }
                        break;
                    case "光之女皇":
                    case "光女":
                        if (!NPC.downedEmpressOfLight)
                        {
                            flag = false;
                        }
                        break;
                    case "猪鲨":
                    case "猪龙鱼公爵":
                        if (!NPC.downedFishron)
                        {
                            flag = false;
                        }
                        break;
                    case "教徒":
                    case "拜月教邪教徒":
                        if (!NPC.downedAncientCultist)
                        {
                            flag = false;
                        }
                        break;
                    case "月亮领主":
                        if (!NPC.downedMoonlord)
                        {
                            flag = false;
                        }
                        break;
                    case "哀木":
                        if (!NPC.downedHalloweenTree)
                        {
                            flag = false;
                        }
                        break;
                    case "南瓜王":
                        if (!NPC.downedHalloweenKing)
                        {
                            flag = false;
                        }
                        break;
                    case "常绿尖叫怪":
                        if (!NPC.downedChristmasTree)
                        {
                            flag = false;
                        }
                        break;
                    case "冰雪女王":
                        if (!NPC.downedChristmasIceQueen)
                        {
                            flag = false;
                        }
                        break;
                    case "圣诞坦克":
                        if (!NPC.downedChristmasSantank)
                        {
                            flag = false;
                        }
                        break;
                    case "火星飞碟":
                        if (!NPC.downedMartians)
                        {
                            flag = false;
                        }
                        break;
                    case "小丑":
                        if (!NPC.downedClown)
                        {
                            flag = false;
                        }
                        break;
                    case "日耀柱":
                        if (!NPC.downedTowerSolar)
                        {
                            flag = false;
                        }
                        break;
                    case "星旋柱":
                        if (!NPC.downedTowerVortex)
                        {
                            flag = false;
                        }
                        break;
                    case "星云柱":
                        if (!NPC.downedTowerNebula)
                        {
                            flag = false;
                        }
                        break;
                    case "星尘柱":
                        if (!NPC.downedTowerStardust)
                        {
                            flag = false;
                        }
                        break;
                    case "一王后":
                        if (!NPC.downedMechBossAny)
                        {
                            flag = false;
                        }
                        break;
                    case "三王后":
                        if (!NPC.downedMechBoss1 || !NPC.downedMechBoss2 || !NPC.downedMechBoss3)
                        {
                            flag = false;
                        }
                        break;
                    case "一柱后":
                        flag = NPC.downedTowerNebula || NPC.downedTowerSolar || NPC.downedTowerStardust || NPC.downedTowerVortex;
                        break;
                    case "四柱后":
                        if (!NPC.downedTowerNebula || !NPC.downedTowerSolar || !NPC.downedTowerStardust || !NPC.downedTowerVortex)
                        {
                            flag = false;
                        }
                        break;
                    case "哥布林入侵":
                        if (!NPC.downedGoblins)
                        {
                            flag = false;
                        }
                        break;
                    case "海盗入侵":
                        if (!NPC.downedPirates)
                        {
                            flag = false;
                        }
                        break;
                    case "霜月":
                        if (!NPC.downedFrost)
                        {
                            flag = false;
                        }
                        break;
                    case "血月":
                        if (!Main.bloodMoon)
                        {
                            flag = false;
                        }
                        break;
                    case "雨天":
                        if (!Main.raining)
                        {
                            flag = false;
                        }
                        break;
                    case "白天":
                        if (!Main.dayTime)
                        {
                            flag = false;
                        }
                        break;
                    case "晚上":
                        if (Main.dayTime)
                        {
                            flag = false;
                        }
                        break;
                    case "大风天":
                        if (!Main.IsItAHappyWindyDay)
                        {
                            flag = false;
                        }
                        break;
                    case "万圣节":
                        if (!Main.halloween)
                        {
                            flag = false;
                        }
                        break;
                    case "圣诞节":
                        if (!Main.xMas)
                        {
                            flag = false;
                        }
                        break;
                    case "派对":
                        if (!BirthdayParty.PartyIsUp)
                        {
                            flag = false;
                        }
                        break;
                    case "2020":
                        if (!Main.drunkWorld)
                        {
                            flag = false;
                        }
                        break;
                    case "2021":
                        if (!Main.tenthAnniversaryWorld)
                        {
                            flag = false;
                        }
                        break;
                    case "ftw":
                        if (!Main.getGoodWorld)
                        {
                            flag = false;
                        }
                        break;
                    case "ntb":
                        if (!Main.notTheBeesWorld)
                        {
                            flag = false;
                        }
                        break;
                    case "dst":
                        if (!Main.dontStarveWorld)
                        {
                            flag = false;
                        }
                        break;
                    case "森林":
                        if (!Main.player[plrIndex].ShoppingZone_Forest)
                        {
                            flag = false;
                        }
                        break;
                    case "丛林":
                        if (!Main.player[plrIndex].ZoneJungle)
                        {
                            flag = false;
                        }
                        break;
                    case "沙漠":
                        if (!Main.player[plrIndex].ZoneDesert)
                        {
                            flag = false;
                        }
                        break;
                    case "雪原":
                        if (!Main.player[plrIndex].ZoneSnow)
                        {
                            flag = false;
                        }
                        break;
                    case "洞穴":
                        if (!Main.player[plrIndex].ZoneUnderworldHeight)
                        {
                            flag = false;
                        }
                        break;
                    case "海洋":
                        if (!Main.player[plrIndex].ZoneBeach)
                        {
                            flag = false;
                        }
                        break;
                    case "神圣":
                        if (!Main.player[plrIndex].ZoneHallow)
                        {
                            flag = false;
                        }
                        break;
                    case "蘑菇":
                        if (!Main.player[plrIndex].ZoneGlowshroom)
                        {
                            flag = false;
                        }
                        break;
                    case "腐化之地":
                        if (!Main.player[plrIndex].ZoneCorrupt)
                        {
                            flag = false;
                        }
                        break;
                    case "猩红之地":
                        if (!Main.player[plrIndex].ZoneCrimson)
                        {
                            flag = false;
                        }
                        break;
                    case "地牢":
                        if (!Main.player[plrIndex].ZoneDungeon)
                        {
                            flag = false;
                        }
                        break;
                    case "墓地":
                        if (!Main.player[plrIndex].ZoneGraveyard)
                        {
                            flag = false;
                        }
                        break;
                    case "蜂巢":
                        if (!Main.player[plrIndex].ZoneHive)
                        {
                            flag = false;
                        }
                        break;
                    case "神庙":
                        if (!Main.player[plrIndex].ZoneLihzhardTemple)
                        {
                            flag = false;
                        }
                        break;
                    case "沙尘暴":
                        if (!Main.player[plrIndex].ZoneSandstorm)
                        {
                            flag = false;
                        }
                        break;
                    case "天空":
                        if (!Main.player[plrIndex].ZoneSkyHeight)
                        {
                            flag = false;
                        }
                        break;
                    case "满月":
                        if (Main.moonPhase != 0)
                        {
                            flag = false;
                        }
                        break;
                    case "亏凸月":
                        if (Main.moonPhase != 1)
                        {
                            flag = false;
                        }
                        break;
                    case "下弦月":
                        if (Main.moonPhase != 2)
                        {
                            flag = false;
                        }
                        break;
                    case "残月":
                        if (Main.moonPhase != 3)
                        {
                            flag = false;
                        }
                        break;
                    case "新月":
                        if (Main.moonPhase != 4)
                        {
                            flag = false;
                        }
                        break;
                    case "娥眉月":
                        if (Main.moonPhase != 5)
                        {
                            flag = false;
                        }
                        break;
                    case "上弦月":
                        if (Main.moonPhase != 6)
                        {
                            flag = false;
                        }
                        break;
                    case "盈凸月":
                        if (Main.moonPhase != 7)
                        {
                            flag = false;
                        }
                        break;
                }
            }
            if (flag)
            {
                list.Add(item);
            }
        }
        if (list.Count > 40)
        {
            list.RemoveRange(40, list.Count - 40);
        }
        return list;
    }
} 
#endregion
