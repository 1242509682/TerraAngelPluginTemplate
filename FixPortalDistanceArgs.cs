using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Terraria;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

internal class FixPortalDistanceArgs
{
    public static ILHook? PortalAIHook;
    private const float DefaultMaxDistance = 12800f;

    #region  注册与卸载钩子
    public static void Register()
    {
        // 获取 Projectile 的 AI 方法
        MethodInfo aiMethod = typeof(Projectile).GetMethod("AI", BindingFlags.Public | BindingFlags.Instance)!;
        if (aiMethod != null)
        {
            PortalAIHook = new ILHook(aiMethod, FixPortalDistance);
        }
    }

    public static void Dispose()
    {
        PortalAIHook?.Dispose();
        PortalAIHook = null;
    }
    #endregion

    #region 修复传送枪距离方法(IL)
    private static void FixPortalDistance(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        // 定位到具体的距离检查代码位置
        bool found = c.TryGotoNext(
            x => x.MatchCall<Entity>("Distance"),  // 调用 Entity.Distance 方法
            x => x.MatchLdcR4(DefaultMaxDistance) // 加载 12800f 常量
        );

        if (found)
        {
            // 移动到 ldc.r4 12800 指令位置
            c.Index++;

            // 移除原版的 12800f 加载指令
            c.Remove();

            // 将当前实例（this，即Projectile）压入堆栈
            c.Emit(OpCodes.Ldarg_0); // 加载第一个参数（即this）

            // 插入自定义距离获取方法
            c.EmitDelegate<Func<Projectile, float>>(projectile =>
            {
                // 如果禁用修改，则使用原版距离
                if (Config != null && !Config.ModifyPortalDistance)
                {
                    return DefaultMaxDistance;
                }

                // 只修改传送门弹幕的距离
                if (projectile.aiStyle == 114)
                {
                    return Config?.PortalMaxDistance ?? DefaultMaxDistance;
                }

                // 其他弹幕保持原版距离
                return DefaultMaxDistance;
            });
        }
    }
    #endregion
}
