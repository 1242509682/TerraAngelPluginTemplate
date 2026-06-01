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
using static MyPlugin.Utils;
using static Terraria.WorldBuilding.Modifiers;

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
             // 初始化物品图标
            AttackIcon = ContentSamples.ItemsByType[ItemID.BeamSword];
            DefenseIcon = ContentSamples.ItemsByType[ItemID.CobaltShield];
            LiftIcon = ContentSamples.ItemsByType[ItemID.Heart];
            ManaIcon = ContentSamples.ItemsByType[ItemID.Star];

            var fonts = ImGui.GetIO().Fonts.Fonts;
            // 通常第二个字体是中文（第一个是英文默认）
            for (int i = 0; i < fonts.Size; i++)
            {
                var font = fonts[i];
                // 通过字体大小或名称判断，最简单是取第二个
                if (i == 1)
                    chineseFont = font;
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

        // 绘制其他玩家头顶UI
        if (Config.ShowPlayerHeadUI && !Main.mapFullscreen && !Main.gameMenu)
        {
            unsafe { DrawHeadUI(); }
        }

        // 寻宝功能 快捷键 O
        if (InputSystem.IsKeyPressed(Config.TreasureKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Utils.ScanTreasure(Main.LocalPlayer, Config.TreasureRange);
        }

        // 绘制图格头顶UI
        if (Config.ShowTileUI && !Main.mapFullscreen && !Main.gameMenu)
        {
            DrawTileUI();
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

    #region 渲染其他玩家头顶UI
    private ImFontPtr chineseFont;
    private Item? AttackIcon;
    private Item? DefenseIcon;
    private Item? LiftIcon;
    private Item? ManaIcon;

    private class PInfo
    {
        public Player? plr;
        public Vector2 pPos;
        public Vector2 pSz;
        public float dist;
    }

    private unsafe void DrawHeadUI()
    {
        // 使用中文字体（用于显示中文名称和数值）
        ImGui.PushFont(chineseFont);

        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
        if (drawList.NativePtr == null) return;

        Vector2 dSize = ImGui.GetIO().DisplaySize;
        Player local = Main.LocalPlayer;
        // 渐变色起始和结束（淡青 -> 淡黄）
        Vector4 gS = new Vector4(0.65f, 0.84f, 0.92f, 1f);
        Vector4 gE = new Vector4(0.96f, 0.97f, 0.69f, 1f);

        // ========== 1. 远距离标记（直接绘制，无呼吸流光）==========
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player plr = Main.player[i];
            if (!plr.active || plr.whoAmI == Main.myPlayer) continue;

            Vector2 hPos = Util.WorldToScreenDynamic(plr.Top - new Vector2(0, 10));
            float distGrid = local.Center.Distance(plr.Center) / 16f;

            if (distGrid > Config.HeadDist)
            {
                string fullText = $"{plr.name} {(int)distGrid}格";
                Vector2 txtSz = ImGui.CalcTextSize(fullText);
                float pad = 4f;
                bool onScr = hPos.X >= 0 && hPos.X <= dSize.X && hPos.Y >= 0 && hPos.Y <= dSize.Y;
                Vector2 finalPos;
                if (onScr)
                {
                    finalPos = hPos - new Vector2(0, 30);
                    finalPos.Y = Math.Max(finalPos.Y, 10);
                }
                else
                {
                    // 屏幕外计算边缘位置
                    Vector2 dir = (hPos - dSize / 2).SafeNormalize(Vector2.Zero);
                    if (dir == Vector2.Zero) dir = Vector2.UnitX;
                    float eX, eY;
                    if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                    {
                        eX = (dir.X > 0) ? dSize.X - txtSz.X - 20 : 20;
                        eY = Math.Clamp(dSize.Y / 2 + (dir.Y / dir.X) * (eX - dSize.X / 2), 20, dSize.Y - txtSz.Y - 20);
                    }
                    else
                    {
                        eY = (dir.Y > 0) ? dSize.Y - txtSz.Y - 20 : 20;
                        eX = Math.Clamp(dSize.X / 2 + (dir.X / dir.Y) * (eY - dSize.Y / 2), 20, dSize.X - txtSz.X - 20);
                    }
                    finalPos = new Vector2(eX, eY);
                }
                // 背景
                Vector2 bgMin = finalPos - new Vector2(pad, pad);
                Vector2 bgMax = finalPos + txtSz + new Vector2(pad, pad);
                drawList.AddRectFilled(bgMin, bgMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.6f)), 4f);
                // 渐变色文本（整体）
                Utils.DrawGrad(drawList, finalPos, fullText, gS, gE);
            }
        }

        // ========== 2. 近距离完整面板 ==========
        // 鼠标位置（用于悬浮检测）
        Vector2 mousePos = InputSystem.MousePosition;
        List<PInfo> cand = new();

        // 收集符合条件的玩家（距离小于阈值，且若启用“仅悬浮显示”则鼠标必须在玩家碰撞箱内）
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player plr = Main.player[i];
            if (!plr.active || plr.dead || plr.whoAmI == Main.myPlayer) continue;

            // 悬浮检测
            if (Config.ShowHeadUIOnlyOnHover)
            {
                Rectangle hitboxScr = new Rectangle(
                    (int)Util.WorldToScreenDynamic(plr.Hitbox.TopLeft()).X,
                    (int)Util.WorldToScreenDynamic(plr.Hitbox.TopLeft()).Y,
                    plr.Hitbox.Width,
                    plr.Hitbox.Height
                );
                if (!hitboxScr.Contains((int)mousePos.X, (int)mousePos.Y))
                    continue;
            }

            Vector2 hPos = Util.WorldToScreenDynamic(plr.Top - new Vector2(0, 10));
            float distGrid = local.Center.Distance(plr.Center) / 16f;
            if (distGrid > Config.HeadDist) continue;   // 距离太远，不显示完整面板

            Vector2 pSz = new Vector2(300, 100);
            Vector2 pPos = hPos - new Vector2(pSz.X / 2, pSz.Y) + new Vector2(0, -22);  // 上移22像素避开头顶
            cand.Add(new PInfo { plr = plr, pPos = pPos, pSz = pSz, dist = distGrid });
        }

        // 没有符合条件的玩家则直接返回
        if (cand.Count == 0) { ImGui.PopFont(); return; }

        // 按距离从小到大排序（近的优先绘制）
        cand.Sort((a, b) => a.dist.CompareTo(b.dist));

        // 本地玩家的屏幕碰撞箱（用于避免UI遮挡自己）
        Rectangle localHit = new Rectangle(
            (int)Util.WorldToScreenDynamic(local.Hitbox.TopLeft()).X,
            (int)Util.WorldToScreenDynamic(local.Hitbox.TopLeft()).Y,
            local.Hitbox.Width, local.Hitbox.Height);

        // 记录已绘制面板的矩形（用于避免UI互相重叠）
        List<Rectangle> occ = new();

        // ========== 流光 + 呼吸效果参数（每帧变化）==========
        // 流光速度（顺时针流动）
        float flow = (float)(Main.GameUpdateCount * 0.015) % 1f;
        // 呼吸亮度：使用余弦，亮暗时间对称，且提高最低亮度
        float breath = 0.85f + 0.25f * (float)Math.Cos(Main.GameUpdateCount * 0.06);
        // 亮度范围 0.6 ~ 1.1，暗部不会太暗，亮部足够亮
        breath = Math.Clamp(breath, 0.6f, 1.1f);

        // 高对比颜色：青与橙
        Vector4 colA = new Vector4(0.2f, 0.8f, 1.0f, 1f);
        Vector4 colB = new Vector4(1.0f, 0.6f, 0.2f, 1f);

        // 四个角颜色
        uint cTL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.00f + flow) % 1f) * breath);
        uint cTR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.25f + flow) % 1f) * breath);
        uint cBR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.50f + flow) % 1f) * breath);
        uint cBL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.75f + flow) % 1f) * breath);

        foreach (PInfo info in cand)
        {
            Rectangle rect = new Rectangle((int)info.pPos.X, (int)info.pPos.Y, (int)info.pSz.X, (int)info.pSz.Y);
            // 避免遮挡本地玩家自己
            if (rect.Intersects(localHit)) continue;
            // 避免与其他已绘制的UI重叠
            bool overlap = false;
            foreach (var r in occ) if (rect.Intersects(r)) { overlap = true; break; }
            if (overlap) continue;
            occ.Add(rect);

            Player plr = info.plr!;
            Vector2 pPos = info.pPos;
            Vector2 pSz = info.pSz;

            // ---------- 外框渐变（流光走马灯 + 呼吸）----------
            Vector2 outPos = pPos - new Vector2(2, 2);
            Vector2 outSz = pSz + new Vector2(4, 4);
            drawList.AddRectFilledMultiColor(outPos, outPos + outSz, cTL, cTR, cBR, cBL);

            // ---------- 内部背景（半透明黑色圆角）----------
            uint bgCol = ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
            drawList.AddRectFilled(pPos, pPos + pSz, bgCol, 6f);

            // ---------- 左侧框（手持物品，右移4像素）----------
            float leftW = 52;
            Vector2 leftPos = pPos + new Vector2(8, 4);     // 原为4，现右移4像素
            Vector2 leftSz = new Vector2(leftW, pSz.Y - 8);
            drawList.AddRectFilled(leftPos, leftPos + leftSz, ImGui.GetColorU32(new Vector4(0.15f, 0.15f, 0.15f, 0.8f)), 4f);

            Item held = plr.inventory[plr.selectedItem];
            Vector2 hdSz = new Vector2(32, 32);
            Vector2 handPos = leftPos + (leftSz - hdSz) / 2;
            if (held?.type > 0 && !held.IsAir)
                ImGuiUtil.DrawItemCentered(drawList, held, handPos + hdSz / 2, hdSz.X);

            // ---------- 右侧区域 ----------
            float rX = leftPos.X + leftW + 8;
            float rW = pSz.X - (rX - pPos.X) - 8;

            // ---------- 右上框（名称 + 属性行）----------
            float topH = 56;
            Vector2 topPos = new Vector2(rX, pPos.Y + 4);
            Vector2 topSz = new Vector2(rW, topH);
            drawList.AddRectFilled(topPos, topPos + topSz, ImGui.GetColorU32(new Vector4(0.12f, 0.12f, 0.12f, 0.8f)), 4f);

            // 玩家名称（无阴影，直接渐变色）
            string pName = plr.name;
            float nameW = ImGui.CalcTextSize(pName).X;
            Vector2 nPos = new Vector2(topPos.X + (topSz.X - nameW) / 2, topPos.Y + 6);
            Utils.DrawGrad(drawList, nPos, pName, gS, gE);

            // 属性行：攻击、防御、生命、魔力（图标 + 数值）
            int atk = plr.GetWeaponDamage(plr.HeldItem);
            int def = plr.statDefense;
            int life = plr.statLife;
            int mana = plr.statMana;

            Vector2 iSz = new Vector2(16, 16);    // 图标大小
            int spc = 8;                          // 间距

            // 预计算各数值文本尺寸
            Vector2 aSz = ImGui.CalcTextSize(atk.ToString());
            Vector2 dSz = ImGui.CalcTextSize(def.ToString());
            Vector2 lSz = ImGui.CalcTextSize(life.ToString());
            Vector2 mSz = ImGui.CalcTextSize(mana.ToString());

            // 属性行总宽度（图标+间距+文本 + 项间间距）
            float totalW = (iSz.X + spc + aSz.X) + spc + (iSz.X + spc + dSz.X) + spc + (iSz.X + spc + lSz.X) + spc + (iSz.X + spc + mSz.X);
            float startX = topPos.X + (topSz.X - totalW) / 2;   // 水平居中起始X

            float nameH = ImGui.CalcTextSize(pName).Y;
            float availH = topSz.Y - nameH - 8;
            float attrY = topPos.Y + nameH + 8 + (availH - iSz.Y) / 2;  // 属性行垂直中心线

            // 攻击
            Vector2 aIcon = new Vector2(startX, attrY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(drawList, AttackIcon!, aIcon + iSz / 2, iSz.X);
            Vector2 aTxt = new Vector2(aIcon.X + iSz.X + spc, attrY - aSz.Y / 2);
            Utils.DrawGrad(drawList, aTxt, atk.ToString(), gS, gE);
            float aW = iSz.X + spc + aSz.X;

            // 防御
            Vector2 dIcon = new Vector2(aIcon.X + aW + spc, attrY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(drawList, DefenseIcon!, dIcon + iSz / 2, iSz.X);
            Vector2 dTxt = new Vector2(dIcon.X + iSz.X + spc, attrY - dSz.Y / 2);
            Utils.DrawGrad(drawList, dTxt, def.ToString(), gS, gE);
            float dW = iSz.X + spc + dSz.X;

            // 生命
            Vector2 lIcon = new Vector2(dIcon.X + dW + spc, attrY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(drawList, LiftIcon!, lIcon + iSz / 2, iSz.X);
            Vector2 lTxt = new Vector2(lIcon.X + iSz.X + spc, attrY - lSz.Y / 2);
            Utils.DrawGrad(drawList, lTxt, life.ToString(), gS, gE);
            float lW = iSz.X + spc + lSz.X;

            // 魔力
            Vector2 mIcon = new Vector2(lIcon.X + lW + spc, attrY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(drawList, ManaIcon!, mIcon + iSz / 2, iSz.X);
            Vector2 mTxt = new Vector2(mIcon.X + iSz.X + spc, attrY - mSz.Y / 2);
            Utils.DrawGrad(drawList, mTxt, mana.ToString(), gS, gE);

            // ---------- 右下框（血条）----------
            float botY = topPos.Y + topSz.Y + 4;
            Vector2 botPos = new Vector2(rX, botY);
            Vector2 botSz = new Vector2(rW, pSz.Y - (botY - pPos.Y) - 4);
            drawList.AddRectFilled(botPos, botPos + botSz, ImGui.GetColorU32(new Vector4(0.12f, 0.12f, 0.12f, 0.8f)), 4f);

            float lp = (float)plr.statLife / plr.statLifeMax;
            Vector2 hpPos = botPos + new Vector2(8, 8);
            Vector2 hpSz = new Vector2(botSz.X - 16, 18);
            // 灰色背景
            drawList.AddRectFilled(hpPos, hpPos + hpSz, ImGui.GetColorU32(new Vector4(0.3f, 0.3f, 0.3f, 1f)));
            // 红色前景（根据血量比例）
            drawList.AddRectFilled(hpPos, hpPos + new Vector2(hpSz.X * lp, hpSz.Y), ImGui.GetColorU32(new Vector4(1f, 0.2f, 0.2f, 1f)));

            string hpTxt = $"{plr.statLife}/{plr.statLifeMax}";
            Vector2 htSz = ImGui.CalcTextSize(hpTxt);
            Vector2 htPos = hpPos + new Vector2((hpSz.X - htSz.X) / 2, (hpSz.Y - htSz.Y) / 2);
            drawList.AddText(htPos, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)), hpTxt);
        }

        ImGui.PopFont();
    }
    #endregion

    #region 渲染图格头顶UI
    private class TileInfo
    {
        public Point pos;      // 坐标
        public WorldItem? it;  // 物品
        public string? n;      // 名称
        public float d;        // 距离
        public Vector2 p;      // 面板屏幕位置
        public Vector2 s;      // 面板尺寸
    }

    private Dictionary<Point, WorldItem> cache = new();  // 物品缓存
    private long lastRef = 0;                           // 上次刷新帧
    private const int refInt = 60;                      // 刷新间隔

    private void DrawTileUI()
    {
        if (!Config.ShowTileUI || Main.gameMenu) return;

        Player pl = Main.LocalPlayer;          // 本地玩家
        if (pl == null) return;

        ImGui.PushFont(chineseFont);           // 使用中文字体
        var dl = ImGui.GetBackgroundDrawList(); // 绘图列表
        var scr = ImGui.GetIO().DisplaySize;   // 屏幕尺寸
        Vector2 mousePos = InputSystem.MousePosition; // 鼠标位置（用于悬浮）

        int rng = Config.TreasureRange;        // 扫描半径（格）
                                               // 计算扫描边界（图格坐标）
        int l = Math.Max(0, (int)(pl.Center.X / 16) - rng);
        int r = Math.Min(Main.maxTilesX - 1, (int)(pl.Center.X / 16) + rng);
        int t = Math.Max(0, (int)(pl.Center.Y / 16) - rng);
        int b = Math.Min(Main.maxTilesY - 1, (int)(pl.Center.Y / 16) + rng);

        float px = pl.Center.X / 16f;          // 玩家图格X坐标
        float py = pl.Center.Y / 16f;          // 玩家图格Y坐标

        // 定期清空物品缓存，避免内存无限增长
        if (Main.GameUpdateCount - lastRef >= refInt)
        {
            cache.Clear();
            lastRef = (int)Main.GameUpdateCount;
        }

        // 预计算所有玩家的屏幕碰撞箱（用于自动隐藏）
        List<Rectangle> pHit = new();          // playerHitboxes
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (p.active && !p.dead)
            {
                Rectangle hit = new Rectangle(
                    (int)Util.WorldToScreenDynamic(p.Hitbox.TopLeft()).X,
                    (int)Util.WorldToScreenDynamic(p.Hitbox.TopLeft()).Y,
                    p.Hitbox.Width,
                    p.Hitbox.Height);
                pHit.Add(hit);
            }
        }

        // 收集所有需要绘制的图格信息
        List<TileInfo> cand = new();
        for (int x = l; x <= r; x++)
            for (int y = t; y <= b; y++)
            {
                Tile? tile = Main.tile[x, y];
                if (tile == null || !tile.Value.active()) continue;
                int id = tile.Value.type;
                if (!Config.TreasureList.Contains(id)) continue;

                Point pt = new Point(x, y);
                if (!cache.TryGetValue(pt, out WorldItem? it))
                {
                    it = Utils.GetTileItem(x, y); // 获取掉落物品
                    if (it == null || it.type <= 0) continue;
                    cache[pt] = it;
                }

                string nm = it.Name;                     // 物品名
                if (string.IsNullOrEmpty(nm)) nm = Utils.GetTileName(id);
                if (string.IsNullOrEmpty(nm)) nm = $"图格{id}";

                float dx = px - x, dy = py - y;         // 玩家到图格的偏移
                float dst = (float)Math.Sqrt(dx * dx + dy * dy); // 距离（格）

                Vector2 wc = new Vector2(x * 16 + 8, y * 16 + 8); // 图格中心世界坐标
                Vector2 sp = Util.WorldToScreenDynamic(wc);       // 屏幕坐标
                if (sp.X < -100 || sp.X > scr.X + 100 || sp.Y < -100 || sp.Y > scr.Y + 100)
                    continue; // 屏幕外跳过

                int pad = 10;                          // 内边距（像素）
                Vector2 iSz = new Vector2(24, 24);     // 图标尺寸
                Vector2 nSz = ImGui.CalcTextSize(nm);  // 名称文字尺寸
                string dt = $"{(int)dst}格";            // 距离文本
                Vector2 dSz = ImGui.CalcTextSize(dt);  // 距离文字尺寸
                                                       // 面板宽度 = 图标 + 间距 + 名称 + 间距 + 距离 + 左右内边距
                float w = iSz.X + 8 + nSz.X + 8 + dSz.X + pad * 2;
                // 面板高度 = 三者最大高度 + 上下内边距
                float h = Math.Max(iSz.Y, Math.Max(nSz.Y, dSz.Y)) + 8 + pad * 2;
                Vector2 pSz = new Vector2(w, h);       // 面板尺寸
                Vector2 pPos = new Vector2(sp.X - pSz.X / 2, sp.Y - 40); // 面板位置（头顶上方40像素）

                cand.Add(new TileInfo { pos = pt, it = it, n = nm, d = dst, p = pPos, s = pSz });
            }

        if (cand.Count == 0) { ImGui.PopFont(); return; }

        // 按距离从小到大排序（近的优先绘制）
        cand.Sort((a, b) => a.d.CompareTo(b.d));

        // 若开启“仅悬浮显示”，则只保留鼠标悬浮的图格（检查矩形包含鼠标）
        List<TileInfo> toDraw;
        if (Config.ShowTileUIOnlyOnHover)
        {
            toDraw = cand.Where(info =>
            {
                Rectangle rect = new Rectangle((int)info.p.X, (int)info.p.Y, (int)info.s.X, (int)info.s.Y);
                return rect.Contains((int)mousePos.X, (int)mousePos.Y);
            }).ToList();
            if (toDraw.Count == 0) { ImGui.PopFont(); return; }
        }
        else
        {
            toDraw = cand;
        }

        // 面板间重叠检测 + 自动隐藏被玩家遮挡的面板
        List<Rectangle> occ = new();                     // 已绘制面板矩形
        Vector4 gS = new Vector4(0.65f, 0.84f, 0.92f, 1f); // 渐变色起始（淡青）
        Vector4 gE = new Vector4(0.96f, 0.97f, 0.69f, 1f); // 渐变色结束（淡黄）

        // 流光 & 呼吸参数（与玩家UI一致）
        float flow = (float)(Main.GameUpdateCount * 0.015) % 1f;  // 流光相位
        float breath = 0.85f + 0.25f * (float)Math.Cos(Main.GameUpdateCount * 0.06);
        breath = Math.Clamp(breath, 0.6f, 1.1f);                 // 呼吸亮度
        Vector4 colA = new Vector4(0.2f, 0.8f, 1.0f, 1f);        // 高对比色：青
        Vector4 colB = new Vector4(1.0f, 0.6f, 0.2f, 1f);        // 高对比色：橙

        foreach (var info in toDraw)
        {
            Rectangle rect = new Rectangle((int)info.p.X, (int)info.p.Y, (int)info.s.X, (int)info.s.Y);

            // 1. 自动隐藏：若面板与任何玩家碰撞箱重叠，则跳过

            bool blk = false; // blocked
            foreach (var hit in pHit)
            {
                if (rect.Intersects(hit))
                {
                    blk = true;
                    break;
                }
            }
            if (blk) continue;

            // 2. 避免与其他已绘制的面板重叠
            bool overlap = false;
            foreach (var o in occ) if (rect.Intersects(o)) { overlap = true; break; }
            if (overlap) continue;
            occ.Add(rect);

            // ---------- 外框流光渐变（走马灯 + 呼吸） ----------
            uint cTL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.00f + flow) % 1f) * breath);
            uint cTR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.25f + flow) % 1f) * breath);
            uint cBR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.50f + flow) % 1f) * breath);
            uint cBL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.75f + flow) % 1f) * breath);
            Vector2 outPos = info.p - new Vector2(2, 2);
            Vector2 outSz = info.s + new Vector2(4, 4);
            dl.AddRectFilledMultiColor(outPos, outPos + outSz, cTL, cTR, cBR, cBL);

            // ---------- 内部半透明背景（圆角） ----------
            uint bgCol = ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
            dl.AddRectFilled(info.p, info.p + info.s, bgCol, 6f);

            // ---------- 绘制物品图标 ----------
            Vector2 iSz = new Vector2(24, 24);
            Vector2 iPos = info.p + new Vector2(10, (info.s.Y - iSz.Y) / 2);
            ImGuiUtil.DrawItemCentered(dl, info.it.inner, iPos + iSz / 2, iSz.X);

            // ---------- 名称 + 距离（整体渐变色） ----------
            string txt = $"{info.n} {(int)info.d}格";
            Vector2 tSz = ImGui.CalcTextSize(txt);
            Vector2 tPos = info.p + new Vector2(iSz.X + 8 + 10, (info.s.Y - tSz.Y) / 2);
            Utils.DrawGrad(dl, tPos, txt, gS, gE);
        }

        ImGui.PopFont();  // 恢复字体
    }
    #endregion
}