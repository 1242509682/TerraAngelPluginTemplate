using Microsoft.Xna.Framework;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Plugin;
using TerraAngel.Tools;
using Terraria;

namespace MyPlugin;

public class MyPlugin : Plugin
{
    #region 插件信息
    public override string Name => typeof(MyPlugin).Namespace!;
    public string Author => "羽学";
    public Version Version => new(1, 0, 1);
    #endregion

    #region 注册与卸载
    public MyPlugin(string path) : base(path) { }
    public override void Load()
    {
        ReloadConfig(null!);

        // 注册UI
        ToolManager.AddTool<UITool>();

        // 向控制台添加命令
        ClientLoader.Console.AddCommand("reload", ReloadConfig, "重载配置文件");
        ClientLoader.Console.AddCommand("kill", x => Commands.KillPlayer(true), "按K键自杀与复活");
        ClientLoader.Console.AddCommand("heal", Commands.AutoHeal, "按H键强制回血");
        ClientLoader.Console.AddCommand("snpc", Commands.MouseStrikeNPC, "使用物品时伤害鼠标附近怪物");

        // 初始化完成提示
        ClientLoader.Console.WriteLine($"[{Name}] 插件已加载 (v{Version})");
        ClientLoader.Console.WriteLine($"[{Name}] 配置文件位置: {Configuration.FilePath}");
    }

    public override void Unload() 
    {
        // 卸载插件时清理UI
        ToolManager.RemoveTool<UITool>();
    }
    #endregion

    #region 配置管理
    private void ReloadConfig(TerraAngel.UI.ClientWindows.Console.ConsoleWindow.CmdStr x)
    {
        Config = Configuration.Read();
        Config.Write();
        ClientLoader.Console.WriteLine($"[{Name}] 配置文件已重载", color);
    }
    #endregion

    #region 全局变量
    internal static Configuration Config = new();
    public static Color color = new(240, 250, 150);
    #endregion

    #region 游戏更新事件(每帧刷新)
    public long timer;
    public override void Update()
    {
        if (!Config.Enabled || timer++ % 1 != 0) return;

        //按H键回血
        HealLife(InputSystem.IsKeyPressed(Config.HealKey));

        //按K键自杀与复活自己
        Commands.KillPlayer(InputSystem.IsKeyPressed(Config.KillKey));

        //使用物品时伤害鼠标范围内的NPC
        UseItemStrikeNPC(Main.player[Main.myPlayer].controlUseItem);
    }
    #endregion

    #region 使用物品时伤害鼠标范围内的NPC
    private static void UseItemStrikeNPC(bool key)
    {
        if (!Config.MouseStrikeNPC || !key) return;

        var plr = Main.player[Main.myPlayer];
        var pos = InputSystem.MousePosition + Main.screenPosition;
        var inRange = Main.npc.Where(n => n.active && !n.friendly && n.Distance(pos) <= Config.MouseStrikeNPCRange).ToList();
        if (!inRange.Any()) return;
        foreach (var npc in inRange)
        {
            if (npc == null) continue;

            npc.StrikeNPC(plr.HeldItem.damage, plr.HeldItem.knockBack, plr.HeldItem.direction, false, false, false);

            if (Main.netMode == 2)
            {
                npc.netUpdate = true; // 更新网络状态
                NetMessage.SendData(23, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI);
                NetMessage.SendData(24, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI);
                NetMessage.SendData(28, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI);
            }
        }
    }
    #endregion

    #region 按H键回血
    private static void HealLife(bool key)
    {
        if (!Config.AutoHeal || !key) return;

        var plr = Main.player[Main.myPlayer];
        plr.Heal(Config.AutoHealVal);

        if (Main.netMode == 2)
        {
            NetMessage.TrySendData(66, -1, -1, Terraria.Localization.NetworkText.Empty, plr.whoAmI, Config.AutoHealVal);
        }
    }
    #endregion

}