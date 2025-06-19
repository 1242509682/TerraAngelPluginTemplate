using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using TerraAngel;

namespace MyPlugin;

internal class Configuration
{
    [JsonProperty("插件开关", Order = 1)]
    public bool Enabled { get; set; } = true;

    [JsonProperty("开启快捷键回血", Order = 2)]
    public bool AutoHeal { get; set; } = true;
    [JsonProperty("回血值", Order = 2)]
    public int AutoHealVal { get; set; } = 100;
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


    #region 预设参数方法
    public void SetDefault()
    {
        Enabled = true;
        AutoHeal = true;
        AutoHealVal = 100;
        MouseStrikeNPC = true;
        MouseStrikeNPCRange = 10;
        KillOrRESpawn = true;
        HealKey = Keys.H;
        KillKey = Keys.K;
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
    #endregion
}