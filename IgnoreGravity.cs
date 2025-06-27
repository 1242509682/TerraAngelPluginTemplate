using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Renderers;
using Terraria.Graphics;
using Terraria;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Terraria.GameContent;
using static MyPlugin.MyPlugin;
using System.Numerics;

namespace MyPlugin;

internal class IgnoreGravity
{
    public static Hook? DoDrawHook; // 处理重力药水不反转屏幕的钩子
    public static Hook? DrawPlayerFullHook;
    public static Hook? ItemCheckHook;
    public static Hook? SmartInteractLookupHook;
    public static Hook? SmartCursorLookupHook;
    public static Hook? QuickGrappleHook;
    public static ILHook? PlayerUpdateHook; // 保存IL钩子实例
    internal static float GravDir = 1f;     // 重力方向缓存

    #region 添加反重力药水钩子
    public static void AddGravityHooks()
    {
        // DoDraw 方法（处理重力药水不反转屏幕）
        var DoDraw = typeof(Main).GetMethod("DoDraw", BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(GameTime)], null)!;
        var NewDoDraw = typeof(IgnoreGravity).GetMethod("OnDoDraw", BindingFlags.Static | BindingFlags.Public)!;
        DoDrawHook = new Hook(DoDraw, NewDoDraw);

        // ItemCheck 方法 （处理重力药水不反转屏幕时物品能正常使用）
        MethodInfo originalItemCheck = typeof(Player).GetMethod("ItemCheck", BindingFlags.Instance | BindingFlags.Public)!;
        MethodInfo modifiedItemCheck = typeof(IgnoreGravity).GetMethod("OnItemCheck", BindingFlags.Static | BindingFlags.Public)!;
        ItemCheckHook = new Hook(originalItemCheck, modifiedItemCheck);

        // SmartInteractLookup 方法（处理重力药水不反转屏幕时智能交互）
        MethodInfo SmartInteractLookup = typeof(Player).GetMethod("SmartInteractLookup",
            BindingFlags.Instance | BindingFlags.Public)!;
        MethodInfo NewSmartInteractLookup = typeof(IgnoreGravity).GetMethod("OnSmartInteractLookup",
            BindingFlags.Static | BindingFlags.Public)!;
        SmartInteractLookupHook = new Hook(SmartInteractLookup, NewSmartInteractLookup);

        // SmartCursorLookup 方法（处理重力药水不反转屏幕时智能光标）
        MethodInfo SmartCursorLookup = typeof(SmartCursorHelper).GetMethod("SmartCursorLookup",
            BindingFlags.Static | BindingFlags.Public)!;
        MethodInfo NewSmartCursorLookup = typeof(IgnoreGravity).GetMethod("OnSmartCursorLookup",
            BindingFlags.Static | BindingFlags.Public)!;
        SmartCursorLookupHook = new Hook(SmartCursorLookup, NewSmartCursorLookup);

        // QuickGrapple 方法（处理重力药水不反转屏幕时快速抓钩）
        MethodInfo QuickGrapple = typeof(Player).GetMethod("QuickGrapple",
            BindingFlags.Instance | BindingFlags.Public)!;
        MethodInfo NewQuickGrapple = typeof(IgnoreGravity).GetMethod("OnQuickGrapple",
            BindingFlags.Static | BindingFlags.Public)!;
        QuickGrappleHook = new Hook(QuickGrapple, NewQuickGrapple);

        // DrawPlayerFull 方法（处理玩家全身渲染）
        MethodInfo DrawPlayerFull = typeof(LegacyPlayerRenderer).GetMethod("DrawPlayerFull", BindingFlags.Instance | BindingFlags.NonPublic)!;
        MethodInfo NewDrawPlayerFull = typeof(IgnoreGravity).GetMethod("OnDrawPlayerFull", BindingFlags.Static | BindingFlags.Public)!;
        DrawPlayerFullHook = new Hook(DrawPlayerFull, NewDrawPlayerFull);

        // 添加IL钩子来修正方块挖掘位置
        MethodInfo UpdateMethod = typeof(Player).GetMethod("Update", BindingFlags.Public | BindingFlags.Instance)!;
        if (UpdateMethod != null)
        {
            PlayerUpdateHook = new ILHook(UpdateMethod, Player_Update);
        }
    }
    #endregion

    #region 卸载反重力药水钩子
    public static void DelGravityHooks()
    {
        // 卸载重力药水不反转屏幕相关钩子
        DoDrawHook?.Dispose();
        DoDrawHook = null;
        ItemCheckHook?.Dispose();
        ItemCheckHook = null;
        SmartInteractLookupHook?.Dispose();
        SmartInteractLookupHook = null;
        SmartCursorLookupHook?.Dispose();
        SmartCursorLookupHook = null;
        QuickGrappleHook?.Dispose();
        QuickGrappleHook = null;
        DrawPlayerFullHook?.Dispose();
        DrawPlayerFullHook = null;
        // 卸载IL钩子
        PlayerUpdateHook?.Dispose();
        PlayerUpdateHook = null;
    }
    #endregion

    #region 重力药水Buff不会反转屏幕
    // 重力药水Buff不会反转屏幕时渲染
    public static void OnDoDraw(Action<Main, GameTime> orig, Main self, GameTime gameTime)
    {
        var plr = Main.LocalPlayer;
        if (!Config.IgnoreGravity || plr.gravDir != -1f)
        {
            orig(self, gameTime);
            return;
        }

        GravDir = Main.LocalPlayer.gravDir;
        Main.LocalPlayer.gravDir = 1f;
        orig(self, gameTime);
        Main.LocalPlayer.gravDir = GravDir;
    }

    // 重力药水Buff不会反转屏幕时渲染玩家全身
    public static void OnDrawPlayerFull(Action<LegacyPlayerRenderer, Camera, Player> orig, LegacyPlayerRenderer self, Camera camera, Player plr)
    {
        if (!Config.IgnoreGravity || plr.gravDir != -1f)
        {
            orig(self, camera, plr);
            return;
        }

        plr.gravDir = GravDir;
        orig.Invoke(self, camera, plr);
        plr.gravDir = 1f;
    }
    #endregion

    #region 使用IL钩子修复反重力时 挖矿和放置方块的准确屏幕位置
    public static void Player_Update(ILContext il)
    {
        ILCursor val = new ILCursor(il);
        val.GotoNext(MoveType.After,
            i => i.Match(OpCodes.Conv_I4),
            i => i.MatchStsfld(typeof(Player), "tileTargetY"),
            i => i.MatchLdarg(0),
            i => i.MatchLdfld<Player>("gravDir")
        );

        val.EmitDelegate<Func<float, float>>(gravDir =>
        {
            return 1f; // 强制使用正常重力方向计算方块坐标
        });
    }
    #endregion

    #region 重力反转时修正鼠标坐标
    public static void OnItemCheck(Action<Player> orig, Player plr)
    {
        // 仅当启用重力控制且当前重力方向反转时处理
        if (!Config.IgnoreGravity || plr.gravDir != -1f)
        {
            orig(plr);
            return;
        }

        int mouseY = Main.mouseY;
        Main.mouseY = Main.screenHeight - Main.mouseY; // 翻转鼠标Y坐标
        orig(plr);
        Main.mouseY = mouseY;   // 恢复原始鼠标位置
    }
    #endregion

    #region 重力反转时智能交互修正
    public static void OnSmartInteractLookup(Action<Player> orig, Player plr)
    {
        // 仅当启用重力控制且当前重力方向反转时处理
        if (!Config.IgnoreGravity || plr.gravDir != -1f)
        {
            orig(plr);
            return;
        }

        plr.gravDir = 1f; // 临时设置正常重力方向
        orig(plr);
        plr.gravDir = -1f; // 恢复原始重力方向
    }
    #endregion

    #region 反重力时智能光标修正
    public static void OnSmartCursorLookup(Action<Player> orig, Player plr)
    {
        // 仅当启用重力控制且当前重力方向反转时处理
        if (!Config.IgnoreGravity || plr.gravDir != -1f)
        {
            orig(plr);
            return;
        }

        plr.gravDir = 1f; // 临时设置正常重力方向
        orig(plr);
        plr.gravDir = -1f; // 恢复原始重力方向
    }
    #endregion

    #region 反重力时快速抓钩修正
    public static void OnQuickGrapple(Action<Player> orig, Player plr)
    {
        // 仅当启用重力控制且当前重力方向反转时处理
        if (!Config.IgnoreGravity || plr.gravDir != -1f)
        {
            orig(plr);
            return;
        }
        plr.gravDir = 1f; // 临时设置正常重力方向
        orig(plr);
        plr.gravDir = -1f; // 恢复原始重力方向
    }
    #endregion

}
