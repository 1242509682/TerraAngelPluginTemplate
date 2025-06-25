using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using TerraAngel;
using Terraria;

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

    [JsonProperty("鼠标范围伤害NPC", Order = 4)]
    public bool MouseStrikeNPC { get; set; } = true;
    [JsonProperty("鼠标伤害NPC格数", Order = 4)]
    public int MouseStrikeNPCRange { get; set; } = 10;

    [JsonProperty("自动使用物品", Order = 5)]
    public bool AutoUseItem { get; set; } = true;
    [JsonProperty("自动使用按键", Order = 5)]
    public Keys AutoUseKey = Keys.J; // 默认按键J
    [JsonProperty("使用间隔(毫秒)", Order = 5)]
    public int AutoUseInterval { get; set; } = 500; // 默认500毫秒

    [JsonProperty("修改前缀按键", Order = 6)]
    public Keys ShowEditPrefixKey = Keys.P;
    [JsonProperty("快速收藏按键", Order = 6)]
    public Keys FavoriteKey = Keys.O;

    [JsonProperty("社交栏饰品加成开关", Order = 7)]
    public bool SocialAccessoriesEnabled { get; set; } = false;
    [JsonProperty("恢复前缀加成", Order = 7)]
    public bool ApplyPrefix { get; set; } = true;
    [JsonProperty("恢复盔甲防御", Order = 7)]
    public bool ApplyArmor { get; set; } = true;
    [JsonProperty("恢复饰品功能", Order = 7)]
    public bool ApplyAccessory { get; set; } = true;
    [JsonProperty("社交栏饰品开关按键", Order = 7)]
    public Keys SocialAccessoriesKey = Keys.N;

    [JsonProperty("物品管理", Order = 10)]
    public bool ItemManager { get; set; } = true;
    [JsonProperty("应用修改按键", Order = 11)]
    public Keys ItemManagerKey = Keys.I;
    [JsonProperty("修改物品", Order = 12)]
    public List<ItemData> items = new List<ItemData>();

    #region 预设参数方法
    public void SetDefault()
    {
        Enabled = true;
        Heal = true;
        HealVal = 100;
        HealKey = Keys.H;
        MouseStrikeNPC = false;
        MouseStrikeNPCRange = 3;
        KillOrRESpawn = true;
        KillKey = Keys.K;
        AutoUseItem = false;
        AutoUseKey = Keys.J;
        AutoUseInterval = 500;
        ItemManager = true;
        ItemManagerKey = Keys.I;
        ShowEditPrefixKey = Keys.P;
        FavoriteKey = Keys.O;
        SocialAccessoriesEnabled = false;
        ApplyPrefix = true;
        ApplyArmor = true;
        ApplyAccessory = true;
        SocialAccessoriesKey = Keys.N;
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