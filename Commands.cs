using TerraAngel;
using TerraAngel.UI.ClientWindows.Console;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

internal class Commands
{
    #region 回血指令方法
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
            Config.HealVal = val;
            ClientLoader.Chat.WriteLine($"已设置回血值为: [c/4C92D8:{val}] HP", color);
        }
        else
        {
            // 没有参数时切换开关状态
            Config.Heal = !Config.Heal;
            string status = Config.Heal ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"强制回血已{status} 当前回血值为: [c/4C92D8:{Config.HealVal}]", color);
            ClientLoader.Chat.WriteLine($"注意: [c/4C92D8:#heal 数值] 可修改回血值", color);
        }

        Config.Write();
    }
    #endregion

    #region 使用物品时对鼠标范围的NPC造成伤害指令
    internal static void MouseStrikeNPC(ConsoleWindow.CmdStr x)
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

            // 更新伤害范围
            Config.MouseStrikeNPCRange = val;
            ClientLoader.Chat.WriteLine($"已设置伤害范围为: [c/4C92D8:{val}] ", color);
        }
        else
        {
            // 没有参数时切换开关状态
            Config.MouseStrikeNPC = !Config.MouseStrikeNPC;

            if (Config.MouseStrikeNPC)
            {
                ClientLoader.Chat.WriteLine($"已开启鼠标范围伤害, 当前伤害范围为: [c/4C92D8:{Config.MouseStrikeNPCRange}]", color);
                ClientLoader.Chat.WriteLine($"注意: [c/4C92D8:#snpc 数值] 可修改伤害范围", color);
            }
            else
            {
                ClientLoader.Chat.WriteLine($"已关闭鼠标范围伤害", color);
            }
        }

        Config.Write();
    } 
    #endregion

    #region 按K键自杀方法
    public static void KillPlayer(bool key)
    {
        if (!Config.Enabled || !Config.KillOrRESpawn || !key) return;
        var plr = Main.player[Main.myPlayer];
        SoundEngine.PlaySound(SoundID.MenuTick);
        if (!plr.dead)
        {
            plr.KillMe(PlayerDeathReason.ByPlayer(plr.whoAmI), 100, 0);
            ClientLoader.Chat.WriteLine($"玩家 [c/4C92D8:{plr.name}] 已自杀", color);
        }
        else
        {
            //plr.Spawn(new PlayerSpawnContext());
            plr.respawnTimer = 0;
            ClientLoader.Chat.WriteLine($"玩家 [c/4C92D8:{plr.name}] 已复活", color);
        }
    }
    #endregion

    #region 自动使用物品开关
    public static void AutoUse(TerraAngel.UI.ClientWindows.Console.ConsoleWindow.CmdStr x)
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
            Config.AutoUseInterval = val;
            ClientLoader.Chat.WriteLine($"已设置自动使用物品间隔为: [c/4C92D8:{val}] ", color);
        }
        else
        {
            // 没有参数时切换开关状态
            Config.AutoUseItem = !Config.AutoUseItem;

            string status = Config.AutoUseItem ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"自动使用物品已{status} 当前间隔为:{Config.AutoUseInterval} ms", color);
            ClientLoader.Chat.WriteLine($"注意: [c/4C92D8:#autouse 数值] 可修改间隔", color);
        }

        Config.Write();
    }
    #endregion
}
