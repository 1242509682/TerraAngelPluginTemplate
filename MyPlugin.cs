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
        if (Config.ShowPlayerHeadUI && !Main.mapFullscreen && !Main.gameMenu && Main.GameMode == 2)
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
    /// <summary>
    /// 绘制其他玩家头顶信息面板（带边缘指示器）
    /// </summary>
    private unsafe void DrawHeadUI()
    {
        ImGui.PushFont(chineseFont);

        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
        if (drawList.NativePtr == null) return;

        Vector2 dSize = ImGui.GetIO().DisplaySize;
        Player local = Main.LocalPlayer;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player plr = Main.player[i];
            if (!plr.active || plr.whoAmI == Main.myPlayer) continue;

            Vector2 hPos = Util.WorldToScreenDynamic(plr.Top - new Vector2(0, 10));
            float distGrid = local.Center.Distance(plr.Center) / 16f;

            if (distGrid > Config.HeadDist)
            {
                // 远距离：渐变色名字 + 白色距离数字
                string nPart = plr.name;        // 名字部分
                string dPart = $" {(int)distGrid}格"; // 距离部分（含空格）
                Vector2 nSz = ImGui.CalcTextSize(nPart);
                Vector2 dSz = ImGui.CalcTextSize(dPart);
                float tWid = nSz.X + dSz.X;      // 总宽度
                Vector2 mSz = new Vector2(tWid, Math.Max(nSz.Y, dSz.Y)); // marker尺寸
                uint dCol = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)); // 白色
                float pad = 4f;

                bool onScr = (hPos.X >= 0 && hPos.X <= dSize.X && hPos.Y >= 0 && hPos.Y <= dSize.Y);
                Vector2 fPos; // final position

                if (onScr)
                {
                    fPos = hPos - new Vector2(0, 30);
                    fPos.Y = Math.Max(fPos.Y, 10);
                }
                else
                {
                    Vector2 dir = (hPos - dSize / 2).SafeNormalize(Vector2.Zero);
                    if (dir == Vector2.Zero) dir = Vector2.UnitX;
                    float eX, eY;
                    if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                    {
                        eX = (dir.X > 0) ? dSize.X - mSz.X - 20 : 20;
                        eY = Math.Clamp(dSize.Y / 2 + (dir.Y / dir.X) * (eX - dSize.X / 2), 20, dSize.Y - mSz.Y - 20);
                    }
                    else
                    {
                        eY = (dir.Y > 0) ? dSize.Y - mSz.Y - 20 : 20;
                        eX = Math.Clamp(dSize.X / 2 + (dir.X / dir.Y) * (eY - dSize.Y / 2), 20, dSize.X - mSz.X - 20);
                    }
                    fPos = new Vector2(eX, eY);
                }

                // 背景
                Vector2 bgMin = fPos - new Vector2(pad, pad);
                Vector2 bgMax = fPos + mSz + new Vector2(pad, pad);
                drawList.AddRectFilled(bgMin, bgMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.6f)), 4f);

                // 渐变色名字
                DrawGrad(drawList, fPos, nPart, new Vector4(0.65f, 0.84f, 0.92f, 1f), new Vector4(0.96f, 0.97f, 0.69f, 1f));
                // 白色距离
                Vector2 dPos = fPos + new Vector2(nSz.X, 0);
                drawList.AddText(dPos, dCol, dPart);
            }
            else
            {
                // 完整面板（不变）
                Vector2 pSize = new Vector2(170, 70);
                Vector2 pPos = hPos - new Vector2(pSize.X / 2, pSize.Y);
                uint bgCol = ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
                uint txtCol = ImGui.GetColorU32(new Vector4(1f, 1f, 0.9f, 1f));

                drawList.AddRectFilled(pPos, pPos + pSize, bgCol, 6f);

                string pName = plr.name;
                Vector2 nStart = pPos + new Vector2((pSize.X - ImGui.CalcTextSize(pName).X) / 2, 6);
                DrawGrad(drawList, nStart, pName, new Vector4(0.65f, 0.84f, 0.92f, 1f), new Vector4(0.96f, 0.97f, 0.69f, 1f));

                string sText = $"攻击 {plr.GetWeaponDamage(plr.HeldItem)}  防御 {plr.statDefense}";
                drawList.AddText(pPos + new Vector2(8, 24), txtCol, sText);

                float lp = (float)plr.statLife / plr.statLifeMax;
                Vector2 hpPos = pPos + new Vector2(8, 44);
                Vector2 hpSz = new Vector2(155, 18);
                drawList.AddRectFilled(hpPos, hpPos + hpSz, ImGui.GetColorU32(new Vector4(0.3f, 0.3f, 0.3f, 1f)));
                drawList.AddRectFilled(hpPos, hpPos + new Vector2(hpSz.X * lp, hpSz.Y), ImGui.GetColorU32(new Vector4(1f, 0.2f, 0.2f, 1f)));

                string hpTxt = $"{plr.statLife}/{plr.statLifeMax}";
                Vector2 hpTxtSz = ImGui.CalcTextSize(hpTxt);
                Vector2 hpTxtPos = hpPos + new Vector2((hpSz.X - hpTxtSz.X) / 2, (hpSz.Y - hpTxtSz.Y) / 2);
                drawList.AddText(hpTxtPos, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)), hpTxt);

                Item held = plr.inventory[plr.selectedItem];
                if (held?.type > 0 && !held.IsAir)
                {
                    Vector2 iPos = new Vector2(pPos.X + pSize.X - 25, pPos.Y + pSize.Y / 2 - 12);
                    if (iPos.X + 24 < dSize.X)
                        ImGuiUtil.DrawItemCentered(drawList, held, iPos, 20f);
                }
            }
        }

        ImGui.PopFont();
    }
    #endregion

    #region 渐变色方法
    /// <summary>
    /// 在指定位置绘制渐变色文本（逐字渐变）
    /// </summary>
    /// <param name="drawList">ImGui 绘图列表</param>
    /// <param name="pos">起始绘制位置（左上角）</param>
    /// <param name="text">要绘制的文本</param>
    /// <param name="sCol">起始颜色 (Vector4)</param>
    /// <param name="eCol">结束颜色 (Vector4)</param>
    private unsafe void DrawGrad(ImDrawListPtr drawList, Vector2 pos, string text, Vector4 sCol, Vector4 eCol)
    {
        if (string.IsNullOrEmpty(text)) return;

        float tChars = text.Length;
        float curX = pos.X;

        for (int idx = 0; idx < text.Length; idx++)
        {
            float t = (float)idx / (tChars - 1);
            Vector4 gradVec = Vector4.Lerp(sCol, eCol, t);
            uint gradCol = ImGui.GetColorU32(gradVec);

            string ch = text[idx].ToString();
            Vector2 chSize = ImGui.CalcTextSize(ch);
            Vector2 chPos = new Vector2(curX, pos.Y);
            drawList.AddText(chPos, gradCol, ch);

            curX += chSize.X;
        }
    }
    #endregion

    #region 渲染图格头顶UI
    private class TileInfo
    {
        public Point pos;           // 位置
        public WorldItem? item;      // 物品
        public string? name;         // 名称
        public float dist;          // 距离
        public Vector2 spos;        // 屏幕位置 (panelPos)
        public Vector2 psize;       // 面板尺寸
    }

    // 缓存图格物品，避免每帧重复调用 GetItem
    private Dictionary<Point, WorldItem> tileCache = new();
    private long CacheRefresh = 0;
    private const int ShowTileInterval = 60; // 每60帧刷新一次

    private void DrawTileUI()
    {
        if (!Config.ShowTileUI || Main.gameMenu) return;

        Player local = Main.LocalPlayer;
        if (local == null) return;

        ImGui.PushFont(chineseFont);
        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        Vector2 screenSize = ImGui.GetIO().DisplaySize;
        int range = Config.TreasureRange;
        int left = Math.Max(0, (int)(local.Center.X / 16) - range);
        int right = Math.Min(Main.maxTilesX - 1, (int)(local.Center.X / 16) + range);
        int top = Math.Max(0, (int)(local.Center.Y / 16) - range);
        int bottom = Math.Min(Main.maxTilesY - 1, (int)(local.Center.Y / 16) + range);

        float TileX = local.Center.X / 16f;
        float TileY = local.Center.Y / 16f;

        // 刷新缓存
        if (Main.GameUpdateCount - CacheRefresh >= ShowTileInterval)
        {
            tileCache.Clear();
            CacheRefresh = (int)Main.GameUpdateCount;
        }

        // 收集所有需要绘制的图格信息
        List<TileInfo> candidates = new();

        for (int x = left; x <= right; x++)
        {
            for (int y = top; y <= bottom; y++)
            {
                Tile? tile = Main.tile[x, y];
                if (tile == null || !tile.Value.active()) continue;
                int tileID = tile.Value.type;
                if (!Config.TreasureList.Contains(tileID)) continue;

                Point p = new Point(x, y);
                if (!tileCache.TryGetValue(p, out WorldItem? item))
                {
                    item = Utils.GetItem(x, y);
                    if (item == null || item.type <= 0) continue;
                    tileCache[p] = item;
                }

                string name = item.Name;
                if (string.IsNullOrEmpty(name)) name = Utils.GetTileName(tileID);
                if (string.IsNullOrEmpty(name)) name = $"图格{tileID}";

                float dx = TileX - x;
                float dy = TileY - y;
                float distSq = dx * dx + dy * dy;
                float dist = (float)Math.Sqrt(distSq);

                Vector2 worldCenter = new Vector2(x * 16 + 8, y * 16 + 8);
                Vector2 screenPos = Util.WorldToScreenDynamic(worldCenter);
                if (screenPos.X < -100 || screenPos.X > screenSize.X + 100 ||
                    screenPos.Y < -100 || screenPos.Y > screenSize.Y + 100)
                    continue;

                // 计算面板尺寸（增加内边距20像素）
                int pad = 10; // 四周各10像素，总宽高增加20
                Vector2 iconSize = new Vector2(24, 24);
                Vector2 nameSize = ImGui.CalcTextSize(name);
                string distText = $"{(int)dist}格";
                Vector2 distSize = ImGui.CalcTextSize(distText);
                float totalWidth = iconSize.X + 8 + nameSize.X + 8 + distSize.X + pad * 2;
                float totalHeight = Math.Max(iconSize.Y, Math.Max(nameSize.Y, distSize.Y)) + 8 + pad * 2;
                Vector2 panelSize = new Vector2(totalWidth, totalHeight);
                Vector2 panelPos = new Vector2(screenPos.X - panelSize.X / 2, screenPos.Y - 40);

                TileInfo info = new TileInfo();
                info.pos = p;
                info.item = item;
                info.name = name;
                info.dist = dist;
                info.spos = panelPos;
                info.psize = panelSize;
                candidates.Add(info);
            }
        }

        // 按距离从小到大排序
        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));

        List<Rectangle> occupied = new();

        // 渐变色起始和结束（可根据喜好调整）
        Vector4 gradStart = new Vector4(1f, 1f, 0.9f, 1f);   // 米白
        Vector4 gradEnd = new Vector4(1f, 0.8f, 0.2f, 1f);   // 橙黄

        
        foreach (TileInfo info in candidates)
        {
            if (info == null) continue;
            Rectangle rect = new Rectangle((int)info.spos.X, (int)info.spos.Y, (int)info.psize.X, (int)info.psize.Y);
            bool overlap = false;
            foreach (var occ in occupied)
            {
                if (rect.Intersects(occ))
                {
                    overlap = true;
                    break;
                }
            }
            if (overlap) continue;

            occupied.Add(rect);

            // 绘制背景
            uint bgCol = ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
            drawList.AddRectFilled(info.spos, info.spos + info.psize, bgCol, 6f);

            // 图标
            Vector2 iconSize = new Vector2(24, 24);
            Vector2 iconPos = info.spos + new Vector2(10, (info.psize.Y - iconSize.Y) / 2);

            if (info.item == null) continue;
            ImGuiUtil.DrawItemCentered(drawList, info.item.inner, iconPos + iconSize / 2, iconSize.X);

            // 名称（渐变色）
            Vector2 nameSize = ImGui.CalcTextSize(info.name);
            Vector2 namePos = info.spos + new Vector2(iconSize.X + 8 + 10, (info.psize.Y - nameSize.Y) / 2);

            if (string.IsNullOrEmpty(info.name)) continue;
            DrawGrad(drawList, namePos, info.name, gradStart, gradEnd);

            // 距离（渐变色）
            string distText = $"{(int)info.dist}格";
            Vector2 distSize = ImGui.CalcTextSize(distText);
            Vector2 distPos = info.spos + new Vector2(iconSize.X + 8 + nameSize.X + 8 + 10, (info.psize.Y - distSize.Y) / 2);
            DrawGrad(drawList, distPos, distText, gradStart, gradEnd);
        }

        ImGui.PopFont();
    }
    #endregion
}