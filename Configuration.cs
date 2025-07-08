using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System.Numerics;
using TerraAngel;

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
    public bool FavoriteItemForJoinWorld { get; set; } = true;

    [JsonProperty("解锁所有配方", Order = 21)]
    public bool UnlockAllRecipes { get; set; } = false;
    [JsonProperty("忽略工作站要求", Order = 21)]
    public bool IgnoreStationRequirements { get; set; } = false;
    [JsonProperty("添加自定义配方", Order = 21)]
    public bool CustomRecipesEnabled { get; set; } = true;
    [JsonProperty("自定义配方列表", Order = 21)]
    public List<CustomRecipeData> CustomRecipes { get; set; } = new List<CustomRecipeData>();


    [JsonProperty("NPC自动对话", Order = 22)]
    public bool AutoTalkNPC { get; set; } = true;
    [JsonProperty("NPC自动对话按键", Order = 22)]
    public Keys AutoTalkKey = Keys.Y;
    [JsonProperty("NPC自动对话的最小格数", Order = 22)]
    public int AutoTalkRange { get; set; } = 2;
    [JsonProperty("NPC自动对话等待帧数", Order = 22)]
    public int AutoTalkNPCWaitTimes { get; set; } = 30;
    [JsonProperty("清除钓鱼任务", Order = 22)]
    public bool ClearAnglerQuests { get; set; } = true;
    [JsonProperty("清除钓鱼任务按键", Order = 22)]
    public Keys ClearQuestsKey = Keys.F;
    [JsonProperty("税务官自定义奖励", Order = 22)]
    public bool TaxCollectorCustomReward { get; set; }
    [JsonProperty("税务官奖励列表", Order = 22)]
    public List<RewardItem> TaxCollectorRewards { get; set; } = new List<RewardItem>();

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
        ClearAnglerQuests = true;
        ClearQuestsKey = Keys.F;
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
        VeinMinerList = new List<VeinMinerItem>() 
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

        FavoriteItemForJoinWorld = true;

        AutoTalkNPC = true;
        AutoTalkKey = Keys.Y;
        AutoTalkNPCWaitTimes = 30;
        AutoTalkRange = 2;
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
}