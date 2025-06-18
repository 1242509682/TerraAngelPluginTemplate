using Terraria;
using TerraAngel;
using TerraAngel.Plugin;
using Microsoft.Xna.Framework;

namespace MyPlugin;

public class MyPlugin : Plugin
{
    #region 插件信息
    public override string Name => typeof(MyPlugin).Namespace!;
    public string Author => "羽学";
    public Version Version => new(1, 0, 0);
    #endregion

    #region 注册与卸载
    public MyPlugin(string path) : base(path) { }
    public override void Load()
    {
        // 向控制台添加命令
        ClientLoader.Console.AddCommand("reload", ReloadConfig, "重载配置文件");
        ClientLoader.Console.AddCommand("kill", Commands.KillPlayer, "自杀");
        ClientLoader.Console.AddCommand("heal", Commands.AutoHeal, "每秒回血");

        // 初始化完成提示
        ClientLoader.Console.WriteLine($"[{Name}] 插件已加载 (v{Version})");
        ClientLoader.Console.WriteLine($"[{Name}] 配置文件位置: {Configuration.FilePath}");
    }

    public override void Unload() { }
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
    public static Color color = new Microsoft.Xna.Framework.Color(240, 250, 150); 
    #endregion

    #region 游戏更新事件(每帧刷新)
    public int timer;
    public override void Update()
    {
        timer++;
        if (!Config.Enabled || !Config.AutoHeal || timer % 60 != 0) return;

        HealLife();
        timer = 0;
    } 
    #endregion

    #region 每帧回血
    private static void HealLife()
    {
        var plr = Main.player[Main.myPlayer];
        var HealLife = Config.AutoHealVal;
        Main.LocalPlayer.Heal(HealLife);

        if (Main.netMode == 2)
        {
            plr.statLife += HealLife;
            NetMessage.TrySendData(66, -1, -1, Terraria.Localization.NetworkText.Empty, plr.whoAmI, HealLife);
        }
    }
    #endregion

}