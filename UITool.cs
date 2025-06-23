using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Tools;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

public class UITool : Tool
{
    public override string Name => "羽学插件设置";
    public override ToolTabs Tab => ToolTabs.MainTools;

    // 用于临时存储按键编辑状态
    private bool EditHealKey = false;
    private bool EditKillKey = false;
    private bool EditAutoUseKey = false;
    private bool editShowEditPrefixKey = false;
    private bool editFavoriteKey = false;
    private bool EditItemManagerKey = false; // 物品管理器按键编辑状态

    private static string SearchFilter = ""; // 物品搜索过滤器
    private static bool ShowItemManagerWindow = false; // 显示物品管理器窗口

    public static bool ShowEditPrefix = false; // 显示批量修改前缀窗口
    private static int PrefixId = 0; // 用于存储新前缀ID的变量

    // 分页相关变量
    private static int NowPage = 0; //当前页码
    private const int PageLimit = 8; // 每页显示8个物品
    private static int AllPages = 0; // 总页数

    #region UI与配置文件交互方法
    public override void DrawUI(ImGuiIOPtr io)
    {
        var plr = Main.player[Main.myPlayer];

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
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);

            // 快速死亡复活开关（单bool + 自定义按键）
            ImGui.Separator();
            ImGui.Checkbox("快速死亡/复活", ref killOrRESpawn);
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("按键", ref killKey, ref EditKillKey);

            // 回血设置 （bool + 滑块 + 自定义按键）
            ImGui.Separator();
            ImGui.Checkbox("强制回血", ref Heal);
            ImGui.SameLine(); // 回血按键设置
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
            DrawKeySelector("按键", ref healKey, ref EditHealKey);
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
            DrawKeySelector("按键", ref autoUseKey, ref EditAutoUseKey);
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
            ImGui.Checkbox("物品管理", ref itemManager);
            ImGui.SameLine();
            DrawKeySelector("按键", ref itemManagerKey, ref EditItemManagerKey);
            if (itemManager)
            {
                ImGui.SameLine();
                if (ImGui.Button("打开物品管理器"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
                    ShowItemManagerWindow = true;
                }

                // 物品管理器窗口
                if (ShowItemManagerWindow)
                {
                    DrawItemManagerWindow(plr);
                }

                // 一键修改饰品前缀按钮
                if (ImGui.Button("一键前缀"))
                {
                    // 播放界面打开音效
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    ShowEditPrefix = true;
                    PrefixId = 0; // 重置为0
                }

                // 修改前缀窗口热键配置
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.ShowEditPrefixKey, ref editShowEditPrefixKey);

                // 添加提示文本
                ImGui.SameLine();
                ImGui.TextDisabled("(?)");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("一键修改盔甲组的所有饰品前缀");
                    ImGui.Text("仅适配大师难度");
                    ImGui.EndTooltip();
                }

                // 显示一键修改前缀窗口
                if (ShowEditPrefix)
                {
                    DrawEditPrefixWindow();
                }

                // +++ 新增一键收藏按钮 +++
                ImGui.SameLine();
                if (ImGui.Button("一键收藏背包"))
                {
                    // 播放收藏音效
                    SoundEngine.PlaySound(SoundID.MenuTick);

                    // 收藏所有格子物品（包括虚空袋）
                    int favoritedItems = FavoriteAllItems(plr);

                    // 显示操作结果
                    ClientLoader.Chat.WriteLine($"已收藏 {favoritedItems} 个物品", Color.Green);
                }

                // 一键收藏热键配置
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.FavoriteKey, ref editFavoriteKey);

                // 添加提示文本
                ImGui.SameLine();
                ImGui.TextDisabled("(?)");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("收藏所有背包中的物品（主背包+虚空袋）");
                    ImGui.Text("防止意外丢弃或出售重要物品");
                    ImGui.EndTooltip();
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

    #region 一键收藏所有物品
    public static int FavoriteAllItems(Player plr)
    {
        int count = 0;

        // 遍历玩家背包（主背包、钱币栏、弹药栏）
        for (int i = 0; i < 49; i++) // 0-49是主背包，50-53是钱币栏，54-57是弹药栏
        {
            Item item = plr.inventory[i];

            if (!item.IsAir)
            {
                item.favorited = !item.favorited;
                count++;
            }
        }

        // 虚空袋（plr.bank4.item：40格）
        for (int i = 0; i < 40; i++)
        {
            Item item = plr.bank4.item[i];

            if (!item.IsAir)
            {
                item.favorited = !item.favorited;
                count++;
            }
        }

        return count;
    }
    #endregion

    #region 实现物品管理器窗口
    private static ItemData EditItem = null!; // 当前正在编辑的物品
    private static bool ShowEditWindow = false; // 是否显示编辑窗口
    internal static void DrawItemManagerWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 400), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("物品管理器", ref ShowItemManagerWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 搜索框
            ImGui.InputText("搜索", ref SearchFilter, 100);

            var AllItems = Config.items.Where(item => string.IsNullOrEmpty(SearchFilter) ||
            FuzzyMatch(item.Name, SearchFilter)).ToList();

            // 计算总页数
            AllPages = (int)Math.Ceiling(AllItems.Count / (float)PageLimit);
            if (AllPages == 0) AllPages = 1;

            // 确保当前页在有效范围内
            if (NowPage >= AllPages) NowPage = AllPages - 1;
            if (NowPage < 0) NowPage = 0;

            // 添加物品按钮
            if (ImGui.Button($"添加(Alt+{Config.ItemManagerKey})"))
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
                    while (Config.items.Any(p => p.Name == newName))
                    {
                        newName = $"{baseName}_{prefix++}";
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

            // 清空列表按钮
            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                ShowEditWindow = false;
                Config.items.Clear();
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
                if (ImGui.Button($"应用({Config.ItemManagerKey})"))
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
                if (ImGui.Button($"删除(Ctrl+{Config.ItemManagerKey})"))
                {
                    // 播放界面关闭音效
                    SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    ShowEditWindow = false;
                    Config.items.Remove(data);
                    Config.Write();
                    ClientLoader.Chat.WriteLine($"已删除物品预设: {data.Name}", Color.Yellow);
                }

                ImGui.Columns(1);
                ImGui.PopID();
            }

            // 如果没有物品显示提示
            if (GetPage.Count == 0)
            {
                ImGui.Text($"没有找到匹配的物品 请使用Alt + {Config.ItemManagerKey} 添加");
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