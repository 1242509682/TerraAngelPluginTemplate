using Microsoft.Xna.Framework;
using System.Reflection;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Plugin;
using TerraAngel.Tools;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace MyPlugin;

public class MyPlugin(string path) : Plugin(path)
{
    #region 插件信息
    public override string Name => typeof(MyPlugin).Namespace!;
    public string Author => "羽学";
    public Version Version => new(1, 1, 1);
    #endregion

    #region 注册与卸载
    public override void Load()
    {
        // 加载世界事件
        WorldGen.Hooks.OnWorldLoad += OnWorldLoad;

        // 注册配方事件
        RecipeHooks.Register();
        RecipeHooks.OnRecipeCheck += OnRecipeCheck; // 配方检查事件
        RecipeHooks.OnPostFindRecipes += OnPostFindRecipes; // 配方查找后事件

        // 注册图格编辑事件
        TileEditEventSystem.Register();
        TileEditEventSystem.OnTileKill += OnTileEdit;

        // 注册NPC更新事件
        NPCEventSystem.Register();
        NPCEventSystem.OnNPCUpdate += OnUpdateNPC;

        // 添加额外饰品的Mono注册钩子
        ExtraAccessory.Register();

        // 添加反重力药水的Mono钩子
        IgnoreGravity.Register();

        // 读取配置文件
        ReloadConfig(null!);

        // 注册UI
        ToolManager.AddTool<UITool>();

        // 向控制台添加命令
        ClientLoader.Console.AddCommand("reload", ReloadConfig, "重载配置文件");
        ClientLoader.Console.AddCommand("kill", x => Commands.KillPlayer(true), "按K键自杀与复活");
        ClientLoader.Console.AddCommand("heal", Commands.AutoHeal, "按H键强制回血");
        ClientLoader.Console.AddCommand("snpc", Commands.MouseStrikeNPC, "使用物品时伤害鼠标附近怪物");
        ClientLoader.Console.AddCommand("autouse", Commands.AutoUse, "切换自动使用物品功能");

        // 初始化完成提示
        ClientLoader.Console.WriteLine($"[{Name}] 插件已加载 (v{Version}) 作者: {Author}");
        ClientLoader.Console.WriteLine($"[{Name}] 配置文件位置: {Configuration.FilePath}");
    }

    public override void Unload()
    {
        // 卸载加载世界事件
        WorldGen.Hooks.OnWorldLoad -= OnWorldLoad;

        // 卸载配方事件
        RecipeHooks.Dispose();
        RecipeHooks.OnRecipeCheck -= OnRecipeCheck;
        RecipeHooks.OnPostFindRecipes -= OnPostFindRecipes;

        //卸载图格编辑事件
        TileEditEventSystem.Dispose();
        TileEditEventSystem.OnTileKill -= OnTileEdit;

        // 卸载NPC更新事件
        NPCEventSystem.Dispose();
        NPCEventSystem.OnNPCUpdate -= OnUpdateNPC;

        // 卸载插件时清理UI
        ToolManager.RemoveTool<UITool>();

        // 卸载额外饰品的Mono注册钩子
        ExtraAccessory.Dispose();

        // 卸载反重力药水的Mono钩子
        IgnoreGravity.Dispose();
    }
    #endregion

    #region 配置管理
    internal static Configuration Config = new();
    public static Color color = new(240, 250, 150);
    private void ReloadConfig(TerraAngel.UI.ClientWindows.Console.ConsoleWindow.CmdStr x)
    {
        Config = Configuration.Read();
        Config.Write();
        ClientLoader.Console.WriteLine($"[{Name}] 配置文件已重载", color);
    }
    #endregion

    #region 游戏更新事件(每帧刷新)
    public override void Update()
    {
        if (!Config.Enabled) return;

        //按H键回血
        Utils.HealLife(InputSystem.IsKeyPressed(Config.HealKey));

        //按K键自杀与复活自己
        Commands.KillPlayer(InputSystem.IsKeyPressed(Config.KillKey));

        //自动使用物品
        Utils.AutoUseItem(InputSystem.IsKeyPressed(Config.AutoUseKey));

        //使用物品时伤害鼠标范围内的NPC
        Utils.UseItemStrikeNPC(Config.MouseStrikeNPC);

        //快捷键I 自动应用修改物品
        Utils.ModifyItem(InputSystem.IsKeyPressed(Config.ItemModifyKey));

        //快捷键P 开启关闭修改批量前缀窗口
        if (InputSystem.IsKeyPressed(Config.ShowEditPrefixKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
            UITool.ShowEditPrefix = !UITool.ShowEditPrefix;
        }

        //快捷键O 快速收藏物品
        if (InputSystem.IsKeyPressed(Config.FavoriteKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Utils.FavoriteAllItems(Main.LocalPlayer);
        }

        // N键切换社交栏饰品生效状态
        if (InputSystem.IsKeyPressed(Config.SocialAccessoriesKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.SocialAccessory = !Config.SocialAccessory;
            Config.Write();
            string status = Config.SocialAccessory ? "开启" : "关闭";
            ClientLoader.Chat.WriteLine($"社交栏饰品功能已{status}", color);
        }

        // 切换重力控制状态
        if (InputSystem.IsKeyPressed(Config.IgnoreGravityKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.IgnoreGravity = !Config.IgnoreGravity;
            Config.Write();
            string status = Config.IgnoreGravity ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"重力控制已{status}", Color.Yellow);
        }

        // 切换自动垃圾桶状态
        if (InputSystem.IsKeyPressed(Config.AutoTrashKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.AutoTrash = !Config.AutoTrash;
            Config.Write();
            string status = Config.AutoTrash ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"自动垃圾桶已{status}", Color.Yellow);
        }

        // 触发自动垃圾桶方法
        Utils.AutoTrash();

        // 更新传送进度
        Utils.UpdateTeleportProgress();

        // 记录死亡坐标
        Utils.RecordDeathPoint(Main.LocalPlayer);

        // 切换清除钓鱼任务状态
        if (InputSystem.IsKeyPressed(Config.ClearQuestsKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.ClearAnglerQuests = !Config.ClearAnglerQuests;
            string status = Config.ClearAnglerQuests ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"清理渔夫任务已{status}", Color.Yellow);
        }

        // 更新清除钓鱼任务状态
        Utils.ClearAnglerQuests(Config.ClearAnglerQuests);

        // 切换NPC自动回血状态
        if (InputSystem.IsKeyPressed(Config.NPCAutoHealKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.NPCAutoHeal = !Config.NPCAutoHeal;
            Config.Write();
            string status = Config.NPCAutoHeal ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"NPC自动回血已{status}", Color.Yellow);
        }

        // 复活城镇NPC
        Utils.Relive(InputSystem.IsKeyPressed(Config.NPCReliveKey));

        // 连锁挖矿
        if (InputSystem.IsKeyPressed(Config.VeinMinerKey))
        {
            Config.VeinMinerEnabled = !Config.VeinMinerEnabled;
            Config.Write();
            string status = Config.VeinMinerEnabled ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"连锁挖矿已{status}", Color.Yellow);
        }
    }
    #endregion

    #region 图格编辑事件
    public static Point[]? TempPoints; // 临时点
    private void OnTileEdit(object? sender, TileKillEventArgs e)
    {
        if (!Config.Enabled) return;

        Utils.VeinMiner(e.X, e.Y); // 连锁挖矿方法
    }
    #endregion

    #region NPC更新事件
    private void OnUpdateNPC(object? sender, NPCUpdateEventArgs e)
    {
        var npc = e.npc;
        // 排除城镇NPC、友好NPC、雕像怪、傀儡
        if (npc == null || !npc.active || !Config.Enabled || npc.townNPC ||
            npc.friendly || npc.SpawnedFromStatue || npc.type == 488) return;

        if (Config.NPCAutoHeal)
        {
            Utils.NPCAutoHeal(npc, e.whoAmI);  // npc自动回血
        }
    }
    #endregion

    #region 加载世界事件
    private void OnWorldLoad()
    {
        var plr = Main.LocalPlayer;

        if (!Config.Enabled || plr is null) return;

        // 进入世界自动收藏背包物品
        if (Config.FavoriteItemForJoinWorld)
        {
            Utils.isFavoriteMode = false;
            int favoritedItems = Utils.FavoriteAllItems(plr);
            ClientLoader.Chat.WriteLine($"已收藏 {favoritedItems} 个物品", Color.Green);
        }
    }
    #endregion

    #region 配方检查事件
    public static HashSet<int> CustomRecipeItems = new HashSet<int>();  // 存储自定义配方结果物品ID
    public static HashSet<int> CustomRecipeIndexes = new HashSet<int>(); // 存储自定义配方索引 用于比较原版
    private void OnRecipeCheck(RecipeCheckEventArgs e)
    {
        if (!Config.Enabled || !Config.CustomRecipesEnabled) return;

        // 隐藏原版配方：如果当前配方结果物品在自定义物品列表中，但不是自定义配方
        if (CustomRecipeItems.Contains(e.Recipe.createItem.type) && !CustomRecipeIndexes.Contains(e.RecipeIndex))
        {
            e.MeetsConditions = false; // 强制不满足条件 只显示自定义配方
            return;
        }

        // 自定义配方处理逻辑 
        if (Config.CustomRecipes.Any(r => r.Index == e.RecipeIndex && e.MeetsStationConditions))
        {
            e.MeetsConditions = RecipeHooks.HasResultItemForRecipe(e.Recipe, e.CollectedItems);
            return;
        }

        // 解锁所有配方
        if (Config.UnlockAllRecipes)
        {
            e.MeetsConditions = true;
            return;
        }

        // 如果开启"忽略工作站要求"且工作站条件不满足
        if (Config.IgnoreStationRequirements && !e.MeetsStationConditions)
        {
            // 只要有材料就允许合成
            e.MeetsConditions = Recipe.CollectedEnoughItemsToCraftRecipeNew(e.Recipe);
            return;
        }
    }
    #endregion

    #region 配方查找后事件（添加自定义配方）
    private void OnPostFindRecipes()
    {
        if (!Config.Enabled || !Config.CustomRecipesEnabled) return;

        // 清空缓存集合
        CustomRecipeItems.Clear();
        CustomRecipeIndexes.Clear();

        // 确保配方组系统已重置
        if (RecipeGroup.recipeGroups == null || RecipeGroup.recipeGroupIDs == null)
        {
            RecipeGroup.recipeGroups = new Dictionary<int, RecipeGroup>();
            RecipeGroup.recipeGroupIDs = new Dictionary<string, int>();
            RecipeGroup.nextRecipeGroupIndex = 0;
        }

        // 检查所有自定义配方的索引有效性
        foreach (var data in Config.CustomRecipes)
        {
            // 只有当配方无效时才重置索引
            if (data.Index == -1 ||
                data.Index >= Recipe.maxRecipes ||
                Main.recipe[data.Index].createItem.type != data.ResultItem)
            {
                data.Index = -1;
            }
            else
            {
                // 记录有效的自定义配方
                CustomRecipeIndexes.Add(data.Index);
                CustomRecipeItems.Add(data.ResultItem);
            }
        }

        int count = 0;
        bool full = false;

        foreach (var data in Config.CustomRecipes)
        {
            // 如果配方已经分配了有效索引，跳过
            if (data.Index != -1) continue;

            // 检查是否已存在相同的配方
            if (RecipeHooks.ExistsRecipe(data))
            {
                // 找到已存在的相同配方并更新索引
                int existing = RecipeHooks.FindExistingRecipeIndex(data);
                if (existing != -1)
                {
                    data.Index = existing;

                    // 记录新添加的配方(用于比较原版物品)
                    CustomRecipeIndexes.Add(existing);
                    CustomRecipeItems.Add(data.ResultItem);
                    continue;
                }
            }

            // 如果槽位已满则跳过后续配方
            if (full) continue;

            // 查找空槽位
            int slot = RecipeHooks.FindEmptyRecipeSlot();
            if (slot == -1)
            {
                ClientLoader.Chat.WriteLine($"错误：配方槽位不足，无法添加 {Lang.GetItemNameValue(data.ResultItem)} 的配方", Color.Red);
                full = true; // 标记槽位已满
                continue;
            }

            // 创建新配方
            Recipe recipe = RecipeHooks.CreateRecipeFromData(data);

            // 应用到主配方数组
            Main.recipe[slot] = recipe;
            data.Index = slot;

            // 记录新添加的配方(用于比较原版物品)
            CustomRecipeIndexes.Add(slot);
            CustomRecipeItems.Add(data.ResultItem);

            count++;
        }

        // 记录添加的配方数量
        if (count > 0)
        {
            ClientLoader.Chat.WriteLine($"已加载 {count} 个自定义配方", Color.Green);
        }
    }
    #endregion

}