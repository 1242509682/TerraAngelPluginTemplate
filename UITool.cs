using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Numerics;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Tools;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

public class UITool : Tool
{
    public override string Name => "羽学插件模板";
    public override ToolTabs Tab => ToolTabs.MainTools;

    // 用于临时存储按键编辑状态
    private bool EditHealKey = false; // 快速回血按键编辑状态
    private bool EditKillKey = false; // 快速死亡与复活按键编辑状态
    private bool EditAutoUseKey = false; // 自动使用物品按键编辑状态
    private bool EditShowEditPrefixKey = false; // 一键修改前缀按键编辑状态
    private bool EditFavoriteKey = false; // 快速收藏按键编辑状态
    private bool EditItemManagerKey = false; // 物品管理器按键编辑状态
    private bool EditSocialAccessoriesKey = false; // 社交栏饰品开关按键编辑状态
    private bool EditIgnoreGravityKey = false; // 忽略重力按键编辑状态
    private bool EditAutoTrashKey = false; // 自动垃圾桶按键编辑状态

    #region UI与配置文件交互方法
    public override void DrawUI(ImGuiIOPtr io)
    {
        var plr = Main.player[Main.myPlayer];

        bool enabled = Config.Enabled; //插件总开关
        bool Heal = Config.Heal; //回血开关
        int HealVal = Config.HealVal; //回血值

        bool mouseStrikeNPC = Config.MouseStrikeNPC; //鼠标范围伤害NPC开关
        int mouseStrikeNPCRange = Config.MouseStrikeNPCRange; //伤害范围

        bool killOrRESpawn = Config.KillOrRESpawn; //快速死亡与复活开关

        bool autoUseItem = Config.AutoUseItem; //自动使用物品开关
        int autoUseInterval = Config.UseItemInterval; //自动使用物品间隔

        bool itemModify = Config.ItemModify; // 修改手上物品开关

        bool socialEnabled = Config.SocialAccessory; // 社交栏饰品生效开关
        bool applyPrefix = Config.ApplyPrefix; // 开启额外饰品的前缀加成
        bool applyArmor = Config.ApplyArmor; // 开启装饰栏盔甲的护甲加成
        bool applyAccessory = Config.ApplyAccessory; // 开启额外饰品的原有被动功能

        bool applyIgnoreGravity = Config.IgnoreGravity; // 启用忽略重力药水效果

        // 绘制插件设置界面
        ImGui.Checkbox("启用羽学插件", ref enabled);
        if (enabled)
        {
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);

            // 快速死亡复活开关（单bool + 自定义按键）
            ImGui.Separator();
            ImGui.Checkbox("快速死亡/复活", ref killOrRESpawn);
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("按键", ref Config.KillKey, ref EditKillKey);

            // 回血设置 （bool + 滑块 + 自定义按键）
            ImGui.Separator();
            ImGui.Checkbox("强制回血", ref Heal);
            ImGui.SameLine(); // 回血按键设置
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("按键", ref Config.HealKey, ref EditHealKey);
            if (Heal)
            {
                ImGui.Indent();
                ImGui.Text("回血量:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150);
                ImGui.SliderInt("##HealAmount", ref HealVal, 1, 500, "%d HP");
                ImGui.SameLine();
                ImGui.Text($"{HealVal} HP");
                ImGui.Unindent();
            }

            // 自动使用物品
            ImGui.Separator();
            ImGui.Checkbox("自动使用物品", ref autoUseItem);
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("按键", ref Config.AutoUseKey, ref EditAutoUseKey);
            if (autoUseItem)
            {
                ImGui.Indent();
                ImGui.Text("使用间隔(毫秒):");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200);
                ImGui.SliderInt("##AutoUseInterval", ref autoUseInterval, 1, 20000, "%d ms");
                ImGui.Unindent();

                ImGui.Checkbox("启用鼠标范围伤害NPC", ref mouseStrikeNPC);
                if (mouseStrikeNPC)
                {
                    ImGui.Indent();
                    ImGui.Text("伤害范围:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(150);
                    ImGui.SliderInt("##StrikeRange", ref mouseStrikeNPCRange, 1, 85, "%d 格");
                    ImGui.SameLine();
                    ImGui.Text($"{mouseStrikeNPCRange} 格");
                    ImGui.Unindent();
                }
            }

            // 在 DrawUI 方法中添加物品管理部分
            ImGui.Separator();
            ImGui.Checkbox("物品管理", ref itemModify);
            if (itemModify)
            {
                ImGui.SameLine();
                if (ImGui.Button("物品编辑器"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
                    ShowItemManagerWindow = true;
                }

                // 物品管理器窗口
                if (ShowItemManagerWindow)
                {
                    DrawItemManagerWindow(plr);
                }

                // 添加自动垃圾桶按钮
                ImGui.SameLine();
                if (ImGui.Button("自动垃圾桶"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    ShowAutoTrashWindow = true;
                }

                // 显示自动垃圾桶窗口
                if (ShowAutoTrashWindow)
                {
                    DrawAutoTrashWindow(plr);
                }

                // 社交栏饰品开关
                ImGui.Checkbox("社交栏饰品生效", ref socialEnabled);
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.SocialAccessoriesKey, ref EditSocialAccessoriesKey);
                if (socialEnabled)
                {
                    // 新增：添加应用前缀加成和应用盔甲防御的选项框
                    ImGui.Indent();
                    ImGui.Checkbox("前缀加成", ref applyPrefix);
                    ImGui.SameLine();
                    ImGui.Checkbox("盔甲防御", ref applyArmor);
                    ImGui.SameLine();
                    ImGui.Checkbox("饰品功能", ref applyAccessory);
                    ImGui.Unindent();
                }

                // 一键修改饰品前缀按钮
                if (ImGui.Button("一键前缀"))
                {
                    // 播放界面打开音效
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    ShowEditPrefix = true;
                    PrefixId = 0; // 重置为0
                }
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.ShowEditPrefixKey, ref EditShowEditPrefixKey);

                // 显示一键修改前缀窗口
                if (ShowEditPrefix)
                {
                    DrawEditPrefixWindow();
                }

                // 一键收藏按钮
                if (ImGui.Button("一键收藏背包"))
                {
                    // 播放收藏音效
                    SoundEngine.PlaySound(SoundID.MenuTick);

                    // 收藏所有格子物品（包括虚空袋）
                    int favoritedItems = FavoriteAllItems(plr);

                    // 显示操作结果
                    ClientLoader.Chat.WriteLine($"已收藏 {favoritedItems} 个物品", Color.Green);
                }
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.FavoriteKey, ref EditFavoriteKey);

                // 使重力药水、重力球等不会反转屏幕效果
                ImGui.Separator();
                ImGui.Checkbox("反重力药水", ref applyIgnoreGravity);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.IgnoreGravityKey, ref EditIgnoreGravityKey);
            }
        }

        // 更新配置值
        Config.Enabled = enabled;
        Config.Heal = Heal;
        Config.HealVal = HealVal;

        Config.MouseStrikeNPC = mouseStrikeNPC;
        Config.MouseStrikeNPCRange = mouseStrikeNPCRange;

        Config.KillOrRESpawn = killOrRESpawn;

        //自动使用物品
        Config.AutoUseItem = autoUseItem;
        Config.UseItemInterval = autoUseInterval;

        // 更新物品管理器配置
        Config.ItemModify = itemModify;

        // 社交栏饰品开关
        Config.SocialAccessory = socialEnabled;
        Config.ApplyPrefix = applyPrefix;
        Config.ApplyArmor = applyArmor;
        Config.ApplyAccessory = applyAccessory; // 应用饰品效果开关

        Config.IgnoreGravity = applyIgnoreGravity; // 忽略重力药水效果开关

        // 保存按钮
        ImGui.Separator();
        if (ImGui.Button("保存设置"))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
            Config.Write();
            ClientLoader.Chat.WriteLine("插件设置已保存", color);
        }

        // 重置按钮
        ImGui.SameLine();
        if (ImGui.Button("重置默认"))
        {
            SoundEngine.PlaySound(SoundID.MenuClose); // 播放界面关闭音效
            Config.SetDefault();
            ClientLoader.Chat.WriteLine("已重置为默认设置", color);
        }
    }
    #endregion

    #region 自动垃圾桶管理窗口
    private static bool ShowAutoTrashWindow = false; // 显示自动垃圾桶窗口
    private static string NewItemName = ""; // 新物品名称
    private static int NewItemId = 0; // 新物品ID
    private static string NewExclusionName = ""; // 新排除物品名称
    private static int NewExclusionId = 0; // 新排除物品ID
    private static int AutoTrashSyncInterval = Config.TrashSyncInterval; // 自动回收同步间隔
    private Dictionary<int, int> returnAmounts = new Dictionary<int, int>(); // 临时存储需要返还的物品数量

    private void DrawAutoTrashWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(550, 550), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("自动垃圾桶编辑器", ref ShowAutoTrashWindow, ImGuiWindowFlags.NoCollapse))
        {
            var data = Config.TrashItems.FirstOrDefault(x => x.Name == plr.name);

            // 如果没有找到配置，创建一个新的
            if (data == null)
            {
                data = new TrashData
                {
                    Name = plr.name,
                    TrashList = new Dictionary<int, int>(),
                    ExcluItem = new HashSet<int>() { 71, 72, 73, 74 } // 默认排除钱币
                };
                Config.TrashItems.Add(data);
                Config.Write();
            }

            // 总开关
            bool autoTrash = Config.AutoTrash;
            ImGui.Checkbox("启用自动垃圾桶", ref autoTrash);
            Config.AutoTrash = autoTrash; // 更新配置值
            ImGui.SameLine();
            DrawKeySelector("开关按键", ref Config.AutoTrashKey, ref EditAutoTrashKey);

            // 同步间隔设置
            ImGui.SameLine();
            ImGui.Text("回收间隔(毫秒):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.SliderInt("##AutoTrashInterval", ref AutoTrashSyncInterval, 100, 5000, "%d ms");
            Config.TrashSyncInterval = AutoTrashSyncInterval;

            // 自动丢弃物品列表
            ImGui.Separator();
            ImGui.TextColored(new Vector4(1, 0.5f, 0.5f, 1), "《自动垃圾桶表》");

            // 添加新物品区域
            ImGui.Columns(3, "add_trash_columns", false);
            ImGui.SetColumnWidth(0, 200);

            // 物品名称输入
            ImGui.Text("物品名称:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(120);
            ImGui.InputText("##NewTrashItemName", ref NewItemName, 100);

            ImGui.NextColumn();

            // 物品ID输入
            ImGui.Text("物品ID:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(80);
            ImGui.InputInt("##NewTrashItemId", ref NewItemId);

            ImGui.NextColumn();

            // 添加按钮
            if (ImGui.Button("添加物品"))
            {
                if (NewItemId > 0 && !data.TrashList.ContainsKey(NewItemId))
                {
                    data.TrashList.Add(NewItemId, 0);
                    Config.Write();
                    NewItemId = 0;
                    NewItemName = "";
                }
                else if (!string.IsNullOrEmpty(NewItemName))
                {
                    // 尝试通过名称查找物品
                    int foundId = FindItemIdByName(NewItemName);
                    if (foundId > 0 && !data.TrashList.ContainsKey(foundId))
                    {
                        data.TrashList.Add(foundId, 0);
                        Config.Write();
                        NewItemName = "";
                    }
                }
            }

            ImGui.Columns(1);

            // 获取所有垃圾桶物品
            var trashItems = data.TrashList.ToList();

            // 显示物品数量信息
            ImGui.Text($"垃圾桶物品 (共 {trashItems.Count} 个物品)");

            // 当前物品列表
            ImGui.BeginChild("TrashItemsList", new Vector2(0, 180), ImGuiChildFlags.Borders);

            // 使用索引显示所有物品
            for (int i = 0; i < trashItems.Count; i++)
            {
                var item = trashItems[i];
                string itemName = Lang.GetItemNameValue(item.Key);
                if (string.IsNullOrEmpty(itemName)) itemName = $"未知物品 ({item.Key})";
                itemName = $"{i + 1}. {itemName}"; // 添加连续索引前缀

                ImGui.PushID($"trash_{item.Key}");

                // 使用紧凑的4列布局
                ImGui.Columns(4, "trash_item_columns", false);
                ImGui.SetColumnWidth(0, 150); // 物品名称
                ImGui.SetColumnWidth(1, 80);  // 物品ID
                ImGui.SetColumnWidth(2, 120); // 返还数量滑块
                ImGui.SetColumnWidth(3, 100); // 按钮区域

                // 物品名称
                ImGui.Text($"{itemName}");
                ImGui.NextColumn();

                // 物品数量
                ImGui.Text($"ID:{item.Key}");
                ImGui.NextColumn();

                // 初始化返还数量（默认为1）
                if (!returnAmounts.ContainsKey(item.Key))
                {
                    returnAmounts[item.Key] = Math.Max(1, item.Value / 2); // 默认取一半数量
                }

                // 返还数量滑块
                int currentAmount = returnAmounts[item.Key];
                ImGui.SetNextItemWidth(110);
                ImGui.SliderInt($"##return_{item.Key}", ref currentAmount, 1, item.Value, $"{currentAmount}/{item.Value}");
                returnAmounts[item.Key] = currentAmount;
                ImGui.NextColumn();

                // 修改返还按钮的处理逻辑
                if (ImGui.Button("返还", new Vector2(40, 0)))
                {
                    int returnAmount = Math.Min(currentAmount, item.Value);
                    int itemType = item.Key;

                    // 获取物品的最大堆叠数量
                    var tempItem = new Item();
                    tempItem.SetDefaults(itemType);
                    int maxStack = tempItem.maxStack;

                    // 分批返还物品
                    while (returnAmount > 0)
                    {
                        int stackSize = Math.Min(returnAmount, maxStack);

                        // 创建物品实体
                        int itemIndex = Item.NewItem(new EntitySource_DebugCommand(),
                                                    (int)plr.position.X, (int)plr.position.Y,
                                                    plr.width, plr.height, itemType, stackSize,
                                                    noBroadcast: true, tempItem.prefix, noGrabDelay: true);

                        // 设置物品归属并同步
                        Main.item[itemIndex].playerIndexTheItemIsReservedFor = plr.whoAmI;
                        NetMessage.SendData(MessageID.ItemOwner, plr.whoAmI, -1, null, itemIndex);
                        NetMessage.SendData(MessageID.SyncItem, plr.whoAmI, -1, null, itemIndex, 1);

                        returnAmount -= stackSize;
                    }

                    // 更新垃圾桶中的物品数量
                    int newAmount = item.Value - currentAmount;
                    if (newAmount <= 0)
                    {
                        data.TrashList.Remove(item.Key);
                        ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{Lang.GetItemNameValue(item.Key)}] 从[c/4C92D8:自动垃圾桶]移除", color);
                    }
                    else
                    {
                        data.TrashList[item.Key] = newAmount;
                        data.ExcluItem.Add(item.Key);  // 如果垃圾桶还有这个物品则返还指定数量时自动加入排除表
                        ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{Lang.GetItemNameValue(item.Key)}] 加入到[c/4C92D8:排除表]", color);
                    }

                    Config.Write();
                }

                ImGui.SameLine();

                if (ImGui.Button("删除", new Vector2(50, 0)))
                {
                    data.TrashList.Remove(item.Key);
                    Config.Write();
                }

                ImGui.Columns(1);
                ImGui.PopID();
            }

            // 如果没有物品显示提示
            if (trashItems.Count == 0)
            {
                ImGui.Text("垃圾桶列表为空");
            }

            ImGui.EndChild();

            // 排除物品列表
            ImGui.Separator();
            ImGui.TextColored(new Vector4(0.5f, 1, 0.5f, 1), "《排除物品表》");

            // 添加新排除物品区域
            ImGui.Columns(3, "add_exclusion_columns", false);
            ImGui.SetColumnWidth(0, 200);

            // 排除物品名称输入
            ImGui.Text("物品名称:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(120);
            ImGui.InputText("##NewExclusionName", ref NewExclusionName, 100);

            ImGui.NextColumn();

            // 排除物品ID输入
            ImGui.Text("物品ID:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(80);
            ImGui.InputInt("##NewExclusionId", ref NewExclusionId);

            ImGui.NextColumn();

            // 添加按钮
            if (ImGui.Button("添加排除"))
            {
                if (NewExclusionId > 0 && !data.ExcluItem.Contains(NewExclusionId))
                {
                    data.ExcluItem.Add(NewExclusionId);
                    Config.Write();
                    NewExclusionId = 0;
                    NewExclusionName = "";
                }
                else if (!string.IsNullOrEmpty(NewExclusionName))
                {
                    // 尝试通过名称查找物品
                    int foundId = FindItemIdByName(NewExclusionName);
                    if (foundId > 0 && !data.ExcluItem.Contains(foundId))
                    {
                        data.ExcluItem.Add(foundId);
                        Config.Write();
                        NewExclusionName = "";
                    }
                }
            }

            ImGui.Columns(1);

            // 获取所有排除物品
            var excluItems = data.ExcluItem.ToList();

            // 显示物品数量信息
            ImGui.Text($"排除物品 (共 {excluItems.Count} 个物品)");

            // 当前排除列表
            ImGui.BeginChild("物品排除表", new Vector2(0, 180), ImGuiChildFlags.Borders);

            // 使用索引显示所有排除物品
            for (int i = 0; i < excluItems.Count; i++)
            {
                int itemId = excluItems[i];
                string itemName = Lang.GetItemNameValue(itemId);
                if (string.IsNullOrEmpty(itemName)) itemName = $"未知物品 ({itemId})";
                itemName = $"{i + 1}. {itemName}"; // 添加连续索引前缀

                ImGui.PushID($"exclu_{itemId}");

                // 使用紧凑的3列布局
                ImGui.Columns(3, "exclusion_item_columns", false);
                ImGui.SetColumnWidth(0, 150); // 物品名称
                ImGui.SetColumnWidth(1, 80);  // 物品ID
                ImGui.SetColumnWidth(2, 120); // 删除按钮

                // 物品名称
                ImGui.Text($"{itemName}");
                ImGui.NextColumn();

                // 物品ID
                ImGui.Text($"ID:{itemId}");
                ImGui.NextColumn();

                // 删除按钮
                if (ImGui.Button("删除", new Vector2(50, 0)))
                {
                    data.ExcluItem.Remove(itemId);
                    Config.Write();
                }

                ImGui.Columns(1);
                ImGui.PopID();
            }

            // 如果没有物品显示提示
            if (excluItems.Count == 0)
            {
                ImGui.Text("排除列表为空");
            }

            ImGui.EndChild();

            // 添加手持物品按钮
            if (ImGui.Button("添加手持物品到垃圾桶"))
            {
                if (!plr.HeldItem.IsAir)
                {
                    int itemId = plr.HeldItem.type;
                    if (!data.TrashList.ContainsKey(itemId))
                    {
                        data.TrashList.Add(itemId, 0);
                        Config.Write();
                    }
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("添加手持物品到排除表"))
            {
                if (!plr.HeldItem.IsAir)
                {
                    int itemId = plr.HeldItem.type;
                    if (!data.ExcluItem.Contains(itemId))
                    {
                        data.ExcluItem.Add(itemId);
                        Config.Write();
                    }
                }
            }

            // 保存按钮
            if (ImGui.Button("保存设置"))
            {
                Config.Write();
                ClientLoader.Chat.WriteLine("自动垃圾桶设置已保存", Color.Green);
            }
        }
        ImGui.End();
    }
    #endregion

    #region 垃圾桶列表查找方法：物品名称找ID
    private int FindItemIdByName(string itemName)
    {
        for (int i = 1; i < ItemID.Count; i++)
        {
            string name = Lang.GetItemNameValue(i);
            if (!string.IsNullOrEmpty(name) && name.Equals(itemName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return 0;
    } 
    #endregion

    #region 一键收藏所有物品
    private static bool isFavoriteMode = true; // 收藏模式开关（true为收藏，false为取消收藏）
    public static int FavoriteAllItems(Player plr)
    {
        int count = 0;

        // 反转收藏模式（每次调用切换模式）
        isFavoriteMode = !isFavoriteMode;

        // 遍历玩家背包（0-50是主背包）
        for (int i = 0; i < 50; i++)
        {
            Item item = plr.inventory[i];

            if (!item.IsAir)
            {
                // 根据当前模式设置收藏状态
                item.favorited = isFavoriteMode;
                count++;
            }
        }

        // 虚空袋（40格）
        for (int i = 0; i < 40; i++)
        {
            Item item = plr.bank4.item[i];

            if (!item.IsAir)
            {
                // 根据当前模式设置收藏状态
                item.favorited = isFavoriteMode;
                count++;
            }
        }

        return count;
    }
    #endregion

    #region 物品编辑管理器窗口
    private static bool ShowItemManagerWindow = false; // 显示物品管理器窗口
    private static string SearchFilter = ""; // 物品搜索过滤器
    internal void DrawItemManagerWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 400), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("物品编辑管理器", ref ShowItemManagerWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 搜索框
            ImGui.InputText("搜索", ref SearchFilter, 70); 
            ImGui.SameLine();
            DrawKeySelector("应用按键", ref Config.ItemModifyKey, ref EditItemManagerKey);

            var AllItems = Config.ItemModifyList.Where(item => string.IsNullOrEmpty(SearchFilter) ||
            FuzzyMatch(item.Name, SearchFilter)).ToList();

            // 计算总页数
            AllPages = (int)Math.Ceiling(AllItems.Count / (float)PageLimit);
            if (AllPages == 0) AllPages = 1;

            // 确保当前页在有效范围内
            if (NowPage >= AllPages) NowPage = AllPages - 1;
            if (NowPage < 0) NowPage = 0;

            // 添加物品按钮
            if (ImGui.Button($"添加(Alt+{Config.ItemModifyKey})"))
            {
                if (plr.HeldItem != null && !plr.HeldItem.IsAir)
                {
                    // 播放界面打开音效
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);

                    // 创建新物品预设
                    ItemData newItem = ItemData.FromItem(plr.HeldItem);

                    // 生成唯一的预设名称
                    string baseName = $"{plr.HeldItem.Name}";
                    string newName = baseName;
                    int prefix = 1;

                    // 检查名称是否已存在，如果存在则添加后缀
                    while (Config.ItemModifyList.Any(p => p.Name == newName))
                    {
                        newName = $"{baseName}_{prefix++}";
                    }

                    newItem.Name = newName;
                    Config.ItemModifyList.Add(newItem);
                    Config.Write();

                    // 显示成功消息
                    ClientLoader.Chat.WriteLine($"已添加物品预设: {newItem.Name}", Color.Green);
                    ClientLoader.Chat.WriteLine($"使用 {Config.ItemModifyKey} 键应用此预设", Color.Yellow);
                }
                else
                {
                    ClientLoader.Chat.WriteLine($"请手持一个物品 使用{Config.ItemModifyKey}", Color.Red);
                }
            }

            // 清空列表按钮
            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                ShowEditWindow = false;
                Config.ItemModifyList.Clear();
                Config.Write();
            }

            // 物品列表
            ImGui.Separator();
            ImGui.BeginChild("物品列表", new Vector2(0, 220), ImGuiChildFlags.Borders);

            // 获取当前页的物品
            var GetPage = AllItems.Skip(NowPage * PageLimit).Take(PageLimit).ToList();

            foreach (var data in GetPage)
            {
                ImGui.PushID(data.Name);
                ImGui.Columns(2, "item_columns", false);
                ImGui.SetColumnWidth(0, 200);

                // 点击物品名称 打开编辑界面
                if (ImGui.Selectable($"{data.Name}({data.Type})"))
                {
                    // 播放界面打开音效
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    EditItem = data;
                    ShowEditWindow = true;
                }

                // 悬停区域显示工具提示
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    Item tempItem = new Item();
                    tempItem.SetDefaults(data.Type);
                    data.ApplyTo(tempItem);
                    TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
                }

                ImGui.NextColumn();
                if (ImGui.Button($"应用({Config.ItemModifyKey})"))
                {
                    data.ApplyTo(Main.player[Main.myPlayer].HeldItem);
                    ClientLoader.Chat.WriteLine($"已应用物品预设: {data.Name}", Color.Green);
                }

                // 编辑按钮
                ImGui.SameLine();
                if (ImGui.Button("编辑"))
                {
                    // 播放界面打开音效
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    EditItem = data;
                    ShowEditWindow = true;
                }

                // 删除按钮
                ImGui.SameLine();
                if (ImGui.Button($"删除(Ctrl+{Config.ItemModifyKey})"))
                {
                    // 播放界面关闭音效
                    SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    ShowEditWindow = false;
                    Config.ItemModifyList.Remove(data);
                    Config.Write();
                    ClientLoader.Chat.WriteLine($"已删除物品预设: {data.Name}", Color.Yellow);
                }

                ImGui.Columns(1);
                ImGui.PopID();
            }

            // 如果没有物品显示提示
            if (GetPage.Count == 0)
            {
                ImGui.Text($"没有找到匹配的物品 请使用Alt + {Config.ItemModifyKey} 添加");
            }

            ImGui.EndChild();

            // 分页与跳转
            ListPage(AllItems);
        }
        ImGui.End();

        // 显示物品编辑窗口
        if (ShowEditWindow && EditItem != null)
        {
            DrawItemEditWindow(plr);
        }
    }
    #endregion

    #region 模糊搜索匹配方法
    private static bool FuzzyMatch(string target, string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return true;

        pattern = pattern.ToLower();
        target = target.ToLower();

        int Index = 0;
        foreach (char c in target)
        {
            if (c == pattern[Index])
            {
                Index++;
                if (Index == pattern.Length)
                {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region 分页与跳转功能
    private static int NowPage = 0; //当前页码
    private const int PageLimit = 8; // 每页显示8个物品
    private static int AllPages = 0; // 总页数
    private static void ListPage(List<ItemData> AllItems)
    {
        // 显示分页信息和控件
        ImGui.Text($"第 {NowPage + 1} / {AllPages} 页 (共{AllItems.Count}个物品)");
        ImGui.SameLine();
        // 上一页按钮
        if (ImGui.Button("上页") && NowPage > 0)
        {
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);
            NowPage--;
        }

        // 添加页面跳转输入框
        ImGui.SameLine();
        ImGui.SetNextItemWidth(80);
        int GotoPage = NowPage + 1;
        if (ImGui.InputInt("跳转", ref GotoPage, 1, 10))
        {
            if (GotoPage > 0 && GotoPage <= AllPages)
            {
                // 播放界面点击音效
                SoundEngine.PlaySound(SoundID.MenuTick);
                NowPage = GotoPage - 1;
            }
        }

        // 下一页按钮
        ImGui.SameLine();
        if (ImGui.Button("下页") && NowPage < AllPages - 1)
        {
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);
            NowPage++;
        }
    }
    #endregion

    #region 物品编辑器窗口
    private static ItemData EditItem = null!; // 当前正在编辑的物品
    private static bool ShowEditWindow = false; // 是否显示编辑窗口
    private static void DrawItemEditWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(200, 200), ImGuiCond.FirstUseEver);
        if (ImGui.Begin($"编辑物品: {EditItem.Name}", ref ShowEditWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 基本信息
            ImGui.Text($"物品类型: {EditItem.Type} ({Lang.GetItemNameValue(EditItem.Type)})");

            // 添加悬停区域显示工具提示
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
            {
                Item tempItem = new Item();
                tempItem.SetDefaults(EditItem.Type);
                EditItem.ApplyTo(tempItem);
                TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
            }

            ImGui.Separator();

            // 保存按钮
            if (ImGui.Button("保存更改"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                ShowEditWindow = false;
                Config.Write();
                EditItem.ApplyTo(Main.player[Main.myPlayer].HeldItem);
                ClientLoader.Chat.WriteLine($"已保存物品预设: {EditItem.Name}", Color.Green);
            }

            ImGui.SameLine();
            if (ImGui.Button("取消"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                ShowEditWindow = false;
                ClientLoader.Chat.WriteLine($"已取消本次修改: {EditItem.Name}", Color.Green);
            }

            // 名称编辑
            string name = EditItem.Name;
            ImGui.InputText("名称", ref name, 100);
            EditItem.Name = name;

            // 基础属性
            ImGui.Separator();
            ImGui.Text("基础属性:");

            int damage = EditItem.Damage;
            ImGui.InputInt("伤害", ref damage);
            EditItem.Damage = damage;

            int defense = EditItem.Defense;
            ImGui.InputInt("防御", ref defense);
            EditItem.Defense = defense;

            int stack = EditItem.Stack;
            ImGui.InputInt("数量", ref stack);
            EditItem.Stack = stack;

            int prefix = (byte)EditItem.Prefix;
            ImGui.InputInt("前缀ID", ref prefix);
            EditItem.Prefix = (byte)prefix;

            int crit = EditItem.Crit;
            ImGui.InputInt("暴击率", ref crit);
            EditItem.Crit = crit;

            float knockBack = EditItem.KnockBack;
            ImGui.InputFloat("击退", ref knockBack);
            EditItem.KnockBack = knockBack;

            int bait = EditItem.bait; // 添加钓鱼饵属性
            ImGui.InputInt("渔饵力", ref bait);
            EditItem.bait = bait;

            int fishingPole = EditItem.fishingPole; // 添加钓鱼竿等级属性
            ImGui.InputInt("渔力", ref fishingPole);
            EditItem.fishingPole = fishingPole;

            int pick = EditItem.pick; // 添加镐力属性
            ImGui.InputInt("镐力", ref pick);
            EditItem.pick = pick;

            int axe = EditItem.axe; // 添加斧力属性
            ImGui.InputInt("斧力", ref axe);
            EditItem.axe = axe;

            int hammer = EditItem.hammer; // 添加锤力属性
            ImGui.InputInt("锤力", ref hammer);
            EditItem.hammer = hammer;

            int createTile = EditItem.createTile; // 创建的方块类型
            ImGui.InputInt("图格ID", ref createTile);
            EditItem.createTile = createTile;

            int createWall = EditItem.createWall; // 创建的墙类型
            ImGui.InputInt("墙壁ID", ref createWall);
            EditItem.createWall = createWall;

            int value = EditItem.Value;
            ImGui.InputInt("价值", ref value);
            EditItem.Value = value;

            // 射击属性
            ImGui.Separator();
            ImGui.Text("射击属性:");
            int shoot = EditItem.Shoot;
            ImGui.InputInt("弹幕ID", ref shoot);
            EditItem.Shoot = shoot;
            float shootSpeed = EditItem.ShootSpeed;
            ImGui.InputFloat("发射速度", ref shootSpeed);
            EditItem.ShootSpeed = shootSpeed;

            // 使用属性
            ImGui.Separator();
            ImGui.Text("使用属性:");

            int useTime = EditItem.UseTime;
            ImGui.InputInt("使用时间", ref useTime);
            EditItem.UseTime = useTime;

            int useAnimation = EditItem.UseAnimation;
            ImGui.InputInt("使用动画", ref useAnimation);
            EditItem.UseAnimation = useAnimation;

            int useAmmo = EditItem.UseAmmo;
            ImGui.InputInt("使用弹药", ref useAmmo);
            EditItem.UseAmmo = useAmmo;

            int ammo = EditItem.Ammo;
            ImGui.InputInt("是否弹药", ref ammo);
            EditItem.Ammo = ammo;

            int useStyle = EditItem.UseStyle;
            ImGui.InputInt("使用样式", ref useStyle);
            EditItem.UseStyle = useStyle;

            int healLife = EditItem.HealLife; // 物品使用时回复的生命值
            ImGui.InputInt("回复生命", ref healLife);
            EditItem.HealLife = healLife;

            int healMana = EditItem.HealMana; // 物品使用时回复的魔法值
            ImGui.InputInt("回复魔法", ref healMana);
            EditItem.HealMana = healMana;

            // 武器类型
            ImGui.Separator();
            ImGui.Text("物品类型:");
            bool melee = EditItem.Melee;
            ImGui.Checkbox("近战", ref melee);
            EditItem.Melee = melee;
            ImGui.SameLine();
            bool magic = EditItem.Magic;
            ImGui.Checkbox("魔法", ref magic);
            EditItem.Magic = magic;
            ImGui.SameLine();
            bool ranged = EditItem.Ranged;
            ImGui.Checkbox("远程", ref ranged);
            EditItem.Ranged = ranged;
            ImGui.SameLine();
            bool summon = EditItem.Summon;
            ImGui.Checkbox("召唤", ref summon);
            EditItem.Summon = summon;
            ImGui.SameLine();
            bool sentry = EditItem.sentry; // 是否为哨兵
            ImGui.SameLine();
            ImGui.Checkbox("哨兵", ref sentry);

            ImGui.Separator();
            bool accessory = EditItem.accessory; // 是否为饰品
            ImGui.Checkbox("饰品", ref accessory);
            EditItem.accessory = accessory;
            ImGui.SameLine();
            bool wornArmor = EditItem.wornArmor;
            ImGui.Checkbox("盔甲", ref wornArmor);
            EditItem.wornArmor = wornArmor;
            ImGui.SameLine();
            bool consumable = EditItem.consumable; // 是否为消耗品
            ImGui.Checkbox("消耗品", ref consumable);
            EditItem.consumable = consumable;
            ImGui.SameLine();
            bool material = EditItem.material; // 是否为材料
            ImGui.Checkbox("材料", ref material);
            EditItem.material = material;

            ImGui.Separator();
            int headSlot = EditItem.headSlot; // 头盔槽位
            ImGui.InputInt("头盔槽位", ref headSlot);
            EditItem.headSlot = headSlot;
            int bodySlot = EditItem.bodySlot; // 上衣槽位
            ImGui.InputInt("上衣槽位", ref bodySlot);
            EditItem.bodySlot = bodySlot;
            int legSlot = EditItem.legSlot; // 裤子槽位
            ImGui.InputInt("裤子槽位", ref legSlot);
            EditItem.legSlot = legSlot;

            ImGui.Separator();
            ImGui.Text("使用类型:");
            bool autoReuse = EditItem.AutoReuse;
            ImGui.Checkbox("自动挥舞", ref autoReuse);
            EditItem.AutoReuse = autoReuse;
            ImGui.SameLine();
            bool useTurn = EditItem.UseTurn;
            ImGui.Checkbox("使用转向", ref useTurn);
            EditItem.UseTurn = useTurn;
            ImGui.SameLine();
            bool channel = EditItem.Channel;
            ImGui.Checkbox("持续使用", ref channel);
            EditItem.Channel = channel;
            ImGui.SameLine();
            bool noMelee = EditItem.NoMelee;
            ImGui.Checkbox("无近战伤害", ref noMelee);
            EditItem.NoMelee = noMelee;
            ImGui.SameLine();
            bool noUseGraphic = EditItem.NoUseGraphic;
            ImGui.Checkbox("无使用图像", ref noUseGraphic);
            EditItem.NoUseGraphic = noUseGraphic;
        }
        ImGui.End();
    }
    #endregion

    #region 一键修改前缀窗口
    public static bool ShowEditPrefix = false; // 显示批量修改前缀窗口
    private static int PrefixId = 0; // 用于存储新前缀ID的变量
    private static void DrawEditPrefixWindow()
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 150), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));

        if (ImGui.Begin("批量修改饰品前缀", ref ShowEditPrefix, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("将修改玩家饰品栏的前缀");

            ImGui.Spacing();
            ImGui.InputInt("新前缀ID", ref PrefixId);

            // 限制前缀ID在有效范围内(0-84)
            if (PrefixId < 0) PrefixId = 0;
            if (PrefixId > 84) PrefixId = 84;

            ImGui.Spacing();
            if (ImGui.Button("应用"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose);
                ApplyPrefix();
                ShowEditPrefix = false;
            }

            ImGui.SameLine();
            if (ImGui.Button("取消"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose);
                ShowEditPrefix = false;
            }
        }
        ImGui.End();
    }
    #endregion

    #region 批量修改饰品
    private static void ApplyPrefix()
    {
        Player plr = Main.player[Main.myPlayer];
        int count = 0;

        var pr = Lang.prefix[PrefixId].ToString();
        if (string.IsNullOrEmpty(pr))
        {
            pr = "无";
        }

        // 默认跳过的槽位：装饰栏
        List<int> NotSlot;
        if (plr.extraAccessory)
        {
            // 开启额外饰品：饰品栏为 3~9，跳过 10、11、12
            NotSlot = new List<int>() { 10, 11, 12 };
        }
        else
        {
            // 未开启额外饰品：饰品栏为 3~8，跳过 9、10、11
            NotSlot = new List<int>() { 9, 10, 11 };
        }

        for (int i = 3; i < plr.armor.Length; i++)
        {
            if (NotSlot.Contains(i)) continue;
            var item = plr.armor[i];

            if (item != null && !item.IsAir &&
                item.accessory && PrefixId != 0)
            {
                item.Prefix((byte)PrefixId);
                count++;
            }
        }

        ClientLoader.Chat.WriteLine($"已为 {count} 个饰品槽位设置前缀: {pr}", color);
    }
    #endregion

    #region 按键选择器辅助方法
    private void DrawKeySelector(string label, ref Keys key, ref bool editing)
    {
        // 显示按键标签和当前按键
        ImGui.Text($"{label}:");
        ImGui.SameLine();

        if (ImGui.Button($"{key}##{label}"))
        {
            editing = !editing;
        }

        // 如果正在编辑，显示提示
        if (editing)
        {
            ImGui.SameLine();
            ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), "[按下新按键]");

            // 检测所有按键
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                if (k == Keys.None) continue;

                if (InputSystem.IsKeyPressed(k))
                {
                    // 播放按键选择音效
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    key = k;
                    editing = false;
                    break;
                }
            }
        }
    }
    #endregion
}