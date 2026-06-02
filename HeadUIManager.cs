using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel;
using TerraAngel.Graphics;
using TerraAngel.Input;
using TerraAngel.Utility;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static MyPlugin.MyPlugin;
using static MyPlugin.UITool;

namespace MyPlugin;

internal class HeadUIManager
{
    // 公共静态字段，供主类赋值
    public static ImFontPtr chFont;      // 中文字体
    public static Item? atkIcon;
    public static Item? defIcon;
    public static Item? lifeIcon;

    // 玩家UI内部类
    private class PInfo
    {
        public Player? plr;
        public Vector2 pPos;
        public Vector2 pSz;
        public float dist;
    }

    // 图格UI内部类
    private class TileInfo
    {
        public Point pos;
        public WorldItem? it;
        public string? n;
        public float d;
        public Vector2 p;
        public Vector2 s;
    }

    // 可点击区域
    private class CArea
    {
        public Rectangle rect;
        public int type;
        public object? data;
    }

    private static List<CArea> areas = new();
    private static bool showMenu = false;      // 更多菜单是否显示
    private static Vector2 menuPos;            // 菜单位置
    private static Player? curPlayer = null;

    // 高亮/点击效果
    private static int clickIdx = -1;
    private static long clickTime = 0;

    // 图格UI缓存
    private static Dictionary<Point, WorldItem> tileCache = new();
    private static long lastRef = 0;
    private const int refInt = 60;

    #region 玩家头顶UI
    public static unsafe void DrawHeadUI()
    {
        ImGui.PushFont(chFont);

        ImDrawListPtr dl = ImGui.GetBackgroundDrawList();
        if (dl.NativePtr == null) return;

        Vector2 dSize = ImGui.GetIO().DisplaySize;
        Player local = Main.LocalPlayer;
        Vector4 gS = new Vector4(0.65f, 0.84f, 0.92f, 1f);
        Vector4 gE = new Vector4(0.96f, 0.97f, 0.69f, 1f);
        Vector2 mouse = InputSystem.MousePosition;

        List<PInfo> cand = new();
        areas.Clear();

        // 第一遍遍历：收集近距离玩家（使用粗略尺寸初步判定）
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player plr = Main.player[i];
            if (!plr.active || plr.whoAmI == Main.myPlayer) continue;

            Vector2 hPos = Util.WorldToScreenDynamic(plr.Top - new Vector2(0, 10));
            float dist = local.Center.Distance(plr.Center) / 16f;
            Vector2 tmpSz = new Vector2(200, 60);
            Vector2 tmpPos = hPos - new Vector2(tmpSz.X / 2, tmpSz.Y) + new Vector2(0, -22);

            if (dist > Config.HeadDist)
            {
                // 远距离标记
                string full = $"{plr.name} {(int)dist}格";
                Vector2 tSz = ImGui.CalcTextSize(full);
                float pad = 4f;
                bool onScr = hPos.X >= 0 && hPos.X <= dSize.X && hPos.Y >= 0 && hPos.Y <= dSize.Y;
                Vector2 fPos;
                if (onScr)
                {
                    fPos = hPos - new Vector2(0, 30);
                    fPos.Y = Math.Max(fPos.Y, 10);
                }
                else
                {
                    Vector2 dir = (hPos - dSize / 2).SafeNormalize(Vector2.Zero);
                    if (dir == Vector2.Zero) dir = Vector2.UnitX;
                    float ex, ey;
                    if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                    {
                        ex = (dir.X > 0) ? dSize.X - tSz.X - 20 : 20;
                        ey = Math.Clamp(dSize.Y / 2 + (dir.Y / dir.X) * (ex - dSize.X / 2), 20, dSize.Y - tSz.Y - 20);
                    }
                    else
                    {
                        ey = (dir.Y > 0) ? dSize.Y - tSz.Y - 20 : 20;
                        ex = Math.Clamp(dSize.X / 2 + (dir.X / dir.Y) * (ey - dSize.Y / 2), 20, dSize.X - tSz.X - 20);
                    }
                    fPos = new Vector2(ex, ey);
                }
                Vector2 bgMin = fPos - new Vector2(pad, pad);
                Vector2 bgMax = fPos + tSz + new Vector2(pad, pad);
                dl.AddRectFilled(bgMin, bgMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.6f)), 4f);
                DrawGrad(dl, fPos, full, gS, gE);
            }
            else
            {
                if (plr.dead) continue;
                if (Config.ShowHeadUIOnlyOnHover)
                {
                    Rectangle hit = new Rectangle(
                        (int)Util.WorldToScreenDynamic(plr.Hitbox.TopLeft()).X,
                        (int)Util.WorldToScreenDynamic(plr.Hitbox.TopLeft()).Y,
                        plr.Hitbox.Width, plr.Hitbox.Height);
                    if (!hit.Contains((int)mouse.X, (int)mouse.Y))
                    {
                        Rectangle preRect = new Rectangle((int)tmpPos.X, (int)tmpPos.Y, (int)tmpSz.X, (int)tmpSz.Y);
                        if (!preRect.Contains((int)mouse.X, (int)mouse.Y))
                            continue;
                    }
                }
                cand.Add(new PInfo { plr = plr, pPos = tmpPos, pSz = tmpSz, dist = dist });
            }
        }

        if (cand.Count == 0) { ImGui.PopFont(); return; }

        cand.Sort((a, b) => a.dist.CompareTo(b.dist));

        Rectangle localHit = new Rectangle(
            (int)Util.WorldToScreenDynamic(local.Hitbox.TopLeft()).X,
            (int)Util.WorldToScreenDynamic(local.Hitbox.TopLeft()).Y,
            local.Hitbox.Width, local.Hitbox.Height);

        List<Rectangle> occ = new();

        float flow = (float)(Main.GameUpdateCount * 0.015) % 1f;
        float breath = 0.85f + 0.25f * (float)Math.Cos(Main.GameUpdateCount * 0.06);
        breath = Math.Clamp(breath, 0.6f, 1.1f);
        Vector4 colA = new Vector4(0.2f, 0.8f, 1.0f, 1f);
        Vector4 colB = new Vector4(1.0f, 0.6f, 0.2f, 1f);
        uint cTL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.00f + flow) % 1f) * breath);
        uint cTR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.25f + flow) % 1f) * breath);
        uint cBR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.50f + flow) % 1f) * breath);
        uint cBL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.75f + flow) % 1f) * breath);

        int idx = 0;
        foreach (PInfo info in cand)
        {
            Player plr = info.plr!;
            Vector2 hPos = Util.WorldToScreenDynamic(plr.Top - new Vector2(0, 10));

            // ---------- 动态计算面板尺寸 ----------
            Item held = plr.inventory[plr.selectedItem];
            bool hasHeld = held?.type > 0 && !held.IsAir;
            Vector2 iSz = new Vector2(16, 16);  // 图标大小

            string rawName = plr.name;
            string pName = rawName.Length > 6 ? rawName.Substring(0, 5) + "…" : rawName;
            Vector2 nameSz = ImGui.CalcTextSize(pName);

            int atk = plr.GetWeaponDamage(plr.HeldItem);
            int def = plr.statDefense;
            int life = plr.statLife;
            Vector2 aSz = ImGui.CalcTextSize(atk.ToString());
            Vector2 dSz = ImGui.CalcTextSize(def.ToString());
            Vector2 lSz = ImGui.CalcTextSize(life.ToString());

            float itemW = hasHeld ? iSz.X + 4 : 0;
            float atkItemW = iSz.X + 4 + aSz.X;
            float defItemW = iSz.X + 4 + dSz.X;
            float lifeItemW = iSz.X + 4 + lSz.X;
            float spc = 8;          // 项目间距
            float totalW = itemW + nameSz.X + spc + atkItemW + spc + defItemW + spc + lifeItemW;
            float pad = 8;          // 左右内边距
            float panelW = totalW + pad * 2;
            float rowH = Math.Max(iSz.Y, Math.Max(nameSz.Y, Math.Max(aSz.Y, Math.Max(dSz.Y, lSz.Y))));

            // 间距高度
            float panelH = rowH + 1;
            panelH = Math.Max(panelH, 36);   // 最小高度从60降到52

            Vector2 pSz = new Vector2(panelW, panelH);
            Vector2 pPos = hPos - new Vector2(pSz.X / 2, pSz.Y) + new Vector2(0, -22);

            // 重叠与遮挡检测
            Rectangle rect = new Rectangle((int)pPos.X, (int)pPos.Y, (int)pSz.X, (int)pSz.Y);
            if (rect.Intersects(localHit)) continue;
            bool overlap = false;
            foreach (var r in occ) if (rect.Intersects(r)) { overlap = true; break; }
            if (overlap) continue;
            occ.Add(rect);

            info.pPos = pPos;
            info.pSz = pSz;

            // ---------- 外框渐变 ----------
            Vector2 outPos = pPos - new Vector2(1, 1);
            Vector2 outSz = pSz + new Vector2(2, 2);
            dl.AddRectFilledMultiColor(outPos, outPos + outSz, cTL, cTR, cBR, cBL);

            // ---------- 内部背景 ----------
            bool isHover = rect.Contains((int)mouse.X, (int)mouse.Y);
            uint bgCol;
            if (isHover && clickIdx == idx && Main.GameUpdateCount - clickTime < 10)
                bgCol = ImGui.GetColorU32(new Vector4(0.8f, 0.6f, 0.2f, 0.9f));
            else if (isHover)
                bgCol = ImGui.GetColorU32(new Vector4(0.2f, 0.3f, 0.4f, 0.9f));
            else
                bgCol = ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
            dl.AddRectFilled(pPos, pPos + pSz, bgCol, 6f);

            // ---------- 第一行绘制（手持图标、名称、攻击、防御、生命）----------
            float startX = pPos.X + pad;
            float centerY = pPos.Y + 6 + rowH / 2;   // 上边距8像素，垂直居中
            float curX = startX;

            if (hasHeld)
            {
                Vector2 iconPos = new Vector2(curX, centerY - iSz.Y / 2);
                ImGuiUtil.DrawItemCentered(dl, held, iconPos + iSz / 2, iSz.X);
                curX += iSz.X + 4;
            }

            // 名称
            Vector2 namePos = new Vector2(curX, centerY - nameSz.Y / 2);
            DrawGrad(dl, namePos, pName, gS, gE);
            curX += nameSz.X + spc;

            // 攻击
            Vector2 aIconPos = new Vector2(curX, centerY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(dl, atkIcon!, aIconPos + iSz / 2, iSz.X);
            Vector2 aTxtPos = new Vector2(curX + iSz.X + 4, centerY - aSz.Y / 2);
            DrawGrad(dl, aTxtPos, atk.ToString(), gS, gE);
            curX += iSz.X + 4 + aSz.X + spc;

            // 防御
            Vector2 dIconPos = new Vector2(curX, centerY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(dl, defIcon!, dIconPos + iSz / 2, iSz.X);
            Vector2 dTxtPos = new Vector2(curX + iSz.X + 4, centerY - dSz.Y / 2);
            DrawGrad(dl, dTxtPos, def.ToString(), gS, gE);
            curX += iSz.X + 4 + dSz.X + spc;

            // 生命
            Vector2 lIconPos = new Vector2(curX, centerY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(dl, lifeIcon!, lIconPos + iSz / 2, iSz.X);
            Vector2 lTxtPos = new Vector2(curX + iSz.X + 4, centerY - lSz.Y / 2);
            DrawGrad(dl, lTxtPos, life.ToString(), gS, gE);

            // ---------- 血条（底部细条，无文字）----------
            float hpW = pSz.X - pad * 2;
            float hpX = pPos.X + pad;
            float hpY = pPos.Y + panelH - 8;   // 距离底部8像素
            float lp = (float)plr.statLife / plr.statLifeMax;
            Vector2 hpPos = new Vector2(hpX, hpY);
            Vector2 hpSz = new Vector2(hpW, 4);
            dl.AddRectFilled(hpPos, hpPos + hpSz, ImGui.GetColorU32(new Vector4(0.3f, 0.3f, 0.3f, 1f)));
            dl.AddRectFilled(hpPos, hpPos + new Vector2(hpSz.X * lp, hpSz.Y), ImGui.GetColorU32(new Vector4(1f, 0.2f, 0.2f, 1f)));

            // ---------- 记录点击区域 ----------
            Rectangle wholeRect = new Rectangle((int)pPos.X, (int)pPos.Y, (int)pSz.X, (int)pSz.Y);
            areas.Add(new CArea { rect = wholeRect, type = 10, data = plr });

            idx++;
        }

        ImGui.PopFont();
    }
    #endregion

    #region 图格头顶UI
    public static void DrawTileUI()
    {
        if (!Config.ShowTileUI || Main.gameMenu) return;

        Player pl = Main.LocalPlayer;
        if (pl == null) return;

        ImGui.PushFont(chFont);
        var dl = ImGui.GetBackgroundDrawList();
        var scr = ImGui.GetIO().DisplaySize;
        Vector2 mouse = InputSystem.MousePosition;

        int rng = Config.TreasureRange;
        int l = Math.Max(0, (int)(pl.Center.X / 16) - rng);
        int r = Math.Min(Main.maxTilesX - 1, (int)(pl.Center.X / 16) + rng);
        int t = Math.Max(0, (int)(pl.Center.Y / 16) - rng);
        int b = Math.Min(Main.maxTilesY - 1, (int)(pl.Center.Y / 16) + rng);

        float px = pl.Center.X / 16f;
        float py = pl.Center.Y / 16f;

        if (Main.GameUpdateCount - lastRef >= refInt)
        {
            tileCache.Clear();
            lastRef = Main.GameUpdateCount;
        }

        // 预计算所有玩家碰撞箱（用于自动隐藏）
        List<Rectangle> hits = new();
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (p.active && !p.dead)
            {
                Rectangle hit = new Rectangle(
                    (int)Util.WorldToScreenDynamic(p.Hitbox.TopLeft()).X,
                    (int)Util.WorldToScreenDynamic(p.Hitbox.TopLeft()).Y,
                    p.Hitbox.Width, p.Hitbox.Height);
                hits.Add(hit);
            }
        }

        // 收集候选图格
        List<TileInfo> cand = new();
        for (int x = l; x <= r; x++)
            for (int y = t; y <= b; y++)
            {
                Tile? tile = Main.tile[x, y];
                if (tile == null || !tile.Value.active()) continue;
                int id = tile.Value.type;
                if (!Config.TreasureList.Contains(id)) continue;

                Point pt = new Point(x, y);
                if (!tileCache.TryGetValue(pt, out WorldItem? it))
                {
                    it = GetTileItem(x, y);
                    if (it == null || it.type <= 0) continue;
                    tileCache[pt] = it;
                }

                string nm = it.Name;
                if (string.IsNullOrEmpty(nm)) nm = GetTileName(id);
                if (string.IsNullOrEmpty(nm)) nm = $"图格{id}";

                float dx = px - x, dy = py - y;
                float dst = (float)Math.Sqrt(dx * dx + dy * dy);

                Vector2 wc = new Vector2(x * 16 + 8, y * 16 + 8);
                Vector2 sp = Util.WorldToScreenDynamic(wc);
                if (sp.X < -100 || sp.X > scr.X + 100 || sp.Y < -100 || sp.Y > scr.Y + 100)
                    continue;

                // 紧凑布局参数
                int iconSz = 16; // 图标大小16x16
                string txt = $"{nm} {(int)dst}格";
                Vector2 txtSz = ImGui.CalcTextSize(txt);
                int pad = 4;  // 内边距
                int spc = 4;  // 图标与文字间距
                float w = iconSz + spc + txtSz.X + pad * 2;
                float h = Math.Max(iconSz, txtSz.Y) + pad * 2;
                Vector2 pSz = new Vector2(w, h);
                // 面板位置：上移4像素
                Vector2 pPos = new Vector2(sp.X - pSz.X / 2, sp.Y - 40 - 4);

                cand.Add(new TileInfo { pos = pt, it = it, n = nm, d = dst, p = pPos, s = pSz });
            }

        if (cand.Count == 0) { ImGui.PopFont(); return; }

        cand.Sort((a, b) => a.d.CompareTo(b.d));

        // 悬浮过滤
        List<TileInfo> toDraw;
        if (Config.ShowTileUIOnlyOnHover)
        {
            toDraw = cand.Where(info =>
            {
                Rectangle rect = new Rectangle((int)info.p.X, (int)info.p.Y, (int)info.s.X, (int)info.s.Y);
                return rect.Contains((int)mouse.X, (int)mouse.Y);
            }).ToList();
            if (toDraw.Count == 0) { ImGui.PopFont(); return; }
        }
        else
        {
            toDraw = cand;
        }

        List<Rectangle> occ = new();
        Vector4 gS = new Vector4(0.65f, 0.84f, 0.92f, 1f);
        Vector4 gE = new Vector4(0.96f, 0.97f, 0.69f, 1f);
        float flow = (float)(Main.GameUpdateCount * 0.015) % 1f;
        float breath = 0.85f + 0.25f * (float)Math.Cos(Main.GameUpdateCount * 0.06);
        breath = Math.Clamp(breath, 0.6f, 1.1f);
        Vector4 colA = new Vector4(0.2f, 0.8f, 1.0f, 1f);
        Vector4 colB = new Vector4(1.0f, 0.6f, 0.2f, 1f);

        int iconSize = 16;
        int padVal = 4;
        int spcVal = 4;

        foreach (var info in toDraw)
        {
            Rectangle rect = new Rectangle((int)info.p.X, (int)info.p.Y, (int)info.s.X, (int)info.s.Y);
            // 避开玩家碰撞箱
            bool blocked = false;
            foreach (var hit in hits) if (rect.Intersects(hit)) { blocked = true; break; }
            if (blocked) continue;

            // 避免面板间重叠
            bool overlap = false;
            foreach (var o in occ) if (rect.Intersects(o)) { overlap = true; break; }
            if (overlap) continue;
            occ.Add(rect);

            // 外框流光渐变
            uint cTL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.00f + flow) % 1f) * breath);
            uint cTR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.25f + flow) % 1f) * breath);
            uint cBR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.50f + flow) % 1f) * breath);
            uint cBL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.75f + flow) % 1f) * breath);
            Vector2 outPos = info.p - new Vector2(1, 1);
            Vector2 outSz = info.s + new Vector2(2, 2);
            dl.AddRectFilledMultiColor(outPos, outPos + outSz, cTL, cTR, cBR, cBL);

            // 内部背景（悬停高亮）
            bool hover = rect.Contains((int)mouse.X, (int)mouse.Y);
            uint bgCol = hover ? ImGui.GetColorU32(new Vector4(0.2f, 0.3f, 0.4f, 0.9f))
                               : ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
            dl.AddRectFilled(info.p, info.p + info.s, bgCol, 6f);

            // 图标
            Vector2 iPos = info.p + new Vector2(padVal, (info.s.Y - iconSize) / 2);
            ImGuiUtil.DrawItemCentered(dl, info.it!.inner, iPos + new Vector2(iconSize / 2, iconSize / 2), iconSize);

            // 文字（名称+距离）
            string txt = $"{info.n} {(int)info.d}格";
            Vector2 txtSz = ImGui.CalcTextSize(txt);
            Vector2 tPos = info.p + new Vector2(padVal + iconSize + spcVal, (info.s.Y - txtSz.Y) / 2);
            DrawGrad(dl, tPos, txt, gS, gE);
        }

        ImGui.PopFont();
    }
    #endregion

    #region 交互检测（修改）
    public static void CheckClicks()
    {
        if (!InputSystem.LeftMousePressed) return;
        Vector2 mouse = InputSystem.MousePosition;
        for (int i = 0; i < areas.Count; i++)
        {
            if (areas[i].rect.Contains((int)mouse.X, (int)mouse.Y))
            {
                clickIdx = i;
                clickTime = Main.GameUpdateCount;
                SoundEngine.PlaySound(SoundID.MenuTick);
                if (areas[i].type == 10)  // 玩家面板
                {
                    // 存储当前选中的玩家
                    curPlayer = areas[i].data as Player;
                    showMenu = !showMenu;          // 切换窗口显示状态
                    if (showMenu)
                        menuPos = areas[i].rect.TopLeft(); // 将窗口定位到面板左上角
                }
                break;
            }
        }
    }
    #endregion

    #region 独立窗口（UITool中渲染）
    public static void DrawMoreWin()
    {
        if (!showMenu) return;

        ImGui.Separator();
        ImGui.PushFont(chFont);
        ImGui.SetNextWindowPos(menuPos, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(380, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("玩家UI设置", ref showMenu, ImGuiWindowFlags.NoCollapse))
        {
            if (curPlayer != null && curPlayer.active && !curPlayer.dead)
            {
                ImGui.TextColored(new Vector4(1f, 0.8f, 0.4f, 1f), $"当前玩家：{curPlayer.name}");

                ImGui.Separator();
                // 手持物品
                Item held = curPlayer.HeldItem;
                if (held != null && !held.IsAir)
                {
                    ImGui.Text("手持物品：");
                    ImGui.SameLine();
                    Vector2 iconPos = ImGui.GetCursorScreenPos();
                    ImGuiUtil.DrawItemCentered(ImGui.GetWindowDrawList(), held, iconPos + new Vector2(12, 12), 24f);
                    ImGui.Dummy(new Vector2(24, 24));
                    ImGui.SameLine();
                    ImGui.Text($"{held.Name}");
                    ImGui.SameLine();
                    if (ImGui.Button("获取##held"))
                    {
                        GiveItemToLocal(held.type, held.stack);
                    }
                    ImGui.Spacing();
                }

                // 盔甲栏（头盔、胸甲、护腿）
                ImGui.Text("盔甲：");
                for (int i = 0; i < 3; i++)
                {
                    Item armor = curPlayer.armor[i];
                    if (armor != null && !armor.IsAir)
                    {
                        ImGui.PushID($"armor_{i}");
                        Vector2 iconPos = ImGui.GetCursorScreenPos();
                        ImGuiUtil.DrawItemCentered(ImGui.GetWindowDrawList(), armor, iconPos + new Vector2(12, 12), 24f);
                        ImGui.Dummy(new Vector2(24, 24));
                        ImGui.SameLine();
                        ImGui.Text($"{armor.Name}");
                        ImGui.SameLine();
                        if (ImGui.Button("获取"))
                        {
                            GiveItemToLocal(armor.type, armor.stack);
                        }
                        ImGui.PopID();
                        ImGui.Spacing();
                    }
                }

                // 饰品栏（索引 3~10，共8格）
                ImGui.Text("饰品：");
                for (int i = 3; i <= 10; i++)
                {
                    Item acc = curPlayer.armor[i];
                    if (acc != null && !acc.IsAir)
                    {
                        ImGui.PushID($"acc_{i}");
                        Vector2 iconPos = ImGui.GetCursorScreenPos();
                        ImGuiUtil.DrawItemCentered(ImGui.GetWindowDrawList(), acc, iconPos + new Vector2(12, 12), 24f);
                        ImGui.Dummy(new Vector2(24, 24));
                        ImGui.SameLine();
                        ImGui.Text($"{acc.Name}");
                        ImGui.SameLine();
                        if (ImGui.Button("获取"))
                        {
                            GiveItemToLocal(acc.type, acc.stack);
                        }
                        ImGui.PopID();
                        ImGui.Spacing();
                    }
                }
                ImGui.Separator();
            }
            else
            {
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), "未选中任何玩家（点击头顶UI面板）");
                ImGui.Separator();
            }
        }
        ImGui.End();
        ImGui.PopFont();
    }
    #endregion

    #region 辅助方法：给予物品给本地玩家
    private static void GiveItemToLocal(int itemID, int stack)
    {
        Player local = Main.LocalPlayer;
        if (local != null)
            Utils.GiveItem(local, itemID, stack, false); // 使用已有的 GiveItem 方法
        ClientLoader.Chat.WriteLine($"获得 {Lang.GetItemNameValue(itemID)} x{stack}", Color.Yellow);
    }
    #endregion

    #region 寻宝功能
    // 判断是否为宝藏图格（箱子、生命水晶、矿物）
    private static bool IsTreasure(int tileID)
    {
        // 箱子
        if (TileID.Sets.BasicChest[tileID])
            return true;

        // 生命水晶
        if (tileID == TileID.Heart) return true;

        // 额外图格列表
        if (Config.TreasureList.Contains(tileID)) return true;

        // 矿物列表
        return TileID.Sets.Ore[tileID];
    }

    // 扫描指定范围内的宝藏，并产生粒子提示
    public static void ScanTreasure(Player plr, int range)
    {
        // 计算扫描区域（以玩家为中心）
        int left = Math.Max(0, (int)(plr.position.X / 16) - range);
        int right = Math.Min(Main.maxTilesX - 1, (int)(plr.position.X / 16) + range);
        int top = Math.Max(0, (int)(plr.position.Y / 16) - range);
        int bottom = Math.Min(Main.maxTilesY - 1, (int)(plr.position.Y / 16) + range);

        int found = 0;
        for (int x = left; x <= right; x++)
        {
            for (int y = top; y <= bottom; y++)
            {
                Tile? tile = Main.tile[x, y];
                if (tile.HasValue && tile.Value.active() && IsTreasure(tile.Value.type))
                {
                    // 在方块中心产生金色粒子
                    Vector2 worldPos = new Vector2(x * 16 + 8, y * 16 + 8);

                    for (int i = 0; i < 3; i++)
                    {
                        Dust.NewDust(worldPos, 0, 0, DustID.GoldFlame, 0f, 0f, 0, default, 1.2f);
                    }
                    found++;
                }
            }
        }
    }
    #endregion

    #region 获取图格的中文名称
    public static string GetTileName(int tileID)
    {
        // 通过 createTile 反向查找物品名
        foreach (var kv in ContentSamples.ItemsByType)
        {
            Item item = kv.Value;
            if (item != null && item.createTile == tileID)
                return Lang.GetItemNameValue(item.type);
        }

        return string.Empty;
    }
    #endregion

    #region 获取图格的物品属性
    public static WorldItem GetTileItem(int x, int y)
    {
        var noPrefix = false;
        WorldGen.KillTile_GetItemDrops(x, y, Main.tile[x, y], out int type, out int stack, out _, out _, out noPrefix);
        WorldItem item = new();
        item.SetDefaults(type);
        item.stack = stack;
        return item;
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
    private static unsafe void DrawGrad(ImDrawListPtr drawList, Vector2 pos, string text, Vector4 sCol, Vector4 eCol)
    {
        if (string.IsNullOrEmpty(text)) return;

        float tChars = text.Length;
        float curX = pos.X;

        for (int idx = 0; idx < text.Length; idx++)
        {
            // 当长度为1时，t = 0，直接使用起始颜色
            float t = (tChars == 1) ? 0f : idx / (tChars - 1);
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
}