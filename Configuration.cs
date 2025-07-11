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
    public Dictionary<string, Vector2> CustomTeleportPoints = new Dictionary<string, Vector2>();

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
    public List<VeinMinerItem> VeinMinerList { get; set; } = new List<VeinMinerItem>();

    [JsonProperty("进入世界收藏背包物品", Order = 20)]
    public bool FavoriteItemForJoinWorld { get; set; } = false;

    [JsonProperty("添加自定义配方", Order = 21)]
    public bool CustomRecipesEnabled { get; set; } = true;
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

    [JsonProperty("修改传送枪弹幕距离", Order = 23)]
    public bool ModifyPortalDistance { get; set; } = true;
    [JsonProperty("传送枪弹幕销毁距离", Order = 23)]
    public float PortalMaxDistance { get; set; } = 4000f * 16;

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
        CustomTeleportPoints = new Dictionary<string, Vector2>();
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
        UnlockAllRecipes = false;
        IgnoreStationRequirements = false;
        CustomRecipes = CustomRecipeSetDefault();

        Shop = ShopItemSetDefault();

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
               ResultItem = ItemID.Wood, // 10个木材
               ResultStack = 10,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.Acorn, //橡实
                       Stack = 1,
                   }
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.DepthMeter, // 深度计
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.Compass, // 罗盘
                       Stack = 1,
                   }
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.Compass, // 罗盘
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.DepthMeter, // 深度计
                       Stack = 1,
                   }
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.GoldenKey,  // 5个金钥匙
               ResultStack = 5,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.ShadowKey, // 暗影钥匙
                       Stack = 1,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.BloodMoonStarter,  // 血泪
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.LifeCrystal, // 3个生命水晶
                       Stack = 5,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.LifeFruit,  // 生命果
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.LifeCrystal, // 生命水晶
                       Stack = 1,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.FrogLeg, // 蛙腿
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.Frog, //5个青蛙合成
                       Stack = 5,
                   }
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.CloudinaBottle,  // 云朵瓶
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.Cloud, // 10个云块
                       Stack = 10,
                   },

                   new IngredientData()
                   {
                       ItemId = ItemID.Bottle, // 1个空瓶
                       Stack = 1,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.MushroomGrassSeeds,  // 蘑菇草种子
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.JungleGrassSeeds, // 丛林草种子
                       Stack = 1,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.JungleGrassSeeds,  // 丛林草种子
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.MushroomGrassSeeds,  // 蘑菇草种子
                       Stack = 1,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.HerbBag,  // 草药袋
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.DaybloomSeeds, // 太阳花种子
                       Stack = 1,
                   },

                   new IngredientData()
                   {
                       ItemId = ItemID.MoonglowSeeds, // 月光草种子
                       Stack = 1,
                   },

                   new IngredientData()
                   {
                       ItemId = ItemID.BlinkrootSeeds, // 闪耀根种子
                       Stack = 1,
                   },

                   new IngredientData()
                   {
                       ItemId = ItemID.DeathweedSeeds, // 死亡草种子
                       Stack = 1,
                   },

                   new IngredientData()
                   {
                       ItemId = ItemID.WaterleafSeeds, // 幌菊种子
                       Stack = 1,
                   },

                   new IngredientData()
                   {
                       ItemId = ItemID.FireblossomSeeds, // 火焰花种子
                       Stack = 1,
                   },

                   new IngredientData()
                   {
                       ItemId = ItemID.ShiverthornSeeds, // 寒颤棘种子
                       Stack = 1,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.Daybloom,  // 太阳花
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.DaybloomSeeds, // 太阳花种子
                       Stack = 3,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.Moonglow,  // 月光草
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.MoonglowSeeds, // 月光草种子
                       Stack = 3,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.Deathweed,  // 死亡草
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.DeathweedSeeds, // 死亡草种子
                       Stack = 3,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.Waterleaf,  // 幌菊
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.WaterleafSeeds, // 幌菊种子
                       Stack = 3,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.Fireblossom,  // 火焰花
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.FireblossomSeeds, // 火焰花种子
                       Stack = 3,
                   },
               }
           },

           new CustomRecipeData()
           {
               ResultItem = ItemID.Shiverthorn,  // 寒颤棘
               ResultStack = 1,
               Ingredients = new List<IngredientData>()
               {
                   new IngredientData()
                   {
                       ItemId = ItemID.ShiverthornSeeds, // 寒颤棘种子
                       Stack = 3,
                   },
               }
           },
        };
    }
    #endregion

    #region 连锁挖矿默认参数
    public static List<VeinMinerItem> VeinMinerItemSetDefault()
    {
        return new List<VeinMinerItem>()
        {
            new VeinMinerItem(6, "铁矿"),
            new VeinMinerItem(7, "铜矿"),
            new VeinMinerItem(8, "金矿"),
            new VeinMinerItem(9, "银矿"),
            new VeinMinerItem(22, "魔矿"),
            new VeinMinerItem(37, "陨石"),
            new VeinMinerItem(48, "尖刺"),
            new VeinMinerItem(56, "黑曜石"),
            new VeinMinerItem(58, "狱石"),
            new VeinMinerItem(63, "蓝玉石块"),
            new VeinMinerItem(64, "红玉石块"),
            new VeinMinerItem(65, "翡翠石块"),
            new VeinMinerItem(66, "黄玉石块"),
            new VeinMinerItem(67, "紫晶石块"),
            new VeinMinerItem(68, "钻石石块"),
            new VeinMinerItem(107, "钴矿"),
            new VeinMinerItem(108, "秘银矿"),
            new VeinMinerItem(111, "精金矿"),
            new VeinMinerItem(166, "锡矿"),
            new VeinMinerItem(167, "铅矿"),
            new VeinMinerItem(168, "钨矿"),
            new VeinMinerItem(169, "铂金矿"),
            new VeinMinerItem(204, "猩红矿"),
            new VeinMinerItem(211, "叶绿矿"),
            new VeinMinerItem(221, "钯金矿"),
            new VeinMinerItem(222, "山铜矿"),
            new VeinMinerItem(223, "钛金矿"),
            new VeinMinerItem(229, "蜂蜜块"),
            new VeinMinerItem(230, "松脆蜂蜜块"),
            new VeinMinerItem(232, "木尖刺"),
            new VeinMinerItem(404, "沙漠化石"),
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
                        price = 100,
                    },

                    new CShopItemInfo()
                    {
                        id = ItemID.BottledWater,
                        prefix = 0,
                        stack = 20,
                        price = 400,
                    },

                    new CShopItemInfo()
                    {
                        id = ItemID.FallenStar,
                        prefix = 0,
                        stack = 1,
                        price = 400,
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