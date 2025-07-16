using Microsoft.Xna.Framework;
using System.Reflection;
using TerraAngel;
using TerraAngel.Config;
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
    public Version Version => new(1, 1, 6);
    #endregion

    #region 注册与卸载
    public override void Load()
    {
        // 初始化完成提示
        ClientLoader.Console.WriteLine($"[{Name}] 插件已加载 (v{Version}) 作者: {Author}", color);
        ClientLoader.Console.WriteLine($"[{Name}] 配置文件位置: {Configuration.FilePath}", Color.LightGoldenrodYellow);

        // 加载世界事件
        WorldGen.Hooks.OnWorldLoad += OnWorldLoad;

        // 传送枪弹幕AI样式最大距离修改
        FixPortalDistanceArgs.Register();

        // 注册移动NPC住房事件
        NPCMoveRoomArgs.Register();

        // 注册配方事件
        RecipeHooks.Register();
        RecipeHooks.OnRecipeCheck += OnRecipeCheck; // 配方检查事件
        RecipeHooks.BeforeRecipeCheck += BuildCustomRecipes; // 配方查找前事件

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
        ReloadConfig();

        // 注册UI
        ToolManager.AddTool<UITool>();

        // 向控制台添加命令
        ClientLoader.Console.AddCommand("reload", ReloadConfig, "重载配置文件");
        ClientLoader.Console.AddCommand("kill", x => Commands.KillPlayer(true), "按K键自杀与复活");
        ClientLoader.Console.AddCommand("heal", Commands.AutoHeal, "按H键强制回血");
        ClientLoader.Console.AddCommand("snpc", Commands.MouseStrikeNPC, "使用物品时伤害鼠标附近怪物");
        ClientLoader.Console.AddCommand("autouse", Commands.AutoUse, "切换自动使用物品功能");
    }

    public override void Unload()
    {
        // 卸载加载世界事件
        WorldGen.Hooks.OnWorldLoad -= OnWorldLoad;

        // 传送枪弹幕AI样式最大距离修改
        FixPortalDistanceArgs.Dispose();

        // 卸载移动NPC住房事件
        NPCMoveRoomArgs.Dispose();

        // 卸载配方事件
        RecipeHooks.Dispose();
        RecipeHooks.OnRecipeCheck -= OnRecipeCheck;
        RecipeHooks.BeforeRecipeCheck -= BuildCustomRecipes;

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
    public static bool NpcTalk = false;
    public static void ReloadConfig(TerraAngel.UI.ClientWindows.Console.ConsoleWindow.CmdStr? x = null)
    {
        Config = Configuration.Read();
        Config.Write();

        if (!NpcTalk)
            ClientLoader.Console.WriteLine($"[{typeof(MyPlugin).Namespace}] 配置文件已重载", Color.LightSkyBlue);
    }
    #endregion

    #region 图格编辑事件
    public static bool TaskRunning { get; set; }
    private void OnTileEdit(object? sender, TileKillEventArgs e)
    {
        if (!Config.Enabled)
        {
            return;
        }

        if (!TaskRunning)
        {
            if (Config.VeinMinerEnabled)
            {
                var task = Task.Run(() =>
                {
                    TaskRunning = true;
                    Utils.VeinMiner(e.X, e.Y); // 连锁挖矿方法
                });

                task.ContinueWith(t =>
                {
                    TaskRunning = false;
                    Utils.UpdateWorld();
                });
            }
        }
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
            ClientLoader.Chat.WriteLine($"社交栏饰品功能已 [c/9DA2E7:{status}]", color);
        }

        // 切换重力控制状态
        if (InputSystem.IsKeyPressed(Config.IgnoreGravityKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.IgnoreGravity = !Config.IgnoreGravity;
            Config.Write();
            string status = Config.IgnoreGravity ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"重力控制已 [c/9DA2E7:{status}]", Color.Yellow);
        }

        // 切换自动垃圾桶状态
        if (InputSystem.IsKeyPressed(Config.AutoTrashKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.AutoTrash = !Config.AutoTrash;
            Config.Write();
            string status = Config.AutoTrash ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"自动垃圾桶已 [c/9DA2E7:{status}]", Color.Yellow);
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
            ClientLoader.Chat.WriteLine($"清理渔夫任务已 [c/9DA2E7:{status}]", Color.Yellow);
        }

        // 切换NPC自动回血状态
        if (InputSystem.IsKeyPressed(Config.NPCAutoHealKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.NPCAutoHeal = !Config.NPCAutoHeal;
            Config.Write();
            string status = Config.NPCAutoHeal ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"NPC自动回血已 [c/9DA2E7:{status}]", Color.Yellow);
        }

        // 切换NPC自动对话状态
        if (InputSystem.IsKeyPressed(Config.AutoTalkKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.AutoTalkNPC = !Config.AutoTalkNPC;
            Config.Write();
            string status = Config.AutoTalkNPC ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"NPC自动对话已 [c/9DA2E7:{status}]", Color.Yellow);
        }

        // 复活城镇NPC
        Utils.Relive(InputSystem.IsKeyPressed(Config.NPCReliveKey));

        // 连锁挖矿
        if (InputSystem.IsKeyPressed(Config.VeinMinerKey))
        {
            Config.VeinMinerEnabled = !Config.VeinMinerEnabled;
            Config.Write();
            string status = Config.VeinMinerEnabled ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"连锁挖矿已 [c/9DA2E7:{status}]", Color.Yellow);
        }
    }
    #endregion

    #region NPC更新事件
    private void OnUpdateNPC(object? sender, NPCUpdateEventArgs e)
    {
        var npc = e.npc;
        // 排除城镇NPC、友好NPC、雕像怪、傀儡
        if (npc == null || !npc.active || !Config.Enabled || npc.SpawnedFromStatue || npc.type == 488) return;
        if (Config.NPCAutoHeal)
        {
            Utils.NPCAutoHeal(npc, e.whoAmI);  // npc自动回血
        }

        // 自动对话处理
        if (Config.AutoTalkNPC && npc.townNPC)
        {
            Utils.AutoNPCTalks(npc, e.whoAmI);
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

        WorldInfo();
    }
    #endregion

    #region 配方检查事件
    public static HashSet<int> CustomRecipeItems = new HashSet<int>();  // 存储自定义配方结果物品ID
    public static HashSet<int> CustomRecipeIndexes = new HashSet<int>(); // 存储自定义配方索引 用于比较原版
    private void OnRecipeCheck(RecipeCheckEventArgs e)
    {
        if (!Config.Enabled) return;


        // 自定义配方处理逻辑 
        if (CustomRecipeIndexes.Contains(e.Index))
        {
            // 获取对应的自定义配方数据
            var CustomRecipe = Config.CustomRecipes.FirstOrDefault(r => r.Index == e.Index);
            if (CustomRecipe != null)
            {
                // 开启自定义配方时：
                // 合成站使用原版遇到对应图格检查
                // 环境使用自己定义的作为检查（已与原版环境相兼容）
                // 材料条件使用收集到的玩家拥有物品与配方物品数量作比较
                e.Conditions = Config.CustomRecipesEnabled && e.MeetTile
                            && CustomRecipeData.IsRecipeUnlocked(CustomRecipe)
                            && RecipeHooks.HasMaterial(e.Recipe, e.HasItem);
            }
            return;
        }

        // 隐藏原版配方
        if (Config.HideOriginalRecipe)
        {
            // 如果当前配方结果物品在自定义物品列表中，但不是自定义配方
            if (CustomRecipeItems.Contains(e.Recipe.createItem.type))
            {
                // 没开启自定义配方时使用原版条件检查，开启后则隐藏原版配方
                e.Conditions = !Config.CustomRecipesEnabled
                    && e.MeetTile // 遇到合成站（图格)
                    && e.MeetEnvironment // 遇到环境
                    && e.MeetMaterial; // 遇到材料
                return;
            }
        }

        // 如果开启"忽略合成站要求"且遇到合成站条件不满足
        if (Config.IgnoreStationRequirements && !e.MeetTile)
        {
            e.Conditions = e.MeetMaterial; // 有材料就能合成(仅对原版配方有效)
            return;
        }

        if (Config.UnlockAllRecipes) // 解锁所有配方
        {
            e.Conditions = true;
        }
    }
    #endregion

    #region 创建自定义配方（配方查找前事件方法）
    private void BuildCustomRecipes()
    {
        // 清空缓存集合
        CustomRecipeItems.Clear();
        CustomRecipeIndexes.Clear();

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
            if (full) break;
            if (data.Index != -1) continue;

            // 检查是否已存在相同的配方
            if (RecipeHooks.ExistsRecipe(data))
            {
                // 找到已存在的相同配方并更新索引
                int existing = RecipeHooks.FindExistingRecipeIndex(data);
                if (existing != -1)
                {
                    // 记录新添加的配方(用于比较原版物品)
                    RecipeHooks.AddRecipeIndex(data, existing);
                    continue;
                }
            }

            // 查找空槽位
            int slot = RecipeHooks.FindEmptyRecipeSlot();
            if (slot == -1)
            {
                ClientLoader.Chat.WriteLine($"错误：配方槽位不足，无法添加 [c/9DA2E7:{Lang.GetItemNameValue(data.ResultItem)}] 的配方", Color.Red);
                full = true; // 标记槽位已满
                continue;
            }

            // 创建新配方
            Recipe recipe = RecipeHooks.CreateRecipeFromData(data);
            Main.recipe[slot] = recipe;
            RecipeHooks.AddRecipeIndex(data, slot);
            count++;
        }

        // 记录添加的配方数量
        if (count > 0)
        {
            ClientLoader.Chat.WriteLine($"已加载 [c/9DA2E7:{count}] 个自定义配方", color);
        }
    }
    #endregion

    #region 重载插件方法
    public static void ReloadPlugins()
    {
        // 将重载操作放入主线程队列
        Main.QueueMainThreadAction(() =>
        {
            ClientConfig.WriteToFile();
            PluginLoader.UnloadPlugins();
            PluginLoader.LoadAndInitializePlugins();
            ClientLoader.PluginUI!.NeedsUpdate = true;
        });
    }
    #endregion

    #region 查询世界信息
    public static void WorldInfo()
    {
        ClientLoader.Console.WriteLine($"\n《世界信息》");
        ClientLoader.Console.WriteLine($"世界名称: {Main.worldName}", color);
        ClientLoader.Console.WriteLine($"世界种子: {WorldGen.currentWorldSeed}" + "(仅显示客户端最后加载世界的种子)", Color.LightGoldenrodYellow);
        string Size = Utils.GetWorldWorldSize();
        ClientLoader.Console.WriteLine($"世界大小: {Size}", Color.LimeGreen);
        string GameMode = Utils.GetWorldGameMode();
        ClientLoader.Console.WriteLine($"世界难度: {GameMode}", Color.LightSeaGreen);
        var (MainProg, EventProg) = Utils.GetWorldProgress();
        ClientLoader.Console.WriteLine($"主要进度: {MainProg}", Color.Gold);
        ClientLoader.Console.WriteLine($"事件进度: {EventProg}", Color.LightBlue);
        ClientLoader.Console.WriteLine($"世界ID: {Main.worldID}", Color.LightSkyBlue);
        ClientLoader.Console.WriteLine($"角色名: {Main.LocalPlayer.name}", Color.LightSalmon);
        ClientLoader.Console.WriteLine($"玩家IP: {Main.getIP}", Color.LightCoral);
        ClientLoader.Console.WriteLine($"设备ID: {Main.clientUUID}", Color.LightYellow);
    }
    #endregion

}