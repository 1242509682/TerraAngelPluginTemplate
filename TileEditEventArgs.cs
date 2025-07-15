using MonoMod.RuntimeDetour;
using System.Reflection;
using Terraria;

namespace MyPlugin;

public class TileEditEventSystem
{
    private static Hook? KillTileHook;
    public static event EventHandler<TileKillEventArgs>? OnTileKill;

    public static void Register()
    {
        MethodInfo KillTile = typeof(WorldGen).GetMethod("KillTile",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(bool)])!;

        MethodInfo NewKillTile = typeof(TileEditEventSystem).GetMethod(
            nameof(Hook_KillTile), BindingFlags.Static | BindingFlags.NonPublic)!;

        KillTileHook = new Hook(KillTile, NewKillTile);
    }

    public static void Dispose()
    {
        KillTileHook?.Dispose();
        OnTileKill = null;
    }

    private delegate void orig_KillTile(int i, int j, bool fail, bool effectOnly, bool noItem);
    private static void Hook_KillTile(orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
    {
        orig(i, j, fail, effectOnly, noItem); // 执行破坏方法后
        var args = new TileKillEventArgs(i, j, fail, effectOnly, noItem);
        OnTileKill?.Invoke(null, args);
    }
}

// 简化的事件参数记录类型
public record TileKillEventArgs(int X, int Y, bool Fail, bool EffectOnly, bool NoItem);

// 使用图格ID与物品名称的记录类型
internal record MinerItem(int TileID, string ItemName);
