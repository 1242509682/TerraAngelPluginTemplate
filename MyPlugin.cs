using Microsoft.Xna.Framework;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Plugin;
using TerraAngel.Tools;
using Terraria;
using Terraria.ID;
using static Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;

namespace MyPlugin;

public class MyPlugin : Plugin
{
    #region 插件信息
    public override string Name => typeof(MyPlugin).Namespace!;
    public string Author => "羽学";
    public Version Version => new(1, 0, 2);
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
        ClientLoader.Console.AddCommand("autouse", Commands.AutoUse, "切换自动使用物品功能");

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

        //自动使用物品
        AutoUseItem(InputSystem.IsKeyPressed(Config.AutoUseKey));

        SetItem(InputSystem.IsKeyPressed(Config.ItemManagerKey));
    }
    #endregion

    #region 修改手上物品方法
    private void SetItem(bool key)
    {
        if (!Config.ItemManager || !key) return;

        var plr = Main.player[Main.myPlayer];
        var item = plr.HeldItem;

        if (item == null || item.IsAir || item.type == 0)
        {
            ClientLoader.Chat.WriteLine("请先手持一个物品", Color.Red);
            return;
        }

        // Alt+按键：添加当前物品到预设列表
        if (InputSystem.Alt)
        {
            // 创建新物品预设
            ItemData newItem = ItemData.FromItem(plr.HeldItem);

            // 生成唯一的预设名称
            string baseName = $"{plr.HeldItem.Name}";
            string newName = baseName;
            int prefix = 1;

            // 检查名称是否已存在，如果存在则添加后缀
            while (Config.items.Any(p => p.Name == newName))
            {
                newName = $"{prefix++}.{baseName}";
            }

            newItem.Name = newName;

            Config.items.Add(newItem);
            Config.Write();
            return;
        }

        // Ctrl+按键：删除当前类型的预设
        if (InputSystem.Ctrl)
        {
            // 查找匹配的预设
            ItemData presetToRemove = Config.items.FirstOrDefault(p => p.Type == item.type)!;

            if (presetToRemove != null)
            {
                Config.items.Remove(presetToRemove);
                Config.Write();
                ClientLoader.Chat.WriteLine($"已删除物品预设: {presetToRemove.Name}", Color.Yellow);
            }
            else
            {
                ClientLoader.Chat.WriteLine($"未找到类型为 {item.type} 的物品预设", Color.Red);
            }
            return;
        }

        // 普通按键：应用预设到当前物品
        ItemData matchingPreset = Config.items.FirstOrDefault(p => p.Type == item.type)!;

        if (matchingPreset != null)
        {
            matchingPreset.ApplyTo(item);

            // 更新物品状态
            item.UpdateItem(item.whoAmI);
            plr.ItemCheck();

            ClientLoader.Chat.WriteLine($"已应用预设: {matchingPreset.Name}", Color.Green);
        }
        else
        {
            ClientLoader.Chat.WriteLine($"未找到类型为 {item.type} 的物品预设", Color.Red);
            ClientLoader.Chat.WriteLine($"使用 Alt + {Config.ItemManagerKey} 添加当前物品为预设", Color.Yellow);
        }
    }
    #endregion

    #region 自动使用物品方法
    private static long AutoUseTime = 0;
    private static void AutoUseItem(bool key)
    {
        if (key)
        {
            Config.AutoUseItem = !Config.AutoUseItem;
            string status = Config.AutoUseItem ? "开启" : "关闭";
            ClientLoader.Chat.WriteLine($"自动使用物品已{status}", Color.Yellow);
        }

        if (Config.AutoUseItem)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now - AutoUseTime < Config.AutoUseInterval) return;

            var plr = Main.player[Main.myPlayer];

            // 使用当前手持物品
            if (plr.HeldItem.IsAir || plr.HeldItem.type == 0)
            {
                plr.controlUseItem = false;
                plr.ItemCheck();
            }
            else
            {
                plr.controlUseItem = true;
                //使用物品时伤害鼠标范围内的NPC
                UseItemStrikeNPC(plr.controlUseItem);
                plr.ItemCheck();
            }

            // 发送网络同步消息
            if (Main.netMode == 2)
            {
                NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, plr.whoAmI);
            }

            // 重置冷却时间
            AutoUseTime = now;
        }
    }
    #endregion

    #region 使用物品时伤害鼠标范围内的NPC
    private static void UseItemStrikeNPC(bool key)
    {
        if (!Config.MouseStrikeNPC || !key) return;

        var plr = Main.player[Main.myPlayer];
        var pos = InputSystem.MousePosition + Main.screenPosition;
        var inRange = Main.npc.Where(n => n.active && !n.friendly && n.Distance(pos) <= Config.MouseStrikeNPCRange * 16).ToList();
        if (!inRange.Any()) return;
        foreach (var npc in inRange)
        {
            if (npc == null) continue;

            npc.StrikeNPC(plr.HeldItem.damage, plr.HeldItem.knockBack, plr.HeldItem.direction, false, false, false);

            if (Main.netMode == 2)
            {
                npc.netUpdate = true; // 更新网络状态
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI);
                NetMessage.SendData(MessageID.StrikeNPCWithHeldItem, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI);
                NetMessage.SendData(MessageID.DamageNPC, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI);
            }
        }
    }
    #endregion

    #region 按H键回血
    private static void HealLife(bool key)
    {
        if (!Config.Heal || !key) return;

        var plr = Main.player[Main.myPlayer];
        plr.Heal(Config.HealVal);

        if (Main.netMode == 2)
        {
            NetMessage.TrySendData(66, -1, -1, Terraria.Localization.NetworkText.Empty, plr.whoAmI, Config.HealVal);
        }
    }
    #endregion

}