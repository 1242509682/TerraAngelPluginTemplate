using MonoMod.RuntimeDetour;
using System.Reflection;
using Terraria;

namespace MyPlugin;

public class NPCEventSystem
{
    private static Hook? NpcUpdateHook; // 钩子
    public static event EventHandler<NPCUpdateEventArgs>? OnNPCUpdate; // NPC更新事件

    public static void Register()
    {
        // 获取NPC.UpdateNPC方法的反射信息
        MethodInfo UpdateNPC = typeof(NPC).GetMethod("UpdateNPC", BindingFlags.Instance | BindingFlags.Public)!;
        MethodInfo NewUpdateNPC = typeof(NPCEventSystem).GetMethod(nameof(OnUpdateNPC), BindingFlags.Static | BindingFlags.Public)!;

        // 创建钩子
        NpcUpdateHook = new Hook(UpdateNPC, NewUpdateNPC);
    }

    public static void Dispose()
    {
        // 卸载钩子
        NpcUpdateHook?.Dispose();
        NpcUpdateHook = null;
        OnNPCUpdate = null;
    }

    // 钩住的UpdateNPC方法
    public static void OnUpdateNPC(Action<NPC, int> orig, NPC npc, int whoAmI)
    {
        orig(npc, whoAmI);  // 调用原始方法
        OnNPCUpdate?.Invoke(null, new NPCUpdateEventArgs(whoAmI, npc)); // 触发事件
    }
}

// 使用记录类型简化事件参数
public record NPCUpdateEventArgs(int whoAmI, NPC npc);

internal record NPCInfo(int ID, string Name, bool IsTownNPC);
