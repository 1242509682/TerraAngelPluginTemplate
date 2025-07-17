using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System.Numerics;
using TerraAngel;
using Terraria;
using Terraria.ID;

namespace MyPlugin;

internal class Configuration
{
    [JsonProperty("插件开关", Order = 1)]
    public bool Enabled { get; set; } = true;

    [JsonProperty("开启快捷键回血", Order = 2)]
    public bool Heal { get; set; } = true;
    [JsonProperty("回血值", Order = 2)]
    public int HealVal { get; set; } = 100;
    [JsonProperty("回血按键", Order = 2)]
    public Keys HealKey = Keys.H;

    [JsonProperty("快速自杀复活", Order = 3)]
    public bool KillOrRESpawn { get; set; } = true;
    [JsonProperty("自杀复活按键", Order = 3)]
    public Keys KillKey = Keys.K;

    [JsonProperty("自动使用物品", Order = 4)]
    public bool AutoUseItem { get; set; } = true;
    [JsonProperty("自动使用按键", Order = 4)]
    public Keys AutoUseKey = Keys.J; // 默认按键J
    [JsonProperty("使用间隔(帧)", Order = 4)]
    public int UseItemInterval { get; set; } = 10;

    [JsonProperty("鼠标范围伤害NPC", Order = 5)]
    public bool MouseStrikeNPC { get; set; } = true;
    [JsonProperty("鼠标范围伤害值", Order = 5)]
    public int MouseStrikeNPCVel { get; set; } = 0;
    [JsonProperty("鼠标伤害NPC格数", Order = 5)]
    public int MouseStrikeNPCRange { get; set; } = 10;
    [JsonProperty("鼠标伤害间隔(帧)", Order = 5)]
    public int MouseStrikeInterval { get; set; } = 30; // 默认30帧

    [JsonProperty("修改前缀按键", Order = 6)]
    public Keys ShowEditPrefixKey = Keys.P;
    [JsonProperty("快速收藏按键", Order = 6)]
    public Keys FavoriteKey = Keys.O;

    [JsonProperty("社交栏饰品加成开关", Order = 7)]
    public bool SocialAccessory { get; set; } = false;
    [JsonProperty("恢复前缀加成", Order = 7)]
    public bool ApplyPrefix { get; set; } = true;
    [JsonProperty("恢复盔甲防御", Order = 7)]
    public bool ApplyArmor { get; set; } = true;
    [JsonProperty("恢复饰品功能", Order = 7)]
    public bool ApplyAccessory { get; set; } = true;
    [JsonProperty("社交栏饰品开关按键", Order = 7)]
    public Keys SocialAccessoriesKey = Keys.N;

    [JsonProperty("物品修改", Order = 10)]
    public bool ItemModify { get; set; } = true;
    [JsonProperty("应用修改按键", Order = 11)]
    public Keys ItemModifyKey = Keys.I;
    [JsonProperty("修改物品", Order = 12)]
    public List<ItemData> ItemModifyList = new List<ItemData>();

    [JsonProperty("忽略重力药水", Order = 13)]
    public bool IgnoreGravity { get; set; } = true;
    [JsonProperty("忽略重力药水按键", Order = 13)]
    public Keys IgnoreGravityKey = Keys.T;

    [JsonProperty("自动垃圾桶", Order = 14)]
    public bool AutoTrash { get; set; } = false;
    [JsonProperty("自动垃圾桶按键", Order = 14)]
    public Keys AutoTrashKey = Keys.C;
    [JsonProperty("自动垃圾桶同步间隔", Order = 14)]
    public int TrashSyncInterval { get; set; } = 10; // 默认10帧
    [JsonProperty("自动垃圾桶表", Order = 14)]
    public List<TrashData> TrashItems { get; set; } = new List<TrashData>();

    [JsonProperty("自定义传送点", Order = 15)]
    public Dictionary<int,Dictionary<string, Vector2>> CustomTeleportPoints = new Dictionary<int, Dictionary<string, Vector2>>();

    [JsonProperty("NPC自动回血", Order = 17)]
    public bool NPCAutoHeal { get; set; } = false;
    [JsonProperty("NPC自动回血按键", Order = 17)]
    public Keys NPCAutoHealKey = Keys.None;
    [JsonProperty("普通NPC回血百分比", Order = 17)]
    public float NPCHealVel { get; set; } = 1;         // 普通NPC回血百分比
    [JsonProperty("普通NPC回血间隔(秒)", Order = 17)]
    public int NPCHealInterval { get; set; } = 1;       // 普通NPC回血间隔
    [JsonProperty("允许Boss回血", Order = 17)]
    public bool Boss { get; set; } = false;
    [JsonProperty("BOSS回血百分比", Order = 17)]
    public float BossHealVel { get; set; } = 0.1f;      // BOSS回血百分比
    [JsonProperty("BOSS每次回血上限", Order = 17)]
    public int BossHealCap { get; set; } = 1000;        // BOSS每次回血上限
    [JsonProperty("BOSS独立回血间隔(秒)", Order = 17)]
    public int BossHealInterval { get; set; } = 3;      // BOSS独立回血间隔(秒)

    [JsonProperty("NPC复活按键", Order = 18)]
    public Keys NPCReliveKey = Keys.Home;

    [JsonProperty("连锁挖矿开关", Order = 19)]
    public bool VeinMinerEnabled { get; set; } = false;
    [JsonProperty("连锁挖矿按键", Order = 19)]
    public Keys VeinMinerKey = Keys.V;
    [JsonProperty("连锁挖矿上限", Order = 19)]
    public int VeinMinerCount { get; set; } = 500;
    [JsonProperty("连锁图格表", Order = 19)]
    public List<MinerItem> VeinMinerList { get; set; } = new List<MinerItem>();

    [JsonProperty("进入世界收藏背包物品", Order = 20)]
    public bool FavoriteItemForJoinWorld { get; set; } = false;

    [JsonProperty("启用自定义配方", Order = 21)]
    public bool CustomRecipesEnabled { get; set; } = true;
    [JsonProperty("添加更多配方槽位", Order = 21)]
    public bool ModifiedMaxRecipesEnabled { get; set; } = true;
    [JsonProperty("预设配方槽位数量", Order = 21)]
    public int MaxRecipes { get; set; } = 6000;
    [JsonProperty("启用隐藏原版配方", Order = 21)]
    public bool HideOriginalRecipe { get; set; } = true;
    [JsonProperty("解锁所有配方", Order = 21)]
    public bool UnlockAllRecipes { get; set; } = false;
    [JsonProperty("忽略工作站要求", Order = 21)]
    public bool IgnoreStationRequirements { get; set; } = false;
    [JsonProperty("自定义配方列表", Order = 21)]
    public List<CustomRecipeData> CustomRecipes { get; set; } = new List<CustomRecipeData>();

    #region NPC自动对话
    [JsonProperty("NPC自动对话", Order = 22)]
    public bool AutoTalkNPC { get; set; } = true;
    [JsonProperty("NPC自动对话按键", Order = 22)]
    public Keys AutoTalkKey = Keys.Y;
    [JsonProperty("NPC自动对话的最小格数", Order = 22)]
    public int AutoTalkRange { get; set; } = 2;
    [JsonProperty("NPC自动对话等待帧数", Order = 22)]
    public int AutoTalkNPCWaitTimes { get; set; } = 30;
    [JsonProperty("自动对话NPC无敌", Order = 22)]
    public bool TalkingNpcImmortal { get; set; } = true;
    [JsonProperty("清除钓鱼任务", Order = 22)]
    public bool ClearAnglerQuests { get; set; } = true;
    [JsonProperty("清除钓鱼任务按键", Order = 22)]
    public Keys ClearQuestsKey = Keys.F;
    [JsonProperty("消耗任务鱼", Order = 22)]
    public bool ClearFish { get; set; } = false;
    [JsonProperty("护士禁言", Order = 22)]
    public bool NurseMute { get; set; } = false;
    [JsonProperty("派对女孩切换音乐", Order = 22)]
    public bool SwapMusicing { get; set; } = true;
    [JsonProperty("派对女孩打开商店", Order = 22)]
    public bool OpenShopForPartyGirl { get; set; } = true;
    [JsonProperty("向导是否提示指导语", Order = 22)]
    public bool HelpTextForGuide { get; set; } = true;
    [JsonProperty("向导打开制作栏", Order = 22)]
    public bool InGuideCraftMenu { get; set; } = true;
    [JsonProperty("酒馆老板打开商店", Order = 22)]
    public bool OpenShopForDD2Bartender { get; set; } = true;
    [JsonProperty("酒馆老板是否提示指导语", Order = 22)]
    public bool HelpTextForDD2Bartender { get; set; } = true;
    [JsonProperty("油漆工打开喷漆商店", Order = 22)]
    public bool OpenShopForPainter { get; set; } = true;
    [JsonProperty("油漆工打开壁纸商店", Order = 22)]
    public bool OpenShopForWall { get; set; } = false;
    [JsonProperty("树妖打开商店", Order = 22)]
    public bool OpenShopForDryad { get; set; } = true;
    [JsonProperty("树妖检查环境", Order = 22)]
    public bool CheckBiomes { get; set; } = true;
    [JsonProperty("哥布林打开商店", Order = 22)]
    public bool OpenShopForGoblin { get; set; } = false;
    [JsonProperty("哥布林打开重铸界面", Order = 22)]
    public bool InReforgeMenu { get; set; } = true;
    [JsonProperty("发型师卖头发", Order = 22)]
    public bool OpenHairWindow { get; set; } = true;
    [JsonProperty("发型师打开商店", Order = 22)]
    public bool OpenShopForStylist { get; set; } = false;
    [JsonProperty("税务官自定义奖励", Order = 22)]
    public bool TaxCollectorCustomReward { get; set; } = false;
    [JsonProperty("税务官奖励列表", Order = 22)]
    public List<RewardItem> TaxCollectorRewards { get; set; } = new List<RewardItem>();
    [JsonProperty("自定义NPC商店表", Order = 23)]
    public List<ShopItem> Shop = new List<ShopItem>();
    #endregion

    [JsonProperty("NPC瞬移回家", Order = 23)]
    public bool NPCMoveRoomForTeleport { get; set; } = true;

    [JsonProperty("修改传送枪弹幕距离", Order = 24)]
    public bool ModifyPortalDistance { get; set; } = true;
    [JsonProperty("传送枪弹幕销毁距离", Order = 24)]
    public float PortalMaxDistance { get; set; } = 4000f * 16;
    public bool ProjUpdate { get; internal set; }

    #region 预设参数方法
    public void SetDefault()
    {
        Enabled = true;
        Heal = true;
        HealVal = 100;
        HealKey = Keys.H;
        KillOrRESpawn = true;
        KillKey = Keys.K;
        AutoUseItem = false;
        AutoUseKey = Keys.J;
        MouseStrikeNPC = false;
        MouseStrikeNPCVel = 0;
        MouseStrikeNPCRange = 3;
        MouseStrikeInterval = 30;
        UseItemInterval = 10;
        ItemModify = true;
        ItemModifyKey = Keys.I;
        ShowEditPrefixKey = Keys.P;
        FavoriteKey = Keys.O;
        SocialAccessory = false;
        ApplyPrefix = true;
        ApplyArmor = true;
        ApplyAccessory = true;
        SocialAccessoriesKey = Keys.N;
        IgnoreGravity = true;
        IgnoreGravityKey = Keys.T;
        AutoTrash = false;
        AutoTrashKey = Keys.C;
        TrashSyncInterval = 10;
        TrashItems = new List<TrashData>();
        CustomTeleportPoints = new Dictionary<int, Dictionary<string, Vector2>>();
        NPCAutoHeal = false;
        NPCAutoHealKey = Keys.None;
        NPCHealVel = 1;
        NPCHealInterval = 1;
        Boss = false;
        BossHealVel = 0.1f;
        BossHealCap = 1000;
        BossHealInterval = 3;
        NPCReliveKey = Keys.Home;
        VeinMinerEnabled = false;
        VeinMinerKey = Keys.V;
        VeinMinerCount = 500;
        VeinMinerList = VeinMinerItemSetDefault();

        FavoriteItemForJoinWorld = false;

        AutoTalkNPC = true;
        AutoTalkKey = Keys.Y;
        AutoTalkNPCWaitTimes = 30;
        AutoTalkRange = 2;
        TalkingNpcImmortal = true;
        ClearAnglerQuests = true;
        ClearQuestsKey = Keys.F;
        ClearFish = false;
        NurseMute = false;
        SwapMusicing = true;
        OpenShopForPartyGirl = true;
        HelpTextForGuide = true;
        InGuideCraftMenu = true;
        OpenShopForDD2Bartender = true;
        HelpTextForDD2Bartender = true;
        OpenShopForPainter = true;
        OpenShopForWall = false;
        OpenShopForDryad = true;
        CheckBiomes = true;
        OpenShopForGoblin = false;
        InReforgeMenu = true;
        OpenHairWindow = true;
        OpenShopForStylist = false;
        TaxCollectorCustomReward = false;
        TaxCollectorRewards = TaxCollectorRewardsSetDefault();

        CustomRecipesEnabled = true;
        ModifiedMaxRecipesEnabled = true;
        MaxRecipes = 6000;
        HideOriginalRecipe = true;
        UnlockAllRecipes = false;
        IgnoreStationRequirements = false;
        CustomRecipes = CustomRecipeSetDefault();

        Shop = ShopItemSetDefault();

        NPCMoveRoomForTeleport = true;

        ModifyPortalDistance = true;
        PortalMaxDistance = 4000 * 16f;
    }
    #endregion

    #region 读取与创建配置文件方法
    public static readonly string FilePath = Path.Combine(ClientLoader.SavePath, "MyPlugin.json");
    public void Write()
    {
        string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }
    public static Configuration Read()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                var NewConfig = new Configuration();
                NewConfig.SetDefault();
                NewConfig.Write();
                return NewConfig;
            }
            else
            {
                string jsonContent = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
            }
        }
        catch (Exception ex)
        {
            // 定义日志目录路径
            string logDir = Path.Combine(Path.GetDirectoryName(FilePath)!, "logs");

            // 创建 logs 文件夹（如果不存在）
            Directory.CreateDirectory(logDir);

            // 使用当前日期作为日志文件名
            string logFileName = $"{DateTime.Now:yyyy-MM-dd}.log";
            string logPath = Path.Combine(logDir, logFileName);

            // 写入日志内容
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 发生异常：{ex.Message}\n堆栈信息：{ex.StackTrace}\n";

            File.AppendAllText(logPath, logMessage);

            // 返回默认配置以避免崩溃
            var backConfig = new Configuration();
            backConfig.SetDefault();
            return backConfig;
        }
    }
    #endregion

    #region 自定义配方默认参数
    public static List<CustomRecipeData> CustomRecipeSetDefault()
    {
        return new List<CustomRecipeData>()
        {
           new CustomRecipeData()
           {
               ItemID = ItemID.Wood, // 10个木材
               Stack = 10,
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Acorn)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.DepthMeter, // 深度计
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Compass)
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Compass, // 罗盘
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DepthMeter)
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.LifeformAnalyzer, // 生命体分析仪
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Radar) // 雷达
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Radar, // 雷达
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.TallyCounter) // 杀怪计数器
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.TallyCounter, // 杀怪计数器
               Stack = 1,
               unlock = new List<string>() { "骷髅王" },
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.LifeformAnalyzer) // 生命体分析仪
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.DPSMeter, // 每秒伤害计数器
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.MetalDetector) // 金属探测器
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Stopwatch, // 秒表
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DPSMeter) // 每秒伤害计数器
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.MetalDetector, // 金属探测器
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Stopwatch) // 秒表
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.MagicConch, // 魔法海螺
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DemonConch) // 恶魔海螺
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.DemonConch, // 恶魔海螺
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.MagicConch) // 魔法海螺
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.SunStone, // 太阳石
               Stack = 1,
               Tile = new List<int>() { TileID.CrystalBall }, // 水晶球
               unlock = new List<string>() { "石巨人" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.MoonStone) // 月亮石
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.MoonStone,  // 月亮石
               Stack = 1,
               Tile = new List<int>() { TileID.CrystalBall }, // 水晶球
               unlock = new List<string>() { "满月" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.SunStone) // 太阳石
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ArtisanLoaf,  // 工匠面包
               Stack = 1,
               Tile = new List<int>() { TileID.Anvils,TileID.MythrilAnvil }, // 铁砧 与 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.TorchGodsFavor) // 火把神的恩赐
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.TorchGodsFavor,  // 火把神的恩赐
               Stack = 1,
               Tile = new List<int>() { TileID.Anvils,TileID.MythrilAnvil }, // 铁砧 与 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ArtisanLoaf) // 工匠面包
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.MoneyTrough,  // 钱币槽
               Stack = 1,
               unlock = new List<string>() { "血月","鹿角怪" },
               Tile = new List<int>() { TileID.Anvils, TileID.MythrilAnvil }, // 铁砧 与 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ChesterPetItem) // 眼骨
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ChesterPetItem,  // 眼骨
               Stack = 1,
               unlock = new List<string>() { "鹿角怪" },
               Tile = new List<int>() { TileID.Anvils, TileID.MythrilAnvil }, // 铁砧 与 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.MoneyTrough) // 钱币槽
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ShadowFlameBow,  // 暗影焰弓
               Stack = 1,
               unlock = new List<string>() { "困难模式","哥布林入侵" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ShadowFlameHexDoll) // 暗影焰妖娃
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ShadowFlameHexDoll,  // 暗影焰妖娃
               Stack = 1,
               unlock = new List<string>() { "困难模式","哥布林入侵" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ShadowFlameKnife) // 暗影焰刀
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ShadowFlameKnife,  // 暗影焰刀
               Stack = 1,
               unlock = new List<string>() { "困难模式","哥布林入侵" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ShadowFlameBow)  // 暗影焰弓
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.CrossNecklace,  // 十字项链
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.PhilosophersStone)  // 点金石
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.PhilosophersStone,  // 点金石
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.CrossNecklace)  // 十字项链
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.MagicDagger,  // 魔法飞刀
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.CrystalSerpent)  // 水晶蛇
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.CrystalSerpent,  // 点金石
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.MagicDagger)  // 水晶蛇
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Uzi,  // 鳄鱼机关枪
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Gatligator)  // 乌兹冲锋枪
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Gatligator,  // 乌兹冲锋枪
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Uzi)  // 鳄鱼机关枪
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.DaedalusStormbow,  // 代达罗斯风暴弓
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.CrystalVileShard)  // 魔晶碎块
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.CrystalVileShard,  // 魔晶碎块
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.FlyingKnife)  // 飞刀
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.FlyingKnife,  // 魔晶碎块
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil }, // 秘银砧
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DaedalusStormbow)  // 代达罗斯风暴弓
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.DD2PhoenixBow,  // 幽灵凤凰
               Stack = 1,
               unlock = new List<string>() { "一王后" },
               Tile = new List<int>() { TileID.MythrilAnvil },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DD2SquireDemonSword)  // 地狱烙印
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.DD2SquireDemonSword,  // 地狱烙印
               Stack = 1,
               unlock = new List<string>() { "一王后" },
               Tile = new List<int>() { TileID.MythrilAnvil },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DD2PhoenixBow)  // 幽灵凤凰
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.DarkShard,  // 暗黑碎块
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.LightShard)  // 光明碎块
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.LightShard,  // 光明碎块
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DarkShard)  // 暗黑碎块
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.AncientBattleArmorMaterial,  // 禁戒碎片
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.FrostCore)  // 寒霜核
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.FrostCore,  // 寒霜核
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.MythrilAnvil },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.AncientBattleArmorMaterial)  // 禁戒碎片
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Ichor,  // 灵液
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.CursedFlame)  // 诅咒焰
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.CursedFlame,  // 诅咒焰
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Ichor)  // 灵液
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.CursedFlame,  // 诅咒焰
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Ichor)  // 灵液
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Vertebrae,  // 椎骨
               Stack = 1,
               Tile = new List<int>() { TileID.DemonAltar }, // 恶魔祭坛
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.RottenChunk)  // 腐肉
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.RottenChunk,  // 腐肉
               Stack = 1,
               Tile = new List<int>() { TileID.DemonAltar }, // 恶魔祭坛
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Vertebrae)  // 椎骨
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ViciousMushroom,  // 毒蘑菇
               Stack = 1,
               Tile = new List<int>() { TileID.DemonAltar }, // 恶魔祭坛
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.VileMushroom)  // 魔菇
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.VileMushroom,  // 魔菇
               Stack = 1,
               Tile = new List<int>() { TileID.DemonAltar }, // 恶魔祭坛
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ViciousMushroom)  // 毒蘑菇
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.CloudinaBottle,  // 云朵瓶
               Stack = 1,
               Tile = new List<int>() { TileID.WorkBenches }, // 工作台
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.TsunamiInABottle)  // 海啸瓶
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.BlizzardinaBottle,  // 暴雪瓶
               Stack = 1,
               Tile = new List<int>() { TileID.WorkBenches }, // 工作台
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.CloudinaBottle)  // 云朵瓶
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.SandstorminaBottle,  // 沙暴瓶
               Stack = 1,
               Tile = new List<int>() { TileID.WorkBenches }, // 工作台
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.BlizzardinaBottle)  // 暴雪瓶
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.TsunamiInABottle,  // 海啸瓶
               Stack = 1,
               Tile = new List<int>() { TileID.WorkBenches }, // 工作台
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.SandstorminaBottle)  // 沙暴瓶
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.UnicornHorn,  // 独角兽角
               Stack = 1,
               unlock = new List<string>() { "困难模式" },
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.PixieDust)  // 妖精尘
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.SharkFin,  // 鲨鱼鳍
               Stack = 1,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Feather)  // 羽毛
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Feather,  // 羽毛
               Stack = 3,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.SharkFin)  // 鲨鱼鳍
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Tombstone,  // 墓石
               Stack = 1,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.StoneBlock,30)  // 石块
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.NaturesGift,  // 大自然恩赐
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.JungleRose)  // 丛林玫瑰
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.JungleSpores,  // 丛林孢子
               Stack = 2,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Stinger)  // 毒刺
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Vine,  // 藤蔓
               Stack = 2,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.JungleSpores)  // 丛林孢子
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Aglet,  // 金属带扣
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.AnkletoftheWind)  // 疾风脚镯
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.AnkletoftheWind,  // 疾风脚镯
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Aglet)  // 金属带扣
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.PaladinsShield,  // 圣骑士护盾
               Stack = 1,
               Tile = new List<int>() { TileID.MythrilAnvil },
               unlock = new List<string>(){ "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.PaladinsHammer)  // 圣骑士锤
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.FlinxFur,  // 小雪怪皮毛
               Stack = 2,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.TatteredCloth)  // 破布
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.TatteredCloth,  // 破布
               Stack = 2,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.FlinxFur)  // 小雪怪皮毛
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.SailfishBoots,  // 航鱼靴
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.HermesBoots)  // 赫尔墨斯靴
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.BlackBelt,  // 黑腰带
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               unlock = new List<string>() { "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Tabi)  // 分趾厚底袜
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Tabi,  // 分趾厚底袜
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               unlock = new List<string>() { "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.BlackBelt)  // 黑腰带
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ShoeSpikes,  // 鞋钉
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ClimbingClaws)  // 攀爬爪
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ClimbingClaws,  // 攀爬爪
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ShoeSpikes)  // 鞋钉
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.BrickLayer,  // 砌砖刀
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ExtendoGrip)  // 加长握爪
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ExtendoGrip,  // 加长握爪
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.PaintSprayer)  // 自动喷漆器
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.PaintSprayer,  // 自动喷漆器
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.PortableCementMixer)  // 便携式水泥搅拌机
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.PortableCementMixer,  // 便携式水泥搅拌机
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.BrickLayer)  // 砌砖刀
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.DryBomb,  // 干炸弹
               Stack = 2,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ScarabBomb)  // 甲虫炸弹
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ScarabBomb,  // 甲虫炸弹
               Stack = 2,
               Tile = new List<int>() { TileID.WorkBenches },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DryBomb)  // 干炸弹
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.GoldenKey,  // 5个金钥匙
               Stack = 5,
               Tile = new List<int>() { TileID.Hellforge },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ShadowKey)
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.BloodMoonStarter,  // 血泪
               Stack = 1,
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.LifeCrystal,5)
               },

               unlock = new List<string>() { "血月" }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.LifeFruit,  // 生命果
               Stack = 1,
               Tile = new List<int>() { TileID.LihzahrdFurnace },
               unlock = new List<string>() { "一王后" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.LifeCrystal)
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.FrogLeg, // 蛙腿
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Frog,5)
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.CloudinaBottle,  // 云朵瓶
               Stack = 1,
               Tile = new List<int>() { TileID.TinkerersWorkbench },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.Cloud,10),
                   new MaterialData(ItemID.Bottle)
               },
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.MushroomGrassSeeds,  // 蘑菇草种子
               Stack = 1,
               unlock = new List<string>() { "炼药" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.JungleGrassSeeds)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.JungleGrassSeeds,  // 丛林草种子
               Stack = 1,
               unlock = new List<string>() { "炼药" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.MushroomGrassSeeds)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.HerbBag,  // 草药袋
               Stack = 1,
               unlock = new List<string>() { "炼药" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DaybloomSeeds),
                   new MaterialData(ItemID.MoonglowSeeds),
                   new MaterialData(ItemID.BlinkrootSeeds),
                   new MaterialData(ItemID.DeathweedSeeds),
                   new MaterialData(ItemID.WaterleafSeeds),
                   new MaterialData(ItemID.FireblossomSeeds),
                   new MaterialData(ItemID.ShiverthornSeeds)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Daybloom,  // 太阳花
               Stack = 1,
               unlock = new List<string>() { "炼药" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DaybloomSeeds,3)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Moonglow,  // 月光草
               Stack = 1,
               unlock = new List<string>() { "炼药" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.MoonglowSeeds,3)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Deathweed,  // 死亡草
               Stack = 1,
               unlock = new List<string>() { "炼药" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DeathweedSeeds,3)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Waterleaf,  // 幌菊
               Stack = 1,
               unlock = new List<string>() { "炼药" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.WaterleafSeeds,3)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Fireblossom,  // 火焰花
               Stack = 1,
               unlock = new List<string>() { "炼药" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.FireblossomSeeds,3)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.Shiverthorn,  // 寒颤棘
               Stack = 1,
               Tile = new List < int >() { TileID.Bottles, TileID.AlchemyTable },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.ShiverthornSeeds,3)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.PiranhaGun, // 食人鱼枪
               Stack = 1,
               Tile = new List < int >() { TileID.MythrilAnvil },
               unlock = new List<string>() { "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.JungleKey)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.ScourgeoftheCorruptor, // 腐化者之戟（腐化灾兵）
               Stack = 1,
               Tile = new List < int >() { TileID.MythrilAnvil },
               unlock = new List<string>() { "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.CorruptionKey)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.VampireKnives, // 吸血鬼刀
               Stack = 1,
               Tile = new List < int >() { TileID.MythrilAnvil },
               unlock = new List<string>() { "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.CrimsonKey)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.RainbowGun, // 彩虹枪
               Stack = 1,
               Tile = new List < int >() { TileID.MythrilAnvil },
               unlock = new List<string>() { "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.HallowedKey)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.StaffoftheFrostHydra, // 寒霜九头蛇法杖
               Stack = 1,
               Tile = new List < int >() { TileID.MythrilAnvil },
               unlock = new List<string>() { "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.FrozenKey)
               }
           },

           new CustomRecipeData()
           {
               ItemID = ItemID.StormTigerStaff, // 沙漠虎杖
               Stack = 1,
               Tile = new List < int >() { TileID.MythrilAnvil },
               unlock = new List<string>() { "世纪之花" },
               Material = new List<MaterialData>()
               {
                   new MaterialData(ItemID.DungeonDesertKey)
               }
           },

        };
    }
    #endregion

    #region 连锁挖矿默认参数
    public static List<MinerItem> VeinMinerItemSetDefault()
    {
        return new List<MinerItem>()
        {
            new MinerItem(6, "铁矿"),
            new MinerItem(7, "铜矿"),
            new MinerItem(8, "金矿"),
            new MinerItem(9, "银矿"),
            new MinerItem(22, "魔矿"),
            new MinerItem(37, "陨石"),
            new MinerItem(48, "尖刺"),
            new MinerItem(56, "黑曜石"),
            new MinerItem(58, "狱石"),
            new MinerItem(63, "蓝玉石块"),
            new MinerItem(64, "红玉石块"),
            new MinerItem(65, "翡翠石块"),
            new MinerItem(66, "黄玉石块"),
            new MinerItem(67, "紫晶石块"),
            new MinerItem(68, "钻石石块"),
            new MinerItem(107, "钴矿"),
            new MinerItem(108, "秘银矿"),
            new MinerItem(111, "精金矿"),
            new MinerItem(166, "锡矿"),
            new MinerItem(167, "铅矿"),
            new MinerItem(168, "钨矿"),
            new MinerItem(169, "铂金矿"),
            new MinerItem(204, "猩红矿"),
            new MinerItem(211, "叶绿矿"),
            new MinerItem(221, "钯金矿"),
            new MinerItem(222, "山铜矿"),
            new MinerItem(223, "钛金矿"),
            new MinerItem(229, "蜂蜜块"),
            new MinerItem(230, "松脆蜂蜜块"),
            new MinerItem(232, "木尖刺"),
            new MinerItem(404, "沙漠化石"),
        };
    }
    #endregion

    #region 税收官随机奖励默认参数
    public static List<RewardItem> TaxCollectorRewardsSetDefault()
    {
        return new List<RewardItem>()
        {
            new RewardItem()
            {
                Enabled = true,
                ItemID = ItemID.PlatinumCoin,
                Stack = 1,
                Chance = 100
            }
        };

    }
    #endregion

    #region 自定义商店默认参数
    public static List<ShopItem> ShopItemSetDefault()
    {
        return new List<ShopItem>()
        {
            new ShopItem()
            {
                Enabled = true,
                Name = "",
                NpcType = NPCID.Merchant,
                item = new List<CShopItemInfo>()
                {
                    new CShopItemInfo()
                    {
                        id = ItemID.Wood,
                        prefix = 0,
                        stack = 20,
                        price = 20,
                    },

                    new CShopItemInfo()
                    {
                        id = ItemID.BottledWater,
                        prefix = 0,
                        stack = 20,
                        price = 30,
                    },

                    new CShopItemInfo()
                    {
                        id = ItemID.FallenStar,
                        prefix = 0,
                        stack = 1,
                        price = 2500,
                        unlock = new List<string>()
                        {
                            "克眼"
                        }
                    }
                }
            }
        };

    }
    #endregion

    #region 查找商店物品来自NPC
    public int FindShopItem(int npcID)
    {
        for (int i = 0; i < Shop.Count; i++)
        {
            if (Shop[i].NpcType == npcID)
            {
                return i;
            }
        }
        return -1;
    } 
    #endregion
}