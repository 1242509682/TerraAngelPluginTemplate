using Microsoft.Xna.Framework;
using System.Numerics;
using TerraAngel;
using TerraAngel.Input;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Events;
using Terraria.ID;
using static MyPlugin.MyPlugin;
using static MyPlugin.UITool;

namespace MyPlugin;

internal class Utils
{
    #region 自动回血
    public static Dictionary<int, long> HealTimes = new Dictionary<int, long>();
    public static void NPCAutoHeal(NPC npc, int whoAmI)
    {
        var now = Main.GameUpdateCount;

        // 初始化npc回血时间
        if (!HealTimes.ContainsKey(whoAmI))
        {
            HealTimes[whoAmI] = now;
        }

        // 获取适合该NPC的回血间隔
        int healInterval = npc.boss ? Config.BossHealInterval : Config.NPCHealInterval;

        // 检查是否超过回血间隔
        if (now - HealTimes[whoAmI] < healInterval * 60) return;

        if (!npc.boss)
        {
            // 普通NPC回血逻辑
            int healAmount = (int)(npc.lifeMax * (Config.NPCHealVel / 100f));
            // 最低回血量为1
            if (healAmount < 1) healAmount = 1;
            npc.life = Math.Min(npc.lifeMax, npc.life + healAmount);

            if (Main.netMode is 2)
            {
                npc.netUpdate = true;
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, whoAmI);
            }
        }
        else if (Config.Boss)
        {
            // BOSS专属回血逻辑
            float healPercent = Config.BossHealVel / 100f;
            int baseHeal = (int)(npc.lifeMax * healPercent);

            // 应用回血上限
            int actualHeal = Math.Min(baseHeal, Config.BossHealCap);

            // 确保不会超过最大血量
            npc.life = Math.Min(npc.lifeMax, npc.life + actualHeal);

            if (Main.netMode is 2)
            {
                npc.netUpdate = true;
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, whoAmI);
            }
        }

        // 更新回血时间
        HealTimes[whoAmI] = now;
    }
    #endregion

    #region 复活城镇NPC方法
    public static void Relive(bool key)
    {
        if (!key) return;
        var plr = Main.LocalPlayer;
        List<int> relive = GetRelive();
        for (int i = 0; i < 200; i++)
        {
            if (Main.npc[i].active && Main.npc[i].townNPC && relive.Contains(Main.npc[i].type))
            {
                relive.Remove(Main.npc[i].type);
            }
        }

        List<string> mess = new List<string>();
        foreach (int id in relive)
        {
            NPC npc = new NPC();
            var tileX = (int)plr.position.X / 16;
            var tileY = (int)plr.position.Y / 16;
            npc.SetDefaults(id, default);
            SpawnNPC(npc.type, npc.FullName, 1, tileX, tileY, 5, 2);
            if (mess.Count != 0 && mess.Count % 10 == 0)
            {
                mess.Add("\n" + npc.FullName);
            }
            else
            {
                mess.Add(npc.FullName);
            }
        }

        // 输出复活的NPC列表
        if (relive.Count > 0)
        {
            string msg = $"{plr.name} 复活了 {relive.Count}个 NPC:\n{string.Join("、", mess)}";
            ClientLoader.Chat.WriteLine($"{msg}", color);
        }
        else
        {
            ClientLoader.Chat.WriteLine($"入住过的NPC都活着", color);
        }
    }

    // 获取复活NPC列表
    public static List<int> GetRelive()
    {
        List<int> list = new List<int>();
        List<int> list2 = new List<int>
        {
            17, 18, 19, 20, 22, 38, 54, 107, 108, 124,
            160, 178, 207, 208, 209, 227, 228, 229, 353, 369,
            441, 550, 588, 633, 663, 637, 638, 656, 670, 678,
            679, 680, 681, 682, 683, 684
        };

        if (Main.xMas)
        {
            list2.Add(142);
        }

        foreach (int item in list2)
        {
            if (BestiaryEntry(item))
            {
                list.Add(item);
            }
        }

        return list;
    }

    // 检查NPC是否解锁于怪物图鉴
    public static bool BestiaryEntry(int npcId)
    {
        return (int)Main.BestiaryDB.FindEntryByNPCID(npcId).UIInfoProvider.GetEntryUICollectionInfo().UnlockState > 0;
    }
    #endregion

    #region 生成NPC方法
    public static void SpawnNPC(int type, string name, int amount, int startTileX, int startTileY, int tileXRange = 100, int tileYRange = 50)
    {
        for (int i = 0; i < amount; i++)
        {
            GetRandomClearTileWithInRange(startTileX, startTileY, tileXRange, tileYRange, out var tileX, out var tileY);
            NPC.NewNPC(new EntitySource_DebugCommand(), tileX * 16, tileY * 16, type);
        }
    }

    // 获取随机清除图格位置
    public static void GetRandomClearTileWithInRange(int startTileX, int startTileY, int tileXRange, int tileYRange, out int tileX, out int tileY)
    {
        int num = 0;
        do
        {
            if (num == 100)
            {
                tileX = startTileX;
                tileY = startTileY;
                break;
            }

            tileX = startTileX + Random.Shared.Next(tileXRange * -1, tileXRange);
            tileY = startTileY + Random.Shared.Next(tileYRange * -1, tileYRange);
            num++;
        }
        while (TilePlacementValid(tileX, tileY) && TileSolid(tileX, tileY));
    }

    // 检查指定位置的图格是否在有效范围内
    public static bool TilePlacementValid(int tileX, int tileY)
    {
        if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0)
        {
            return tileY < Main.maxTilesY;
        }

        return false;
    }

    // 检查指定位置的图格是否为实心图格
    public static bool TileSolid(int tileX, int tileY)
    {
        if (TilePlacementValid(tileX, tileY) && Main.tile[tileX, tileY] != null! && Main.tile[tileX, tileY].active() && Main.tileSolid[Main.tile[tileX, tileY].type] && !Main.tile[tileX, tileY].inActive() && !Main.tile[tileX, tileY].halfBrick() && Main.tile[tileX, tileY].slope() == 0)
        {
            return Main.tile[tileX, tileY].type != 379;
        }

        return false;
    }
    #endregion

    #region 连锁挖矿方法
    public static void VeinMiner(int x, int y)
    {
        if (!Config.VeinMinerEnabled) return;

        try
        {
            var plr = Main.player[Main.myPlayer];
            var Tile = Main.tile[x, y];
            if (plr == null || Tile == null! || !Tile.active()) return;

            if (!Config.VeinMinerList.Any(x => x.TileID == Tile.type)) return;

            var vein = GetVein(new HashSet<Point>(), x, y, Tile.type).Result;
            var count = vein.Count;
            if (count == 0 || count > Config.VeinMinerCount) return;

            // 缓存原始图格的物品名称
            var item = GetItemFromTile(x, y);
            string name = $"[i/s{count}:{item.type}] {Lang.GetItemNameValue(item.type)}";

            // 给玩家物品
            GiveItem(plr, item.type, count);

            foreach (var point in vein)
            {
                WorldGen.KillTile(point.X, point.Y, false, false, true);
            }

            if (Main.netMode is 2)
            {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 4, x, y, false.GetHashCode());
            }

            ClientLoader.Chat.WriteLine($"【[c/79E365:连锁挖矿]】触发成功: {name}", color);
        }
        catch (Exception ex)
        {
            // 日志目录路径
            string Combine = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Configuration.FilePath)!, "logs");
            string FileName = $"{DateTime.Now:yyyy-MM-dd}.log";  // 使用当前日期作为日志文件名
            string Path = System.IO.Path.Combine(Combine, FileName);
            // 写入日志内容
            string mess = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 发生异常：{ex.Message}\n堆栈信息：{ex.StackTrace}\n";
            File.AppendAllText(Path, mess);
        }
    }
    #endregion

    #region 加载NPC列表
    public static void LoadNPCList()
    {
        npcList.Clear();

        // 添加所有NPC
        for (int id = -65; id < NPCID.Count; id++)
        {
            // 跳过无效NPC
            if (id == 0 || id == -64 || id == -65) continue;

            string name = Lang.GetNPCNameValue(id);

            // 跳过无效名称
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (name.Contains("Unloaded")) continue;

            npcList.Add(new NPCInfo(id, name, ContentSamples.NpcsByNetId[id].townNPC));
        }

        // 按ID排序
        npcList = npcList.OrderBy(n => n.ID).ToList();
    }
    #endregion

    #region 根据输入生成NPC
    public static void SpawnNPCByInput()
    {
        if (string.IsNullOrWhiteSpace(spawnNPCInput))
        {
            ClientLoader.Chat.WriteLine("请输入NPC ID或名称", Color.Red);
            return;
        }

        // 尝试解析为整数
        if (int.TryParse(spawnNPCInput, out int npcId))
        {
            if (npcId < -65 || npcId >= NPCID.Count)
            {
                ClientLoader.Chat.WriteLine($"无效的NPC ID: {npcId}", Color.Red);
                return;
            }

            string name = Lang.GetNPCNameValue(npcId);
            if (string.IsNullOrWhiteSpace(name) || name.Contains("Unloaded"))
            {
                ClientLoader.Chat.WriteLine($"未找到ID为 {npcId} 的NPC", Color.Red);
                return;
            }

            Utils.SpawnNPC(npcId, name, spawnNPCAmount,
                     (int)Main.LocalPlayer.position.X / 16,
                     (int)Main.LocalPlayer.position.Y / 16);
            ClientLoader.Chat.WriteLine($"已生成 {spawnNPCAmount} 个 {name}", Color.Green);
            return;
        }

        // 按名称搜索
        var matches = npcList
            .Where(n => n.Name.Equals(spawnNPCInput, StringComparison.OrdinalIgnoreCase) ||
                        n.Name.Contains(spawnNPCInput, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
        {
            ClientLoader.Chat.WriteLine($"未找到名称为 '{spawnNPCInput}' 的NPC", Color.Red);
            return;
        }

        if (matches.Count > 1)
        {
            ClientLoader.Chat.WriteLine($"找到多个匹配的NPC，请使用ID:", Color.Yellow);
            foreach (var match in matches)
            {
                ClientLoader.Chat.WriteLine($"{match.Name} (ID: {match.ID})", Color.Yellow);
            }
            return;
        }

        // 只有一个匹配项
        var npc = matches[0];
        Utils.SpawnNPC(npc.ID, npc.Name, spawnNPCAmount,
                 (int)Main.LocalPlayer.position.X / 16,
                 (int)Main.LocalPlayer.position.Y / 16);
        ClientLoader.Chat.WriteLine($"已生成 {spawnNPCAmount} 个 {npc.Name}", Color.Green);
    }
    #endregion

    #region 优化后的连锁区域搜索方法
    private static readonly (int dx, int dy)[] Directions =
    [
        (1, 0), (-1, 0), (0, 1), (0, -1),
        (1, 1), (1, -1), (-1, 1), (-1, -1)
    ];

    public static Task<HashSet<Point>> GetVein(HashSet<Point> list, int x, int y, int type)
    {
        return Task.Run(() =>
        {
            // 使用队列实现广度优先搜索(BFS)
            var queue = new Queue<Point>(Config.VeinMinerCount);
            queue.Enqueue(new Point(x, y));

            // 使用独立集合跟踪已访问节点
            var visited = new HashSet<Point>(Config.VeinMinerCount) { new Point(x, y) };

            while (queue.Count > 0 && list.Count < Config.VeinMinerCount)
            {
                var point = queue.Dequeue();
                int curX = point.X, curY = point.Y;

                // 检查当前图格是否符合条件
                if (curX < 0 || curX >= Main.maxTilesX ||
                    curY < 0 || curY >= Main.maxTilesY) continue;

                Tile tile = Main.tile[curX, curY];
                if (tile == null! || !tile.active() || tile.type != type) continue;

                // 添加符合条件的图格
                list.Add(point);

                // 检查邻居
                foreach (var (dx, dy) in Directions)
                {
                    int newX = curX + dx;
                    int newY = curY + dy;
                    var newPoint = new Point(newX, newY);

                    // 跳过已访问节点
                    if (visited.Contains(newPoint)) continue;

                    // 跳过无效坐标
                    if (newX < 0 || newX >= Main.maxTilesX ||
                        newY < 0 || newY >= Main.maxTilesY) continue;

                    // 标记为已访问并加入队列
                    visited.Add(newPoint);
                    queue.Enqueue(newPoint);
                }
            }
            return list;
        });
    }
    #endregion

    #region 给予玩家物品的方法 (简化版)
    public static void GiveItem(Player plr, int type, int stack)
    {
        if (stack <= 0) return;

        // 获取物品属性
        Item tempItem = new Item();
        tempItem.SetDefaults(type);
        int maxStack = tempItem.maxStack;

        // 尝试存放物品到储物空间
        bool TryStoreItem(Item[] container)
        {
            // 优先堆叠已有物品
            foreach (Item item in container)
            {
                if (item.type == type && item.stack < maxStack)
                {
                    int add = Math.Min(maxStack - item.stack, stack);
                    item.stack += add;
                    if ((stack -= add) <= 0) return true;
                }
            }

            // 在空位创建新堆叠
            foreach (Item item in container)
            {
                if (item.type == 0)
                {
                    int newStack = Math.Min(maxStack, stack);
                    item.SetDefaults(type);
                    item.stack = newStack;
                    if ((stack -= newStack) <= 0) return true;
                }
            }
            return false;
        }

        // 按优先级尝试存放不同储物空间
        if (TryStoreItem(plr.inventory)) return; // 背包
        if (TryStoreItem(plr.bank.item)) return; // 存钱罐
        if (TryStoreItem(plr.bank4.item)) return; // 虚空袋
        if (TryStoreItem(plr.bank2.item)) return; // 保险箱
        if (TryStoreItem(plr.bank3.item)) return; // 护卫熔炉

        // 生成剩余物品到世界
        while (stack > 0)
        {
            int spawnStack = Math.Min(maxStack, stack);
            stack -= spawnStack;
            plr.QuickSpawnItem(new EntitySource_DebugCommand(), type, spawnStack);
        }
    }
    #endregion

    #region 获取连锁破坏图格的物品属性
    public static Item GetItemFromTile(int x, int y)
    {
        WorldGen.KillTile_GetItemDrops(x, y, Main.tile[x, y], out int type, out int stack, out _, out _);
        Item item = new();
        item.SetDefaults(type);
        item.stack = stack;
        return item;
    }
    #endregion

    #region 获取入侵事件名称
    public static string GetInvasionName(int type)
    {
        return type switch
        {
            1 => "哥布林入侵",
            2 => "雪人军团",
            3 => "海盗入侵",
            4 => "火星人入侵",
            _ => "未知入侵"
        };
    }
    #endregion

    #region 开始入侵事件
    public static void StartInvasion(int type)
    {
        // 重置现有入侵状态
        Main.invasionType = 0;
        Main.invasionSize = 0;
        Main.invasionDelay = 0;

        // 计算入侵规模
        int playerCount = 0;
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            if (Main.player[i].active && Main.player[i].statLifeMax >= 200) playerCount++;
        }

        // 根据类型设置不同规模
        switch (type)
        {
            case 1: // 哥布林入侵
                Main.invasionSize = 80 + 40 * playerCount;
                break;
            case 2: // 雪人军团
                Main.invasionSize = 80 + 40 * playerCount;
                break;
            case 3: // 海盗入侵
                Main.invasionSize = 120 + 60 * playerCount;
                break;
            case 4: // 火星人入侵
                Main.invasionSize = 160 + 40 * playerCount;
                break;
        }

        Main.invasionSizeStart = Main.invasionSize;
        Main.invasionType = type;

        // 设置入侵起始位置
        if (type == 4) // 火星人特殊处理
            Main.invasionX = Main.spawnTileX - 1;
        else
            Main.invasionX = (Main.rand.Next(2) == 0) ? 0 : Main.maxTilesX;

        // 设置警告状态
        Main.invasionWarn = (type == 4) ? 2 : 0;

        Main.StartInvasion(type);

        // 发送网络同步
        if (Main.netMode is 2)
        {
            NetMessage.SendData(MessageID.WorldData);
            NetMessage.SendData(MessageID.InvasionProgressReport);
        }

        ClientLoader.Chat.WriteLine($"已开始{GetInvasionName(type)}事件", Color.Yellow);
    }
    #endregion

    #region 停止入侵事件
    public static void StopInvasion()
    {
        if (DD2Event.Ongoing)
        {
            DD2Event.StopInvasion();
        }
        else if (Main.pumpkinMoon || Main.snowMoon)
        {
            // 完全重置月亮事件状态
            Main.pumpkinMoon = false;
            Main.snowMoon = false;
            Main.bloodMoon = false;

            // 重置月亮事件计数器
            NPC.waveNumber = 0;
            NPC.waveKills = 0f;
            Main.stopMoonEvent();

            // 重置事件进度
            Terraria.GameContent.Events.LanternNight.GenuineLanterns = false;
        }
        else // 普通入侵
        {
            Main.invasionSize = 0;
            Main.invasionType = 0;
            Main.invasionDelay = 0;
            Main.invasionSizeStart = 0;
            Main.invasionProgress = 0;
        }


        if (Main.netMode is 2)
        {
            NetMessage.SendData(MessageID.WorldData);
            NetMessage.SendData(MessageID.InvasionProgressReport);
        }

        ClientLoader.Chat.WriteLine("已完全停止当前入侵", Color.Yellow);
    }
    #endregion

    #region 开始月亮事件
    public static void StartMoonEvent(int moonType)
    {
        // 延迟一帧确保状态完全重置
        Main.QueueMainThreadAction(() =>
        {
            if (moonType == 1)
            {
                Main.pumpkinMoon = true;
                NPC.waveNumber = 1;  // 必须设置初始波数
                NPC.waveKills = 0f;
            }
            else if (moonType == 2)
            {
                Main.snowMoon = true;
                NPC.waveNumber = 1;
                NPC.waveKills = 0f;
            }

            Main.dayTime = false;
            Main.time = 0.0;

            if (Main.netMode is 2)
            {
                NetMessage.SendData(MessageID.WorldData);
                NetMessage.SendData(MessageID.InvasionProgressReport);
            }

            ClientLoader.Chat.WriteLine($"已开始{(moonType == 1 ? "南瓜月" : "霜月")}事件", Color.Yellow);
        });
    }
    #endregion

    #region 世界事件控制方法
    public static void ToggleTime() // 切换时间状态
    {
        if (Main.dayTime)
        {
            Main.dayTime = false; // 如果是日食则切换到晚上
            Main.time = 0.0;
        }
        else
        {
            Main.dayTime = true;
            Main.time = 9000.0;
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.SetTime);

        ClientLoader.Chat.WriteLine($"时间已修改为{(Main.dayTime ? "白天" : "晚上")}", Color.Yellow);
    }

    // 切换血月状态
    public static void ToggleBloodMoon()
    {
        if (Main.bloodMoon)
        {
            Main.bloodMoon = false;
        }
        else
        {
            Main.dayTime = false;
            Main.bloodMoon = true;
            Main.time = 0.0;
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"血月事件已{(Main.bloodMoon ? "开始" : "停止")}", Color.Yellow);
    }

    // 切换日食
    public static void ToggleEclipse()
    {
        if (Main.dayTime)
        {
            Main.eclipse = !Main.eclipse; // 切换日食状态
        }
        else
        {
            Main.dayTime = true;
            Main.eclipse = true; // 切换日食状态
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"日食事件已{(Main.eclipse ? "开始" : "停止")}", Color.Yellow);
    }

    // 切换满月事件
    public static void ToggleFullMoon()
    {
        if (Main.dayTime)
        {
            Main.SkipToTime(0, true); // 跳到晚上
            Main.dayTime = false;
            Main.moonPhase = 0;
            Main.time = 0.0;
        }
        else if (Main.moonPhase != 0)
        {
            Main.moonPhase = 0; // 如果不是满月则设置为满月
        }
        else
        {
            Main.moonPhase = Main.rand.Next(1, 8); // 如果是满月则随机设置为其他月相
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine("已触发满月", Color.Yellow);
    }

    // 切换下雨状态
    public static void ToggleRain()
    {
        if (Main.raining)
        {
            Main.StopRain();
        }
        else
        {
            Main.StartRain();
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"下雨已{(Main.raining ? "开始" : "停止")}", Color.Yellow);
    }

    // 切换史莱姆雨状态
    public static void ToggleSlimeRain()
    {
        if (Main.slimeRain)
        {
            Main.slimeRain = false;
            Main.StopSlimeRain(false);
        }
        else
        {
            Main.slimeRain = true;
            Main.StartSlimeRain(true);
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"史莱姆雨已{(Main.slimeRain ? "开始" : "停止")}", Color.Yellow);
    }

    // 切换沙尘暴状态
    public static void ToggleSandstorm()
    {
        if (Sandstorm.Happening)
        {
            Sandstorm.Happening = false;
            Sandstorm.TimeLeft = 0;
            ChangeSeverityIntentions();
        }
        else
        {
            Sandstorm.Happening = true;
            Sandstorm.TimeLeft = Main.rand.Next(28800, 86401);
            ChangeSeverityIntentions();
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"沙尘暴已{(Sandstorm.Happening ? "开始" : "停止")}", Color.Yellow);
    }

    // 更改沙尘暴的严重程度
    public static void ChangeSeverityIntentions()
    {
        if (Sandstorm.Happening)
        {
            Sandstorm.IntendedSeverity = 0.4f + Main.rand.NextFloat();
        }
        else if (Main.rand.Next(3) == 0)
        {
            Sandstorm.IntendedSeverity = 0f;
        }
        else
        {
            Sandstorm.IntendedSeverity = Main.rand.NextFloat() * 0.3f;
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);
    }

    // 切换灯笼夜状态
    public static void ToggleLanternNight()
    {

        if (Terraria.GameContent.Events.LanternNight.ManualLanterns)
        {
            LanternNight.ToggleManualLanterns();
        }
        else
        {
            Main.dayTime = false;
            Main.time = 0.0;
            LanternNight.ToggleManualLanterns();
        }

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);
        ClientLoader.Chat.WriteLine($"灯笼夜已{(LanternNight.LanternsUp ? "开始" : "停止")}", Color.Yellow);
    }

    // 触发陨石事件
    public static void TriggerMeteor()
    {
        WorldGen.spawnMeteor = false;
        WorldGen.dropMeteor();

        if (Main.netMode is 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine("已触发陨石事件", Color.Yellow);
    }
    #endregion

    #region 清理钓鱼任务方法
    public static long LastQuestsTime = 0;
    public static void ClearAnglerQuests(bool clear)
    {
        long now = Main.GameUpdateCount;
        if (!clear || now - LastQuestsTime < Config.ClearQuestsInterval) return;

        Main.anglerWhoFinishedToday.Clear();
        Main.anglerQuestFinished = false;
        if (Main.netMode is 2)
            NetMessage.SendAnglerQuest(Main.LocalPlayer.whoAmI);

        LastQuestsTime = now;
    }
    #endregion

    #region 记录死亡位置（在玩家死亡时调用）
    public static long LastDeadTime = 0;
    public static void RecordDeathPoint(Player plr)
    {
        // 只在死亡状态下 复活时间为0秒时记录
        var now = Main.GameUpdateCount;
        if (!plr.dead || now - LastDeadTime < 300) return;

        Vector2 point = plr.position;

        // 检查是否已经存在相同或非常接近的位置
        bool Exists = DeathPositions.Any(pos =>
            Math.Abs(pos.X - point.X) < 16 &&
            Math.Abs(pos.Y - point.Y) < 16);

        // 如果不重复，则添加新位置
        if (!Exists)
        {
            // 添加当前死亡位置
            DeathPositions.Add(point);
            ClientLoader.Chat.WriteLine($"已记录死亡位置 ({(int)point.X / 16}, {(int)point.Y / 16})", Color.Yellow);

            LastDeadTime = now;
        }
    }
    #endregion

    #region 定位传送方法实现
    public static void StartTeleport(string message, Vector4 color)
    {
        // 检查冷却时间
        if (TPCooldown)
        {
            int cooldown = Math.Max(0, 3 - (int)((Main.GameUpdateCount - LastTPTime) / 60f));
            ClientLoader.Chat.WriteLine($"传送冷却中，请等待 {cooldown} 秒", Color.Yellow);
            TPColor = new Vector4(1f, 0.5f, 0.5f, 1f);
            return;
        }

        TP = true;
        TPColor = color;
        TPProgress = 0f;
        LastTPTime = Main.GameUpdateCount;
        TPCooldown = true;
    }

    #region 传送出生点
    public static void TPSpawnPoint(Player plr)
    {
        StartTeleport("正在传送至世界出生点...", new Vector4(0.8f, 1f, 0.8f, 1f));

        Vector2 pos = new Vector2(Main.spawnTileX * 16, (Main.spawnTileY - 3) * 16);
        plr.Teleport(pos, 10);
        ClientLoader.Chat.WriteLine($"已传送到世界出生点 ({Main.spawnTileX}, {Main.spawnTileY - 3})", Color.Yellow);
    }
    #endregion

    #region 传送到床位置
    public static void TPBed(Player plr)
    {
        if (plr.SpawnX == -1 || plr.SpawnY == -1)
        {
            ClientLoader.Chat.WriteLine("未设置床位置! 请先放置并右键点击床", Color.Red);
            return;
        }

        StartTeleport("正在传送至床位置...", new Vector4(1f, 0.8f, 1f, 1f));
        plr.Spawn(new PlayerSpawnContext());
        ClientLoader.Chat.WriteLine($"已传送到床位置 ({plr.SpawnX}, {plr.SpawnY})", Color.Yellow);
    }
    #endregion

    #region 传送到特定死亡地点
    public static void TPDeathPoint(Player plr, Vector2 position)
    {
        StartTeleport("正在传送至死亡地点...", new Vector4(0.5f, 0.5f, 0.5f, 1f));
        plr.Teleport(position, 10);
        ClientLoader.Chat.WriteLine($"已传送到死亡地点 ({(int)position.X / 16}, {(int)position.Y / 16})", Color.Yellow);
    }
    #endregion

    #region 传送到NPC
    public static void TPNPC(Player plr, int npcType)
    {
        NPC npc = FindNPC(npcType);
        if (npc == null || !npc.active)
        {
            ClientLoader.Chat.WriteLine($"未找到 {Lang.GetNPCNameValue(npcType)}", Color.Red);
            return;
        }

        StartTeleport($"正在传送至{Lang.GetNPCNameValue(npcType)}...", new Vector4(0.8f, 0.8f, 1f, 1f));

        // 传送到NPC上方一点的位置
        Vector2 pos = npc.position - new Vector2(0, 48);
        plr.Teleport(pos, 10);

        ClientLoader.Chat.WriteLine($"已传送到 {Lang.GetNPCNameValue(npcType)} 附近", Color.Yellow);
    }

    // 查找指定类型的NPC
    public static NPC FindNPC(int npcType)
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.type == npcType &&
                !npc.SpawnedFromStatue && npc.type != 488) //排除假人 雕像怪
            {
                return npc;
            }
        }
        return null!;
    }
    #endregion

    #region 微光
    // 传送微光湖
    public static void TPShimmerLake(Player plr)
    {
        StartTeleport("正在定位微光湖位置...", new Vector4(0.6f, 0.8f, 1f, 1f));

        Vector2 pos = FindShimmerLake();
        if (pos != Vector2.Zero)
        {
            plr.Teleport(pos * 16, 10);
            ClientLoader.Chat.WriteLine($"已传送到微光湖附近 ({pos.X}, {pos.Y})", Color.Yellow);
        }
        else
        {
            ClientLoader.Chat.WriteLine("未找到微光湖!", Color.Yellow);
            TPColor = new Vector4(1f, 0.3f, 0.3f, 1f);
        }
    }

    // 查找微光
    public static Vector2 FindShimmerLake()
    {
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile == null! || tile.liquidType() != LiquidID.Shimmer)
                {
                    continue;
                }

                return new Vector2(x, y - 3);
            }
        }
        return Vector2.Zero;
    }
    #endregion

    #region 神庙
    // 传送神庙
    public static void TPJungleTemple(Player plr)
    {
        StartTeleport("正在定位神庙入口...", new Vector4(0.8f, 0.6f, 0.3f, 1f));

        Vector2 pos = FindJungleTemple();
        if (pos != Vector2.Zero)
        {
            plr.Teleport(pos * 16, 10);
            ClientLoader.Chat.WriteLine($"已传送到神庙附近 ({pos.X}, {pos.Y})", Color.Yellow);
        }
        else
        {
            ClientLoader.Chat.WriteLine("未找到神庙!", Color.Yellow);
            TPColor = new Vector4(1f, 0.3f, 0.3f, 1f);
        }
    }

    // 查找神庙
    public static Vector2 FindJungleTemple()
    {
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile == null!) continue;

                if (tile.type == 237)
                {
                    return new Vector2(x, y - 3);
                }
            }
        }
        return Vector2.Zero;
    }
    #endregion

    #region 花苞
    // 传送到花苞
    public static void TPPlanteraBulb(Player plr)
    {
        StartTeleport("正在定位世纪之花苞...", new Vector4(0.6f, 1f, 0.6f, 1f));

        Vector2 pos = FindPlanteraBulb();
        if (pos != Vector2.Zero)
        {
            plr.Teleport(pos * 16, 10);
            ClientLoader.Chat.WriteLine($"已传送到花苞附近 ({pos.X}, {pos.Y})", Color.Yellow);
        }
        else
        {
            ClientLoader.Chat.WriteLine("未找到花苞!", Color.Yellow);
            TPColor = new Vector4(1f, 0.3f, 0.3f, 1f);
        }
    }

    // 在丛林地下寻找花苞
    public static Vector2 FindPlanteraBulb()
    {
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile.type == TileID.PlanteraBulb)
                {
                    return new Vector2(x, y - 3);
                }
            }
        }
        return Vector2.Zero;
    }
    #endregion

    #region 地牢
    // 传送到地牢
    public static void TPDungeon(Player plr)
    {
        StartTeleport("正在定位地牢入口...", new Vector4(0.7f, 0.6f, 1f, 1f));

        Vector2 pos = FindDungeon();
        if (pos != Vector2.Zero)
        {
            plr.Teleport(pos * 16, 10);
            ClientLoader.Chat.WriteLine($"已传送到地牢附近 ({pos.X}, {pos.Y})", Color.Yellow);
        }
        else
        {
            ClientLoader.Chat.WriteLine("未找到地牢!", Color.Yellow);
            TPColor = new Vector4(1f, 0.3f, 0.3f, 1f);
        }
    }

    // 查找地牢位置
    public static Vector2 FindDungeon()
    {
        if (Main.dungeonX > 0 && Main.dungeonY > 0)
        {
            return new Vector2(Main.dungeonX, Main.dungeonY - 3);
        }

        return Vector2.Zero;
    }
    #endregion

    #region 宝藏袋
    //传送到宝藏袋
    public static void TPBossBag(Player plr)
    {
        StartTeleport("正在定位宝藏袋...", new Vector4(1f, 0.8f, 0.3f, 1f));

        Vector2 pos = FindBossBag();
        if (pos != Vector2.Zero)
        {
            plr.Teleport(pos, 10);
            ClientLoader.Chat.WriteLine($"已传送到宝藏袋附近 ({pos.X / 16}, {pos.Y / 16})", Color.Yellow);
        }
        else
        {
            ClientLoader.Chat.WriteLine("未找到宝藏袋!", Color.Yellow);
            TPColor = new Vector4(1f, 0.3f, 0.3f, 1f);
        }
    }

    // 查找宝藏袋
    public static Vector2 FindBossBag()
    {
        for (int i = 0; i < Main.maxItems; i++)
        {
            Item item = Main.item[i];

            // 检查物品是否活跃且是宝藏袋
            if (!item.active || !ItemID.Sets.BossBag[item.type]) continue;

            return item.position;
        }

        return Vector2.Zero;
    }
    #endregion

    #region 陨石
    // 传送到陨石坑安全位置
    public static void TPMeteor(Player plr)
    {
        StartTeleport("正在定位陨石位置...", new Vector4(0.6f, 0.8f, 1f, 1f));

        // 查找安全位置
        Vector2? safePos = FindSafeMeteorPosition();

        if (safePos.HasValue)
        {
            plr.Teleport(safePos.Value, 10);
            ClientLoader.Chat.WriteLine($"已传送到陨石坑安全位置 ({(int)safePos.Value.X / 16}, {(int)safePos.Value.Y / 16})", Color.Yellow);
        }
        else
        {
            ClientLoader.Chat.WriteLine("未找到安全的陨石坑位置，无法传送", Color.Red);
        }
    }

    // 查找陨石并返回安全位置
    public static Vector2? FindSafeMeteorPosition()
    {
        int meteorCount = 0; // 统计陨石方块数量

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile == null! || !tile.active() || tile.type != TileID.Meteorite)
                {
                    continue;
                }

                meteorCount++; // 统计陨石方块

                // 在陨石上方寻找安全位置
                Vector2? safePos = FindSafePositionAbove(x, y - 3);
                if (safePos.HasValue)
                {
                    // 只有陨石数量超过100时才返回位置
                    if (meteorCount > 100)
                    {
                        return safePos.Value;
                    }
                }
            }
        }

        // 如果陨石数量不足，显示信息
        if (meteorCount > 0 && meteorCount <= 100)
        {
            ClientLoader.Chat.WriteLine($"陨石数量不足 ({meteorCount}/100)，无法安全传送", Color.Yellow);
        }

        return null;
    }

    // 在指定位置上方寻找安全站立点
    public static Vector2? FindSafePositionAbove(int tileX, int tileY)
    {
        // 从陨石位置向上搜索安全站立点
        for (int yOffset = -5; yOffset > -50; yOffset--)
        {
            int checkY = tileY + yOffset;

            // 检查当前位置是否安全
            if (IsPositionSafe(tileX, checkY))
            {
                return new Vector2(tileX * 16, (checkY - 3) * 16);
            }
        }
        return null;
    }

    // 检查位置是否安全（没有方块阻挡）
    public static bool IsPositionSafe(int tileX, int tileY)
    {
        // 检查玩家站立区域（2x3区域）是否有方块
        for (int x = tileX - 1; x <= tileX + 1; x++)
        {
            for (int y = tileY - 2; y <= tileY; y++)
            {
                // 跳过无效坐标
                if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                    return false;

                Tile tile = Main.tile[x, y];
                if (tile != null! && tile.active() && Main.tileSolid[tile.type])
                {
                    return false;
                }
            }
        }

        // 检查脚下是否有支撑物
        int groundY = tileY + 1;
        if (groundY >= Main.maxTilesY) return false;

        bool hasGround = false;
        for (int x = tileX - 1; x <= tileX + 1; x++)
        {
            if (x < 0 || x >= Main.maxTilesX) continue;

            Tile groundTile = Main.tile[x, groundY];
            if (groundTile != null! && groundTile.active() && Main.tileSolid[groundTile.type])
            {
                hasGround = true;
                break;
            }
        }

        return hasGround;
    }
    #endregion

    #region 自定义
    // 传送到自定义点
    public static void TPCustomPoint(Player plr, Vector2 pos, string pointName)
    {
        StartTeleport($"正在传送到 {pointName}...", new Vector4(0.8f, 0.6f, 0.9f, 1f));
        plr.Teleport(pos, 10);
        ClientLoader.Chat.WriteLine($"已传送到 {pointName} ({(int)pos.X / 16}, {(int)pos.Y / 16})", Color.Yellow);
    }

    // 添加自定义传送点
    public static void AddCustomPoint()
    {
        Config.CustomTeleportPoints[NewPointName] = Main.LocalPlayer.position;
        Config.Write();

        // 播放添加成功音效
        SoundEngine.PlaySound(SoundID.Item29);
        ClientLoader.Chat.WriteLine($"已添加传送点: {NewPointName}", Color.Green);

        // 重置表单
        NewPointName = "";
    }
    #endregion

    #endregion

    #region 更新传送程序
    public static void UpdateTeleportProgress()
    {
        if (TP)
        {
            TPProgress += 0.05f;
            if (TPProgress >= 1f)
            {
                TP = false;
                ClientLoader.Chat.WriteLine("传送完成!", Color.Yellow);
                SoundEngine.PlaySound(SoundID.Item6); // 传送完成音效
            }
        }

        // 检查冷却时间结束
        if (TPCooldown && (Main.GameUpdateCount - LastTPTime) > 180)
        {
            TPCooldown = false;
        }
    }
    #endregion

    #region 触发自动垃圾桶方法
    public static Dictionary<string, long> SyncTrashTime = new Dictionary<string, long>();
    public static Dictionary<int, DateTime> AdventExclusions = new Dictionary<int, DateTime>();
    public static void AutoTrash()
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

        // 给自动回收物品增加同步时间
        long now = Main.GameUpdateCount;

        //初始化同步时间
        if (!SyncTrashTime.ContainsKey(plr.name))
        {
            SyncTrashTime[plr.name] = now;
        }

        if (now - SyncTrashTime[plr.name] < Config.TrashSyncInterval) return;

        //获取玩家垃圾桶格子
        var trash = plr.trashItem;

        // 排除钱币、玩家指定排除物品和临时排除物品
        if (data.ExcluItem.Contains(trash.type)) return;

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

            if (data.TrashList.ContainsKey(inv.type) &&
                !data.ExcluItem.Contains(inv.type) &&
                !AdventExcluded(inv.type))
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

    #region 检查物品是否在临时排除期内
    public static bool AdventExcluded(int type)
    {
        if (AdventExclusions.TryGetValue(type, out var ExclusionEndTime))
        {
            if (DateTime.Now < ExclusionEndTime)
            {
                return true;
            }
            AdventExclusions.Remove(type);
        }
        return false;
    }
    #endregion

    #region 获取临时排除剩余时间
    public static string GetAdventTime(int itemType)
    {
        if (AdventExclusions.TryGetValue(itemType, out var ExclusionEndTime))
        {
            if (DateTime.Now < ExclusionEndTime)
            {
                TimeSpan left = ExclusionEndTime - DateTime.Now;
                return $"{left.TotalSeconds:F0}秒";
            }
            AdventExclusions.Remove(itemType);
        }
        return "0秒";
    }
    #endregion

    #region 修改手上物品方法
    public static void ModifyItem(bool key)
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
    public static long AutoUseTime = 0;
    public static void AutoUseItem(bool key)
    {
        var plr = Main.player[Main.myPlayer];

        if (key)
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Config.AutoUseItem = !Config.AutoUseItem;
            string status = Config.AutoUseItem ? "开启" : "关闭";
            ClientLoader.Chat.WriteLine($"自动使用物品已{status}", Color.Yellow);
        }

        if (!Config.AutoUseItem) return;

        long now = Main.GameUpdateCount;
        if (now - AutoUseTime < Config.UseItemInterval) return;

        // 使用当前手持物品
        if (plr.HeldItem.IsAir || plr.HeldItem.type == 0)
        {
            plr.controlUseItem = false;
            plr.ItemCheck();
        }
        else
        {
            plr.controlUseItem = true;
            plr.dashDelay = 0; // 重置冲刺冷却
            plr.dashType = 2; // 设置冲刺类型
            plr.ItemCheck();
        }

        // 发送网络同步消息
        if (Main.netMode is 2)
        {
            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, plr.whoAmI);
        }

        // 重置冷却时间
        AutoUseTime = now;
    }
    #endregion

    #region 批量修改饰品
    public static void ApplyPrefix()
    {
        Player plr = Main.player[Main.myPlayer];
        int count = 0;

        var pr = Lang.prefix[PrefixId].ToString();
        if (string.IsNullOrEmpty(pr))
        {
            pr = "无";
        }

        // 默认跳过的槽位：装饰栏
        List<int> NotSlot;
        if (plr.extraAccessory)
        {
            // 开启额外饰品：饰品栏为 3~9，跳过 10、11、12
            NotSlot = new List<int>() { 10, 11, 12 };
        }
        else
        {
            // 未开启额外饰品：饰品栏为 3~8，跳过 9、10、11
            NotSlot = new List<int>() { 9, 10, 11 };
        }

        for (int i = 3; i < plr.armor.Length; i++)
        {
            if (NotSlot.Contains(i)) continue;
            var item = plr.armor[i];

            if (item != null && !item.IsAir &&
                item.accessory && PrefixId != 0)
            {
                item.Prefix((byte)PrefixId);
                count++;
            }
        }

        ClientLoader.Chat.WriteLine($"已为 {count} 个饰品槽位设置前缀: {pr}", color);
    }
    #endregion

    #region 使用物品时伤害鼠标范围内的NPC
    public static long MouseStrikeTime = 0;
    public static void UseItemStrikeNPC(bool key)
    {
        long now = Main.GameUpdateCount;
        if (!key || now - MouseStrikeTime < Config.MouseStrikeInterval) return;

        var plr = Main.player[Main.myPlayer];
        var pos = InputSystem.MousePosition + Main.screenPosition;
        var inRange = Main.npc.Where(n => n.active && !n.friendly && n.Distance(pos) <= Config.MouseStrikeNPCRange * 16).ToList();
        if (!inRange.Any()) return;
        foreach (var npc in inRange)
        {
            if (npc == null) continue;

            if (Config.MouseStrikeNPCVel > 0)
            {
                npc.StrikeNPC(Config.MouseStrikeNPCVel, 0, 0, false, false, false);
                plr.ApplyDamageToNPC(npc, Config.MouseStrikeNPCVel, 0, 0, plr.HeldItem.crit > 0); // 应用伤害到NPC
                if (Main.netMode is 2)
                {
                    npc.netUpdate = true; // 更新网络状态
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI, Config.MouseStrikeNPCVel, 0, 0, plr.HeldItem.crit);
                }
            }
            else
            {
                npc.StrikeNPC(plr.HeldItem.damage, plr.HeldItem.knockBack, plr.HeldItem.direction, false, false, false);
                plr.ApplyDamageToNPC(npc, plr.HeldItem.damage, plr.HeldItem.knockBack, plr.HeldItem.direction, plr.HeldItem.crit > 0); // 应用伤害到NPC
                if (Main.netMode is 2)
                {
                    npc.netUpdate = true; // 更新网络状态
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, Terraria.Localization.NetworkText.Empty, npc.whoAmI, plr.HeldItem.damage, plr.HeldItem.knockBack, plr.HeldItem.direction, plr.HeldItem.crit);
                }
            }
        }

        // 重置冷却时间
        MouseStrikeTime = now;
    }
    #endregion

    #region 按H键回血
    public static void HealLife(bool key)
    {
        if (!Config.Heal || !key) return;

        SoundEngine.PlaySound(SoundID.MenuTick);
        var plr = Main.player[Main.myPlayer];
        plr.Heal(Config.HealVal);
        if (Main.netMode is 2)
        {
            NetMessage.TrySendData(66, -1, -1, Terraria.Localization.NetworkText.Empty, plr.whoAmI, Config.HealVal);
        }
    }
    #endregion

    #region 一键收藏所有物品
    public static bool isFavoriteMode = false; // 收藏模式开关（true为收藏，false为取消收藏）
    public static int FavoriteAllItems(Player plr)
    {
        int count = 0;

        // 反转收藏模式（每次调用切换模式）
        isFavoriteMode = !isFavoriteMode;

        // 遍历玩家背包（0-50是主背包）
        for (int i = 0; i < 50; i++)
        {
            Item item = plr.inventory[i];

            if (!item.IsAir)
            {
                // 根据当前模式设置收藏状态
                item.favorited = isFavoriteMode;
                count++;
            }
        }

        // 虚空袋（40格）
        for (int i = 0; i < 40; i++)
        {
            Item item = plr.bank4.item[i];

            if (!item.IsAir)
            {
                // 根据当前模式设置收藏状态
                item.favorited = isFavoriteMode;
                count++;
            }
        }

        return count;
    }
    #endregion

    #region 获取地图大小
    public static string GetWorldWorldSize()
    {
        switch (Main.maxTilesX)
        {
            case 4200 when Main.maxTilesY == 1200:
                return "小（4200x1200）";
            case 6400 when Main.maxTilesY == 1800:
                return "中（6400x1800）";
            case 8400 when Main.maxTilesY == 2400:
                return "大（8400x2400）";
            default:
                return "未知";
        }

    }
    #endregion

    #region 获取地图难度
    public static string GetWorldGameMode()
    {
        string GameMode = "";
        switch (Main.GameMode)
        {
            case 0: GameMode = "普通模式"; break;
            case 1: GameMode = "专家模式"; break;
            case 2:
                if (!Main.zenithWorld)
                    GameMode = "大师模式";
                else
                    GameMode = "传奇模式";
                break;
            case 3: GameMode = "旅途模式"; break;
            default:
                break;
        }

        return GameMode;
    }
    #endregion

    #region 获取地图进度
    public static (string mainProgress, string eventProgress) GetWorldProgress()
    {
        var mainProgress = new List<string>();
        var eventProgress = new List<string>();

        // 主进度收集
        if (NPC.downedSlimeKing) mainProgress.Add("史王");
        if (NPC.downedBoss1) mainProgress.Add("克眼");
        if (NPC.downedBoss2 && (Utils.BestiaryEntry(13) || Utils.BestiaryEntry(14) || Utils.BestiaryEntry(15))) mainProgress.Add("世吞");
        if (NPC.downedBoss2 && Utils.BestiaryEntry(266) && Utils.BestiaryEntry(267)) mainProgress.Add("克脑");

        if (NPC.downedDeerclops) mainProgress.Add("鹿角怪");
        if (NPC.downedBoss3) mainProgress.Add("骷髅王");
        if (NPC.downedQueenBee) mainProgress.Add("蜂王");
        if (Main.hardMode && (Utils.BestiaryEntry(NPCID.WallofFlesh) || Utils.BestiaryEntry(NPCID.WallofFleshEye))) mainProgress.Add("肉山");

        if (NPC.downedMechBoss1) mainProgress.Add("毁灭者");
        if (NPC.downedMechBoss2) mainProgress.Add("双子眼");
        if (NPC.downedMechBoss3) mainProgress.Add("铁骷髅王");
        if (NPC.downedPlantBoss) mainProgress.Add("世花");
        if (NPC.downedGolemBoss) mainProgress.Add("石巨人");
        if (NPC.downedQueenSlime) mainProgress.Add("史后");
        if (NPC.downedEmpressOfLight) mainProgress.Add("光女");
        if (NPC.downedFishron) mainProgress.Add("猪鲨");
        if (NPC.downedAncientCultist) mainProgress.Add("拜月教");

        bool allPillars = NPC.downedTowerSolar && NPC.downedTowerVortex &&
                          NPC.downedTowerNebula && NPC.downedTowerStardust;
        if (allPillars) mainProgress.Add("四柱后");
        if (NPC.downedMoonlord) mainProgress.Add("月总");

        // 事件进度收集
        if (NPC.downedGoblins) eventProgress.Add("哥布林");
        if (NPC.downedPirates) eventProgress.Add("海盗");
        if (NPC.downedMartians) eventProgress.Add("火星");

        if (NPC.downedHalloweenTree || NPC.downedHalloweenKing)
            eventProgress.Add("南瓜月");

        if (NPC.downedChristmasIceQueen || NPC.downedChristmasTree ||
            NPC.downedChristmasSantank)
            eventProgress.Add("霜月");

        return (
            mainProgress.Count > 0 ?
            string.Join("、", mainProgress) : "无",
            eventProgress.Count > 0 ?
            string.Join("、", eventProgress) : "无");
    }
    #endregion

    #region 判断玩家与NPC是否在指定格数范围内
    public static bool IsWithinRange(Player plr, NPC npc, int tileRange)
    {
        // 计算玩家和NPC中心点坐标
        Vector2 pCenter = plr.Center;
        Vector2 nCenter = npc.Center;

        // 计算两点间距离
        float dX = pCenter.X - nCenter.X;
        float dY = pCenter.Y - nCenter.Y;
        float distance = (float)Math.Sqrt(dX * dX + dY * dY);

        // 将格数转换为像素距离（1格 = 16像素）
        float maxDistance = tileRange * 16f;

        return distance <= maxDistance;
    }
    #endregion

    #region 检查是否是最接近玩家的NPC
    public static bool IsClosestNPC(Player plr, NPC currentNPC)
    {
        float closestDistance = float.MaxValue;
        NPC closestNPC = new NPC();

        // 遍历所有NPC寻找最近的
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || !npc.townNPC || npc.SpawnedFromStatue || npc.type == 488)
                continue;

            if (IsWithinRange(plr, npc, Config.NurseRange))
            {
                float distance = Vector2.Distance(plr.Center, npc.Center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNPC = npc;
                }
            }
        }

        // 检查当前NPC是否是最接近的
        return closestNPC != null && closestNPC.whoAmI == currentNPC.whoAmI;
    }
    #endregion

    #region 自动对话消息方法（从Main类的GUIChatDrawInner方法抄来的)
    public static void TalkText(Player plr)
    {
        if (Main.npc[plr.talkNPC].type == NPCID.Angler)
        {
            Main.npcChatCornerItem = 0;
            SoundEngine.PlaySound(12);
            bool flag4 = false;
            if (!Main.anglerQuestFinished && !Main.anglerWhoFinishedToday.Contains(plr.name))
            {
                int num20 = plr.FindItem(Main.anglerQuestItemNetIDs[Main.anglerQuest]);
                if (num20 != -1)
                {
                    plr.inventory[num20].stack--;
                    if (plr.inventory[num20].stack <= 0)
                    {
                        plr.inventory[num20] = new Item();
                    }

                    flag4 = true;
                    SoundEngine.PlaySound(24);
                    plr.anglerQuestsFinished++;
                    plr.GetAnglerReward(Main.npc[plr.talkNPC], Main.anglerQuestItemNetIDs[Main.anglerQuest]);
                }
            }

            Main.npcChatText = Lang.AnglerQuestChat(flag4);
            if (flag4)
            {
                Main.anglerQuestFinished = true;
                if (Main.netMode == 1)
                {
                    NetMessage.SendData(75);
                }
                else
                {
                    Main.anglerWhoFinishedToday.Add(plr.name);
                }

                AchievementsHelper.HandleAnglerService();
            }
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Merchant)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [1]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.ArmsDealer)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [2]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Mechanic)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [8]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.SantaClaus)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [9]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.OldMan)
        {
            if (Main.netMode == 0)
            {
                NPC.SpawnSkeletron(Main.myPlayer);
            }
            else
            {
                NetMessage.SendData(51, -1, -1, null, Main.myPlayer, 1f);
            }

            Main.npcChatText = "";
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Dryad)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [3]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Demolitionist)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [4]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Clothier)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [5]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.GoblinTinkerer)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [6]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Wizard)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [7]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Truffle)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [10]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Steampunker)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [11]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.DyeTrader)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [12]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.PartyGirl)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [13]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Cyborg)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [14]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Painter)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [15]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.WitchDoctor)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [16]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Pirate)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [17]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Stylist)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [18]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.TravellingMerchant)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [19]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.SkeletonMerchant)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [20]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.DD2Bartender)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [21]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Golfer)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [22]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.BestiaryGirl)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [23]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Princess)
        {
            NPCEventSystem.OpenShopMethod?.Invoke(Main.instance, [24]);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.Guide)
        {
            NPCEventSystem.HelpTextMethod?.Invoke(null, null);
        }
        else if (Main.npc[plr.talkNPC].type == NPCID.TaxCollector)
        {
            if (plr.taxMoney > 0)
            {
                int taxMoney3 = plr.taxMoney;
                taxMoney3 = (int)(taxMoney3 / plr.currentShoppingSettings.PriceAdjustment);
                while (taxMoney3 > 0)
                {
                    EntitySource_Gift source = new EntitySource_Gift(Main.npc[plr.talkNPC]);
                    if (taxMoney3 > 1000000)
                    {
                        int num21 = taxMoney3 / 1000000;
                        taxMoney3 -= 1000000 * num21;
                        int number = Item.NewItem(source, (int)plr.position.X, (int)plr.position.Y, plr.width, plr.height, 74, num21);
                        if (Main.netMode == 1)
                        {
                            NetMessage.SendData(21, -1, -1, null, number, 1f);
                        }

                        continue;
                    }

                    if (taxMoney3 > 10000)
                    {
                        int num22 = taxMoney3 / 10000;
                        taxMoney3 -= 10000 * num22;
                        int number2 = Item.NewItem(source, (int)plr.position.X, (int)plr.position.Y, plr.width, plr.height, 73, num22);
                        if (Main.netMode == 1)
                        {
                            NetMessage.SendData(21, -1, -1, null, number2, 1f);
                        }

                        continue;
                    }

                    if (taxMoney3 > 100)
                    {
                        int num23 = taxMoney3 / 100;
                        taxMoney3 -= 100 * num23;
                        int number3 = Item.NewItem(source, (int)plr.position.X, (int)plr.position.Y, plr.width, plr.height, 72, num23);
                        if (Main.netMode == 1)
                        {
                            NetMessage.SendData(21, -1, -1, null, number3, 1f);
                        }

                        continue;
                    }

                    int num24 = taxMoney3;
                    if (num24 < 1)
                    {
                        num24 = 1;
                    }

                    taxMoney3 -= num24;
                    int number4 = Item.NewItem(source, (int)plr.position.X, (int)plr.position.Y, plr.width, plr.height, 71, num24);
                    if (Main.netMode == 1)
                    {
                        NetMessage.SendData(21, -1, -1, null, number4, 1f);
                    }
                }

                Main.npcChatText = Lang.dialog(Main.rand.Next(380, 382));
                plr.taxMoney = 0;
            }
            else
            {
                Main.npcChatText = Lang.dialog(Main.rand.Next(390, 401));
            }
        }
    }
    #endregion
}
