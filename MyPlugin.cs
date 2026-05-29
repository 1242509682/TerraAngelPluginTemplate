using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel;
using TerraAngel.Config;
using TerraAngel.Input;
using TerraAngel.Plugin;
using TerraAngel.Tools;
using TerraAngel.Utility;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace MyPlugin;

public class MyPlugin(string path) : Plugin(path)
{
    #region 插件信息
    public override string Name => typeof(MyPlugin).Namespace!;
    public string Author => "羽学";
    public Version Version => new(1, 1, 8);
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

        // 注册图格编辑事件
        TileEditEventSystem.Register();
        TileEditEventSystem.OnTileKill += OnTileEdit;

        // 注册NPC更新事件
        NPCEventSystem.Register();
        NPCEventSystem.OnNPCUpdate += OnUpdateNPC;
        NPCEventSystem.OnNPCStrike += ApplyDamageMultiplier;

        // 添加反重力药水的Mono钩子
        IgnoreGravity.Register();

        // 读取配置文件
        ReloadConfig();

        // 注册UI
        ToolManager.AddTool<UITool>();

        AutoFishTool.Load(); // 自动钓鱼注册钩子

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

        //卸载图格编辑事件
        TileEditEventSystem.Dispose();
        TileEditEventSystem.OnTileKill -= OnTileEdit;

        // 卸载NPC更新事件
        NPCEventSystem.Dispose();
        NPCEventSystem.OnNPCUpdate -= OnUpdateNPC;
        NPCEventSystem.OnNPCStrike -= ApplyDamageMultiplier;

        // 卸载插件时清理UI
        ToolManager.RemoveTool<UITool>();

        AutoFishTool.Unload(); // 自动钓鱼卸载钩子

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

        // ========== 绘制鼠标伤害范围圆形 ==========
        if (Config.MouseStrikeNPC && !Main.mapFullscreen)
        {
            // 获取背景绘制列表
            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

            // 鼠标世界坐标（屏幕坐标 + 屏幕偏移）
            Vector2 mouseWorld = InputSystem.MousePosition + Main.screenPosition;

            // 半径：格数 * 16（1格 = 16像素）
            float radius = Config.MouseStrikeNPCRange * 16f;

            // 世界坐标转屏幕坐标（供ImGui绘制）
            Vector2 screenCenter = Util.WorldToScreenWorld(mouseWorld);
            Vector2 screenEdge = Util.WorldToScreenWorld(mouseWorld + new Vector2(radius, 0));
            float screenRadius = screenCenter.Distance(screenEdge);

            // 半透明红色圆形（线框）
            uint color = Color.Red.WithAlpha(0.5f).PackedValue;
            drawList.AddCircle(screenCenter, screenRadius, color, 32, 2f);
        }

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

        // 自动钓鱼更新
        AutoFishTool.Instance.Update();
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

    #region NPC伤害事件
    private static void ApplyDamageMultiplier(object? sender, NPCStrikeEventArgs e)
    {
        if (Config.Enabled && Config.DamageMultiplier > 1f && e.Owner == Main.myPlayer)
        {
            e.Damage = (int)(e.Damage * Config.DamageMultiplier);
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