using Newtonsoft.Json;
using TerraAngel;

namespace MyPlugin;

internal class Configuration
{
    [JsonProperty("插件开关", Order = 0)]
    public bool Enabled { get; set; } = true;

    [JsonProperty("每秒回血", Order = 1)]
    public bool AutoHeal { get; set; } = true;

    [JsonProperty("回血值", Order = 2)]
    public int AutoHealVal { get; set; } = 30;

    #region 预设参数方法
    public void SetDefault()
    {
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