using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Tools;
using Terraria;
using Microsoft.Xna.Framework;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

public class UITool : Tool
{
    public override string Name => "羽学插件设置";
    public override ToolTabs Tab => ToolTabs.MainTools;

    // 用于临时存储按键编辑状态
    private bool editingHealKey = false;
    private bool editingKillKey = false;
    private bool editingAutoUseKey = false;

    // 添加编辑状态变量
    private bool editingItemManagerKey = false;
    private static string searchFilter = ""; // 物品搜索过滤器
    private static bool showItemManager = false; // 是否显示物品管理器窗口

    #region UI与配置文件交互方法
    public override void DrawUI(ImGuiIOPtr io)
    {
        bool enabled = Config.Enabled; //插件总开关
        bool Heal = Config.Heal; //回血开关
        int HealVal = Config.HealVal; //回血值
        Keys healKey = Config.HealKey; //回血按键

        bool mouseStrikeNPC = Config.MouseStrikeNPC; //鼠标范围伤害NPC开关
        int mouseStrikeNPCRange = Config.MouseStrikeNPCRange; //伤害范围

        bool killOrRESpawn = Config.KillOrRESpawn; //快速死亡与复活开关
        Keys killKey = Config.KillKey; //自杀与复活按键

        bool autoUseItem = Config.AutoUseItem; //自动使用物品开关
        Keys autoUseKey = Config.AutoUseKey; //自动使用物品按键
        int autoUseInterval = Config.AutoUseInterval; //自动使用物品间隔

        bool itemManager = Config.ItemManager; //物品管理开关
        Keys itemManagerKey = Config.ItemManagerKey; //物品管理按键

        // 绘制插件设置界面
        ImGui.Checkbox("启用羽学插件", ref enabled);
        if (enabled)
        {
            // 快速死亡复活开关（单bool + 自定义按键）
            ImGui.Separator();
            ImGui.Checkbox("快速死亡/复活", ref killOrRESpawn);
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("按键", ref killKey, ref editingKillKey);

            // 回血设置 （bool + 滑块 + 自定义按键）
            ImGui.Separator();
            ImGui.Checkbox("强制回血", ref Heal);
            ImGui.SameLine(); // 回血按键设置
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("按键", ref healKey, ref editingHealKey);
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
            DrawKeySelector("按键", ref autoUseKey, ref editingAutoUseKey);
            if (autoUseItem)
            {
                ImGui.Indent();
                ImGui.Text("使用间隔(毫秒):");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(150);
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
            ImGui.Checkbox("物品管理", ref itemManager);
            ImGui.SameLine();
            DrawKeySelector("按键", ref itemManagerKey, ref editingItemManagerKey);
            if (itemManager)
            {
                ImGui.SameLine();
                if (ImGui.Button("打开物品管理器"))
                {
                    showItemManager = true;
                }

                // 添加物品管理器窗口
                if (showItemManager)
                {
                    DrawItemManagerWindow();
                }
            }

        }

        // 更新配置值
        Config.Enabled = enabled;
        Config.Heal = Heal;
        Config.HealVal = HealVal;
        Config.HealKey = healKey;

        Config.MouseStrikeNPC = mouseStrikeNPC;
        Config.MouseStrikeNPCRange = mouseStrikeNPCRange;

        Config.KillOrRESpawn = killOrRESpawn;
        Config.KillKey = killKey;

        //自动使用物品
        Config.AutoUseItem = autoUseItem;
        Config.AutoUseInterval = autoUseInterval;
        Config.AutoUseKey = autoUseKey;

        // 更新配置变量
        Config.ItemManager = itemManager;
        Config.ItemManagerKey = itemManagerKey;

        // 保存按钮
        if (ImGui.Button("保存设置"))
        {
            Config.Write();
            ClientLoader.Chat.WriteLine("插件设置已保存", color);
        }

        // 重置按钮
        ImGui.SameLine();
        if (ImGui.Button("重置默认"))
        {
            Config.SetDefault();
            ClientLoader.Chat.WriteLine("已重置为默认设置", color);
        }
    }
    #endregion

    #region 实现物品管理器窗口
    private static ItemData editingItem = null!; // 当前正在编辑的物品
    private static bool showEditItemWindow = false; // 是否显示编辑窗口
    internal static void DrawItemManagerWindow()
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 400), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("物品管理器", ref showItemManager, ImGuiWindowFlags.NoCollapse))
        {
            // 搜索框
            ImGui.InputText("搜索", ref searchFilter, 100);

            // 添加物品按钮
            if (ImGui.Button($"添加当前手持物品(Alt+{Config.ItemManagerKey})"))
            {
                var plr = Main.player[Main.myPlayer];
             
                if (plr.HeldItem != null && !plr.HeldItem.IsAir)
                {
                    // 创建新物品预设
                    ItemData newItem = ItemData.FromItem(plr.HeldItem);

                    // 生成唯一的预设名称
                    string baseName = $"{plr.HeldItem.Name}";
                    string newName = baseName;
                    int prefix = 1;

                    // 检查名称是否已存在，如果存在则添加后缀
                    while (Config.items.Any(p => p.Name == newName))
                    {
                        newName = $"{prefix++}.{baseName}";
                    }

                    newItem.Name = newName;

                    Config.items.Add(newItem);
                    Config.Write();

                    // 显示成功消息
                    ClientLoader.Chat.WriteLine($"已添加物品预设: {newItem.Name}", Color.Green);
                    ClientLoader.Chat.WriteLine($"使用 {Config.ItemManagerKey} 键应用此预设", Color.Yellow);
                }
                else
                {
                    ClientLoader.Chat.WriteLine($"请手持一个物品 使用{Config.ItemManagerKey}", Color.Red);
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                Config.items.Clear();
                Config.Write();
            }

            // 物品列表
            ImGui.Separator();
            ImGui.BeginChild("物品列表", new System.Numerics.Vector2(0, 300), ImGuiChildFlags.Borders);

            foreach (var item in Config.items.ToList())
            {
                // 应用搜索过滤器
                if (!string.IsNullOrEmpty(searchFilter) &&
                    !item.Name.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ImGui.PushID(item.Name);
                ImGui.Columns(2, "item_columns", false);
                ImGui.SetColumnWidth(0, 200);

                // 显示物品名称
                ImGui.Text($"{item.Name}({item.Type})");
                ImGui.Text($"伤害: {item.Damage}  防御: {item.Defense}  数量: {item.Stack}");
                ImGui.Text($"暴击: {item.Crit}%  击退: {item.KnockBack:F1}");

                // 第二列：按钮
                ImGui.NextColumn();

                // 应用按钮
                if (ImGui.Button($"应用({Config.ItemManagerKey})"))
                {
                    var player = Main.player[Main.myPlayer];
                    editingItem.ApplyTo(player.HeldItem);
                    ClientLoader.Chat.WriteLine($"已应用物品预设: {item.Name}", Color.Green);
                }

                ImGui.SameLine();

                // 编辑按钮
                if (ImGui.Button("编辑"))
                {
                    editingItem = item;
                    showEditItemWindow = true;
                }

                ImGui.SameLine();

                // 删除按钮
                if (ImGui.Button($"删除(Ctrl+{Config.ItemManagerKey})"))
                {
                    Config.items.Remove(item);
                    Config.Write();
                    ClientLoader.Chat.WriteLine($"已删除物品预设: {item.Name}", Color.Yellow);
                }

                ImGui.Columns(1);
                ImGui.PopID();
            }

            ImGui.EndChild();
        }
        ImGui.End();

        // 显示物品编辑窗口
        if (showEditItemWindow && editingItem != null)
        {
            DrawItemEditorWindow();
        }
    }
    #endregion

    #region 物品编辑器窗口
    private static void DrawItemEditorWindow()
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(600, 700), ImGuiCond.FirstUseEver);
        if (ImGui.Begin($"编辑物品: {editingItem.Name}", ref showEditItemWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 基本信息
            ImGui.Text($"物品类型: {editingItem.Type} ({Lang.GetItemNameValue(editingItem.Type)})");
            ImGui.Separator();

            // 名称编辑
            string name = editingItem.Name;
            ImGui.InputText("名称", ref name, 100);
            editingItem.Name = name;

            // 基础属性
            ImGui.Separator();
            ImGui.Text("基础属性:");

            int damage = editingItem.Damage;
            ImGui.InputInt("伤害", ref damage);
            editingItem.Damage = damage;

            int defense = editingItem.Defense;
            ImGui.InputInt("防御", ref defense);
            editingItem.Defense = defense;

            int stack = editingItem.Stack;
            ImGui.InputInt("数量", ref stack);
            editingItem.Stack = stack;

            int prefix = (byte)editingItem.Prefix;
            ImGui.InputInt("前缀ID", ref prefix);
            editingItem.Prefix = (byte)prefix;

            int crit = editingItem.Crit;
            ImGui.InputInt("暴击率", ref crit);
            editingItem.Crit = crit;

            float knockBack = editingItem.KnockBack;
            ImGui.InputFloat("击退", ref knockBack);
            editingItem.KnockBack = knockBack;

            // 使用属性
            ImGui.Separator();
            ImGui.Text("使用属性:");

            int useTime = editingItem.UseTime;
            ImGui.InputInt("使用时间", ref useTime);
            editingItem.UseTime = useTime;

            int useAnimation = editingItem.UseAnimation;
            ImGui.InputInt("使用动画", ref useAnimation);
            editingItem.UseAnimation = useAnimation;

            int useAmmo = editingItem.UseAmmo;
            ImGui.InputInt("使用弹药", ref useAmmo);
            editingItem.UseAmmo = useAmmo;

            int ammo = editingItem.Ammo;
            ImGui.InputInt("是否弹药", ref ammo);
            editingItem.Ammo = ammo;

            int useStyle = editingItem.UseStyle;
            ImGui.InputInt("使用样式", ref useStyle);
            editingItem.UseStyle = useStyle;

            int bait = editingItem.bait; // 添加钓鱼饵属性
            ImGui.InputInt("鱼饵力", ref bait);
            editingItem.bait = bait;

            int healLife = editingItem.HealLife; // 物品使用时回复的生命值
            ImGui.InputInt("使用时回复生命", ref healLife);
            editingItem.HealLife = healLife;

            int healMana = editingItem.HealMana; // 物品使用时回复的魔法值
            ImGui.InputInt("使用时回复魔法", ref healMana);
            editingItem.HealMana = healMana;

            bool autoReuse = editingItem.AutoReuse;
            ImGui.Checkbox("自动挥舞", ref autoReuse);
            editingItem.AutoReuse = autoReuse;

            bool useTurn = editingItem.UseTurn;
            ImGui.Checkbox("使用转向", ref useTurn);
            editingItem.UseTurn = useTurn;

            bool channel = editingItem.Channel;
            ImGui.Checkbox("持续使用", ref channel);
            editingItem.Channel = channel;

            bool noMelee = editingItem.NoMelee;
            ImGui.Checkbox("无近战伤害", ref noMelee);
            editingItem.NoMelee = noMelee;

            bool noUseGraphic = editingItem.NoUseGraphic;
            ImGui.Checkbox("无使用图像", ref noUseGraphic);
            editingItem.NoUseGraphic = noUseGraphic;

            // 射击属性
            ImGui.Separator();
            ImGui.Text("射击属性:");

            int shoot = editingItem.Shoot;
            ImGui.InputInt("弹幕ID", ref shoot);
            editingItem.Shoot = shoot;

            float shootSpeed = editingItem.ShootSpeed;
            ImGui.InputFloat("发射速度", ref shootSpeed);
            editingItem.ShootSpeed = shootSpeed;

            // 武器类型
            ImGui.Separator();
            ImGui.Text("武器类型:");

            bool melee = editingItem.Melee;
            ImGui.Checkbox("近战", ref melee);
            editingItem.Melee = melee;

            bool magic = editingItem.Magic;
            ImGui.Checkbox("魔法", ref magic);
            editingItem.Magic = magic;

            bool ranged = editingItem.Ranged;
            ImGui.Checkbox("远程", ref ranged);
            editingItem.Ranged = ranged;

            bool summon = editingItem.Summon;
            ImGui.Checkbox("召唤", ref summon);
            editingItem.Summon = summon;

            // 价值
            ImGui.Separator();
            int value = editingItem.Value;
            ImGui.InputInt("价值", ref value);
            editingItem.Value = value;

            // 保存按钮
            if (ImGui.Button("保存更改"))
            {
                Config.Write();
                showEditItemWindow = false;
                ClientLoader.Chat.WriteLine($"已保存物品预设: {editingItem.Name}", Color.Green);
            }

            ImGui.SameLine();
            if (ImGui.Button("取消"))
            {
                showEditItemWindow = false;
            }
        }
        ImGui.End();
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
                    key = k;
                    editing = false;
                    break;
                }
            }
        }
    }
    #endregion
}