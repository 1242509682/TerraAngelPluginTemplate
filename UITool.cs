using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Numerics;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Tools;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Events;
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
    private bool EditClearAnglerQuestsKey = false; // 清除钓鱼任务按键编辑状态
    private bool EditNPCAutoHealKey = false; // NPC自动回血按键编辑状态
    public bool EditNPCReliveKey = false; // 复活NPC按键编辑状态
    public bool EditVeinMinerKey = false; // 连锁挖矿按键编辑状态

    #region UI与配置文件交互方法
    public override void DrawUI(ImGuiIOPtr io)
    {
        var plr = Main.player[Main.myPlayer];

        bool enabled = Config.Enabled; //插件总开关
        bool Heal = Config.Heal; //回血开关
        int HealVal = Config.HealVal; //回血值

        bool killOrRESpawn = Config.KillOrRESpawn; //快速死亡与复活开关

        bool autoUseItem = Config.AutoUseItem; //自动使用物品开关
        int autoUseInterval = Config.UseItemInterval; //自动使用物品间隔

        bool mouseStrikeNPC = Config.MouseStrikeNPC; //鼠标范围伤害NPC开关
        int mouseStrikeNPCRange = Config.MouseStrikeNPCRange; //伤害范围
        int mouseStrikeNPCInterval = Config.MouseStrikeInterval; // 伤害NPC间隔
        int StrikeVel = Config.MouseStrikeNPCVel; // 伤害值

        bool socialEnabled = Config.SocialAccessory; // 社交栏饰品生效开关
        bool applyPrefix = Config.ApplyPrefix; // 开启额外饰品的前缀加成
        bool applyArmor = Config.ApplyArmor; // 开启装饰栏盔甲的护甲加成
        bool applyAccessory = Config.ApplyAccessory; // 开启额外饰品的原有被动功能

        bool applyIgnoreGravity = Config.IgnoreGravity; // 启用忽略重力药水效果

        bool AutoClearAngel = Config.ClearAnglerQuests; // 清除钓鱼任务开关
        int ClearAngelInterval = Config.ClearQuestsInterval; // 清除钓鱼任务按键

        bool nPCAutoHeal = Config.NPCAutoHeal; // NPC自动回血开关
        float NPCHealVel = Config.NPCHealVel; // 普通NPC回血百分比
        int NPCHealVelInterval = Config.NPCHealInterval; // 普通NPC回血间隔(秒)
        bool Boss = Config.Boss; // 允许boss回血
        float BossHealVel = Config.BossHealVel; // BOSS回血百分比
        int BossHealCap = Config.BossHealCap; // BOSS每次回血上限
        int BossHealInterval = Config.BossHealInterval; //BOSS独立回血间隔(秒)

        bool FavoriteItemForJoinWorld = Config.FavoriteItemForJoinWorld; // 进入世界自动收藏物品

        // 绘制插件设置界面
        ImGui.Checkbox("启用羽学插件", ref enabled);
        if (enabled)
        {
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);

            // 定位传送区域
            ImGui.Separator();
            if (ImGui.TreeNodeEx("传送管理", ImGuiTreeNodeFlags.Framed))
            {
                DrawTeleportUI(plr);
                ImGui.TreePop();

                // 显示NPC传送窗口
                if (ShowNPCTeleportWindow)
                {
                    DrawNPCTeleportWindow(plr);
                }

                // 显示自定义传送点窗口
                if (ShowCustomTeleportWindow)
                {
                    DrawCustomTeleportWindow(plr);
                }

                // 显示死亡地点选择窗口
                if (ShowDeathTeleportWindow)
                {
                    DrawDeathTeleportWindow(plr);
                }
            }

            // 事件控制区域
            ImGui.Separator();
            float Width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X * 8) / 8f;
            if (ImGui.TreeNodeEx("事件管理", ImGuiTreeNodeFlags.Framed))
            {
                // 血月按钮
                if (ImGui.Button("血月", new Vector2(Width, 40)))
                {
                    Utils.ToggleBloodMoon();
                }

                // 日食按钮
                ImGui.SameLine();
                if (ImGui.Button("日食", new Vector2(Width, 40)))
                {
                    Utils.ToggleEclipse();
                }

                // 满月按钮
                ImGui.SameLine();
                if (ImGui.Button("满月", new Vector2(Width, 40)))
                {
                    Utils.ToggleFullMoon();
                }

                // 下雨按钮
                ImGui.SameLine();
                if (ImGui.Button("下雨", new Vector2(Width, 40)))
                {
                    Utils.ToggleRain();
                }

                // 史莱姆雨按钮
                ImGui.SameLine();
                if (ImGui.Button("史莱姆雨", new Vector2(Width, 40)))
                {
                    Utils.ToggleSlimeRain();
                }

                // 时间按钮
                if (ImGui.Button("时间", new Vector2(Width, 40)))
                {
                    Utils.ToggleTime();
                }

                // 沙尘暴按钮
                ImGui.SameLine();
                if (ImGui.Button("沙尘暴", new Vector2(Width, 40)))
                {
                    Utils.ToggleSandstorm();
                }

                // 灯笼夜按钮
                ImGui.SameLine();
                if (ImGui.Button("灯笼夜", new Vector2(Width, 40)))
                {
                    Utils.ToggleLanternNight();
                }

                // 陨石按钮
                ImGui.SameLine();
                if (ImGui.Button("陨石", new Vector2(Width, 40)))
                {
                    Utils.TriggerMeteor();
                }

                // 入侵按钮
                ImGui.SameLine();
                if (ImGui.Button("入侵事件", new Vector2(Width, 40)))
                {
                    ShowInvasionWindow = !ShowInvasionWindow;
                }

                // 显示入侵选择窗口
                if (ShowInvasionWindow)
                {
                    DrawInvasionWindow();
                }

                ImGui.TreePop();
            }

            // 辅助功能区域
            ImGui.Separator();
            if (ImGui.TreeNodeEx("辅助功能", ImGuiTreeNodeFlags.Framed))
            {
                ImGui.Checkbox("强制回血", ref Heal);
                ImGui.SameLine(); // 回血按键设置
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                DrawKeySelector("按键", ref Config.HealKey, ref EditHealKey);
                if (Heal)
                {
                    ImGui.Text("回血量:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(150);
                    ImGui.SliderInt("##HealAmount", ref HealVal, 1, 500, "%d HP");
                    ImGui.SameLine();
                    ImGui.Text($"{HealVal} HP");
                }

                // 快速死亡复活开关（单bool + 自定义按键）
                ImGui.Checkbox("快速死亡/复活", ref killOrRESpawn);
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                DrawKeySelector("按键", ref Config.KillKey, ref EditKillKey);

                // 自动清理渔夫任务
                ImGui.Checkbox("清渔夫任务", ref AutoClearAngel);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.ClearQuestsKey, ref EditClearAnglerQuestsKey);
                if (AutoClearAngel)
                {
                    ImGui.Text("清理间隔(帧):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##AnglerQuestsInterval", ref ClearAngelInterval, 1, 3000, "%d fps");
                }

                // 自动使用物品
                ImGui.Checkbox("自动使用物品", ref autoUseItem);
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                DrawKeySelector("按键", ref Config.AutoUseKey, ref EditAutoUseKey);
                if (autoUseItem)
                {
                    ImGui.Text("使用间隔(帧):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##AutoUseInterval", ref autoUseInterval, 1, 1800, "%d fps");
                }

                ImGui.Checkbox("启用鼠标范围伤害NPC", ref mouseStrikeNPC);
                if (mouseStrikeNPC)
                {
                    ImGui.Text("伤害范围:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(150);
                    ImGui.SliderInt("##StrikeRange", ref mouseStrikeNPCRange, 0, 85, "%d 格");
                    ImGui.SameLine();
                    ImGui.Text($"{mouseStrikeNPCRange} 格");

                    ImGui.Text("伤害间隔(帧):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##StrikeInterval", ref mouseStrikeNPCInterval, 1, 1800, "%d fps");

                    ImGui.Text("伤害值:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##StrikeVel", ref StrikeVel, 0, 20000, "%d 点");
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("不设置数值时使用手上物品伤害");
                }

                ImGui.TreePop();
            }

            // 物品管理区域
            ImGui.Separator();
            if (ImGui.TreeNodeEx("物品管理", ImGuiTreeNodeFlags.Framed))
            {
                if (ImGui.Button("物品编辑器"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
                    ShowItemManagerWindow = !ShowItemManagerWindow;
                }

                // 物品管理器窗口
                if (ShowItemManagerWindow)
                {
                    DrawItemManagerWindow(plr);
                }

                // 自动垃圾桶
                ImGui.SameLine();
                if (ImGui.Button("自动垃圾桶"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    ShowAutoTrashWindow = !ShowAutoTrashWindow;
                }

                // 显示自动垃圾桶窗口
                if (ShowAutoTrashWindow)
                {
                    DrawAutoTrashWindow(plr);
                }

                // 连锁挖矿
                ImGui.SameLine();
                if (ImGui.Button("连锁挖矿"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    ShowVeinMinerWindow = !ShowVeinMinerWindow; // 连锁挖矿窗口
                }

                if (ShowVeinMinerWindow)
                {
                    VeinMineWindows();
                }

                // 在物品管理区域添加按钮
                ImGui.SameLine();
                if (ImGui.Button("配方管理器"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    ShowRecipeManagerWindow = !ShowRecipeManagerWindow;
                    EditingRecipe = null; // 重置编辑状态
                }

                // 显示配方管理器窗口
                if (ShowRecipeManagerWindow)
                {
                    RecipeManagerWindow();
                }

                // 使重力药水、重力球等不会反转屏幕效果
                ImGui.Separator();
                ImGui.Checkbox("反重力药水", ref applyIgnoreGravity);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.IgnoreGravityKey, ref EditIgnoreGravityKey);

                // 社交栏饰品开关
                ImGui.Checkbox("社交栏饰品生效", ref socialEnabled);
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.SocialAccessoriesKey, ref EditSocialAccessoriesKey);
                if (socialEnabled)
                {
                    ImGui.Checkbox("前缀加成", ref applyPrefix);
                    ImGui.SameLine();
                    ImGui.Checkbox("盔甲防御", ref applyArmor);
                    ImGui.SameLine();
                    ImGui.Checkbox("饰品功能", ref applyAccessory);
                }

                ImGui.Separator();
                if (ImGui.TreeNodeEx("背包设置", ImGuiTreeNodeFlags.Framed))
                {
                    // 一键修改饰品前缀按钮
                    if (ImGui.Button("一键饰品前缀"))
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
                    if (ImGui.Button("一键收藏物品"))
                    {
                        // 播放收藏音效
                        SoundEngine.PlaySound(SoundID.MenuTick);

                        // 收藏所有格子物品（包括虚空袋）
                        int favoritedItems = Utils.FavoriteAllItems(plr);

                        // 显示操作结果
                        ClientLoader.Chat.WriteLine($"已收藏 {favoritedItems} 个物品", Color.Green);
                    }
                    ImGui.SameLine();
                    DrawKeySelector("按键", ref Config.FavoriteKey, ref EditFavoriteKey);

                    ImGui.Checkbox("进入世界自动收藏物品", ref FavoriteItemForJoinWorld);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("每次进入世界时自动收藏背包所有物品");

                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }

            // NPC管理区域
            ImGui.Separator();
            if (ImGui.TreeNodeEx("NPC管理", ImGuiTreeNodeFlags.Framed))
            {
                // 第一次打开时加载NPC列表
                if (!npcListLoaded)
                {
                    Utils.LoadNPCList();
                    npcListLoaded = true;
                }

                ImGui.TextColored(new Vector4(1f, 0.8f, 0.6f, 1f), "生成与复活NPC");

                // 生成NPC区域
                ImGui.Spacing();
                if (ImGui.Button("生成NPC"))
                {
                    ShowSpawnNpcWindow = !ShowSpawnNpcWindow;
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("生成怪物或NPC在屏幕外");

                if (ShowSpawnNpcWindow)
                {
                    SpawnNpcWindows();
                }

                // 复活NPC区域
                ImGui.SameLine();
                if (ImGui.Button("复活NPC"))
                {
                    Utils.Relive(true);
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("复活所有已解锁图鉴的城镇NPC");
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.NPCReliveKey, ref EditNPCReliveKey);

                // npc自动回血
                ImGui.TextColored(new Vector4(1f, 0.8f, 0.6f, 1f), "NPC自动回血");
                ImGui.Separator();
                ImGui.Checkbox("NPC自动回血", ref nPCAutoHeal);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.NPCAutoHealKey, ref EditNPCAutoHealKey);
                if (nPCAutoHeal)
                {
                    ImGui.Text("普通NPC间隔(秒):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##NPCHealVelInterval", ref NPCHealVelInterval, 1, 60 * 5); //最久5分钟回一次

                    // 普通NPC回血设置
                    ImGui.Text("普通NPC回血(百分比):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderFloat("##NPCHealVel", ref NPCHealVel, 0.01f, 20f, "%.2f%%");

                    // BOSS回血设置
                    ImGui.Checkbox("允许Boss回血", ref Boss);
                    if (Boss)
                    {
                        ImGui.Text("BOSS回血限制");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(200);
                        ImGui.InputInt("##BossHealCap", ref BossHealCap);

                        ImGui.Text("BOSS回血间隔(秒)");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(200);
                        ImGui.SliderInt("##BossHealInterval", ref BossHealInterval, 1, 60 * 5); //最久5分钟回一次

                        ImGui.Text("BOSS回血值(百分比)");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(200);
                        ImGui.SliderFloat("##BossHealVel", ref BossHealVel, 0.01f, 20f, "%.2f%%");
                    }
                }

                ImGui.TreePop();
            }
        }

        // 更新配置值
        Config.Enabled = enabled;
        Config.Heal = Heal;
        Config.HealVal = HealVal;

        Config.KillOrRESpawn = killOrRESpawn;

        //自动使用物品
        Config.AutoUseItem = autoUseItem;
        Config.UseItemInterval = autoUseInterval;

        // 鼠标位置伤害NPC
        Config.MouseStrikeNPC = mouseStrikeNPC;
        Config.MouseStrikeNPCRange = mouseStrikeNPCRange;
        Config.MouseStrikeInterval = mouseStrikeNPCInterval;
        Config.MouseStrikeNPCVel = StrikeVel;

        // 社交栏饰品开关
        Config.SocialAccessory = socialEnabled;
        Config.ApplyPrefix = applyPrefix;
        Config.ApplyArmor = applyArmor;
        Config.ApplyAccessory = applyAccessory; // 应用饰品效果开关

        Config.IgnoreGravity = applyIgnoreGravity; // 忽略重力药水效果开关

        Config.ClearAnglerQuests = AutoClearAngel; // 清除钓鱼任务开关
        Config.ClearQuestsInterval = ClearAngelInterval; // 清除钓鱼任务间隔

        Config.NPCAutoHeal = nPCAutoHeal; // NPC自动回血开关
        Config.NPCHealVel = NPCHealVel; // 普通NPC回血百分比
        Config.NPCHealInterval = NPCHealVelInterval; // 普通NPC回血间隔(秒)
        Config.Boss = Boss; // 允许boss回血
        Config.BossHealVel = BossHealVel; // BOSS回血百分比
        Config.BossHealCap = BossHealCap; // BOSS每次回血上限
        Config.BossHealInterval = BossHealInterval; //BOSS独立回血间隔(秒)

        Config.FavoriteItemForJoinWorld = FavoriteItemForJoinWorld; // 进入世界自动收藏物品

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
            Config.Write();
            ClientLoader.Chat.WriteLine("已重置为默认设置", color);
        }
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

    #region 配方管理器窗口
    private static bool ShowRecipeManagerWindow = false; // 控制配方管理器窗口显示
    private string RecipeSearch = ""; // 配方搜索过滤器
    public static CustomRecipeData? EditingRecipe = null; // 当前正在编辑的配方
    public static bool IsNewRecipe = false; // 是否是新配方
    private int NewIngredientItemId = 0; // 新添加的材料物品ID
    private int NewIngredientStack = 1; // 新添加的材料数量
    private void RecipeManagerWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(400, 600), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("配方管理器", ref ShowRecipeManagerWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 顶部按钮区域
            ImGui.Separator();
            if (ImGui.Button("新建配方"))
            {
                EditingRecipe = new CustomRecipeData();
                IsNewRecipe = true;
            }

            // 新增：清空所有配方按钮
            ImGui.SameLine();
            if (ImGui.Button("清空所有配方"))
            {
                // 1. 从游戏配方槽位中移除所有自定义配方
                foreach (var recipe in Config.CustomRecipes)
                {
                    if (recipe.Index != -1)
                    {
                        RecipeHooks.RemoveRecipe(recipe.Index);
                        Main.availableRecipe[recipe.Index] = 0;
                    }
                }

                // 2. 清除自定义配方物品缓存
                CustomRecipeItems.Clear();
                CustomRecipeIndexes.Clear();

                // 3. 清空配置中的配方列表
                Config.CustomRecipes.Clear();

                // 4. 重置编辑状态
                EditingRecipe = null;

                // 5. 保存配置
                Config.Write();

                // 6. 重新加载配方
                Recipe.FindRecipes();

                ClientLoader.Chat.WriteLine("已清空所有自定义配方", Color.Green);
            }

            ImGui.SameLine();
            if (ImGui.Button("保存所有配方"))
            {
                Config.Write();
                ClientLoader.Chat.WriteLine("配方已保存", color);
                Recipe.FindRecipes(); // 强制重新加载所有配方
            }

            ImGui.SameLine();
            if (ImGui.Button("从原版导入配方"))
            {
                ShowVanillaRecipeSelector = !ShowVanillaRecipeSelector;
            }

            // 显示原版配方选择器
            if (ShowVanillaRecipeSelector)
            {
                VanillaRecipeSelectorWindow();
            }

            ImGui.SameLine();
            if (ImGui.Button("忽略原版工作站需求"))
            {
                Config.IgnoreStationRequirements = !Config.IgnoreStationRequirements;
                Config.Write();
                string status = Config.IgnoreStationRequirements ? "开启" : "关闭";
                ClientLoader.Chat.WriteLine($"忽略原版工作站需求功能已{status}", color);
            }

            ImGui.SameLine();
            if (ImGui.Button("解锁所有配方"))
            {
                Config.UnlockAllRecipes = !Config.UnlockAllRecipes;
                Config.Write();
                string status = Config.UnlockAllRecipes ? "开启" : "关闭";
                ClientLoader.Chat.WriteLine($"解锁所有配方功能已{status}", color);
            }

            // 搜索框
            ImGui.Separator();
            ImGui.Text("搜索配方:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##RecipeSearch", "输入物品名称或ID", ref RecipeSearch, 100);

            // 配方列表区域
            ImGui.Separator();
            ImGui.Text("配方列表:");
            ImGui.SameLine();
            ImGui.Text($"({Config.CustomRecipes.Count} 个配方)");

            ImGui.BeginChild("RecipeList", new Vector2(250, 300), ImGuiChildFlags.Borders);

            // 应用搜索过滤
            var GetRecipes = Config.CustomRecipes.Where(r =>
            {
                if (string.IsNullOrWhiteSpace(RecipeSearch)) return true;

                // 检查结果物品
                Item resultItem = new Item();
                resultItem.SetDefaults(r.ResultItem);
                if (resultItem.Name.Contains(RecipeSearch, StringComparison.OrdinalIgnoreCase) || r.ResultItem.ToString().Contains(RecipeSearch))
                    return true;

                // 检查材料
                foreach (var ing in r.Ingredients)
                {
                    Item ingItem = new Item();
                    ingItem.SetDefaults(ing.ItemId);
                    if (ingItem.Name.Contains(RecipeSearch, StringComparison.OrdinalIgnoreCase) || ing.ItemId.ToString().Contains(RecipeSearch))
                        return true;
                }

                return false;

            }).ToList();

            if (GetRecipes.Count == 0)
            {
                ImGui.TextDisabled("没有找到配方");
            }
            else
            {
                for (int i = 0; i < GetRecipes.Count; i++)
                {
                    var recipe = GetRecipes[i];
                    Item resultItem = new Item();
                    resultItem.SetDefaults(recipe.ResultItem);

                    bool isSelected = EditingRecipe == recipe;
                    if (ImGui.Selectable($"{resultItem.Name} x{recipe.ResultStack}##{i}", isSelected))
                    {
                        EditingRecipe = recipe;
                        IsNewRecipe = false;
                    }
                }
            }
            ImGui.EndChild();

            // 配方编辑区域
            ImGui.SameLine();
            ImGui.BeginGroup();

            if (EditingRecipe != null)
            {
                ImGui.Text("编辑配方:");
                ImGui.Separator();

                // 结果物品设置
                ImGui.Text("结果物品:");
                ImGui.SameLine();

                Item resultItem = new Item();
                resultItem.SetDefaults(EditingRecipe.ResultItem);
                string resultLabel = resultItem.Name != "" ? $"{resultItem.Name} (ID:{EditingRecipe.ResultItem})" : "未选择";

                if (ImGui.Button(resultLabel))
                {
                    ShowItemSelectorForResult = !ShowItemSelectorForResult;
                }

                if (ShowItemSelectorForResult)
                {
                    var selectedItemId = EditingRecipe.ResultItem;
                    DrawItemSelector(ref selectedItemId);
                    EditingRecipe.ResultItem = selectedItemId;
                }

                var selectedItemsStack = EditingRecipe.ResultStack;
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.InputInt("数量", ref selectedItemsStack);
                EditingRecipe.ResultStack = Math.Max(1, selectedItemsStack);

                // 材料列表
                ImGui.Separator();
                ImGui.Text("所需材料:");

                ImGui.BeginChild("IngredientsList", new Vector2(0, 150), ImGuiChildFlags.Borders);

                if (EditingRecipe.Ingredients.Count == 0)
                {
                    ImGui.TextDisabled("没有添加材料");
                }
                else
                {
                    for (int i = 0; i < EditingRecipe.Ingredients.Count; i++)
                    {
                        var ingredient = EditingRecipe.Ingredients[i];
                        Item ingItem = new Item();
                        ingItem.SetDefaults(ingredient.ItemId);

                        ImGui.PushID(i);

                        ImGui.Text($"{ingItem.Name} x{ingredient.Stack}");

                        ImGui.SameLine();
                        if (ImGui.Button("删除"))
                        {
                            EditingRecipe.Ingredients.RemoveAt(i);
                            i--;
                        }

                        ImGui.PopID();
                    }
                }

                ImGui.EndChild();

                // 添加新材料
                ImGui.Text("添加新材料:");
                ImGui.SameLine();

                Item newIngItem = new Item();
                newIngItem.SetDefaults(NewIngredientItemId);
                string newIngLabel = newIngItem.Name != "" ? $"{newIngItem.Name} (ID:{NewIngredientItemId})" : "选择物品";

                if (ImGui.Button(newIngLabel))
                {
                    ShowItemSelectorForIngredient = !ShowItemSelectorForIngredient;
                }

                if (ShowItemSelectorForIngredient)
                {
                    DrawItemSelector(ref NewIngredientItemId);
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.InputInt("数量##NewIng", ref NewIngredientStack);
                NewIngredientStack = Math.Max(1, NewIngredientStack);

                ImGui.SameLine();
                if (ImGui.Button("添加") && NewIngredientItemId > 0)
                {
                    EditingRecipe.Ingredients.Add(new IngredientData
                    {
                        ItemId = NewIngredientItemId,
                        Stack = NewIngredientStack
                    });

                    // 重置
                    NewIngredientItemId = 0;
                    NewIngredientStack = 1;
                }

                // 合成站设置 - 多选支持
                ImGui.Separator();
                ImGui.Text("合成站:");

                // 显示当前选择的合成站
                if (EditingRecipe.RequiredTile.Count() > 0)
                {
                    string stationNames = string.Join(", ", EditingRecipe.RequiredTile
                        .Select(id => TileID.Search.GetName(id))
                        .Where(name => !string.IsNullOrEmpty(name)));

                    ImGui.TextColored(new Vector4(0.8f, 1.0f, 0.8f, 1.0f), stationNames);
                }
                else
                {
                    ImGui.TextDisabled("无合成站需求");
                }

                ImGui.SameLine();
                if (ImGui.Button("选择合成站"))
                {
                    ShowTileSelector = !ShowTileSelector;
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("手持可放置图格的物品可添加自定义合成站");

                // 清空合成站按钮
                ImGui.SameLine();
                if (ImGui.Button("清空"))
                {
                    EditingRecipe.RequiredTile.Clear();
                }

                if (ShowTileSelector)
                {
                    DrawTileSelector();
                }

                // 保存/删除按钮
                ImGui.Separator();
                if (ImGui.Button(IsNewRecipe ? "添加配方" : "保存修改"))
                {
                    if (IsNewRecipe)
                    {
                        // 确保配方不存在再添加
                        bool Exists = Config.CustomRecipes.Any(r =>
                            r.ResultItem == EditingRecipe.ResultItem &&
                            r.Ingredients.SequenceEqual(EditingRecipe.Ingredients));

                        if (!Exists)
                        {
                            EditingRecipe.Index = -1;
                            Config.CustomRecipes.Add(EditingRecipe);
                            ClientLoader.Chat.WriteLine($"已添加配方: {resultItem.Name}", Color.Green);

                            // 保存并重新加载配方
                            Config.Write();
                            Recipe.FindRecipes();
                        }
                        else
                        {
                            ClientLoader.Chat.WriteLine($"配方已存在: {resultItem.Name}", Color.Yellow);
                        }
                    }
                    else
                    {
                        // 更新现有配方
                        ClientLoader.Chat.WriteLine($"配方已更新: {resultItem.Name}", Color.Green);

                        // 保存并重新加载配方
                        Config.Write();
                        Recipe.FindRecipes();
                    }

                    // 重置编辑状态
                    EditingRecipe = null;
                }

                ImGui.SameLine();
                if (ImGui.Button("取消"))
                {
                    EditingRecipe = null;
                }

                ImGui.SameLine();
                if (!IsNewRecipe && EditingRecipe != null && ImGui.Button("删除配方"))
                {
                    // 清除配方槽位
                    if (EditingRecipe.Index != -1)
                    {
                        RecipeHooks.RemoveRecipe(EditingRecipe.Index);
                    }

                    // 从配置中移除
                    Config.CustomRecipes.Remove(EditingRecipe);

                    ClientLoader.Chat.WriteLine($"已删除配方: {resultItem.Name}", Color.Yellow);

                    // 保存并重新加载配方
                    Config.Write();
                    Recipe.FindRecipes();

                    EditingRecipe = null;
                }
            }
            else
            {
                ImGui.TextDisabled("请从左侧选择一个配方或新建配方");
            }

            ImGui.EndGroup();
        }
        ImGui.End();
    }
    #endregion

    #region 原版配方选择器窗口
    private bool ShowVanillaRecipeSelector = false;
    private string VanillaRecipeSearch = "";
    private void VanillaRecipeSelectorWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(600, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("从原版配方导入", ref ShowVanillaRecipeSelector, ImGuiWindowFlags.NoCollapse))
        {
            // 搜索框
            ImGui.Text("搜索配方:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##VanillaRecipeSearch", "输入物品名称或ID", ref VanillaRecipeSearch, 100);

            // 配方列表
            ImGui.Separator();
            ImGui.Text("原版配方列表:");
            ImGui.BeginChild("VanillaRecipeList", new Vector2(0, 400), ImGuiChildFlags.Borders);

            // 应用搜索过滤
            var GetRecipes = Main.recipe
                .Where(r => r != null && r.createItem.type > 0)
                .Where(r =>
                {
                    if (string.IsNullOrWhiteSpace(VanillaRecipeSearch)) return true;

                    Item resultItem = r.createItem;
                    if (resultItem.Name.Contains(VanillaRecipeSearch, StringComparison.OrdinalIgnoreCase) ||
                        resultItem.type.ToString().Contains(VanillaRecipeSearch))
                        return true;

                    return false;
                })
                .ToList();

            if (GetRecipes.Count == 0)
            {
                ImGui.TextDisabled("没有找到配方");
            }
            else
            {
                for (int i = 0; i < GetRecipes.Count; i++)
                {
                    var recipe = GetRecipes[i];
                    Item resultItem = recipe.createItem;

                    // 显示配方信息
                    string recipeInfo = $"{resultItem.Name} x{resultItem.stack}";

                    // 显示材料
                    recipeInfo += " (材料: ";
                    for (int j = 0; j < recipe.requiredItem.Length; j++)
                    {
                        if (recipe.requiredItem[j].type > 0)
                        {
                            Item ingItem = recipe.requiredItem[j];
                            recipeInfo += $"{ingItem.Name} x{ingItem.stack}, ";
                        }
                    }
                    recipeInfo = recipeInfo.TrimEnd(',', ' ') + ")";

                    if (ImGui.Selectable($"{recipeInfo}##{i}"))
                    {
                        // 将原版配方转换为自定义格式
                        RecipeHooks.ConvertRecipe(recipe);
                        ShowVanillaRecipeSelector = false;
                        ClientLoader.Chat.WriteLine($"已导入配方: {resultItem.Name}", Color.Green);
                    }
                }
            }

            ImGui.EndChild();
        }
        ImGui.End();
    }
    #endregion

    #region  配方材料物品选择器窗口
    private bool ShowItemSelectorForResult = false;
    private bool ShowItemSelectorForIngredient = false;
    private string ItemSearchText = "";
    private void DrawItemSelector(ref int selectedItemId)
    {
        ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("选择物品", ref ShowItemSelectorForResult, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("搜索物品:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("##ItemSearch", ref ItemSearchText, 100);

            ImGui.BeginChild("ItemList", new Vector2(0, 400), ImGuiChildFlags.Borders);

            var filteredItems = ContentSamples.ItemsByType.Where(kvp =>
            {
                if (kvp.Value == null || kvp.Key <= 0) return false;
                if (string.IsNullOrWhiteSpace(ItemSearchText)) return true;

                return kvp.Value.Name.Contains(ItemSearchText, StringComparison.OrdinalIgnoreCase) ||
                       kvp.Key.ToString().Contains(ItemSearchText);
            });

            foreach (var kvp in filteredItems)
            {
                Item item = kvp.Value;
                if (item.type == 0 || item.Name == null) continue;

                if (ImGui.Selectable($"{item.Name} (ID:{item.type})", selectedItemId == item.type))
                {
                    selectedItemId = item.type;
                    ShowItemSelectorForResult = false;
                    ShowItemSelectorForIngredient = false;
                }
            }

            ImGui.EndChild();
        }
        ImGui.End();
    }
    #endregion

    #region 配方工作台图格选择器窗口
    private bool ShowTileSelector = false;
    private string TileSearchText = "";
    private List<int> TileSelectionCache = new List<int>();
    private static Dictionary<int, string> CustomStations = new Dictionary<int, string>();

    private void DrawTileSelector()
    {
        ImGui.SetNextWindowSize(new Vector2(450, 550), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("选择合成站", ref ShowTileSelector, ImGuiWindowFlags.NoCollapse))
        {
            // 显示当前已选择的合成站
            Dictionary<int, string> Stations = StationList();
            if (TileSelectionCache.Count > 0)
            {
                ImGui.Text("已选择:");
                ImGui.SameLine();

                string SelectedStations = string.Join(", ", TileSelectionCache
                    .Select(id =>
                    {
                        if (Stations.TryGetValue(id, out var name))
                            return name;
                        return $"未知图格({id})";
                    }));

                ImGui.TextColored(new Vector4(0.8f, 1.0f, 0.8f, 1.0f), SelectedStations);
            }
            else
            {
                ImGui.Text("已选择: 无合成站需求");
            }

            ImGui.Text("搜索合成站:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(250);
            ImGui.InputText("##TileSearch", ref TileSearchText, 100);

            ImGui.BeginChild("TileList", new Vector2(0, 380), ImGuiChildFlags.Borders);

            // 过滤常用合成站
            var GetStations = Stations.Where(s =>
                string.IsNullOrWhiteSpace(TileSearchText) ||
                s.Value.Contains(TileSearchText, StringComparison.OrdinalIgnoreCase) ||
                s.Key.ToString().Contains(TileSearchText)
            );

            foreach (var station in GetStations)
            {
                bool isSelected = TileSelectionCache.Contains(station.Key);
                if (ImGui.Checkbox($"{station.Value}##{station.Key}", ref isSelected))
                {
                    if (isSelected)
                    {
                        if (!TileSelectionCache.Contains(station.Key))
                            TileSelectionCache.Add(station.Key);
                    }
                    else
                    {
                        TileSelectionCache.Remove(station.Key);
                    }
                }
            }

            ImGui.EndChild();

            // 按钮区域
            ImGui.Separator();
            if (ImGui.Button("清空选择"))
            {
                TileSelectionCache.Clear();
            }

            ImGui.SameLine();
            if (ImGui.Button("确认"))
            {
                // 更新当前编辑配方的合成站
                if (EditingRecipe != null)
                {
                    EditingRecipe.RequiredTile.Clear();
                    EditingRecipe.RequiredTile.AddRange(TileSelectionCache);
                }
                TileSelectionCache.Clear();
                ShowTileSelector = false;
            }

            ImGui.SameLine();
            if (ImGui.Button("取消"))
            {
                TileSelectionCache.Clear();
                ShowTileSelector = false;
            }

            // 添加手持物品合成站按钮
            ImGui.SameLine();
            Item heldItem = Main.LocalPlayer.HeldItem;
            if (heldItem != null && heldItem.createTile > 0)
            {
                string itemName = Lang.GetItemNameValue(heldItem.type);
                if (ImGui.Button($"添加手持物品: {itemName}"))
                {
                    int tileID = heldItem.createTile;
                    string tileName = itemName + " (自定义)";

                    // 添加到自定义缓存
                    if (!CustomStations.ContainsKey(tileID))
                    {
                        CustomStations.Add(tileID, tileName);
                        Main.NewText($"已添加合成站: {tileName}");
                    }

                    // 自动选中新添加的合成站
                    if (!TileSelectionCache.Contains(tileID))
                    {
                        TileSelectionCache.Add(tileID);
                    }
                }
            }
        }
        ImGui.End();
    }

    #region 合成站表（移除了"无"选项）
    private static Dictionary<int, string> StationList()
    {
        // 常用合成站列表（移除了"无"选项）
        var baseStations = new Dictionary<int, string>
        {
            { TileID.WorkBenches, "工作台" },
            { TileID.Furnaces, "熔炉" },
            { TileID.Anvils, "铁砧/铅砧" },
            { TileID.Hellforge, "地狱熔炉" },
            { TileID.AlchemyTable, "炼药桌" },
            { TileID.ImbuingStation, "灌注站" },
            { TileID.CrystalBall, "水晶球" },
            { TileID.Autohammer, "自动锤炼机" },
            { TileID.Loom, "织布机" },
            { TileID.Sawmill, "锯木机" },
            { TileID.Kegs, "酒桶" },
            { TileID.CookingPots, "烹饪锅" },
            { TileID.Blendomatic, "搅拌机" },
            { TileID.MeatGrinder, "绞肉机" },
            { TileID.LesionStation, "病变工作站" },
            { TileID.SteampunkBoiler, "蒸汽朋克锅炉" },
            { TileID.GlassKiln, "玻璃窑" },
            { TileID.Solidifier, "固化机" },
            { TileID.BoneWelder, "骨焊机" },
            { TileID.FleshCloningVat, "血肉克隆台" },
            { TileID.SkyMill, "天磨" },
            { TileID.LivingLoom, "活木织机" },
            { TileID.IceMachine, "冰雪机" },
            { TileID.HeavyWorkBench, "重型工作台" },
            { 412, "远古操纵机" },
        };

        // 合并自定义合成站
        var mergedStations = new Dictionary<int, string>(baseStations);
        foreach (var custom in CustomStations)
        {
            if (!mergedStations.ContainsKey(custom.Key))
            {
                mergedStations.Add(custom.Key, custom.Value);
            }
        }

        return mergedStations;
    }
    #endregion
    #endregion

    #region 连锁挖矿窗口
    private static bool ShowVeinMinerWindow = false; // 显示自动垃圾桶窗口
    private string VeinMinerSearch = ""; // 连锁挖矿搜索过滤器
    private void VeinMineWindows()
    {
        bool VeinMinerEnabled = Config.VeinMinerEnabled; // 连锁挖矿开关
        ImGui.SetNextWindowSize(new Vector2(0, 100), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("连锁挖矿", ref ShowVeinMinerWindow, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Separator();
            ImGui.Checkbox("启用连锁挖矿", ref VeinMinerEnabled);
            Config.VeinMinerEnabled = VeinMinerEnabled;
            ImGui.SameLine();
            DrawKeySelector("按键", ref Config.VeinMinerKey, ref EditVeinMinerKey);

            // 添加矿物按钮
            if (ImGui.Button("手持物品添加图格"))
            {
                Item heldItem = Main.LocalPlayer.HeldItem;
                if (heldItem.createTile >= 0)
                {
                    int tileID = heldItem.createTile;
                    string itemName = heldItem.Name;

                    // 检查是否已存在相同图格ID
                    bool exists = false;
                    foreach (var mineral in Config.VeinMinerList)
                    {
                        if (mineral.TileID == tileID)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        Config.VeinMinerList.Add(new VeinMinerItem(tileID, itemName));
                        ClientLoader.Chat.WriteLine($"已添加连锁图格: {itemName} (ID: {tileID})", Color.Green);
                    }
                    else
                    {
                        ClientLoader.Chat.WriteLine("该图格已在列表中", Color.Yellow);
                    }
                }
                else
                {
                    ClientLoader.Chat.WriteLine("手持物品不是可放置的图格", Color.Red);
                }
            }

            // 清除按钮
            ImGui.SameLine();
            if (ImGui.Button("清除连锁图格表"))
            {
                Config.VeinMinerList.Clear();
                Config.Write();
                ClientLoader.Chat.WriteLine("已清除连锁图格表", Color.Yellow);
            }

            // 最大挖掘数量设置
            int count = Config.VeinMinerCount;
            ImGui.Text("最大挖掘上限:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.SliderInt("##VeinMinerCount", ref count, 10, 1000, "%d");
            Config.VeinMinerCount = count;

            // 添加搜索框
            ImGui.Separator();
            ImGui.Text("搜索图格:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##VeinMinerSearch", "输入名称或ID", ref VeinMinerSearch, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                VeinMinerSearch = "";
            }

            // 应用过滤
            var filteredMinerals = Config.VeinMinerList
                .Where(m => string.IsNullOrWhiteSpace(VeinMinerSearch) ||
                            m.ItemName.Contains(VeinMinerSearch, StringComparison.OrdinalIgnoreCase) ||
                            m.TileID.ToString().Contains(VeinMinerSearch))
                .ToList();

            ImGui.Text($"当前连锁图格列表: ({filteredMinerals.Count} 个)");

            // 矿物表显示
            ImGui.BeginChild("VeinMinerList", new Vector2(0, 200), ImGuiChildFlags.Borders);
            if (filteredMinerals.Count > 0)
            {
                // 表格样式设置
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(8, 4));

                // 开始表格（3列：ID、矿物名称、操作）
                if (ImGui.BeginTable("VeinMinerTable", 3,
                    ImGuiTableFlags.Borders |
                    ImGuiTableFlags.RowBg |
                    ImGuiTableFlags.SizingFixedFit))
                {
                    // 设置列宽
                    ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 60);
                    ImGui.TableSetupColumn("物品名称", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("操作", ImGuiTableColumnFlags.WidthFixed, 80);
                    ImGui.TableHeadersRow();

                    // 遍历所有矿物
                    for (int i = 0; i < filteredMinerals.Count; i++)
                    {
                        var mineral = filteredMinerals[i];
                        int tileID = mineral.TileID;
                        string itemName = mineral.ItemName;

                        ImGui.TableNextRow();

                        // ID列
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{tileID}");

                        // 名称列
                        ImGui.TableSetColumnIndex(1);
                        ImGui.Text(itemName);

                        // 操作列
                        ImGui.TableSetColumnIndex(2);
                        if (ImGui.Button($"删除##{tileID}"))
                        {
                            string removedName = itemName;
                            Config.VeinMinerList.Remove(mineral);
                            ClientLoader.Chat.WriteLine($"已移除图格: {removedName}", Color.Yellow);
                        }
                    }

                    ImGui.EndTable();
                }
                ImGui.PopStyleVar();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(VeinMinerSearch))
                {
                    ImGui.TextDisabled("连锁图格表为空");
                }
                else
                {
                    ImGui.TextDisabled($"没有找到包含 '{VeinMinerSearch}' 的连锁图格");
                }
            }
            ImGui.EndChild(); // 结束子窗口
        }
        ImGui.End();
    }
    #endregion

    #region 生成NPC窗口
    private static bool ShowSpawnNpcWindow = false; // 显示生成NPC窗口
    public static string spawnNPCInput = ""; // 生成NPC输入
    public static int spawnNPCAmount = 1; // 生成数量
    public static string npcSearchFilter = ""; // NPC搜索过滤器
    internal static List<NPCInfo> npcList = new List<NPCInfo>(); // NPC列表缓存
    public static bool npcListLoaded = false; // NPC列表是否已加载
    private void SpawnNpcWindows()
    {
        ImGui.SetNextWindowSize(new Vector2(0, 200), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("生成NPC", ref ShowSpawnNpcWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 输入框和搜索
            ImGui.Separator();
            ImGui.Text("搜索NPC:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##NPCSearch", "输入名称或ID", ref npcSearchFilter, 100);

            // 生成数量
            ImGui.Text("生成数量:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##NPCAmount", ref spawnNPCAmount);
            if (spawnNPCAmount < 1) spawnNPCAmount = 1;
            if (spawnNPCAmount > 100) spawnNPCAmount = 100;

            // 应用过滤后的NPC列表
            var filteredNPCs = npcList
                .Where(n => string.IsNullOrWhiteSpace(npcSearchFilter) ||
                            n.Name.Contains(npcSearchFilter, StringComparison.OrdinalIgnoreCase) ||
                            n.ID.ToString().Contains(npcSearchFilter))
                .ToList();

            // 显示NPC列表
            ImGui.BeginChild("NPCList", new Vector2(0, 300), ImGuiChildFlags.Borders);

            if (filteredNPCs.Count > 0)
            {
                // 显示表头
                ImGui.Columns(3, "npc_columns", true);
                ImGui.SetColumnWidth(0, 60);  // ID
                ImGui.SetColumnWidth(1, 250); // 名称
                ImGui.SetColumnWidth(2, 120); // 操作

                ImGui.Text("ID"); ImGui.NextColumn();
                ImGui.Text("名称"); ImGui.NextColumn();
                ImGui.Text("操作"); ImGui.NextColumn();
                ImGui.Separator();

                foreach (var npc in filteredNPCs)
                {
                    ImGui.Text($"{npc.ID}"); ImGui.NextColumn();
                    ImGui.Text($"{npc.Name}"); ImGui.NextColumn();

                    // 生成按钮
                    if (ImGui.Button($"生成##{npc.ID}"))
                    {
                        Utils.SpawnNPC(npc.ID, npc.Name, spawnNPCAmount,
                                 (int)Main.LocalPlayer.position.X / 16,
                                 (int)Main.LocalPlayer.position.Y / 16);
                        ClientLoader.Chat.WriteLine($"已生成 {spawnNPCAmount} 个 {npc.Name}", Color.Green);
                    }

                    ImGui.NextColumn();
                }
                ImGui.Columns(1);
            }
            else
            {
                ImGui.Text("没有找到匹配的NPC");
            }

            ImGui.EndChild();

            // 手动输入生成
            ImGui.Text("手动生成:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##ManualSpawn", "输入NPC ID或名称", ref spawnNPCInput, 100);
            ImGui.SameLine();
            if (ImGui.Button("生成"))
            {
                Utils.SpawnNPCByInput();
            }
        }
        ImGui.End();
    }
    #endregion

    #region 入侵事件管理器
    public static bool ShowInvasionWindow = false; // 显示入侵选择窗口
    private static void DrawInvasionWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(350, 300), ImGuiCond.FirstUseEver);
        float Width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X * 6) / 7f;
        if (ImGui.Begin("选择入侵类型", ref ShowInvasionWindow, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("选择入侵类型:");
            ImGui.Separator();

            if (ImGui.Button("哥布林入侵", new Vector2(Width, 40)))
            {
                Utils.StartInvasion(1);
            }

            ImGui.SameLine();
            if (ImGui.Button("雪人军团", new Vector2(Width, 40)))
            {
                Utils.StartInvasion(2);
            }

            ImGui.SameLine();
            if (ImGui.Button("海盗入侵", new Vector2(Width, 40)))
            {
                Utils.StartInvasion(3);
            }

            if (ImGui.Button("火星人入侵", new Vector2(Width, 40)))
            {
                Utils.StartInvasion(4);
            }

            ImGui.SameLine();
            if (ImGui.Button("南瓜月", new Vector2(Width, 40)))
            {
                Utils.StartMoonEvent(1);
            }

            ImGui.SameLine();
            if (ImGui.Button("霜月", new Vector2(Width, 40)))
            {
                Utils.StartMoonEvent(2);
            }

            ImGui.Separator();
            ImGui.Text("当前入侵状态:");
            ImGui.SameLine();
            if (ImGui.Button("停止入侵"))
            {
                Utils.StopInvasion();
            }

            if (Main.invasionSize > 0)
            {
                string status = $"{Utils.GetInvasionName(Main.invasionType)}: ";
                status += $"{Main.invasionSize}/{Main.invasionSizeStart}";

                if (Main.invasionSize <= 0)
                    status += " (已完成)";
                else
                    status += $" ({(int)((float)Main.invasionSize / Main.invasionSizeStart * 100)}%)";

                ImGui.Text(status);

            }
            else if (DD2Event.Ongoing)
            {
                ImGui.Text("撒旦军队进行中");

            }
            else if (Main.pumpkinMoon)  // 新增南瓜月检测
            {
                ImGui.Text("南瓜月进行中 (波数: " + NPC.waveNumber + ")");

            }
            else if (Main.snowMoon)  // 新增霜月检测
            {
                ImGui.Text("霜月进行中 (波数: " + NPC.waveNumber + ")");
            }
            else
            {
                ImGui.Text("没有进行中的入侵");
            }
        }
        ImGui.End();
    }
    #endregion

    #region 定位传送UI
    public static bool TP = false;
    public static Vector4 TPColor = new Vector4(1f, 1f, 1f, 1f);
    public static float TPProgress = 0f;
    public static uint LastTPTime = 0;
    public static bool TPCooldown = false;
    private bool ShowNPCTeleportWindow = false; // 显示NPC传送窗口
    private bool ShowDeathTeleportWindow = false; // 显示死亡地点选择窗口
    private bool ShowCustomTeleportWindow = false; // 显示自定义传送点窗口
    private void DrawTeleportUI(Player plr)
    {
        // 状态显示
        if (TP)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, TPColor);
            ImGui.PopStyleColor();

            // 进度条
            ImGui.ProgressBar(TPProgress, new Vector2(ImGui.GetContentRegionAvail().X, 20));

            // 冷却提示
            if (TPCooldown)
            {
                int cooldown = Math.Max(0, 3 - (int)((Main.GameUpdateCount - LastTPTime) / 60f));
                ImGui.TextColored(new Vector4(1f, 0.5f, 0.5f, 1f), $"传送冷却中: {cooldown}秒");
            }
        }
        else
        {
            ImGui.Text("选择传送目的地:");

            // 位置信息
            ImGui.SameLine();
            ImGui.Text("当前位置:");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1f, 1f, 0.5f, 1f), $"{(int)plr.position.X / 16}, {(int)plr.position.Y / 16}");
        }

        // 按钮区域
        float Width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X * 7) / 8f;

        // 出生点按钮
        if (ImGui.Button("出生点", new Vector2(Width, 40)))
        {
            Utils.TPSpawnPoint(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到世界出生点");

        // 床按钮
        ImGui.SameLine();
        if (ImGui.Button("床", new Vector2(Width, 40)))
        {
            Utils.TPBed(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到床的位置");

        // 死亡地点按钮（修改为打开选择窗口）
        ImGui.SameLine();
        if (DeathPositions.Count > 0)
        {
            if (ImGui.Button("死亡", new Vector2(Width, 40)))
            {
                ShowDeathTeleportWindow = true;
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"传送到死亡地点 ({DeathPositions.Count}个记录)");
        }
        else
        {
            // 没有死亡位置时，按钮不可用
            ImGui.BeginDisabled();
            ImGui.Button("死亡", new Vector2(Width, 40));
            ImGui.EndDisabled();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("没有死亡记录");
        }

        // 自定义按钮
        ImGui.SameLine();
        if (ImGui.Button("自定义", new Vector2(Width, 40)))
        {
            ShowCustomTeleportWindow = !ShowCustomTeleportWindow;
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("管理自定义传送点");

        // NPC按钮
        ImGui.SameLine();
        if (ImGui.Button("NPC", new Vector2(Width, 40)))
        {
            ShowNPCTeleportWindow = !ShowNPCTeleportWindow;
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到活跃NPC位置,排除傀儡与雕像怪,排列优先级:城镇npc→boss→其他怪");

        ImGui.Spacing(); //换行

        // 宝藏袋按钮
        if (ImGui.Button("宝藏袋", new Vector2(Width, 40)))
        {
            Utils.TPBossBag(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到最近的宝藏袋位置");

        // 微光湖按钮
        ImGui.SameLine();
        if (ImGui.Button("微光湖", new Vector2(Width, 40)))
        {
            Utils.TPShimmerLake(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到最近的微光湖");

        // 神庙按钮
        ImGui.SameLine();
        if (ImGui.Button("神庙", new Vector2(Width, 40)))
        {
            Utils.TPJungleTemple(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到丛林神庙入口");

        ImGui.SameLine();

        // 花苞按钮
        if (ImGui.Button("花苞", new Vector2(Width, 40)))
        {
            Utils.TPPlanteraBulb(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到最近的世纪之花苞");

        ImGui.SameLine();

        // 地牢按钮
        if (ImGui.Button("地牢", new Vector2(Width, 40)))
        {
            Utils.TPDungeon(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到地牢入口");

        ImGui.SameLine();

        // 陨石按钮
        if (ImGui.Button("陨石", new Vector2(Width, 40)))
        {
            Utils.TPMeteor(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到陨石附近");
    }
    #endregion

    #region 死亡地点选择窗口
    public static List<Vector2> DeathPositions = new List<Vector2>(); // 存储多个死亡位置
    private void DrawDeathTeleportWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(400, 400), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("选择死亡地点", ref ShowDeathTeleportWindow, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text($"已记录 {DeathPositions.Count} 个死亡地点");
            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                DeathPositions.Clear();
                ClientLoader.Chat.WriteLine("已清空所有死亡地点记录", Color.Yellow);
            }
            ImGui.Separator();


            var reversedPositions = new List<Vector2>(DeathPositions);
            reversedPositions.Reverse();

            for (int i = 0; i < reversedPositions.Count; i++)
            {
                Vector2 pos = reversedPositions[i];
                int x = (int)pos.X / 16;
                int y = (int)pos.Y / 16;

                if (ImGui.Button($"死亡地点 {i + 1} ({x}, {y})##{i}"))
                {
                    Utils.TPDeathPoint(plr, pos);
                    ShowDeathTeleportWindow = false;
                }

                ImGui.SameLine();

                if (ImGui.Button($"删除##del{i}"))
                {
                    int originalIndex = DeathPositions.Count - 1 - i;
                    DeathPositions.RemoveAt(originalIndex);
                    break; // 修改集合后立即跳出
                }
            }
        }
        ImGui.End();
    }
    #endregion

    #region 自定义传送点窗口
    private string CustomPointSearch = ""; // 自定义点搜索文本
    public static string NewPointName = ""; // 新传送点名称
    private void DrawCustomTeleportWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(450, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("自定义传送点管理", ref ShowCustomTeleportWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 添加新传送点区域
            ImGui.Text("添加新传送点:");
            ImGui.Separator();

            // 名称输入
            ImGui.Text("名称:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(80);
            ImGui.InputText("##NewPointName", ref NewPointName, 100);

            // 位置显示
            ImGui.SameLine();
            ImGui.Text("位置:");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1f, 1f, 0.5f, 1f), $"{(int)plr.position.X / 16}, {(int)plr.position.Y / 16}");

            // 添加按钮
            ImGui.SameLine();
            if (ImGui.Button("添加传送点"))
            {
                if (string.IsNullOrWhiteSpace(NewPointName))
                {
                    ClientLoader.Chat.WriteLine("传送点名称不能为空!", Color.Red);
                }
                else
                {
                    Utils.AddCustomPoint();
                }
            }

            // 搜索区域
            ImGui.Separator();
            ImGui.Text("搜索传送点:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##CustomPointSearch", "输入名称搜索", ref CustomPointSearch, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                CustomPointSearch = "";
            }

            // 获取过滤后的传送点
            var filteredPoints = Config.CustomTeleportPoints
                .Where(p => string.IsNullOrWhiteSpace(CustomPointSearch) ||
                            p.Key.Contains(CustomPointSearch, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Key)
                .ToList();

            // 显示传送点数量
            ImGui.Text($"找到 {filteredPoints.Count} 个自定义传送点");

            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                Config.CustomTeleportPoints.Clear();
                Config.Write();
                ClientLoader.Chat.WriteLine("已清空所有传送点", Color.Yellow);
            }

            // 传送点列表
            ImGui.BeginChild("CustomPointsList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            if (filteredPoints.Count > 0)
            {
                // 使用表格布局
                ImGui.Columns(4, "custom_point_columns", true);
                ImGui.SetColumnWidth(0, 150); // 名称
                ImGui.SetColumnWidth(1, 100); // X坐标
                ImGui.SetColumnWidth(2, 100); // Y坐标
                ImGui.SetColumnWidth(3, 120); // 操作

                // 表头
                ImGui.Text("名称"); ImGui.NextColumn();
                ImGui.Text("X坐标"); ImGui.NextColumn();
                ImGui.Text("Y坐标"); ImGui.NextColumn();
                ImGui.Text("操作"); ImGui.NextColumn();
                ImGui.Separator();

                foreach (var point in filteredPoints)
                {
                    ImGui.Text(point.Key); ImGui.NextColumn();
                    ImGui.Text($"{(int)point.Value.X / 16}"); ImGui.NextColumn();
                    ImGui.Text($"{(int)point.Value.Y / 16}"); ImGui.NextColumn();

                    // 操作按钮
                    if (ImGui.Button($"传送##{point.Key}"))
                    {
                        Utils.TPCustomPoint(plr, point.Value, point.Key);
                    }

                    ImGui.SameLine();

                    if (ImGui.Button($"删除##{point.Key}"))
                    {
                        Config.CustomTeleportPoints.Remove(point.Key);
                        Config.Write();
                        ClientLoader.Chat.WriteLine($"已删除传送点: {point.Key}", Color.Yellow);
                    }

                    ImGui.NextColumn();
                }

                ImGui.Columns(1);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(CustomPointSearch))
                {
                    ImGui.Text("没有自定义传送点，请添加新的传送点");
                }
                else
                {
                    ImGui.Text($"没有找到包含 '{CustomPointSearch}' 的传送点");
                }
            }

            ImGui.EndChild();
        }
        ImGui.End();
    }
    #endregion

    #region NPC传送窗口
    private void DrawNPCTeleportWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(500, 600), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("传送到NPC", ref ShowNPCTeleportWindow, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("选择要传送的NPC:");
            ImGui.Separator();

            // 搜索框
            string npcSearch = "";
            ImGui.InputTextWithHint("##NPCSearch", "输入NPC名称搜索", ref npcSearch, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                npcSearch = "";
            }

            // 获取所有NPC
            var newNPCs = new List<NPC>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.SpawnedFromStatue && npc.type != 488)
                {
                    string npcName = Lang.GetNPCNameValue(npc.type);
                    if (string.IsNullOrWhiteSpace(npcSearch) ||
                        npcName.Contains(npcSearch, StringComparison.OrdinalIgnoreCase))
                    {
                        newNPCs.Add(npc);
                    }
                }
            }

            // 按NPC类型分组（显示唯一的NPC类型）
            var groupedNPCs = newNPCs
                .GroupBy(n => n.type)
                .Select(g => g.First())
                .ToList();

            // 将NPC分为三类：城镇NPC、BOSS、其他NPC
            var townNPCs = new List<NPC>();
            var bossNPCs = new List<NPC>();
            var otherNPCs = new List<NPC>();

            foreach (var npc in groupedNPCs)
            {
                string npcName = Lang.GetNPCNameValue(npc.type);

                if (npc.townNPC)
                {
                    townNPCs.Add(npc);
                }
                else if (npc.boss || npcName.Contains("boss", StringComparison.OrdinalIgnoreCase))
                {
                    bossNPCs.Add(npc);
                }
                else
                {
                    otherNPCs.Add(npc);
                }
            }

            // 每类按名称排序
            townNPCs = townNPCs.OrderBy(n => Lang.GetNPCNameValue(n.type)).ToList();
            bossNPCs = bossNPCs.OrderBy(n => Lang.GetNPCNameValue(n.type)).ToList();
            otherNPCs = otherNPCs.OrderBy(n => Lang.GetNPCNameValue(n.type)).ToList();

            // 合并列表（城镇NPC → BOSS → 其他NPC）
            var allNpc = new List<NPC>();
            allNpc.AddRange(townNPCs);
            allNpc.AddRange(bossNPCs);
            allNpc.AddRange(otherNPCs);

            // 显示NPC数量信息（包括分类统计）
            ImGui.Text($"找到 {allNpc.Count} 个NPC (城镇:{townNPCs.Count} BOSS:{bossNPCs.Count} 其他:{otherNPCs.Count})");

            // 使用网格布局
            ImGui.BeginChild("NPCList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            // 动态计算列数 - 根据窗口宽度
            float windowWidth = ImGui.GetContentRegionAvail().X;
            int columns = (int)Math.Max(1, Math.Floor(windowWidth / 120)); // 每列至少120px宽

            int count = allNpc.Count;

            for (int i = 0; i < count; i++)
            {
                if (i % columns != 0)
                    ImGui.SameLine();

                NPC npc = allNpc[i];
                string npcName = Lang.GetNPCNameValue(npc.type);
                string displayName = npcName;

                // 名称超过5个字时截断并添加省略号
                if (npcName.Length > 5)
                {
                    displayName = npcName.Substring(0, 5) + "...";
                }

                // 设置不同类别按钮颜色
                Vector4 buttonColor;
                string category;

                if (npc.townNPC)
                {
                    buttonColor = new Vector4(0.2f, 0.7f, 0.2f, 0.5f); // 城镇NPC - 绿色
                    category = "城镇NPC";
                }
                else if (npc.boss || npcName.Contains("boss", StringComparison.OrdinalIgnoreCase))
                {
                    buttonColor = new Vector4(0.8f, 0.2f, 0.2f, 0.5f); // BOSS - 红色
                    category = "BOSS";
                }
                else
                {
                    buttonColor = new Vector4(0.2f, 0.5f, 0.8f, 0.5f); // 其他NPC - 蓝色
                    category = "其他NPC";
                }

                ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);

                // 固定按钮高度
                Vector2 buttonSize = new Vector2(ImGui.GetContentRegionAvail().X / columns - 5, 30);

                // 创建按钮
                if (ImGui.Button($"{displayName}##{npc.type}", buttonSize))
                {
                    Utils.TPNPC(plr, npc.type);
                }

                ImGui.PopStyleColor();

                // 悬停显示NPC完整信息
                if (ImGui.IsItemHovered())
                {
                    Vector2 pos = npc.position / 16f;

                    // 如果显示的是缩写名称，在工具提示中显示完整名称
                    if (displayName != npcName)
                    {
                        ImGui.SetTooltip($"名称: {npcName}\n位置: {pos.X:F0}, {pos.Y:F0}\n类别: {category}");
                    }
                    else
                    {
                        ImGui.SetTooltip($"位置: {pos.X:F0}, {pos.Y:F0}\n类别: {category}");
                    }
                }
            }

            ImGui.EndChild();
        }
        ImGui.End();
    }
    #endregion

    #region 自动垃圾桶管理窗口
    private static bool ShowAutoTrashWindow = false; // 显示自动垃圾桶窗口
    private static string TrashSearchInput = ""; // 垃圾桶表搜索输入
    private static string ExclusionSearchInput = ""; // 排除表搜索输入
    private static int AutoTrashSyncInterval = Config.TrashSyncInterval; // 自动回收同步间隔
    private Dictionary<int, int> ReturnAmounts = new Dictionary<int, int>(); // 临时存储需要返还的物品数量
    private int? WaitExcludeType = null; // 待处理的排除物品ID
    private bool ShowExclusionWindows = false; // 是否显示排除弹窗
    private int TryExcludeTime = 60; // 临时排除时间（秒）
    private static int CustomTime = 60; // 默认排除时间（秒）
    private bool ReturnAfterExclusion = false; // 是否在设置排除后执行返还
    private Dictionary<int, int> AmountCache = new Dictionary<int, int>(); // 缓存返还数量
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
            ImGui.Text("回收间隔(帧):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.SliderInt("##AutoTrashInterval", ref AutoTrashSyncInterval, 1, 600, "%d fps");
            Config.TrashSyncInterval = AutoTrashSyncInterval;

            // 默认排除时间设置
            ImGui.Text("默认排除时间(秒):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.SliderInt("##DefaultExcludeTime", ref CustomTime, 1, 600, "%d 秒");

            // 自动垃圾桶物品列表
            ImGui.Separator();
            ImGui.TextColored(new Vector4(1, 0.5f, 0.5f, 1), "《自动垃圾桶表》");

            // 搜索区域
            ImGui.Text("搜索物品:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##TrashSearchInput", "输入名称或ID", ref TrashSearchInput, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                TrashSearchInput = "";
            }

            // 获取所有垃圾桶物品并应用搜索过滤
            var trashItems = data.TrashList
                .Select(item => new
                {
                    Id = item.Key,
                    Name = Lang.GetItemNameValue(item.Key) ?? $"未知物品 ({item.Key})",
                    Amount = item.Value
                })
                .Where(item =>
                    string.IsNullOrWhiteSpace(TrashSearchInput) ||
                    item.Name.Contains(TrashSearchInput, StringComparison.OrdinalIgnoreCase) ||
                    item.Id.ToString().Contains(TrashSearchInput)
                )
                .ToList();

            // 显示物品数量信息 + 清空按钮
            ImGui.Text($"垃圾桶物品 (共 {trashItems.Count} 个物品)");
            ImGui.SameLine();

            // 添加全部返还按钮
            if (ImGui.Button("全部返还"))
            {
                if (trashItems.Count > 0)
                {
                    int totalItemsReturned = 0;
                    int totalTypesReturned = 0;

                    foreach (var item in trashItems)
                    {
                        // 使用 GiveItem 方法返还物品
                        Utils.GiveItem(plr, item.Id, item.Amount);
                        totalItemsReturned += item.Amount;
                        totalTypesReturned++;
                    }

                    // 清空垃圾桶列表
                    data.TrashList.Clear();
                    Config.Write();

                    ClientLoader.Chat.WriteLine($"已返还全部 {totalTypesReturned} 种物品，共 {totalItemsReturned} 个物品", Color.Yellow);
                }
            }

            ImGui.SameLine();

            // 垃圾桶清空按钮
            if (ImGui.Button("清空垃圾桶表"))
            {
                data.TrashList.Clear();
                Config.Write();
                ClientLoader.Chat.WriteLine("已清空自动垃圾桶表", Color.Yellow);
            }

            // 当前物品列表
            ImGui.BeginChild("TrashItemsList", new Vector2(0, 180), ImGuiChildFlags.Borders);

            // 使用索引显示所有物品
            for (int i = 0; i < trashItems.Count; i++)
            {
                var item = trashItems[i];
                string displayName = $"{i + 1}. {item.Name}"; // 添加连续索引前缀

                ImGui.PushID($"trash_{item.Id}");

                // 使用紧凑的5列布局
                ImGui.Columns(5, "trash_item_columns", false);
                ImGui.SetColumnWidth(0, 150); // 物品名称
                ImGui.SetColumnWidth(1, 80);  // 物品ID
                ImGui.SetColumnWidth(2, 120); // 返还数量滑块
                ImGui.SetColumnWidth(3, 100); // 按钮区域
                ImGui.SetColumnWidth(4, 120); // 临时排除时间（新增列）

                // 物品名称
                ImGui.Text(displayName);
                // 添加悬停提示
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    Item tempItem = new Item();
                    tempItem.SetDefaults(item.Id);
                    tempItem.stack = item.Amount;
                    TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
                }
                ImGui.NextColumn();

                // 物品ID
                ImGui.Text($"ID:{item.Id}");
                ImGui.NextColumn();

                // 初始化返还数量（默认为1）
                if (!ReturnAmounts.ContainsKey(item.Id))
                {
                    ReturnAmounts[item.Id] = Math.Max(1, item.Amount / 2); // 默认取一半数量
                }

                // 返还数量滑块
                int currentAmount = ReturnAmounts[item.Id];
                ImGui.SetNextItemWidth(110);
                ImGui.SliderInt($"##return_{item.Id}", ref currentAmount, 1, item.Amount, $"{currentAmount}/{item.Amount}");
                ReturnAmounts[item.Id] = currentAmount;
                ImGui.NextColumn();

                // 单个物品的返还按钮处理
                if (ImGui.Button("返还", new Vector2(40, 0)))
                {
                    // 首先检查物品是否还在垃圾桶中
                    if (!data.TrashList.ContainsKey(item.Id))
                    {
                        // 如果物品已不存在于垃圾桶中，重置所有相关状态
                        ClientLoader.Chat.WriteLine($"物品 [c/4C92D8:{item.Name}] 已不存在于垃圾桶中", Color.Yellow);

                        // 重置所有临时状态变量
                        if (WaitExcludeType == item.Id)
                        {
                            WaitExcludeType = 0;
                            ReturnAfterExclusion = false;
                            ShowExclusionWindows = false;
                        }
                        return;
                    }

                    // 检查物品是否在临时排除期内
                    if (Utils.AdventExcluded(item.Id))
                    {
                        string timeLeft = Utils.GetAdventTime(item.Id);
                        ClientLoader.Chat.WriteLine($"物品 [c/4C92D8:{item.Name}] 已被临时排除，剩余时间: {timeLeft}。正在返还...", Color.Yellow);
                        ExecuteReturn(plr, data, item.Id, item.Amount, currentAmount);
                    }
                    // 检查物品是否在排除表中
                    else if (data.ExcluItem.Contains(item.Id))
                    {
                        // 永久排除，直接返还
                        ExecuteReturn(plr, data, item.Id, item.Amount, currentAmount);
                    }
                    else
                    {
                        // 缓存返还数量
                        AmountCache[item.Id] = currentAmount;

                        // 如果不在排除表中，设置待处理状态
                        WaitExcludeType = item.Id;
                        TryExcludeTime = CustomTime; // 使用自定义默认时间
                        ReturnAfterExclusion = true; // 标记需要执行返还
                        ShowExclusionWindows = true;
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("删除", new Vector2(50, 0)))
                {
                    ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{item.Name}] 从自动垃圾桶删除", color);
                    data.TrashList.Remove(item.Id);
                    Config.Write();

                    // 如果删除的是等待排除的物品，重置状态
                    if (WaitExcludeType == item.Id)
                    {
                        WaitExcludeType = null;
                        ShowExclusionWindows = false;
                        ReturnAfterExclusion = false;
                    }

                    // 清除缓存
                    AmountCache.Remove(item.Id);
                    ReturnAmounts.Remove(item.Id);
                }

                ImGui.NextColumn();

                // 临时排除时间显示
                if (Utils.AdventExclusions != null)
                {
                    if (Utils.AdventExclusions.ContainsKey(item.Id) && Utils.AdventExclusions[item.Id] > DateTime.Now)
                    {
                        TimeSpan remaining = Utils.AdventExclusions[item.Id] - DateTime.Now;
                        int secondsLeft = (int)remaining.TotalSeconds;
                        ImGui.TextColored(new Vector4(1, 1, 0.5f, 1), $"剩余: {secondsLeft}秒");
                    }
                    else
                    {
                        ImGui.Text(""); // 空文本保持对齐
                    }
                }

                ImGui.Columns(1);
                ImGui.PopID();
            }

            // 如果没有物品显示提示
            if (trashItems.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(TrashSearchInput))
                {
                    ImGui.Text("垃圾桶列表为空,请将物品放入垃圾桶格子");
                }
                else
                {
                    ImGui.Text($"没有找到包含 '{TrashSearchInput}' 的物品");
                }
            }

            ImGui.EndChild();

            //排除表窗口
            ExclusionTableWindows(data);

            // 添加手持物品按钮
            if (ImGui.Button("添加手持物品到垃圾桶"))
            {
                if (!plr.HeldItem.IsAir)
                {
                    int itemId = plr.HeldItem.type;
                    if (!data.TrashList.ContainsKey(itemId))
                    {
                        string itemName = Lang.GetItemNameValue(itemId) ?? $"未知物品 ({itemId})";
                        ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{itemName}] 添加到自动垃圾桶", color);
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
                        string itemName = Lang.GetItemNameValue(itemId) ?? $"未知物品 ({itemId})";
                        ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{itemName}] 添加到排除表", color);
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

        // 处理排除提示弹窗
        if (ShowExclusionWindows)
        {
            ExclusionWindows(plr);
        }
    }
    #endregion

    #region 垃圾桶排除表窗口
    private static void ExclusionTableWindows(TrashData? data)
    {
        if (data is null) return;

        // 排除物品列表
        ImGui.Separator();
        ImGui.TextColored(new Vector4(0.5f, 1, 0.5f, 1), "《排除物品表》");

        // 搜索区域
        ImGui.Text("搜索物品:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        ImGui.InputTextWithHint("##ExclusionSearchInput", "输入名称或ID", ref ExclusionSearchInput, 100);
        ImGui.SameLine();
        if (ImGui.Button("清空搜索##Exclusion"))
        {
            ExclusionSearchInput = "";
        }

        // 获取所有排除物品并应用搜索过滤
        var excluItems = data.ExcluItem
            .Select(id => new
            {
                Id = id,
                Name = Lang.GetItemNameValue(id) ?? $"未知物品 ({id})"
            })
            .Where(item =>
                string.IsNullOrWhiteSpace(ExclusionSearchInput) ||
                item.Name.Contains(ExclusionSearchInput, StringComparison.OrdinalIgnoreCase) ||
                item.Id.ToString().Contains(ExclusionSearchInput)
            )
            .ToList();

        // 显示物品数量信息 + 清空按钮
        ImGui.Text($"排除物品 (共 {excluItems.Count} 个物品)");
        ImGui.SameLine();
        // 添加排除表清空按钮
        if (ImGui.Button("清空排除表"))
        {
            data.ExcluItem.Clear();
            // 添加默认排除的钱币ID
            data.ExcluItem = new HashSet<int>() { 71, 72, 73, 74 };
            Config.Write();
            ClientLoader.Chat.WriteLine("已清空排除表并默认排除钱币", Color.Yellow);
        }

        // 当前排除列表
        ImGui.BeginChild("物品排除表", new Vector2(0, 180), ImGuiChildFlags.Borders);

        // 使用索引显示所有排除物品
        for (int i = 0; i < excluItems.Count; i++)
        {
            var item = excluItems[i];
            string displayName = $"{i + 1}. {item.Name}"; // 添加连续索引前缀

            ImGui.PushID($"exclu_{item.Id}");

            // 使用紧凑的3列布局
            ImGui.Columns(3, "exclusion_item_columns", false);
            ImGui.SetColumnWidth(0, 150); // 物品名称
            ImGui.SetColumnWidth(1, 80);  // 物品ID
            ImGui.SetColumnWidth(2, 120); // 删除按钮

            // 物品名称
            ImGui.Text(displayName);
            // 添加悬停提示
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
            {
                Item tempItem = new Item();
                tempItem.SetDefaults(item.Id);
                TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
            }
            ImGui.NextColumn();

            // 物品ID
            ImGui.Text($"ID:{item.Id}");
            ImGui.NextColumn();

            // 删除按钮
            if (ImGui.Button("删除", new Vector2(50, 0)))
            {
                ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{item.Name}] 从排除表中删除", color);
                data.ExcluItem.Remove(item.Id);
                Config.Write();
            }

            ImGui.Columns(1);
            ImGui.PopID();
        }

        // 如果没有物品显示提示
        if (excluItems.Count == 0)
        {
            if (string.IsNullOrWhiteSpace(ExclusionSearchInput))
            {
                ImGui.Text("排除列表为空");
            }
            else
            {
                ImGui.Text($"没有找到包含 '{ExclusionSearchInput}' 的排除物品");
            }
        }

        ImGui.EndChild();
    }
    #endregion

    #region 临时排除窗口
    private void ExclusionWindows(Player plr)
    {
        if (!WaitExcludeType.HasValue) return;

        int itemId = WaitExcludeType.Value;
        string itemName = Lang.GetItemNameValue(itemId);
        if (string.IsNullOrEmpty(itemName)) itemName = $"未知物品 ({itemId})";

        ImGui.OpenPopup("添加排除");
        if (ImGui.BeginPopupModal("添加排除", ref ShowExclusionWindows, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var data = Config.TrashItems.FirstOrDefault(x => x.Name == plr.name);
            if (data == null)
            {
                return;
            }

            ImGui.Text($"物品 '{itemName}' 不在排除表中，请选择操作：");

            // 时间输入框
            ImGui.Text("排除时间(秒):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##ExcludeTime", ref TryExcludeTime);
            if (TryExcludeTime < 1) TryExcludeTime = 1;
            if (TryExcludeTime > 3600) TryExcludeTime = 3600; // 限制最大1小时

            ImGui.Spacing();

            // 按钮行
            if (ImGui.Button("加入排除表"))
            {
                if (!data.ExcluItem.Contains(itemId))
                {
                    data.ExcluItem.Add(itemId);
                    Config.Write();
                    ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{itemName}] 添加到排除表", Color.Green);
                }

                // 如果需要返还，执行返还操作
                if (ReturnAfterExclusion)
                {
                    // 使用缓存的返还数量
                    int cachedAmount = AmountCache.TryGetValue(itemId, out var amt) ? amt : 1;
                    ExecuteReturn(plr, data, itemId, data.TrashList[itemId], cachedAmount);
                    ReturnAfterExclusion = false;
                }
            }

            ImGui.SameLine();

            if (ImGui.Button($"临时排除({TryExcludeTime}秒)"))
            {
                Utils.AdventExclusions[itemId] = DateTime.Now.AddSeconds(TryExcludeTime);
                ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{itemName}] 临时排除{TryExcludeTime}秒", Color.Yellow);

                // 如果需要返还，执行返还操作
                if (ReturnAfterExclusion)
                {
                    // 使用缓存的返还数量
                    int cachedAmount = AmountCache.TryGetValue(itemId, out var amt) ? amt : 1;
                    ExecuteReturn(plr, data, itemId, data.TrashList[itemId], cachedAmount);
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("取消"))
            {
                // 取消时重置返还标志
                ReturnAfterExclusion = false;
                ShowExclusionWindows = false;
                WaitExcludeType = null;
                AmountCache.Remove(itemId); // 清除该物品的缓存
            }

            ImGui.EndPopup();
        }
    }
    #endregion

    #region 执行返还操作的方法 (使用GiveItem)
    private void ExecuteReturn(Player plr, TrashData data, int itemKey, int itemValue, int currentAmount)
    {
        int returnAmount = Math.Min(currentAmount, itemValue);

        // 直接使用GiveItem方法返还物品
        Utils.GiveItem(plr, itemKey, returnAmount);

        // 更新垃圾桶中的物品数量
        int newAmount = itemValue - returnAmount;
        if (newAmount <= 0)
        {
            data.TrashList.Remove(itemKey);
            ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{Lang.GetItemNameValue(itemKey)}] 从[c/4C92D8:自动垃圾桶]移除", color);
        }
        else
        {
            data.TrashList[itemKey] = newAmount;
        }

        Config.Write();

        // 返还完成后重置相关状态
        if (WaitExcludeType == itemKey)
        {
            WaitExcludeType = null;
            ReturnAfterExclusion = false;
        }

        // 清除缓存
        AmountCache.Remove(itemKey);
        ReturnAmounts.Remove(itemKey);

        // 如果弹窗显示的是当前物品，关闭弹窗
        if (ShowExclusionWindows && WaitExcludeType == itemKey)
        {
            ShowExclusionWindows = false;
        }
    }
    #endregion

    #region 物品编辑管理器窗口
    private static bool ShowItemManagerWindow = false; // 显示物品管理器窗口
    private static string SearchFilter = ""; // 物品搜索过滤器
    internal void DrawItemManagerWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(600, 450), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("物品编辑管理器", ref ShowItemManagerWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 搜索框和按钮
            ImGui.InputTextWithHint("##SearchFilter", "输入名称或ID搜索", ref SearchFilter, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                SearchFilter = "";
            }
            ImGui.SameLine();
            DrawKeySelector("应用按键", ref Config.ItemModifyKey, ref EditItemManagerKey);

            // 获取所有物品并应用搜索过滤
            var AllItems = Config.ItemModifyList
                .Select(item => new
                {
                    Data = item,
                    Name = item.Name,
                    Type = item.Type,
                    ItemName = Lang.GetItemNameValue(item.Type) ?? $"未知物品 ({item.Type})"
                })
                .Where(item =>
                    string.IsNullOrWhiteSpace(SearchFilter) ||
                    item.Name.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                    item.ItemName.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                    item.Type.ToString().Contains(SearchFilter)
                )
                .ToList();

            // 计算总页数
            AllPages = (int)Math.Ceiling(AllItems.Count / (float)PageLimit);
            if (AllPages == 0) AllPages = 1;

            // 确保当前页在有效范围内
            if (NowPage >= AllPages) NowPage = AllPages - 1;
            if (NowPage < 0) NowPage = 0;

            // 按钮区域
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

                    // 清空搜索以便显示新添加的物品
                    SearchFilter = "";
                }
                else
                {
                    ClientLoader.Chat.WriteLine($"请手持一个物品 使用{Config.ItemModifyKey}", Color.Red);
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                ShowEditWindow = false;
                Config.ItemModifyList.Clear();
                Config.Write();
                SearchFilter = ""; // 清空搜索
            }

            ImGui.SameLine();
            ListPage(AllItems.Count); // 分页

            // 显示搜索结果信息
            if (!string.IsNullOrWhiteSpace(SearchFilter))
            {
                ImGui.TextColored(new Vector4(1, 1, 0.5f, 1), $"找到 {AllItems.Count} 个匹配项");
            }

            // 物品列表
            ImGui.Separator();
            ImGui.BeginChild("物品列表", new Vector2(0, 250), ImGuiChildFlags.Borders);

            // 获取当前页的物品
            var GetPage = AllItems.Skip(NowPage * PageLimit).Take(PageLimit).ToList();

            // 表头
            ImGui.Columns(5, "item_columns", false);
            ImGui.SetColumnWidth(0, 40);   // 索引
            ImGui.SetColumnWidth(1, 150);  // 预设名称
            ImGui.SetColumnWidth(2, 150);  // 物品名称
            ImGui.SetColumnWidth(3, 70);   // 物品ID
            ImGui.SetColumnWidth(4, 150);  // 操作按钮

            ImGui.Text("#"); ImGui.NextColumn();
            ImGui.Text("预设名称"); ImGui.NextColumn();
            ImGui.Text("物品名称"); ImGui.NextColumn();
            ImGui.Text("物品ID"); ImGui.NextColumn();
            ImGui.Text("操作"); ImGui.NextColumn();

            ImGui.Separator();
            ImGui.Columns(1);

            // 物品行
            for (int i = 0; i < GetPage.Count; i++)
            {
                var itemInfo = GetPage[i];
                var data = itemInfo.Data;

                ImGui.PushID(data.Name);

                // 使用5列布局
                ImGui.Columns(5, "item_columns", false);
                ImGui.SetColumnWidth(0, 40);   // 索引
                ImGui.SetColumnWidth(1, 150);  // 预设名称
                ImGui.SetColumnWidth(2, 150);  // 物品名称
                ImGui.SetColumnWidth(3, 70);   // 物品ID
                ImGui.SetColumnWidth(4, 150);  // 操作按钮

                // 索引号 (当前页的序号)
                ImGui.Text($"{NowPage * PageLimit + i + 1}");
                ImGui.NextColumn();

                // 预设名称
                ImGui.Text($"{data.Name}");
                ImGui.NextColumn();

                // 物品名称
                ImGui.Text($"{itemInfo.ItemName}");
                ImGui.NextColumn();

                // 物品ID
                ImGui.Text($"{data.Type}");
                ImGui.NextColumn();

                // 操作按钮
                if (ImGui.Button($"应用##{i}"))
                {
                    data.ApplyTo(Main.player[Main.myPlayer].HeldItem);
                    ClientLoader.Chat.WriteLine($"已应用物品预设: {data.Name}", Color.Green);
                }
                ImGui.SameLine();
                if (ImGui.Button($"编辑##{i}"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    EditItem = data;
                    ShowEditWindow = true;
                }
                ImGui.SameLine();
                if (ImGui.Button($"删除##{i}"))
                {
                    SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    ShowEditWindow = false;
                    Config.ItemModifyList.Remove(data);
                    Config.Write();
                    ClientLoader.Chat.WriteLine($"已删除物品预设: {data.Name}", Color.Yellow);
                }
                ImGui.NextColumn();

                ImGui.Columns(1);
                ImGui.PopID();

                // 悬停区域显示工具提示
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    Item tempItem = new Item();
                    tempItem.SetDefaults(data.Type);
                    data.ApplyTo(tempItem);
                    TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
                }
            }

            // 如果没有物品显示提示
            if (GetPage.Count == 0)
            {
                ImGui.Text("");
                if (string.IsNullOrWhiteSpace(SearchFilter))
                {
                    ImGui.Text($"没有物品预设 请使用Alt + {Config.ItemModifyKey} 添加");
                }
                else
                {
                    ImGui.Text($"没有找到包含 '{SearchFilter}' 的物品预设");
                }
            }

            ImGui.EndChild();
        }
        ImGui.End();

        // 显示物品编辑窗口
        if (ShowEditWindow && EditItem != null)
        {
            DrawItemEditWindow(plr);
        }
    }
    #endregion

    #region 分页与跳转功能
    private static int NowPage = 0; // 当前页码
    private const int PageLimit = 8; // 每页显示8个物品
    private static int AllPages = 0; // 总页数
    private static void ListPage(int totalItems)
    {
        // 显示分页信息和控件
        ImGui.Text($"第 {NowPage + 1} / {AllPages} 页 (共 {totalItems} 个物品)");
        ImGui.SameLine();

        // 上一页按钮
        if (ImGui.Button("上页") && NowPage > 0)
        {
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);
            NowPage--;
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
    public static int PrefixId = 0; // 用于存储新前缀ID的变量
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
                Utils.ApplyPrefix();
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
}