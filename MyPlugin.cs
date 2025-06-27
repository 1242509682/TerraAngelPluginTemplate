using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerraAngel;
using TerraAngel.Graphics;
using TerraAngel.Input;
using TerraAngel.Plugin;
using TerraAngel.Tools;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace MyPlugin;

public class MyPlugin(string path) : Plugin(path)
{
    #region 插件信息
    public override string Name => typeof(MyPlugin).Namespace!;
    public string Author => "羽学";
    public Version Version => new(1, 0, 6);
    #endregion

    #region 注册与卸载
    public override void Load()
    {
        // 添加额外饰品的Mono注册钩子
        ExtraAccessory.AddAccessoryHooks();

        // 添加反重力药水的Mono钩子
        IgnoreGravity.AddGravityHooks();

        // 读取配置文件
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
        ClientLoader.Console.WriteLine($"[{Name}] 插件已加载 (v{Version}) 作者: {Author}");
        ClientLoader.Console.WriteLine($"[{Name}] 配置文件位置: {Configuration.FilePath}");
    }

    public override void Unload()
    {
        // 卸载插件时清理UI
        ToolManager.RemoveTool<UITool>();

        // 卸载额外饰品的Mono注册钩子
        ExtraAccessory.RemoveExtraAccessory();

        // 卸载反重力药水的Mono钩子
        IgnoreGravity.DelGravityHooks();
    }
    #endregion

    #region 配置管理
    internal static Configuration Config = new();
    public static Color color = new(240, 250, 150);
    private void ReloadConfig(TerraAngel.UI.ClientWindows.Console.ConsoleWindow.CmdStr x)
    {
        Config = Configuration.Read();
        Config.Write();
        ClientLoader.Console.WriteLine($"[{Name}] 配置文件已重载", color);
    }
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

        SetItem(InputSystem.IsKeyPressed(Config.ItemModifyKey));

        //快捷键P 开启关闭修改批量前缀窗口
        if (InputSystem.IsKeyPressed(Config.ShowEditPrefixKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
            UITool.ShowEditPrefix = !UITool.ShowEditPrefix;
        }

        //快捷键O 快速收藏物品
        if (InputSystem.IsKeyPressed(Config.FavoriteKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            UITool.FavoriteAllItems(Main.LocalPlayer);
        }

        // N键切换社交栏饰品生效状态
        if (InputSystem.IsKeyPressed(Config.SocialAccessoriesKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.SocialAccessory = !Config.SocialAccessory;
            string status = Config.SocialAccessory ? "开启" : "关闭";
            ClientLoader.Chat.WriteLine($"社交栏饰品功能已{status}", color);
        }

        // 切换重力控制状态
        if (InputSystem.IsKeyPressed(Config.IgnoreGravityKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.IgnoreGravity = !Config.IgnoreGravity;
            string status = Config.IgnoreGravity ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"重力控制已{status}", Color.Yellow);
        }

        // 切换自动垃圾桶状态
        if (InputSystem.IsKeyPressed(Config.AutoTrashKey))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.AutoTrash = !Config.AutoTrash;
            string status = Config.AutoTrash ? "启用" : "禁用";
            ClientLoader.Chat.WriteLine($"自动垃圾桶已{status}", Color.Yellow);
        }

        // 触发自动垃圾桶方法
        AutoTrash();
    }
    #endregion

    #region 触发自动垃圾桶方法
    private static Dictionary<string, long> SyncTrashTime = new Dictionary<string, long>();
    private static void AutoTrash()
    {
        if (!Config.AutoTrash) return;

        var plr = Main.player[Main.myPlayer];
        var data = Config.TrashItems.FirstOrDefault(x => x.Name == plr.name);
        if (data == null) //如果没有获取到的玩家数据
        {
            var newData = new TrashData()
            {
                Name = plr.name,
                TrashList = new Dictionary<int, int>(),
                ExcluItem = new HashSet<int>() { 71, 72, 73, 74 }
            };
            Config.TrashItems.Add(newData);
            return;
        }

        //初始化同步时间
        if (!SyncTrashTime.ContainsKey(plr.name))
        {
            SyncTrashTime[plr.name] = 0;
        }

        // 给自动回收物品增加同步时间
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (now - SyncTrashTime[plr.name] < Config.TrashSyncInterval) return;

        //获取玩家垃圾桶格子
        var trash = plr.trashItem;

        //排除钱币 与 玩家自己指定排除物品
        if (data.ExcluItem.Contains(trash.type))
        {
            return;
        }

        // 如果垃圾桶格子不是空的 且不在自动垃圾桶物品表中 则添加到自动垃圾桶物品表中
        if (!data.TrashList.ContainsKey(trash.type) && trash.type != 0 && !trash.IsAir)
        {
            //添加垃圾桶的物品与数量 到 “自动垃圾桶物品表”
            ClientLoader.Chat.WriteLine($"首次将 [c/4C92D8:{Lang.GetItemNameValue(trash.type)}] 放入自动垃圾桶", color);
            data.TrashList.Add(trash.type, trash.stack);
            Config.Write();
            trash.stack = 0;
            trash.TurnToAir();
        }

        for (int i = 0; i < plr.inventory.Length; i++)
        {
            var inv = plr.inventory[i];

            if (inv.IsAir || inv.type == 0 || inv == plr.HeldItem) continue;

            if (data.TrashList.ContainsKey(inv.type) && !data.ExcluItem.Contains(inv.type))
            {
                //将该格子的物品数量 添加到“自动垃圾桶物品表”
                data.TrashList[inv.type] += inv.stack;
                Config.Write();
                inv.stack = 0;
                inv.TurnToAir();
            }
        }

        SyncTrashTime[plr.name] = now;
    }
    #endregion

    #region 修改手上物品方法
    private void SetItem(bool key)
    {
        if (!Config.ItemModify || !key) return;

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
            SoundEngine.PlaySound(SoundID.MenuTick);

            // 创建新物品预设
            ItemData newItem = ItemData.FromItem(plr.HeldItem);

            // 生成唯一的预设名称
            string baseName = $"{plr.HeldItem.Name}";
            string newName = baseName;
            int prefix = 1;

            // 检查名称是否已存在，如果存在则添加后缀
            while (Config.ItemModifyList.Any(p => p.Name == newName))
            {
                newName = $"{baseName}_{prefix++}";
            }

            newItem.Name = newName;

            Config.ItemModifyList.Add(newItem);
            Config.Write();
            return;
        }

        // Ctrl+按键：删除当前类型的预设
        if (InputSystem.Ctrl)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);

            // 查找匹配的预设
            ItemData presetToRemove = Config.ItemModifyList.FirstOrDefault(p => p.Type == item.type)!;

            if (presetToRemove != null)
            {
                Config.ItemModifyList.Remove(presetToRemove);
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
        ItemData matchingPreset = Config.ItemModifyList.FirstOrDefault(p => p.Type == item.type)!;

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
            ClientLoader.Chat.WriteLine($"使用 Alt + {Config.ItemModifyKey} 添加当前物品为预设", Color.Yellow);
        }
    }
    #endregion

    #region 自动使用物品方法
    private static long AutoUseTime = 0;
    private static void AutoUseItem(bool key)
    {
        if (key)
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.AutoUseItem = !Config.AutoUseItem;
            string status = Config.AutoUseItem ? "开启" : "关闭";
            ClientLoader.Chat.WriteLine($"自动使用物品已{status}", Color.Yellow);
        }

        if (Config.AutoUseItem)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now - AutoUseTime < Config.UseItemInterval) return;

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
                plr.dashDelay = 0; //重置冲刺冷却
                plr.dashType = 2; // 设置冲刺类型为2（分趾袜距离）
                plr.ItemCheck();
                //使用物品时伤害鼠标范围内的NPC
                UseItemStrikeNPC(plr.controlUseItem);
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
            plr.ApplyDamageToNPC(npc, plr.HeldItem.damage, plr.HeldItem.knockBack, plr.HeldItem.direction, plr.HeldItem.crit > 0); // 应用伤害到NPC
            if (Main.netMode == 2)
            {
                npc.netUpdate = true; // 更新网络状态
                NetMessage.SendData(MessageID.DamageNPC, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI, plr.HeldItem.damage, plr.HeldItem.knockBack, plr.HeldItem.direction, plr.HeldItem.crit);
            }
        }
    }
    #endregion

    #region 按H键回血
    private static void HealLife(bool key)
    {
        if (!Config.Heal || !key) return;

        SoundEngine.PlaySound(SoundID.MenuTick);
        var plr = Main.player[Main.myPlayer];
        plr.Heal(Config.HealVal);
        if (Main.netMode == 2)
        {
            NetMessage.TrySendData(66, -1, -1, Terraria.Localization.NetworkText.Empty, plr.whoAmI, Config.HealVal);
        }
    }
    #endregion

}