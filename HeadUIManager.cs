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
using static MyPlugin.Utils;

namespace MyPlugin;

/// <summary>
/// 玩家头顶UI与图格头顶UI的管理类，负责绘制、交互及额外窗口。
/// </summary>
internal class HeadUIManager
{
    // 公共静态字段，供主类赋值
    public static ImFontPtr chFont;      // 中文字体指针（用于正确显示中文）
    public static Item? atkIcon;         // 攻击图标（物品实例，如光束剑）
    public static Item? defIcon;         // 防御图标（物品实例，如钴蓝护盾）
    public static Item? lifeIcon;        // 生命图标（物品实例，如心）

    // 全局UI重叠检测（所有头顶UI共享）
    private static List<Rectangle> usedRects = new();   // 本帧已绘制的UI矩形
    private static long lastFrame = -1;                 // 上次清空的帧号

    // 每帧清空一次矩形列表（利用帧号避免重复清空）
    private static void clearRect()
    {
        if (Main.GameUpdateCount != lastFrame)
        {
            usedRects.Clear();
            lastFrame = Main.GameUpdateCount;
        }
    }

    // 玩家UI内部类：存储单个玩家的屏幕面板位置、尺寸和距离
    private class PInfo
    {
        public Player? plr;      // 玩家对象引用
        public Vector2 pPos;     // 面板左上角屏幕坐标
        public Vector2 pSz;      // 面板尺寸（宽、高）
        public float dist;       // 玩家与本地玩家的距离（格数）
    }

    // 图格UI内部类：存储单个宝藏图格的物品信息及屏幕面板
    private class TileInfo
    {
        public Point pos;        // 图格坐标（x, y）
        public WorldItem? it;    // 图格掉落的物品对象
        public string? n;        // 物品名称（中文）
        public float d;          // 图格与本地玩家的距离（格数）
        public Vector2 p;        // 面板左上角屏幕坐标
        public Vector2 s;        // 面板尺寸
    }

    // 可点击区域：记录面板的矩形区域及其关联数据（用于点击交互）
    private class CArea
    {
        public Rectangle rect;   // 屏幕矩形区域
        public Player? player;   // 关联数据（玩家对象）
    }

    private static List<CArea> areas = new();      // 存储当前帧所有可点击面板区域
    private static bool showMenu = false;          // 是否显示“更多设置”窗口
    private static Vector2 menuPos;                // “更多设置”窗口的屏幕位置
    private static Player? curPlayer = null;       // 当前选中的玩家对象

    // 高亮/点击效果：用于点击反馈（短暂改变背景色）
    private static int clickIdx = -1;              // 被点击的区域索引
    private static long clickTime = 0;             // 点击时的游戏帧计数

    // 图格UI缓存：避免每帧重复调用 GetTileItem（性能优化）
    private static Dictionary<Point, WorldItem> tileCache = new();
    private static long lastRef = 0;               // 上次清空缓存的帧计数
    private const int refInt = 60;                 // 每 60 帧清空一次缓存

    // NPC 伤害 UI 相关
    public static NPC? curNPC = null;           // 当前受击的高亮 NPC
    public static int NpcDamage = 0;            // 最后一次伤害值
    public static Dictionary<NPC, int> maxLifeSeen = new();

    #region 玩家头顶UI
    /// <summary>
    /// 绘制其他玩家的头顶信息面板（包含名称、属性、血条等）。
    /// 支持远距离简化标记、近距离动态面板、鼠标悬浮/点击交互。
    /// </summary>
    public static unsafe void DrawHeadUI()
    {
        clearRect();   // 每帧清空一次全局矩形列表

        // 使用中文字体，确保名称和数值正常显示
        ImGui.PushFont(chFont);

        ImDrawListPtr dl = ImGui.GetBackgroundDrawList();
        if (dl.NativePtr == null) return;

        Vector2 dSize = ImGui.GetIO().DisplaySize;          // 屏幕尺寸
        Player local = Main.LocalPlayer;                    // 本地玩家
        Vector4 gS = new Vector4(0.65f, 0.84f, 0.92f, 1f); // 渐变起始色（淡青）
        Vector4 gE = new Vector4(0.96f, 0.97f, 0.69f, 1f); // 渐变结束色（淡黄）
        Vector2 mouse = InputSystem.MousePosition;          // 鼠标屏幕坐标

        List<PInfo> cand = new();   // 候选玩家列表（近距离且可能显示完整面板）
        areas.Clear(); // 清空上一帧的可点击区域

        // === 第一遍遍历：收集所有玩家并区分远距离/近距离 ===
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player plr = Main.player[i];
            if (!plr.active || plr.dead) continue;

            if (!Config.ShowMeHeadUI && plr.whoAmI == Main.myPlayer) continue;

            // 计算玩家头顶的屏幕坐标（头顶上方10像素）
            Vector2 hPos = Util.WorldToScreenDynamic(plr.Top - new Vector2(0, 10));
            // 计算与本地玩家的距离（格数）
            float dist = local.Center.Distance(plr.Center) / 16f;
            // 临时面板尺寸和位置（仅用于初步判定是否在屏幕内或鼠标悬浮）
            Vector2 tmpSz = new Vector2(200, 60);
            Vector2 tmpPos = hPos - new Vector2(tmpSz.X / 2, tmpSz.Y) + new Vector2(0, -22);

            // 如果开启了“仅鼠标悬浮显示”，则检查鼠标是否在玩家碰撞箱或粗略面板内
            if (Config.ShowHeadUIOnlyOnHover)
            {
                // 玩家碰撞箱屏幕矩形
                Rectangle hit = new Rectangle(
                    (int)Util.WorldToScreenDynamic(plr.Hitbox.TopLeft()).X,
                    (int)Util.WorldToScreenDynamic(plr.Hitbox.TopLeft()).Y,
                    plr.Hitbox.Width, plr.Hitbox.Height);

                // 粗略面板矩形（临时尺寸）
                Rectangle preRect = new Rectangle((int)tmpPos.X, (int)tmpPos.Y, (int)tmpSz.X, (int)tmpSz.Y);
                if (!hit.Contains((int)mouse.X, (int)mouse.Y) && !preRect.Contains((int)mouse.X, (int)mouse.Y))
                    continue; // 鼠标既不在玩家身上也不在粗略面板上，跳过
            }

            // 远距离标记（距离大于设定阈值）
            if (dist > Config.HeadDist)
            {
                // 合成文本：“玩家名 距离格”
                string full = $"{plr.name} {(int)dist}格";
                Vector2 tSz = ImGui.CalcTextSize(full);
                float pad = 4f;
                bool onScr = hPos.X >= 0 && hPos.X <= dSize.X && hPos.Y >= 0 && hPos.Y <= dSize.Y;
                Vector2 fPos; // 最终屏幕位置

                if (onScr)
                {
                    // 屏幕内：显示在头顶上方30像素
                    fPos = hPos - new Vector2(0, 30);
                    fPos.Y = Math.Max(fPos.Y, 10);
                }
                else
                {
                    // 屏幕外：计算屏幕边缘指示器位置（指向玩家方向）
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
                // 绘制半透明黑色背景
                Vector2 bgMin = fPos - new Vector2(pad, pad);
                Vector2 bgMax = fPos + tSz + new Vector2(pad, pad);
                dl.AddRectFilled(bgMin, bgMax, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.6f)), 4f);
                // 绘制渐变色文本（名称+距离）
                DrawGrad(dl, fPos, full, gS, gE);
            }
            else
            {
                // 加入候选列表（后续会重新计算精确面板尺寸）
                cand.Add(new PInfo { plr = plr, pPos = tmpPos, pSz = tmpSz, dist = dist });
            }
        }

        // 如果没有候选近距离玩家，直接返回
        if (cand.Count == 0) { ImGui.PopFont(); return; }

        // 按距离从小到大排序（近的优先绘制，减少遮挡）
        cand.Sort((a, b) => a.dist.CompareTo(b.dist));

        // 本地玩家碰撞箱（屏幕空间），用于避免UI遮挡自己
        Rectangle localHit = new Rectangle(
            (int)Util.WorldToScreenDynamic(local.Hitbox.TopLeft()).X,
            (int)Util.WorldToScreenDynamic(local.Hitbox.TopLeft()).Y,
            local.Hitbox.Width, local.Hitbox.Height);

        // === 流光 + 呼吸效果参数（每帧变化） ===
        float flow = (float)(Main.GameUpdateCount * 0.015) % 1f;  // 颜色流动相位（0~1）
        float breath = 0.85f + 0.25f * (float)Math.Cos(Main.GameUpdateCount * 0.06); // 呼吸亮度因子
        breath = Math.Clamp(breath, 0.6f, 1.1f);
        Vector4 colA = new Vector4(0.2f, 0.8f, 1.0f, 1f);  // 流光起始色（青）
        Vector4 colB = new Vector4(1.0f, 0.6f, 0.2f, 1f);  // 流光结束色（橙）
        // 计算外框四个角的渐变颜色（顺时针方向）
        uint cTL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.00f + flow) % 1f) * breath);
        uint cTR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.25f + flow) % 1f) * breath);
        uint cBR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.50f + flow) % 1f) * breath);
        uint cBL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.75f + flow) % 1f) * breath);

        int idx = 0; // 当前面板索引（用于点击反馈）
        foreach (PInfo info in cand)
        {
            Player plr = info.plr!;
            // 重新计算精确的头顶屏幕坐标（因为玩家可能移动）
            Vector2 hPos = Util.WorldToScreenDynamic(plr.Top - new Vector2(0, 10));

            // --- 动态计算面板尺寸（根据内容自适应） ---
            Item held = plr.inventory[plr.selectedItem];
            bool hasHeld = held?.type > 0 && !held.IsAir;
            Vector2 iSz = new Vector2(16, 16);   // 图标大小（16x16）

            // 玩家名称（超过6字截断并加省略号）
            string rawName = plr.name;
            string pName = rawName.Length > 6 ? rawName.Substring(0, 5) + "…" : rawName;
            Vector2 nameSz = ImGui.CalcTextSize(pName);

            // 攻击、防御、生命数值及文本尺寸
            int atk = plr.GetWeaponDamage(plr.HeldItem);
            int def = plr.statDefense;
            int life = plr.statLife;
            int lifeMax = plr.statLifeMax;
            string lifeValue = $"{life}/{lifeMax}";   // 直接显示 "当前/最大"
            Vector2 aSz = ImGui.CalcTextSize(atk.ToString());
            Vector2 dSz = ImGui.CalcTextSize(def.ToString());
            Vector2 lSz = ImGui.CalcTextSize(lifeValue);

            // 各项宽度计算（图标+数值+间距）
            float itemW = hasHeld ? iSz.X + 4 : 0;                    // 手持物品宽度
            float atkItemW = iSz.X + 4 + aSz.X;                      // 攻击项（图标+数值）
            float defItemW = iSz.X + 4 + dSz.X;                      // 防御项
            float lifeItemW = iSz.X + 4 + lSz.X;                     // 生命项
            float spc = 8;                                           // 项间间距
            float pad = 8;                                           // 左右内边距
            float totalW = itemW + nameSz.X + spc + atkItemW + spc + defItemW + spc + lifeItemW;
            float panelW = totalW + pad * 2;                         // 面板总宽度

            // 行高：取图标和各文本高度的最大值
            float rowH = Math.Max(iSz.Y, Math.Max(nameSz.Y, Math.Max(aSz.Y, Math.Max(dSz.Y, lSz.Y))));
            float panelH = Math.Max(rowH + 1, 36);                   // 面板高度，最小36像素
            Vector2 pSz = new Vector2(panelW, panelH);
            // 面板位置：头顶上方22像素，水平居中
            Vector2 pPos = hPos - new Vector2(pSz.X / 2, pSz.Y) + new Vector2(0, -22);

            Rectangle rect = new Rectangle((int)pPos.X, (int)pPos.Y, (int)pSz.X, (int)pSz.Y);
            // 避免遮挡本地玩家自己
            if (rect.Intersects(localHit)) continue;
            // 避免与其他已绘制面板重叠（全局检测）
            if (usedRects.Any(r => rect.Intersects(r))) continue;

            // 将计算好的精确位置和尺寸存回候选信息（供后续点击区域使用）
            info.pPos = pPos;
            info.pSz = pSz;

            // --- 绘制外框（流光渐变） ---
            Vector2 outPos = pPos - new Vector2(1, 1);
            Vector2 outSz = pSz + new Vector2(2, 2);
            dl.AddRectFilledMultiColor(outPos, outPos + outSz, cTL, cTR, cBR, cBL);

            // --- 内部背景（根据鼠标悬浮和点击状态改变颜色） ---
            bool isHover = rect.Contains((int)mouse.X, (int)mouse.Y);
            uint bgCol;
            // 如果刚被点击（10帧内），显示橘黄色
            if (isHover && clickIdx == idx && Main.GameUpdateCount - clickTime < 10)
                bgCol = ImGui.GetColorU32(new Vector4(0.8f, 0.6f, 0.2f, 0.9f));
            else if (isHover)  // 悬浮时深蓝灰色
                bgCol = ImGui.GetColorU32(new Vector4(0.2f, 0.3f, 0.4f, 0.9f));
            else               // 默认半透明黑色
                bgCol = ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
            dl.AddRectFilled(pPos, pPos + pSz, bgCol, 6f);  // 圆角矩形

            // --- 第一行：手持图标、名称、攻击、防御、生命 ---
            float startX = pPos.X + pad;
            float centerY = pPos.Y + 6 + rowH / 2;           // 垂直中心线（y坐标）
            float curX = startX;

            // 手持物品图标
            if (hasHeld)
            {
                Vector2 iconPos = new Vector2(curX, centerY - iSz.Y / 2);
                ImGuiUtil.DrawItemCentered(dl, held, iconPos + iSz / 2, iSz.X);
                curX += iSz.X + 4;
            }

            // 玩家名称（渐变色）
            Vector2 namePos = new Vector2(curX, centerY - nameSz.Y / 2);
            DrawGrad(dl, namePos, pName, gS, gE);
            curX += nameSz.X + spc;

            // 攻击数值（青色）
            Vector2 aIconPos = new Vector2(curX, centerY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(dl, atkIcon!, aIconPos + iSz / 2, iSz.X);
            Vector2 aTxtPos = new Vector2(curX + iSz.X + 4, centerY - aSz.Y / 2);
            Vector4 cyan = new Vector4(0f, 1f, 1f, 1f);        // 纯青色
            DrawGrad(dl, aTxtPos, atk.ToString(), cyan, cyan);
            curX += iSz.X + 4 + aSz.X + spc;

            // 防御数值（天蓝色）
            Vector2 dIconPos = new Vector2(curX, centerY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(dl, defIcon!, dIconPos + iSz / 2, iSz.X);
            Vector2 dTxtPos = new Vector2(curX + iSz.X + 4, centerY - dSz.Y / 2);
            Vector4 skyBlue = new Vector4(0.53f, 0.81f, 0.98f, 1f); // 天蓝色
            DrawGrad(dl, dTxtPos, def.ToString(), skyBlue, skyBlue);
            curX += iSz.X + 4 + dSz.X + spc;

            // 生命数值（深红色）
            Vector2 lIconPos = new Vector2(curX, centerY - iSz.Y / 2);
            ImGuiUtil.DrawItemCentered(dl, lifeIcon!, lIconPos + iSz / 2, iSz.X);
            Vector2 lTxtPos = new Vector2(curX + iSz.X + 4, centerY - lSz.Y / 2);
            Vector4 darkRed = new Vector4(1f, 0.4f, 0.4f, 1f);     // 深红色
            DrawGrad(dl, lTxtPos, lifeValue, darkRed, darkRed);
            curX += iSz.X + 4 + lSz.X + spc;

            // --- 血条（底部细条，渐变，带裁剪避免溢出圆角） ---
            float hpW = pSz.X - pad * 2;               // 血条宽度（扣除左右边距）
            float hpX = pPos.X + pad;
            float hpY = pPos.Y + panelH - 6;           // 距离底部6像素（原8像素可能太近）
            float lp = Math.Clamp((float)plr.statLife / plr.statLifeMax, 0f, 1f);
            Vector2 hpPos = new Vector2(hpX, hpY);
            Vector2 hpSz = new Vector2(hpW, 4);        // 血条高度4像素

            // 先裁剪到面板内部（避免圆角处穿出）
            dl.PushClipRect(pPos, pPos + pSz, true);
            // 深灰色背景
            dl.AddRectFilled(hpPos, hpPos + hpSz, ImGui.GetColorU32(new Vector4(0.3f, 0.3f, 0.3f, 1f)), 0f);
            if (lp > 0f)
            {
                float fillW = hpW * lp;
                // 防止浮点误差导致超出
                fillW = Math.Min(fillW, hpW);
                Vector2 fillPos = hpPos;
                Vector2 fillSz = new Vector2(fillW, hpSz.Y);
                uint leftCol = ImGui.GetColorU32(colA * breath);
                uint rightCol = ImGui.GetColorU32(colB * breath);
                dl.AddRectFilledMultiColor(fillPos, fillPos + fillSz, leftCol, rightCol, rightCol, leftCol);
            }
            dl.PopClipRect();

            // --- 记录可点击区域（用于打开更多菜单） ---
            Rectangle wholeRect = new Rectangle((int)pPos.X, (int)pPos.Y, (int)pSz.X, (int)pSz.Y);
            areas.Add(new CArea { rect = wholeRect, player = plr });

            // 将当前面板加入全局列表，供后续UI重叠检测
            usedRects.Add(rect);

            idx++; // 面板索引递增
        }

        ImGui.PopFont();
    }
    #endregion

    #region 正在伤害的NPC 头顶UI
    /// <summary>
    /// 绘制当前受击 NPC 的头顶信息面板（单行：名称、攻防、伤害、距离，血条内显示生命值）
    /// </summary>
    /// <summary>
    /// 绘制当前受击 NPC 的头顶信息面板（单行：名称、攻防、伤害、距离，血条内显示生命值）
    /// </summary>
    public static void DrawNPCUI()
    {
        clearRect();   // 每帧清空一次全局矩形列表

        // 使用中文字体
        ImGui.PushFont(chFont);

        if (!Config.ShowNPCDamageUI ||
            curNPC == null || !curNPC.active ||
            curNPC.life <= 0)
        {
            // 清理失效 NPC 的缓存
            if (curNPC != null && (!curNPC.active || curNPC.life <= 0))
            {
                maxLifeSeen.Remove(curNPC);
                curNPC = null;
            }
            ImGui.PopFont();
            return;
        }

        Player plr = Main.LocalPlayer;
        if (plr == null) { ImGui.PopFont(); return; }

        // 距离检查
        float dist = plr.Center.Distance(curNPC.Center) / 16f;
        if (dist > Config.NPCDamageUIDistance) { ImGui.PopFont(); return; }

        // 鼠标悬浮检测（如果开启）
        Vector2 mouse = InputSystem.MousePosition;
        if (Config.ShowNPCUIOnlyOnHover)
        {
            Rectangle hitbox = new Rectangle(
                (int)Util.WorldToScreenDynamic(curNPC.Hitbox.TopLeft()).X,
                (int)Util.WorldToScreenDynamic(curNPC.Hitbox.TopLeft()).Y,
                curNPC.Hitbox.Width, curNPC.Hitbox.Height);
            if (!hitbox.Contains((int)mouse.X, (int)mouse.Y))
            { ImGui.PopFont(); return; }
        }

        // 计算头顶屏幕坐标（上方10像素）
        Vector2 headPos = Util.WorldToScreenDynamic(curNPC.Top - new Vector2(0, 10));

        // 获取 NPC 原始数值
        int rawLife = curNPC.life;
        int rawLifeMax = curNPC.lifeMax;
        int npcAtk = curNPC.damage;
        int npcDef = curNPC.defense;

        // ----- 缓存服务端真实最大生命（客户端不同步的解决方案）-----
        if (!maxLifeSeen.TryGetValue(curNPC, out int seenMax))
            seenMax = rawLifeMax;

        // 如果当前生命超过缓存，说明服务端提高了上限
        if (rawLife > seenMax)
            seenMax = rawLife;
        // 如果客户端收到的 lifeMax 比缓存大，也更新（防御）
        if (rawLifeMax > seenMax)
            seenMax = rawLifeMax;

        maxLifeSeen[curNPC] = seenMax;

        // 计算血条比例（分母用缓存的最大值，分子不超过分母）
        float hpPercent = (float)Math.Min(rawLife, seenMax) / seenMax;
        string lifeText = $"{rawLife}/{seenMax}";   // 文字显示真实值

        // 准备其他文本
        string npcTitle = $"{curNPC.FullName}({curNPC.type})";
        string dmgText = NpcDamage > 0 ? $"-{NpcDamage}" : "0";
        string distText = $"{(int)dist}格";
        string atkStr = npcAtk.ToString();
        string defStr = npcDef.ToString();

        // 预计算尺寸
        Vector2 titleSz = ImGui.CalcTextSize(npcTitle);
        Vector2 dmgSz = ImGui.CalcTextSize(dmgText);
        Vector2 distSz = ImGui.CalcTextSize(distText);
        Vector2 atkSz = ImGui.CalcTextSize(atkStr);
        Vector2 defSz = ImGui.CalcTextSize(defStr);
        Vector2 lifeSz = ImGui.CalcTextSize(lifeText);
        Vector2 iconSz = new Vector2(14, 14);   // 图标大小

        // 组合项宽度
        float atkItemW = iconSz.X + 4 + atkSz.X;
        float defItemW = iconSz.X + 4 + defSz.X;

        float spc = 6;          // 项目间距
        float pad = 8;          // 左右内边距
        float margin = 6;       // 上下内边距
        float gap = 4;          // 内容区域与血条之间的间距
        float bloodH = 18;      // 血条高度

        float contentH = Math.Max(iconSz.Y, Math.Max(titleSz.Y, Math.Max(atkSz.Y, Math.Max(defSz.Y, Math.Max(dmgSz.Y, distSz.Y)))));
        float totalH = margin * 2 + contentH + gap + bloodH;

        float rowW = titleSz.X + spc + atkItemW + spc + defItemW + spc + dmgSz.X + spc + distSz.X;
        float panelW = rowW + pad * 2;

        Vector2 pPos = headPos - new Vector2(panelW / 2, totalH) + new Vector2(0, -22);
        Rectangle rect = new Rectangle((int)pPos.X, (int)pPos.Y, (int)panelW, (int)totalH);

        // 避免遮挡本地玩家
        Player localPlayer = Main.LocalPlayer;
        if (localPlayer != null)
        {
            Rectangle localHit = new Rectangle(
                (int)Util.WorldToScreenDynamic(localPlayer.Hitbox.TopLeft()).X,
                (int)Util.WorldToScreenDynamic(localPlayer.Hitbox.TopLeft()).Y,
                localPlayer.Hitbox.Width, localPlayer.Hitbox.Height);
            if (rect.Intersects(localHit)) { ImGui.PopFont(); return; }
        }

        // 避免与其他头顶UI重叠
        if (usedRects.Any(r => rect.Intersects(r))) { ImGui.PopFont(); return; }

        // 流光 & 呼吸参数
        float flow = (float)(Main.GameUpdateCount * 0.015) % 1f;
        float breath = 0.85f + 0.25f * (float)Math.Cos(Main.GameUpdateCount * 0.06);
        breath = Math.Clamp(breath, 0.6f, 1.1f);
        Vector4 colA = new Vector4(0.2f, 0.8f, 1.0f, 1f);
        Vector4 colB = new Vector4(1.0f, 0.6f, 0.2f, 1f);

        ImDrawListPtr dl = ImGui.GetBackgroundDrawList();

        // 外框渐变
        uint cTL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.00f + flow) % 1f) * breath);
        uint cTR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.25f + flow) % 1f) * breath);
        uint cBR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.50f + flow) % 1f) * breath);
        uint cBL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.75f + flow) % 1f) * breath);
        Vector2 outPos = pPos - new Vector2(1, 1);
        Vector2 outSz = new Vector2(panelW, totalH) + new Vector2(2, 2);
        dl.AddRectFilledMultiColor(outPos, outPos + outSz, cTL, cTR, cBR, cBL);

        // 内部背景
        uint bgCol = ImGui.GetColorU32(new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
        dl.AddRectFilled(pPos, pPos + new Vector2(panelW, totalH), bgCol, 6f);

        // ========== 绘制第一行 ==========
        float startX = pPos.X + pad;
        float centerY = pPos.Y + margin + contentH / 2;
        float curX = startX;

        Vector4 gS = new Vector4(0.65f, 0.84f, 0.92f, 1f);
        Vector4 gE = new Vector4(0.96f, 0.97f, 0.69f, 1f);
        Vector4 cyan = new Vector4(0f, 1f, 1f, 1f);
        Vector4 skyBlue = new Vector4(0.53f, 0.81f, 0.98f, 1f);
        Vector4 yellow = new Vector4(1f, 0.9f, 0.2f, 1f);
        Vector4 white = new Vector4(1f, 1f, 1f, 1f);

        // 1. 名称
        Vector2 namePos = new Vector2(curX, centerY - titleSz.Y / 2);
        DrawGrad(dl, namePos, npcTitle, gS, gE);
        curX += titleSz.X + spc;

        // 2. 攻击
        Vector2 atkIconPos = new Vector2(curX, centerY - iconSz.Y / 2);
        ImGuiUtil.DrawItemCentered(dl, atkIcon, atkIconPos + iconSz / 2, iconSz.X);
        Vector2 atkTxtPos = new Vector2(curX + iconSz.X + 4, centerY - atkSz.Y / 2);
        DrawGrad(dl, atkTxtPos, atkStr, cyan, cyan);
        curX += atkItemW + spc;

        // 3. 防御
        Vector2 defIconPos = new Vector2(curX, centerY - iconSz.Y / 2);
        ImGuiUtil.DrawItemCentered(dl, defIcon, defIconPos + iconSz / 2, iconSz.X);
        Vector2 defTxtPos = new Vector2(curX + iconSz.X + 4, centerY - defSz.Y / 2);
        DrawGrad(dl, defTxtPos, defStr, skyBlue, skyBlue);
        curX += defItemW + spc;

        // 4. 伤害
        Vector2 dmgPos = new Vector2(curX, centerY - dmgSz.Y / 2);
        DrawGrad(dl, dmgPos, dmgText, yellow, yellow);
        curX += dmgSz.X + spc;

        // 5. 距离
        Vector2 distPos = new Vector2(curX, centerY - distSz.Y / 2);
        DrawGrad(dl, distPos, distText, white, white);

        // ========== 绘制血条 ==========
        float barW = panelW - pad * 2;
        float barX = pPos.X + pad;
        float barY = pPos.Y + totalH - bloodH - margin;
        Vector2 barPos = new Vector2(barX, barY);
        Vector2 barSz = new Vector2(barW, bloodH);

        dl.PushClipRect(barPos, barPos + barSz, true);
        dl.AddRectFilled(barPos, barPos + barSz, ImGui.GetColorU32(new Vector4(0.2f, 0.2f, 0.2f, 0.9f)), 3f);

        if (hpPercent > 0)
        {
            float fillW = Math.Min(barW * hpPercent, barW);
            Vector2 fillPos = barPos;
            Vector2 fillSz = new Vector2(fillW, bloodH);
            uint leftCol = ImGui.GetColorU32(colA * breath);
            uint rightCol = ImGui.GetColorU32(colB * breath);
            dl.AddRectFilledMultiColor(fillPos, fillPos + fillSz, leftCol, rightCol, rightCol, leftCol);
        }

        Vector2 lifeTextPos = barPos + new Vector2(barW / 2 - lifeSz.X / 2, (bloodH - lifeSz.Y) / 2);
        dl.AddText(lifeTextPos, ImGui.GetColorU32(new Vector4(1f, 1f, 0.5f, 1f)), lifeText);

        dl.PopClipRect();

        usedRects.Add(rect);

        ImGui.PopFont();
    }
    #endregion

    #region 图格头顶UI
    /// <summary>
    /// 绘制宝藏图格头顶信息面板（图标+名称+距离），支持流光边框、自动避开玩家。
    /// </summary>
    public static void DrawTileUI()
    {
        clearRect();   // 每帧清空一次全局矩形列表

        // 如果配置中未启用图格UI或者当前处于游戏菜单界面，则直接返回
        if (!Config.ShowTileUI || Main.gameMenu) return;

        // 获取本地玩家实例，若为空则返回
        Player pl = Main.LocalPlayer;
        if (pl == null) return;

        // 使用中文字体，确保中文名称正常显示
        ImGui.PushFont(chFont);
        // 获取背景绘制列表，用于在游戏世界中绘制UI
        var dl = ImGui.GetBackgroundDrawList();
        // 获取当前屏幕尺寸（用于屏幕外剔除）
        var scr = ImGui.GetIO().DisplaySize;
        // 获取鼠标屏幕坐标（用于悬浮检测）
        Vector2 mouse = InputSystem.MousePosition;

        // 扫描半径（格数）
        int rng = Config.TreasureRange;
        // 计算扫描区域的图格边界（左上角、右下角）
        int l = Math.Max(0, (int)(pl.Center.X / 16) - rng);
        int r = Math.Min(Main.maxTilesX - 1, (int)(pl.Center.X / 16) + rng);
        int t = Math.Max(0, (int)(pl.Center.Y / 16) - rng);
        int b = Math.Min(Main.maxTilesY - 1, (int)(pl.Center.Y / 16) + rng);

        // 玩家中心所在的图格坐标（浮点数）
        float px = pl.Center.X / 16f;
        float py = pl.Center.Y / 16f;

        // 定期清空物品缓存（防止无限增长，同时保证掉落物变化后能及时更新）
        if (Main.GameUpdateCount - lastRef >= refInt)
        {
            tileCache.Clear();
            lastRef = Main.GameUpdateCount;
        }

        // 预计算所有活跃玩家的屏幕碰撞箱（用于自动隐藏，避免UI遮挡玩家）
        List<Rectangle> hits = new();
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (p.active && !p.dead)
            {
                // 将玩家的世界碰撞箱转换为屏幕坐标矩形
                Rectangle hit = new Rectangle(
                    (int)Util.WorldToScreenDynamic(p.Hitbox.TopLeft()).X,
                    (int)Util.WorldToScreenDynamic(p.Hitbox.TopLeft()).Y,
                    p.Hitbox.Width, p.Hitbox.Height);
                hits.Add(hit);
            }
        }

        // 收集所有需要绘制的候选图格（在扫描范围内且属于额外寻宝表）
        List<TileInfo> cand = new();
        for (int x = l; x <= r; x++)
            for (int y = t; y <= b; y++)
            {
                // 获取图格实例
                Tile? tile = Main.tile[x, y];
                if (tile == null || !tile.Value.active()) continue;
                int id = tile.Value.type;

                // 仅处理配置中的额外寻宝表图格（用户自定义）
                if (!Config.TreasureList.Contains(id)) continue;

                // 使用缓存获取物品信息（避免每帧调用 GetTileItem）
                Point pt = new Point(x, y);
                if (!tileCache.TryGetValue(pt, out WorldItem? it))
                {
                    // 根据图格坐标获取对应的掉落物品
                    it = GetTileItem(x, y);
                    if (it == null || it.type <= 0) continue;
                    tileCache[pt] = it;
                }

                // 获取物品名称（优先使用物品名，否则通过图格ID查找，最后用ID作为后备）
                string nm = it.Name;
                if (string.IsNullOrEmpty(nm))
                    nm = GetTileName(id);
                if (string.IsNullOrEmpty(nm))
                    nm = $"图格{id}";

                // 计算图格与玩家的距离（格数，取整用于显示）
                float dx = px - x, dy = py - y;
                float dst = (float)Math.Sqrt(dx * dx + dy * dy);

                // 图格中心的世界坐标 -> 屏幕坐标
                Vector2 wc = new Vector2(x * 16 + 8, y * 16 + 8);
                Vector2 sp = Util.WorldToScreenDynamic(wc);
                // 如果屏幕坐标远离可视区域，则跳过（性能优化）
                if (sp.X < -100 || sp.X > scr.X + 100 || sp.Y < -100 || sp.Y > scr.Y + 100)
                    continue;

                // 紧凑布局参数：图标大小、内边距、间距
                int iconSz = 16;
                string txt = $"{nm} {(int)dst}格";
                Vector2 txtSz = ImGui.CalcTextSize(txt);
                int pad = 4;   // 内边距
                int spc = 4;   // 图标与文字间距
                               // 面板宽度 = 图标 + 间距 + 文字宽度 + 左右内边距
                float w = iconSz + spc + txtSz.X + pad * 2;
                // 面板高度 = 图标/文字最大高度 + 上下内边距
                float h = Math.Max(iconSz, txtSz.Y) + pad * 2;
                Vector2 pSz = new Vector2(w, h);
                // 面板位置：图格中心屏幕坐标上方偏移40+4像素，水平居中
                Vector2 pPos = new Vector2(sp.X - pSz.X / 2, sp.Y - 40 - 4);

                cand.Add(new TileInfo { pos = pt, it = it, n = nm, d = dst, p = pPos, s = pSz });
            }

        // 如果没有候选图格，则直接返回（恢复字体后返回）
        if (cand.Count == 0) { ImGui.PopFont(); return; }

        // 按距离从小到大排序（近的优先绘制，便于重叠检测）
        cand.Sort((a, b) => a.d.CompareTo(b.d));

        // 悬浮过滤：如果配置了“仅悬浮显示”，则只保留鼠标指针位于面板矩形内的图格
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

        // 渐变色起始和结束（用于文本）
        Vector4 gS = new Vector4(0.65f, 0.84f, 0.92f, 1f);
        Vector4 gE = new Vector4(0.96f, 0.97f, 0.69f, 1f);

        // 流光 + 呼吸效果参数（与玩家UI保持一致）
        float flow = (float)(Main.GameUpdateCount * 0.015) % 1f;          // 颜色相位
        float breath = 0.85f + 0.25f * (float)Math.Cos(Main.GameUpdateCount * 0.06);
        breath = Math.Clamp(breath, 0.6f, 1.1f);                          // 呼吸亮度系数
        Vector4 colA = new Vector4(0.2f, 0.8f, 1.0f, 1f);                 // 流光起始色（青）
        Vector4 colB = new Vector4(1.0f, 0.6f, 0.2f, 1f);                 // 流光结束色（橙）

        // 布局常量（与上面计算一致，再次声明便于阅读）
        int iconSize = 16;
        int padVal = 4;
        int spcVal = 4;

        foreach (var info in toDraw)
        {
            // 当前图格面板的矩形（屏幕空间）
            Rectangle rect = new Rectangle((int)info.p.X, (int)info.p.Y, (int)info.s.X, (int)info.s.Y);

            // 自动隐藏：如果面板与任何玩家的碰撞箱重叠，则跳过绘制（避免遮挡玩家）
            bool blocked = false;
            foreach (var hit in hits) if (rect.Intersects(hit)) { blocked = true; break; }
            if (blocked) continue;

            // 避免与其他头顶UI重叠（全局检测）
            if (usedRects.Any(r => rect.Intersects(r))) continue;

            // 计算外框流光渐变矩形的四个角颜色（随流动和时间变化）
            uint cTL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.00f + flow) % 1f) * breath);
            uint cTR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.25f + flow) % 1f) * breath);
            uint cBR = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.50f + flow) % 1f) * breath);
            uint cBL = ImGui.GetColorU32(Vector4.Lerp(colA, colB, (0.75f + flow) % 1f) * breath);
            // 外框比面板大一圈（1像素），形成边框效果
            Vector2 outPos = info.p - new Vector2(1, 1);
            Vector2 outSz = info.s + new Vector2(2, 2);
            // 绘制四色渐变填充矩形（流光走马灯）
            dl.AddRectFilledMultiColor(outPos, outPos + outSz, cTL, cTR, cBR, cBL);

            // 叠加半透明黑色遮罩，降低背景纹理亮度，使文字和图标更清晰
            dl.AddRectFilled(info.p, info.p + info.s, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.6f)), 6f);

            // 绘制物品图标（居中在面板左侧）
            Vector2 iPos = info.p + new Vector2(padVal, (info.s.Y - iconSize) / 2);
            ImGuiUtil.DrawItemCentered(dl, info.it!.inner, iPos + new Vector2(iconSize / 2, iconSize / 2), iconSize);

            // 绘制文本（名称+距离，整体渐变色）
            string txt = $"{info.n} {(int)info.d}格";
            Vector2 txtSz = ImGui.CalcTextSize(txt);
            Vector2 tPos = info.p + new Vector2(padVal + iconSize + spcVal, (info.s.Y - txtSz.Y) / 2);
            DrawGrad(dl, tPos, txt, gS, gE);

            // 将当前面板加入全局列表，供后续UI重叠检测
            usedRects.Add(rect);
        }

        // 恢复字体
        ImGui.PopFont();
    }
    #endregion

    #region 交互检测（点击玩家面板打开更多菜单）
    /// <summary>
    /// 检测鼠标左键点击，如果点在可点击区域内，则打开/关闭“更多设置”窗口。
    /// 应在 Update 循环中每帧调用。
    /// </summary>
    public static void CheckClicks()
    {
        if (!InputSystem.LeftMousePressed) return;          // 左键未按下
        Vector2 mouse = InputSystem.MousePosition;
        for (int i = 0; i < areas.Count; i++)
        {
            if (areas[i].rect.Contains((int)mouse.X, (int)mouse.Y))
            {
                clickIdx = i;                               // 记录点击的面板索引
                clickTime = Main.GameUpdateCount;           // 记录点击时的帧数（用于高亮反馈）
                SoundEngine.PlaySound(SoundID.MenuTick);    // 播放音效
                if (areas[i].player != null) // 玩家面板
                {
                    curPlayer = areas[i].player;    // 获取玩家对象
                    showMenu = !showMenu;                   // 切换窗口显示状态
                    if (showMenu)
                        menuPos = areas[i].rect.BottomRight();  // 将窗口定位到面板右下面
                }
                break; // 只处理第一个点击的区域
            }
        }
    }
    #endregion

    #region 独立窗口（更多设置）
    /// <summary>
    /// 绘制“更多设置”窗口，显示选中玩家的装备信息，并允许一键获取物品。
    /// 需在 Update 中每帧调用（若 showMenu 为 true）。
    /// </summary>
    public static void DrawMoreWin()
    {
        if (!showMenu) return;

        ImGui.Separator();
        ImGui.PushFont(chFont);
        // 设置窗口初始位置（最近一次点击的面板左上角）
        ImGui.SetNextWindowPos(menuPos, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(380, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("玩家UI设置", ref showMenu, ImGuiWindowFlags.NoCollapse))
        {
            if (curPlayer != null && curPlayer.active)
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

    #region 给予物品给本地玩家
    /// <summary>
    /// 将指定物品（ID和数量）给予本地玩家，并输出聊天提示。
    /// </summary>
    private static void GiveItemToLocal(int itemID, int stack)
    {
        Player local = Main.LocalPlayer;
        if (local != null)
            Utils.GiveItem(local, itemID, stack, false);
        ClientLoader.Chat.WriteLine($"获得 {Lang.GetItemNameValue(itemID)} x{stack}", Color.Yellow);
    }
    #endregion

    #region 寻宝功能
    /// <summary>
    /// 判断给定图格ID是否属于宝藏（箱子、生命水晶、矿石或额外列表中的图格）。
    /// </summary>
    private static bool IsTreasure(int tileID)
    {
        // 箱子（包括普通箱子和金箱子等）
        if (TileID.Sets.BasicChest[tileID])
            return true;
        // 生命水晶
        if (tileID == TileID.Heart) return true;
        // 额外寻宝表
        if (Config.TreasureList.Contains(tileID)) return true;
        // 矿物（所有原版矿石）
        return TileID.Sets.Ore[tileID];
    }

    /// <summary>
    /// 以玩家为中心，扫描指定半径内的宝藏图格，并在每个宝藏位置生成金色粒子特效。
    /// </summary>
    public static void ScanTreasure(Player plr, int range)
    {
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
                    // 图格中心世界坐标
                    Vector2 worldPos = new Vector2(x * 16 + 8, y * 16 + 8);
                    // 产生3个金色火焰粒子
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

    #region 渐变色方法
    /// <summary>
    /// 在指定位置绘制渐变色文本（逐字渐变）。
    /// 当文本长度为1时直接使用起始颜色，避免除零错误。
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
            // 单字符时直接用起始颜色，避免除以0
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