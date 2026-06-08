using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TerraAngel;
using TerraAngel.Config;
using TerraAngel.Graphics;
using TerraAngel.Input;
using TerraAngel.Plugin;
using TerraAngel.Tools;
using TerraAngel.Utility;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static MyPlugin.HeadUIManager;

namespace MyPlugin;

public class MyPlugin(string path) : Plugin(path)
{
    #region 插件信息
    public override string Name => typeof(MyPlugin).Namespace!;
    public string Author => "羽学";
    public Version Version => new(1, 1, 9);
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

        // 初始化中文字体（用于头顶UI）
        unsafe
        {
            // 初始化物品图标（直接赋值给 HeadUIManager 的静态字段）
            atkIcon = ContentSamples.ItemsByType[ItemID.BeamSword];
            defIcon = ContentSamples.ItemsByType[ItemID.CobaltShield];
            lifeIcon = ContentSamples.ItemsByType[ItemID.Heart];

            var fonts = ImGui.GetIO().Fonts.Fonts;
            // 通常第二个字体是中文（第一个是英文默认）
            for (int i = 0; i < fonts.Size; i++)
            {
                var font = fonts[i];
                // 通过字体大小或名称判断，最简单是取第二个
                if (i == 1)
                    chFont = font;
            }
        }
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

        // 传送到最近死亡地点（快捷键 B）
        if (InputSystem.IsKeyPressed(Config.DeathTPKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Utils.TPLatest(Main.LocalPlayer);
        }

        // 修改饰品前缀 快捷键P
        if (InputSystem.IsKeyPressed(Keys.P))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
            Utils.ApplyPrefix(Config.DefaultPrefixId);
        }

        // 切换自动钓鱼状态
        if (InputSystem.IsKeyPressed(Config.AutoFishKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.AutoFishEnabled = !Config.AutoFishEnabled;
            string status = Config.AutoFishEnabled ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"自动钓鱼已 [c/9DA2E7:{status}]", Color.Yellow);
        }

        // 切换伤害倍数状态（快捷键）
        if (InputSystem.IsKeyPressed(Config.DamageMultiplierKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.DamageMultiplierEnabled = !Config.DamageMultiplierEnabled;
            Config.Write();
            string status = Config.DamageMultiplierEnabled ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"NPC伤害倍数已 [c/9DA2E7:{status}]", Color.Yellow);
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

        // 头顶 UI 显示快捷键 U
        if (InputSystem.IsKeyPressed(Config.HeadUIKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.ShowPlayerHeadUI = !Config.ShowPlayerHeadUI;
            Config.Write();
            string status = Config.ShowPlayerHeadUI ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"显示头顶UI已 [c/9DA2E7:{status}]", Color.Yellow);
        }

        // 寻宝功能 快捷键 O
        if (InputSystem.IsKeyPressed(Config.TreasureKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            ScanTreasure(Main.LocalPlayer, Config.TreasureRange);
        }

        // 
        if (!Main.mapFullscreen && !Main.gameMenu)
        {
            // 绘制图格头顶UI
            if (Config.ShowTileUI)
                DrawTileUI();

            // 绘制玩家头顶UI
            if (Config.ShowPlayerHeadUI)
                unsafe { DrawHeadUI(); }

            // 绘制 NPC 伤害头顶 UI
            if (Config.ShowNPCDamageUI) 
                DrawNPCUI();

            // 绘制UI后检测点击
            CheckClicks();

            // 绘制头顶UI的更多操作弹窗
            DrawMoreWin();
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

        // 如果 NPC 死亡，清除高亮
        if (curNPC != null && (!curNPC.active || curNPC.life <= 0))
        {
            maxLifeSeen.Remove(curNPC);
            curNPC = null;
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

        // 记录受击 NPC 用于 UI 显示
        if (Config.ShowNPCDamageUI && e.Owner == Main.myPlayer && e.NPC != null && e.NPC.active && e.Damage > 0)
        {
            // 如果攻击了新的 NPC，则切换高亮
            if (curNPC != e.NPC)
                curNPC = e.NPC;

            NpcDamage = e.Damage;
        }

        // 如果 NPC 死亡，清除高亮
        if (curNPC != null && (!curNPC.active || curNPC.life <= 0))
        {
            maxLifeSeen.Remove(curNPC);
            curNPC = null;
        }
    }
    #endregion

    #region 加载世界事件
    private void OnWorldLoad()
    {
        var plr = Main.LocalPlayer;

        if (!Config.Enabled || plr is null) return;

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