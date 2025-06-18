using TerraAngel;
using Terraria;
using Terraria.DataStructures;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

internal class Commands
{
    #region 自动回血指令方法
    public static void AutoHeal(TerraAngel.UI.ClientWindows.Console.ConsoleWindow.CmdStr x)
    {
        // 检查是否有参数
        if (x.Args.Count >= 1)
        {
            // 尝试解析第一个参数为整数
            if (!int.TryParse(x.Args[0], out int val))
            {
                ClientLoader.Chat.WriteLine($"无效参数: [c/4C92D8:{x.Args[0]}] 必须是整数", color);
                return;
            }

            // 更新回血值
            Config.AutoHealVal = val;
            ClientLoader.Chat.WriteLine($"已设置回血值为: [c/4C92D8:{val}] HP/秒", color);
        }
        else
        {
            // 没有参数时切换开关状态
            Config.AutoHeal = !Config.AutoHeal;

            if (Config.AutoHeal)
            {
                ClientLoader.Chat.WriteLine($"已开启每秒回血, 当前回血值为: [c/4C92D8:{Config.AutoHealVal}] HP/秒", color);
                ClientLoader.Chat.WriteLine($"注意: [c/4C92D8:#heal 数值] 可修改回血值", color);
            }
            else
            {
                ClientLoader.Chat.WriteLine($"已关闭每秒回血", color);
            }
        }

        Config.Write();
    }
    #endregion

    #region 自杀指令方法
    public static void KillPlayer(TerraAngel.UI.ClientWindows.Console.ConsoleWindow.CmdStr x)
    {
        if (!Config.Enabled) return;
        List<string> other = x.Args;

        var plr = Main.player[Main.myPlayer];

        //Main.LocalPlayer.KillMe(PlayerDeathReason.ByPlayer(plr.whoAmI), 100, 0);
        plr.KillMe(PlayerDeathReason.ByPlayer(plr.whoAmI), 100, 0);
        ClientLoader.Chat.WriteLine($"玩家 [c/4C92D8:{plr.name}] 已自杀", color);
    }
    #endregion
}
