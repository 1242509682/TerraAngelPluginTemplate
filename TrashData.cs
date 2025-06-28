using Newtonsoft.Json;

namespace MyPlugin;

public class TrashData
{
    [JsonProperty("玩家名字", Order = 0)]
    public string Name { get; set; }

    [JsonProperty("自动回收物品", Order = 1)]
    public Dictionary<int, int> TrashList { get; set; } = new Dictionary<int, int>();

    [JsonProperty("排除表", Order = 0)]
    public HashSet<int> ExcluItem { get; set; } = new HashSet<int>();

    public TrashData(string name = "",Dictionary<int, int> trashList = null!, HashSet<int> excluItem = null!)
    {
        this.Name = name ?? "";
        this.TrashList = trashList;
        this.ExcluItem = excluItem ?? new HashSet<int>();
    }
}