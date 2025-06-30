using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Numerics;
using TerraAngel;
using TerraAngel.Input;
using TerraAngel.Tools;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.ID;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

public class UITool : Tool
{
    public override string Name => "羽学插件模板";
    public override ToolTabs Tab => ToolTabs.MainTools;

    // 用于临时存储按键编辑状态
    private bool EditHealKey = false; // 快速回血按键编辑状态
    private bool EditKillKey = false; // 快速死亡与复活按键编辑状态
    private bool EditAutoUseKey = false; // 自动使用物品按键编辑状态
    private bool EditShowEditPrefixKey = false; // 一键修改前缀按键编辑状态
    private bool EditFavoriteKey = false; // 快速收藏按键编辑状态
    private bool EditItemManagerKey = false; // 物品管理器按键编辑状态
    private bool EditSocialAccessoriesKey = false; // 社交栏饰品开关按键编辑状态
    private bool EditIgnoreGravityKey = false; // 忽略重力按键编辑状态
    private bool EditAutoTrashKey = false; // 自动垃圾桶按键编辑状态
    private bool EditClearAnglerQuestsKey = false; // 清除钓鱼任务按键编辑状态
    private bool EditNPCAutoHealKey = false; // NPC自动回血按键编辑状态
    public bool EditNPCReliveKey = false; // 复活NPC按键编辑状态

    #region UI与配置文件交互方法
    public override void DrawUI(ImGuiIOPtr io)
    {
        var plr = Main.player[Main.myPlayer];

        bool enabled = Config.Enabled; //插件总开关
        bool Heal = Config.Heal; //回血开关
        int HealVal = Config.HealVal; //回血值

        bool killOrRESpawn = Config.KillOrRESpawn; //快速死亡与复活开关

        bool autoUseItem = Config.AutoUseItem; //自动使用物品开关
        int autoUseInterval = Config.UseItemInterval; //自动使用物品间隔

        bool mouseStrikeNPC = Config.MouseStrikeNPC; //鼠标范围伤害NPC开关
        int mouseStrikeNPCRange = Config.MouseStrikeNPCRange; //伤害范围
        int mouseStrikeNPCInterval = Config.MouseStrikeInterval; // 伤害NPC间隔
        int StrikeVel = Config.MouseStrikeNPCVel; // 伤害值

        bool socialEnabled = Config.SocialAccessory; // 社交栏饰品生效开关
        bool applyPrefix = Config.ApplyPrefix; // 开启额外饰品的前缀加成
        bool applyArmor = Config.ApplyArmor; // 开启装饰栏盔甲的护甲加成
        bool applyAccessory = Config.ApplyAccessory; // 开启额外饰品的原有被动功能

        bool applyIgnoreGravity = Config.IgnoreGravity; // 启用忽略重力药水效果

        bool AutoClearAngel = Config.ClearAnglerQuests; // 清除钓鱼任务开关
        int ClearAngelInterval = Config.ClearQuestsInterval; // 清除钓鱼任务按键

        bool nPCAutoHeal = Config.NPCAutoHeal; // NPC自动回血开关
        float NPCHealVel = Config.NPCHealVel; // 普通NPC回血百分比
        int NPCHealVelInterval = Config.NPCHealInterval; // 普通NPC回血间隔(秒)
        bool Boss = Config.Boss; // 允许boss回血
        float BossHealVel = Config.BossHealVel; // BOSS回血百分比
        int BossHealCap = Config.BossHealCap; // BOSS每次回血上限
        int BossHealInterval = Config.BossHealInterval; //BOSS独立回血间隔(秒)

        // 绘制插件设置界面
        ImGui.Checkbox("启用羽学插件", ref enabled);
        if (enabled)
        {
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);

            // 定位传送区域
            ImGui.Separator();
            if (ImGui.TreeNodeEx("传送管理", ImGuiTreeNodeFlags.Framed))
            {
                DrawTeleportUI(plr);
                ImGui.TreePop();

                // 显示NPC传送窗口
                if (ShowNPCTeleportWindow)
                {
                    DrawNPCTeleportWindow(plr);
                }

                // 显示自定义传送点窗口
                if (ShowCustomTeleportWindow)
                {
                    DrawCustomTeleportWindow(plr);
                }

                // 显示死亡地点选择窗口
                if (ShowDeathTeleportWindow)
                {
                    DrawDeathTeleportWindow(plr);
                }
            }

            // 事件控制区域
            ImGui.Separator();
            float Width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X * 8) / 8f;
            if (ImGui.TreeNodeEx("事件管理", ImGuiTreeNodeFlags.Framed))
            {
                // 血月按钮
                if (ImGui.Button("血月", new Vector2(Width, 40)))
                {
                    ToggleBloodMoon();
                }

                // 日食按钮
                ImGui.SameLine();
                if (ImGui.Button("日食", new Vector2(Width, 40)))
                {
                    ToggleEclipse();
                }

                // 满月按钮
                ImGui.SameLine();
                if (ImGui.Button("满月", new Vector2(Width, 40)))
                {
                    ToggleFullMoon();
                }

                // 下雨按钮
                ImGui.SameLine();
                if (ImGui.Button("下雨", new Vector2(Width, 40)))
                {
                    ToggleRain();
                }

                // 史莱姆雨按钮
                ImGui.SameLine();
                if (ImGui.Button("史莱姆雨", new Vector2(Width, 40)))
                {
                    ToggleSlimeRain();
                }

                // 时间按钮
                if (ImGui.Button("时间", new Vector2(Width, 40)))
                {
                    ToggleTime();
                }

                // 沙尘暴按钮
                ImGui.SameLine();
                if (ImGui.Button("沙尘暴", new Vector2(Width, 40)))
                {
                    ToggleSandstorm();
                }

                // 灯笼夜按钮
                ImGui.SameLine();
                if (ImGui.Button("灯笼夜", new Vector2(Width, 40)))
                {
                    ToggleLanternNight();
                }

                // 陨石按钮
                ImGui.SameLine();
                if (ImGui.Button("陨石", new Vector2(Width, 40)))
                {
                    TriggerMeteor();
                }

                // 入侵按钮
                ImGui.SameLine();
                if (ImGui.Button("入侵事件", new Vector2(Width, 40)))
                {
                    ShowInvasionWindow = true;
                }

                // 显示入侵选择窗口
                if (ShowInvasionWindow)
                {
                    DrawInvasionWindow();
                }

                ImGui.TreePop();
            }

            // 辅助功能区域
            ImGui.Separator();
            if (ImGui.TreeNodeEx("辅助功能", ImGuiTreeNodeFlags.Framed))
            {
                ImGui.Checkbox("强制回血", ref Heal);
                ImGui.SameLine(); // 回血按键设置
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                DrawKeySelector("按键", ref Config.HealKey, ref EditHealKey);
                if (Heal)
                {
                    ImGui.Text("回血量:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(150);
                    ImGui.SliderInt("##HealAmount", ref HealVal, 1, 500, "%d HP");
                    ImGui.SameLine();
                    ImGui.Text($"{HealVal} HP");
                }

                // 快速死亡复活开关（单bool + 自定义按键）
                ImGui.Checkbox("快速死亡/复活", ref killOrRESpawn);
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                DrawKeySelector("按键", ref Config.KillKey, ref EditKillKey);

                // 自动清理渔夫任务
                ImGui.Checkbox("清渔夫任务", ref AutoClearAngel);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.ClearQuestsKey, ref EditClearAnglerQuestsKey);
                if (AutoClearAngel)
                {
                    ImGui.Text("清理间隔(帧):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##AnglerQuestsInterval", ref ClearAngelInterval, 1, 3000, "%d fps");
                }

                // 自动使用物品
                ImGui.Checkbox("自动使用物品", ref autoUseItem);
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                DrawKeySelector("按键", ref Config.AutoUseKey, ref EditAutoUseKey);
                if (autoUseItem)
                {
                    ImGui.Text("使用间隔(帧):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##AutoUseInterval", ref autoUseInterval, 1, 1800, "%d fps");
                }

                ImGui.Checkbox("启用鼠标范围伤害NPC", ref mouseStrikeNPC);
                if (mouseStrikeNPC)
                {
                    ImGui.Text("伤害范围:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(150);
                    ImGui.SliderInt("##StrikeRange", ref mouseStrikeNPCRange, 0, 85, "%d 格");
                    ImGui.SameLine();
                    ImGui.Text($"{mouseStrikeNPCRange} 格");

                    ImGui.Text("伤害间隔(帧):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##StrikeInterval", ref mouseStrikeNPCInterval, 1, 1800, "%d fps");

                    ImGui.Text("伤害值:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##StrikeVel", ref StrikeVel, 0, 20000, "%d 点");
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("不设置数值时使用手上物品伤害");
                }

                ImGui.TreePop();
            }

            // 物品管理区域
            ImGui.Separator();
            if (ImGui.TreeNodeEx("物品管理", ImGuiTreeNodeFlags.Framed))
            {
                if (ImGui.Button("物品编辑器"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
                    ShowItemManagerWindow = true;
                }

                // 物品管理器窗口
                if (ShowItemManagerWindow)
                {
                    DrawItemManagerWindow(plr);
                }

                if (ImGui.Button("自动垃圾桶"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    ShowAutoTrashWindow = true;
                }

                // 显示自动垃圾桶窗口
                if (ShowAutoTrashWindow)
                {
                    DrawAutoTrashWindow(plr);
                }

                // 一键修改饰品前缀按钮
                if (ImGui.Button("一键前缀"))
                {
                    // 播放界面打开音效
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    ShowEditPrefix = true;
                    PrefixId = 0; // 重置为0
                }
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.ShowEditPrefixKey, ref EditShowEditPrefixKey);

                // 显示一键修改前缀窗口
                if (ShowEditPrefix)
                {
                    DrawEditPrefixWindow();
                }

                // 一键收藏按钮
                if (ImGui.Button("一键收藏背包"))
                {
                    // 播放收藏音效
                    SoundEngine.PlaySound(SoundID.MenuTick);

                    // 收藏所有格子物品（包括虚空袋）
                    int favoritedItems = FavoriteAllItems(plr);

                    // 显示操作结果
                    ClientLoader.Chat.WriteLine($"已收藏 {favoritedItems} 个物品", Color.Green);
                }
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.FavoriteKey, ref EditFavoriteKey);

                // 使重力药水、重力球等不会反转屏幕效果
                ImGui.Checkbox("反重力药水", ref applyIgnoreGravity);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.IgnoreGravityKey, ref EditIgnoreGravityKey);

                // 社交栏饰品开关
                ImGui.Checkbox("社交栏饰品生效", ref socialEnabled);
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.SocialAccessoriesKey, ref EditSocialAccessoriesKey);
                if (socialEnabled)
                {
                    ImGui.Checkbox("前缀加成", ref applyPrefix);
                    ImGui.SameLine();
                    ImGui.Checkbox("盔甲防御", ref applyArmor);
                    ImGui.SameLine();
                    ImGui.Checkbox("饰品功能", ref applyAccessory);
                }

                ImGui.TreePop();
            }

            // NPC管理区域
            ImGui.Separator();
            if (ImGui.TreeNodeEx("NPC管理", ImGuiTreeNodeFlags.Framed))
            {
                // 第一次打开时加载NPC列表
                if (!npcListLoaded)
                {
                    LoadNPCList();
                    npcListLoaded = true;
                }

                // npc自动回血
                ImGui.TextColored(new Vector4(1f, 0.8f, 0.6f, 1f), "NPC自动回血");
                ImGui.Separator();
                ImGui.Checkbox("NPC自动回血", ref nPCAutoHeal);
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.NPCAutoHealKey, ref EditNPCAutoHealKey);
                if (nPCAutoHeal)
                {
                    ImGui.Text("普通NPC间隔(秒):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderInt("##NPCHealVelInterval", ref NPCHealVelInterval, 1, 60 * 5); //最久5分钟回一次

                    // 普通NPC回血设置
                    ImGui.Text("普通NPC回血(百分比):");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(200);
                    ImGui.SliderFloat("##NPCHealVel", ref NPCHealVel, 0.01f, 20f, "%.2f%%");

                    // BOSS回血设置
                    ImGui.Checkbox("允许Boss回血", ref Boss);
                    if (Boss)
                    {
                        ImGui.Text("BOSS回血限制");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(200);
                        ImGui.InputInt("##BossHealCap", ref BossHealCap);

                        ImGui.Text("BOSS回血间隔(秒)");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(200);
                        ImGui.SliderInt("##BossHealInterval", ref BossHealInterval, 1, 60 * 5); //最久5分钟回一次

                        ImGui.Text("BOSS回血值(百分比)");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(200);
                        ImGui.SliderFloat("##BossHealVel", ref BossHealVel, 0.01f, 20f, "%.2f%%");
                    }
                }

                // 复活NPC区域
                ImGui.TextColored(new Vector4(1f, 0.8f, 0.6f, 1f), "复活NPC");
                ImGui.Separator();

                if (ImGui.Button("复活所有城镇NPC"))
                {
                    Relive(true);
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("复活所有已解锁图鉴的城镇NPC");
                ImGui.SameLine();
                DrawKeySelector("按键", ref Config.NPCReliveKey, ref EditNPCReliveKey);

                // 生成NPC区域
                ImGui.Spacing();
                ImGui.TextColored(new Vector4(0.8f, 1f, 0.6f, 1f), "生成NPC");
                ImGui.Separator();

                // 输入框和搜索
                ImGui.Text("搜索NPC:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200);
                ImGui.InputTextWithHint("##NPCSearch", "输入名称或ID", ref npcSearchFilter, 100);

                // 生成数量
                ImGui.Text("生成数量:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.InputInt("##NPCAmount", ref spawnNPCAmount);
                if (spawnNPCAmount < 1) spawnNPCAmount = 1;
                if (spawnNPCAmount > 100) spawnNPCAmount = 100;

                // 应用过滤后的NPC列表
                var filteredNPCs = npcList
                    .Where(n => string.IsNullOrWhiteSpace(npcSearchFilter) ||
                                n.Name.Contains(npcSearchFilter, StringComparison.OrdinalIgnoreCase) ||
                                n.ID.ToString().Contains(npcSearchFilter))
                    .ToList();

                // 显示NPC列表
                ImGui.BeginChild("NPCList", new Vector2(0, 200), ImGuiChildFlags.Borders);

                if (filteredNPCs.Count > 0)
                {
                    // 显示表头
                    ImGui.Columns(3, "npc_columns", true);
                    ImGui.SetColumnWidth(0, 60);  // ID
                    ImGui.SetColumnWidth(1, 250); // 名称
                    ImGui.SetColumnWidth(2, 120); // 操作

                    ImGui.Text("ID"); ImGui.NextColumn();
                    ImGui.Text("名称"); ImGui.NextColumn();
                    ImGui.Text("操作"); ImGui.NextColumn();
                    ImGui.Separator();

                    foreach (var npc in filteredNPCs)
                    {
                        ImGui.Text($"{npc.ID}"); ImGui.NextColumn();
                        ImGui.Text($"{npc.Name}"); ImGui.NextColumn();

                        // 生成按钮
                        if (ImGui.Button($"生成##{npc.ID}"))
                        {
                            SpawnNPC(npc.ID, npc.Name, spawnNPCAmount,
                                     (int)Main.LocalPlayer.position.X / 16,
                                     (int)Main.LocalPlayer.position.Y / 16);
                            ClientLoader.Chat.WriteLine($"已生成 {spawnNPCAmount} 个 {npc.Name}", Color.Green);
                        }

                        ImGui.NextColumn();
                    }
                    ImGui.Columns(1);
                }
                else
                {
                    ImGui.Text("没有找到匹配的NPC");
                }

                ImGui.EndChild();

                // 手动输入生成
                ImGui.Text("手动生成:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200);
                ImGui.InputTextWithHint("##ManualSpawn", "输入NPC ID或名称", ref spawnNPCInput, 100);
                ImGui.SameLine();
                if (ImGui.Button("生成"))
                {
                    SpawnNPCByInput();
                }

                ImGui.TreePop();
            }
        }

        // 更新配置值
        Config.Enabled = enabled;
        Config.Heal = Heal;
        Config.HealVal = HealVal;

        Config.KillOrRESpawn = killOrRESpawn;

        //自动使用物品
        Config.AutoUseItem = autoUseItem;
        Config.UseItemInterval = autoUseInterval;

        // 鼠标位置伤害NPC
        Config.MouseStrikeNPC = mouseStrikeNPC;
        Config.MouseStrikeNPCRange = mouseStrikeNPCRange;
        Config.MouseStrikeInterval = mouseStrikeNPCInterval;
        Config.MouseStrikeNPCVel = StrikeVel;

        // 社交栏饰品开关
        Config.SocialAccessory = socialEnabled;
        Config.ApplyPrefix = applyPrefix;
        Config.ApplyArmor = applyArmor;
        Config.ApplyAccessory = applyAccessory; // 应用饰品效果开关

        Config.IgnoreGravity = applyIgnoreGravity; // 忽略重力药水效果开关

        Config.ClearAnglerQuests = AutoClearAngel; // 清除钓鱼任务开关
        Config.ClearQuestsInterval = ClearAngelInterval; // 清除钓鱼任务间隔

        Config.NPCAutoHeal = nPCAutoHeal; // NPC自动回血开关
        Config.NPCHealVel = NPCHealVel; // 普通NPC回血百分比
        Config.NPCHealInterval = NPCHealVelInterval; // 普通NPC回血间隔(秒)
        Config.Boss = Boss; // 允许boss回血
        Config.BossHealVel = BossHealVel; // BOSS回血百分比
        Config.BossHealCap = BossHealCap; // BOSS每次回血上限
        Config.BossHealInterval = BossHealInterval; //BOSS独立回血间隔(秒)

        // 保存按钮
        ImGui.Separator();
        if (ImGui.Button("保存设置"))
        {
            SoundEngine.PlaySound(SoundID.MenuOpen); // 播放界面打开音效
            Config.Write();
            ClientLoader.Chat.WriteLine("插件设置已保存", color);
        }

        // 重置按钮
        ImGui.SameLine();
        if (ImGui.Button("重置默认"))
        {
            SoundEngine.PlaySound(SoundID.MenuClose); // 播放界面关闭音效
            Config.SetDefault();
            Config.Write();
            ClientLoader.Chat.WriteLine("已重置为默认设置", color);
        }
    }
    #endregion

    #region NPC管理器
    private string spawnNPCInput = ""; // 生成NPC输入
    private int spawnNPCAmount = 1; // 生成数量
    private string npcSearchFilter = ""; // NPC搜索过滤器
    private List<NPCInfo> npcList = new List<NPCInfo>(); // NPC列表缓存
    private bool npcListLoaded = false; // NPC列表是否已加载


    // 加载NPC列表
    private void LoadNPCList()
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

            npcList.Add(new NPCInfo
            {
                ID = id,
                Name = name,
                IsTownNPC = ContentSamples.NpcsByNetId[id].townNPC
            });
        }

        // 按ID排序
        npcList = npcList.OrderBy(n => n.ID).ToList();
    }

    // 根据输入生成NPC
    private void SpawnNPCByInput()
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

            SpawnNPC(npcId, name, spawnNPCAmount,
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
        SpawnNPC(npc.ID, npc.Name, spawnNPCAmount,
                 (int)Main.LocalPlayer.position.X / 16,
                 (int)Main.LocalPlayer.position.Y / 16);
        ClientLoader.Chat.WriteLine($"已生成 {spawnNPCAmount} 个 {npc.Name}", Color.Green);
    }
    #endregion

    #region 世界事件控制方法
    private void ToggleTime() // 切换时间状态
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

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.SetTime);

        ClientLoader.Chat.WriteLine($"时间已修改为{(Main.dayTime ? "白天" : "晚上")}", Color.Yellow);
    }

    // 切换血月状态
    private void ToggleBloodMoon()
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

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"血月事件已{(Main.bloodMoon ? "开始" : "停止")}", Color.Yellow);
    }

    // 切换日食
    private void ToggleEclipse()
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

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"日食事件已{(Main.eclipse ? "开始" : "停止")}", Color.Yellow);
    }

    // 切换满月事件
    private void ToggleFullMoon()
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

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine("已触发满月", Color.Yellow);
    }

    // 切换下雨状态
    private void ToggleRain()
    {
        if (Main.raining)
        {
            Main.StopRain();
        }
        else
        {
            Main.StartRain();
        }

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"下雨已{(Main.raining ? "开始" : "停止")}", Color.Yellow);
    }

    // 切换史莱姆雨状态
    private void ToggleSlimeRain()
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

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine($"史莱姆雨已{(Main.slimeRain ? "开始" : "停止")}", Color.Yellow);
    }

    // 切换沙尘暴状态
    private void ToggleSandstorm()
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

        if (Main.netMode == 2)
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

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.WorldData);
    }

    // 切换灯笼夜状态
    private void ToggleLanternNight()
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

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.WorldData);
        ClientLoader.Chat.WriteLine($"灯笼夜已{(LanternNight.LanternsUp ? "开始" : "停止")}", Color.Yellow);
    }

    // 触发陨石事件
    private void TriggerMeteor()
    {
        WorldGen.spawnMeteor = false;
        WorldGen.dropMeteor();

        if (Main.netMode == 2)
            NetMessage.SendData(MessageID.WorldData);

        ClientLoader.Chat.WriteLine("已触发陨石事件", Color.Yellow);
    }
    #endregion

    #region 入侵事件管理器
    private bool ShowInvasionWindow = false; // 显示入侵选择窗口
    private void DrawInvasionWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(350, 300), ImGuiCond.FirstUseEver);
        float Width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X * 6) / 7f;
        if (ImGui.Begin("选择入侵类型", ref ShowInvasionWindow, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("选择入侵类型:");
            ImGui.Separator();

            if (ImGui.Button("哥布林入侵", new Vector2(Width, 40)))
            {
                StartInvasion(1);
            }

            ImGui.SameLine();
            if (ImGui.Button("雪人军团", new Vector2(Width, 40)))
            {
                StartInvasion(2);
            }

            ImGui.SameLine();
            if (ImGui.Button("海盗入侵", new Vector2(Width, 40)))
            {
                StartInvasion(3);
            }

            if (ImGui.Button("火星人入侵", new Vector2(Width, 40)))
            {
                StartInvasion(4);
            }

            ImGui.SameLine();
            if (ImGui.Button("南瓜月", new Vector2(Width, 40)))
            {
                StartMoonEvent(1);
            }

            ImGui.SameLine();
            if (ImGui.Button("霜月", new Vector2(Width, 40)))
            {
                StartMoonEvent(2);
            }

            ImGui.Separator();
            ImGui.Text("当前入侵状态:");
            ImGui.SameLine();
            if (ImGui.Button("停止入侵"))
            {
                StopInvasion();
            }

            if (Main.invasionSize > 0)
            {
                string status = $"{GetInvasionName(Main.invasionType)}: ";
                status += $"{Main.invasionSize}/{Main.invasionSizeStart}";

                if (Main.invasionSize <= 0)
                    status += " (已完成)";
                else
                    status += $" ({(int)((float)Main.invasionSize / Main.invasionSizeStart * 100)}%)";

                ImGui.Text(status);

            }
            else if (DD2Event.Ongoing)
            {
                ImGui.Text("撒旦军队进行中");

            }
            else if (Main.pumpkinMoon)  // 新增南瓜月检测
            {
                ImGui.Text("南瓜月进行中 (波数: " + NPC.waveNumber + ")");

            }
            else if (Main.snowMoon)  // 新增霜月检测
            {
                ImGui.Text("霜月进行中 (波数: " + NPC.waveNumber + ")");
            }
            else
            {
                ImGui.Text("没有进行中的入侵");
            }
        }
        ImGui.End();
    }

    // 开始入侵事件
    private void StartInvasion(int type)
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
        if (Main.netMode == 2)
        {
            NetMessage.SendData(MessageID.WorldData);
            NetMessage.SendData(MessageID.InvasionProgressReport);
        }

        ClientLoader.Chat.WriteLine($"已开始{GetInvasionName(type)}事件", Color.Yellow);
    }

    // 停止入侵事件
    private void StopInvasion()
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


        if (Main.netMode == 2)
        {
            NetMessage.SendData(MessageID.WorldData);
            NetMessage.SendData(MessageID.InvasionProgressReport);
        }

        ClientLoader.Chat.WriteLine("已完全停止当前入侵", Color.Yellow);
    }

    // 开始月亮事件
    private void StartMoonEvent(int moonType)
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

            if (Main.netMode == 2)
            {
                NetMessage.SendData(MessageID.WorldData);
                NetMessage.SendData(MessageID.InvasionProgressReport);
            }

            ClientLoader.Chat.WriteLine($"已开始{(moonType == 1 ? "南瓜月" : "霜月")}事件", Color.Yellow);
        });
    }
    #endregion

    #region 获取入侵事件名称
    private string GetInvasionName(int type)
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

    #region 定位传送UI
    public static bool TP = false;
    public static Vector4 TPColor = new Vector4(1f, 1f, 1f, 1f);
    public static float TPProgress = 0f;
    public static uint LastTPTime = 0;
    public static bool TPCooldown = false;
    private bool ShowNPCTeleportWindow = false; // 显示NPC传送窗口
    private bool ShowDeathTeleportWindow = false; // 显示死亡地点选择窗口
    private bool ShowCustomTeleportWindow = false; // 显示自定义传送点窗口
    private void DrawTeleportUI(Player plr)
    {
        // 状态显示
        if (TP)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, TPColor);
            ImGui.PopStyleColor();

            // 进度条
            ImGui.ProgressBar(TPProgress, new Vector2(ImGui.GetContentRegionAvail().X, 20));

            // 冷却提示
            if (TPCooldown)
            {
                int cooldown = Math.Max(0, 3 - (int)((Main.GameUpdateCount - LastTPTime) / 60f));
                ImGui.TextColored(new Vector4(1f, 0.5f, 0.5f, 1f), $"传送冷却中: {cooldown}秒");
            }
        }
        else
        {
            ImGui.Text("选择传送目的地:");

            // 位置信息
            ImGui.SameLine();
            ImGui.Text("当前位置:");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1f, 1f, 0.5f, 1f), $"{(int)plr.position.X / 16}, {(int)plr.position.Y / 16}");
        }

        // 按钮区域
        float Width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X * 7) / 8f;

        // 出生点按钮
        if (ImGui.Button("出生点", new Vector2(Width, 40)))
        {
            TPSpawnPoint(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到世界出生点");

        // 床按钮
        ImGui.SameLine();
        if (ImGui.Button("床", new Vector2(Width, 40)))
        {
            TPBed(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到床的位置");

        // 死亡地点按钮（修改为打开选择窗口）
        ImGui.SameLine();
        if (DeathPositions.Count > 0)
        {
            if (ImGui.Button("死亡", new Vector2(Width, 40)))
            {
                ShowDeathTeleportWindow = true;
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"传送到死亡地点 ({DeathPositions.Count}个记录)");
        }
        else
        {
            // 没有死亡位置时，按钮不可用
            ImGui.BeginDisabled();
            ImGui.Button("死亡", new Vector2(Width, 40));
            ImGui.EndDisabled();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("没有死亡记录");
        }

        // 自定义按钮
        ImGui.SameLine();
        if (ImGui.Button("自定义", new Vector2(Width, 40)))
        {
            ShowCustomTeleportWindow = true;
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("管理自定义传送点");

        // NPC按钮
        ImGui.SameLine();
        if (ImGui.Button("NPC", new Vector2(Width, 40)))
        {
            ShowNPCTeleportWindow = true;
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到活跃NPC位置,排除傀儡与雕像怪,排列优先级:城镇npc→boss→其他怪");

        ImGui.Spacing(); //换行

        // 宝藏袋按钮
        if (ImGui.Button("宝藏袋", new Vector2(Width, 40)))
        {
            TPBossBag(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到最近的宝藏袋位置");

        // 微光湖按钮
        ImGui.SameLine();
        if (ImGui.Button("微光湖", new Vector2(Width, 40)))
        {
            TPShimmerLake(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到最近的微光湖");

        // 神庙按钮
        ImGui.SameLine();
        if (ImGui.Button("神庙", new Vector2(Width, 40)))
        {
            TPJungleTemple(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到丛林神庙入口");

        ImGui.SameLine();

        // 花苞按钮
        if (ImGui.Button("花苞", new Vector2(Width, 40)))
        {
            TPPlanteraBulb(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到最近的世纪之花苞");

        ImGui.SameLine();

        // 地牢按钮
        if (ImGui.Button("地牢", new Vector2(Width, 40)))
        {
            TPDungeon(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到地牢入口");

        ImGui.SameLine();

        // 陨石按钮
        if (ImGui.Button("陨石", new Vector2(Width, 40)))
        {
            TPMeteor(plr);
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("传送到陨石附近");
    }
    #endregion

    #region 定位传送方法实现
    private void StartTeleport(string message, Vector4 color)
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

    // 传送出生点
    private void TPSpawnPoint(Player plr)
    {
        StartTeleport("正在传送至世界出生点...", new Vector4(0.8f, 1f, 0.8f, 1f));

        Vector2 pos = new Vector2(Main.spawnTileX * 16, (Main.spawnTileY - 3) * 16);
        plr.Teleport(pos, 10);
        ClientLoader.Chat.WriteLine($"已传送到世界出生点 ({Main.spawnTileX}, {Main.spawnTileY - 3})", Color.Yellow);
    }

    // 传送到床位置
    private void TPBed(Player plr)
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

    // 传送到特定死亡地点
    private void TPDeathPoint(Player plr, Vector2 position)
    {
        StartTeleport("正在传送至死亡地点...", new Vector4(0.5f, 0.5f, 0.5f, 1f));
        plr.Teleport(position, 10);
        ClientLoader.Chat.WriteLine($"已传送到死亡地点 ({(int)position.X / 16}, {(int)position.Y / 16})", Color.Yellow);
    }

    #region 传送到NPC
    //  NPC传送方法
    private void TPNPC(Player plr, int npcType)
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
    private NPC FindNPC(int npcType)
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
    private void TPShimmerLake(Player plr)
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
    private Vector2 FindShimmerLake()
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
    private void TPJungleTemple(Player plr)
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
    private Vector2 FindJungleTemple()
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
    private void TPPlanteraBulb(Player plr)
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
    private Vector2 FindPlanteraBulb()
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
    private void TPDungeon(Player plr)
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
    private Vector2 FindDungeon()
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
    private void TPBossBag(Player plr)
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
    private Vector2 FindBossBag()
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
    private void TPMeteor(Player plr)
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
    private Vector2? FindSafeMeteorPosition()
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
    private Vector2? FindSafePositionAbove(int tileX, int tileY)
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
    private bool IsPositionSafe(int tileX, int tileY)
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
    private void TPCustomPoint(Player plr, Vector2 pos, string pointName)
    {
        StartTeleport($"正在传送到 {pointName}...", new Vector4(0.8f, 0.6f, 0.9f, 1f));
        plr.Teleport(pos, 10);
        ClientLoader.Chat.WriteLine($"已传送到 {pointName} ({(int)pos.X / 16}, {(int)pos.Y / 16})", Color.Yellow);
    }

    // 添加自定义传送点
    private void AddCustomPoint()
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

    #region 死亡地点选择窗口
    public static List<Vector2> DeathPositions = new List<Vector2>(); // 存储多个死亡位置
    private void DrawDeathTeleportWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(400, 400), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("选择死亡地点", ref ShowDeathTeleportWindow, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text($"已记录 {DeathPositions.Count} 个死亡地点");
            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                DeathPositions.Clear();
                ClientLoader.Chat.WriteLine("已清空所有死亡地点记录", Color.Yellow);
            }
            ImGui.Separator();


            var reversedPositions = new List<Vector2>(DeathPositions);
            reversedPositions.Reverse();

            for (int i = 0; i < reversedPositions.Count; i++)
            {
                Vector2 pos = reversedPositions[i];
                int x = (int)pos.X / 16;
                int y = (int)pos.Y / 16;

                if (ImGui.Button($"死亡地点 {i + 1} ({x}, {y})##{i}"))
                {
                    TPDeathPoint(plr, pos);
                    ShowDeathTeleportWindow = false;
                }

                ImGui.SameLine();

                if (ImGui.Button($"删除##del{i}"))
                {
                    int originalIndex = DeathPositions.Count - 1 - i;
                    DeathPositions.RemoveAt(originalIndex);
                    break; // 修改集合后立即跳出
                }
            }
        }
        ImGui.End();
    }
    #endregion

    #region 自定义传送点窗口
    private string CustomPointSearch = ""; // 自定义点搜索文本
    private string NewPointName = ""; // 新传送点名称
    private void DrawCustomTeleportWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(450, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("自定义传送点管理", ref ShowCustomTeleportWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 添加新传送点区域
            ImGui.Text("添加新传送点:");
            ImGui.Separator();

            // 名称输入
            ImGui.Text("名称:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(80);
            ImGui.InputText("##NewPointName", ref NewPointName, 100);

            // 位置显示
            ImGui.SameLine();
            ImGui.Text("位置:");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1f, 1f, 0.5f, 1f), $"{(int)plr.position.X / 16}, {(int)plr.position.Y / 16}");

            // 添加按钮
            ImGui.SameLine();
            if (ImGui.Button("添加传送点"))
            {
                if (string.IsNullOrWhiteSpace(NewPointName))
                {
                    ClientLoader.Chat.WriteLine("传送点名称不能为空!", Color.Red);
                }
                else
                {
                    AddCustomPoint();
                }
            }

            // 搜索区域
            ImGui.Separator();
            ImGui.Text("搜索传送点:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##CustomPointSearch", "输入名称搜索", ref CustomPointSearch, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                CustomPointSearch = "";
            }

            // 获取过滤后的传送点
            var filteredPoints = Config.CustomTeleportPoints
                .Where(p => string.IsNullOrWhiteSpace(CustomPointSearch) ||
                            p.Key.Contains(CustomPointSearch, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Key)
                .ToList();

            // 显示传送点数量
            ImGui.Text($"找到 {filteredPoints.Count} 个自定义传送点");

            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                Config.CustomTeleportPoints.Clear();
                Config.Write();
                ClientLoader.Chat.WriteLine("已清空所有传送点", Color.Yellow);
            }

            // 传送点列表
            ImGui.BeginChild("CustomPointsList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            if (filteredPoints.Count > 0)
            {
                // 使用表格布局
                ImGui.Columns(4, "custom_point_columns", true);
                ImGui.SetColumnWidth(0, 150); // 名称
                ImGui.SetColumnWidth(1, 100); // X坐标
                ImGui.SetColumnWidth(2, 100); // Y坐标
                ImGui.SetColumnWidth(3, 120); // 操作

                // 表头
                ImGui.Text("名称"); ImGui.NextColumn();
                ImGui.Text("X坐标"); ImGui.NextColumn();
                ImGui.Text("Y坐标"); ImGui.NextColumn();
                ImGui.Text("操作"); ImGui.NextColumn();
                ImGui.Separator();

                foreach (var point in filteredPoints)
                {
                    ImGui.Text(point.Key); ImGui.NextColumn();
                    ImGui.Text($"{(int)point.Value.X / 16}"); ImGui.NextColumn();
                    ImGui.Text($"{(int)point.Value.Y / 16}"); ImGui.NextColumn();

                    // 操作按钮
                    if (ImGui.Button($"传送##{point.Key}"))
                    {
                        TPCustomPoint(plr, point.Value, point.Key);
                    }

                    ImGui.SameLine();

                    if (ImGui.Button($"删除##{point.Key}"))
                    {
                        Config.CustomTeleportPoints.Remove(point.Key);
                        Config.Write();
                        ClientLoader.Chat.WriteLine($"已删除传送点: {point.Key}", Color.Yellow);
                    }

                    ImGui.NextColumn();
                }

                ImGui.Columns(1);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(CustomPointSearch))
                {
                    ImGui.Text("没有自定义传送点，请添加新的传送点");
                }
                else
                {
                    ImGui.Text($"没有找到包含 '{CustomPointSearch}' 的传送点");
                }
            }

            ImGui.EndChild();
        }
        ImGui.End();
    }
    #endregion

    #region NPC传送窗口
    private void DrawNPCTeleportWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(500, 600), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("传送到NPC", ref ShowNPCTeleportWindow, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("选择要传送的NPC:");
            ImGui.Separator();

            // 搜索框
            string npcSearch = "";
            ImGui.InputTextWithHint("##NPCSearch", "输入NPC名称搜索", ref npcSearch, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                npcSearch = "";
            }

            // 获取所有NPC
            var newNPCs = new List<NPC>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.SpawnedFromStatue && npc.type != 488)
                {
                    string npcName = Lang.GetNPCNameValue(npc.type);
                    if (string.IsNullOrWhiteSpace(npcSearch) ||
                        npcName.Contains(npcSearch, StringComparison.OrdinalIgnoreCase))
                    {
                        newNPCs.Add(npc);
                    }
                }
            }

            // 按NPC类型分组（显示唯一的NPC类型）
            var groupedNPCs = newNPCs
                .GroupBy(n => n.type)
                .Select(g => g.First())
                .ToList();

            // 将NPC分为三类：城镇NPC、BOSS、其他NPC
            var townNPCs = new List<NPC>();
            var bossNPCs = new List<NPC>();
            var otherNPCs = new List<NPC>();

            foreach (var npc in groupedNPCs)
            {
                string npcName = Lang.GetNPCNameValue(npc.type);

                if (npc.townNPC)
                {
                    townNPCs.Add(npc);
                }
                else if (npc.boss || npcName.Contains("boss", StringComparison.OrdinalIgnoreCase))
                {
                    bossNPCs.Add(npc);
                }
                else
                {
                    otherNPCs.Add(npc);
                }
            }

            // 每类按名称排序
            townNPCs = townNPCs.OrderBy(n => Lang.GetNPCNameValue(n.type)).ToList();
            bossNPCs = bossNPCs.OrderBy(n => Lang.GetNPCNameValue(n.type)).ToList();
            otherNPCs = otherNPCs.OrderBy(n => Lang.GetNPCNameValue(n.type)).ToList();

            // 合并列表（城镇NPC → BOSS → 其他NPC）
            var allNpc = new List<NPC>();
            allNpc.AddRange(townNPCs);
            allNpc.AddRange(bossNPCs);
            allNpc.AddRange(otherNPCs);

            // 显示NPC数量信息（包括分类统计）
            ImGui.Text($"找到 {allNpc.Count} 个NPC (城镇:{townNPCs.Count} BOSS:{bossNPCs.Count} 其他:{otherNPCs.Count})");

            // 使用网格布局
            ImGui.BeginChild("NPCList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            // 动态计算列数 - 根据窗口宽度
            float windowWidth = ImGui.GetContentRegionAvail().X;
            int columns = (int)Math.Max(1, Math.Floor(windowWidth / 120)); // 每列至少120px宽

            int count = allNpc.Count;

            for (int i = 0; i < count; i++)
            {
                if (i % columns != 0)
                    ImGui.SameLine();

                NPC npc = allNpc[i];
                string npcName = Lang.GetNPCNameValue(npc.type);
                string displayName = npcName;

                // 名称超过5个字时截断并添加省略号
                if (npcName.Length > 5)
                {
                    displayName = npcName.Substring(0, 5) + "...";
                }

                // 设置不同类别按钮颜色
                Vector4 buttonColor;
                string category;

                if (npc.townNPC)
                {
                    buttonColor = new Vector4(0.2f, 0.7f, 0.2f, 0.5f); // 城镇NPC - 绿色
                    category = "城镇NPC";
                }
                else if (npc.boss || npcName.Contains("boss", StringComparison.OrdinalIgnoreCase))
                {
                    buttonColor = new Vector4(0.8f, 0.2f, 0.2f, 0.5f); // BOSS - 红色
                    category = "BOSS";
                }
                else
                {
                    buttonColor = new Vector4(0.2f, 0.5f, 0.8f, 0.5f); // 其他NPC - 蓝色
                    category = "其他NPC";
                }

                ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);

                // 固定按钮高度
                Vector2 buttonSize = new Vector2(ImGui.GetContentRegionAvail().X / columns - 5, 30);

                // 创建按钮
                if (ImGui.Button($"{displayName}##{npc.type}", buttonSize))
                {
                    TPNPC(plr, npc.type);
                }

                ImGui.PopStyleColor();

                // 悬停显示NPC完整信息
                if (ImGui.IsItemHovered())
                {
                    Vector2 pos = npc.position / 16f;

                    // 如果显示的是缩写名称，在工具提示中显示完整名称
                    if (displayName != npcName)
                    {
                        ImGui.SetTooltip($"名称: {npcName}\n位置: {pos.X:F0}, {pos.Y:F0}\n类别: {category}");
                    }
                    else
                    {
                        ImGui.SetTooltip($"位置: {pos.X:F0}, {pos.Y:F0}\n类别: {category}");
                    }
                }
            }

            ImGui.EndChild();
        }
        ImGui.End();
    }
    #endregion

    #region 自动垃圾桶管理窗口
    private static bool ShowAutoTrashWindow = false; // 显示自动垃圾桶窗口
    private static string TrashSearchInput = ""; // 垃圾桶表搜索输入
    private static string ExclusionSearchInput = ""; // 排除表搜索输入
    private static int AutoTrashSyncInterval = Config.TrashSyncInterval; // 自动回收同步间隔
    private Dictionary<int, int> ReturnAmounts = new Dictionary<int, int>(); // 临时存储需要返还的物品数量
    private int? WaitExcludeType = null; // 待处理的排除物品ID
    private bool ShowExclusionWindows = false; // 是否显示排除弹窗
    private int TryExcludeTime = 60; // 临时排除时间（秒）
    private static int CustomTime = 60; // 默认排除时间（秒）
    private bool ReturnAfterExclusion = false; // 是否在设置排除后执行返还
    private Dictionary<int, int> AmountCache = new Dictionary<int, int>(); // 缓存返还数量
    private void DrawAutoTrashWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(550, 550), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("自动垃圾桶编辑器", ref ShowAutoTrashWindow, ImGuiWindowFlags.NoCollapse))
        {
            var data = Config.TrashItems.FirstOrDefault(x => x.Name == plr.name);

            // 如果没有找到配置，创建一个新的
            if (data == null)
            {
                data = new TrashData
                {
                    Name = plr.name,
                    TrashList = new Dictionary<int, int>(),
                    ExcluItem = new HashSet<int>() { 71, 72, 73, 74 } // 默认排除钱币
                };
                Config.TrashItems.Add(data);
                Config.Write();
            }

            // 总开关
            bool autoTrash = Config.AutoTrash;
            ImGui.Checkbox("启用自动垃圾桶", ref autoTrash);
            Config.AutoTrash = autoTrash; // 更新配置值
            ImGui.SameLine();
            DrawKeySelector("开关按键", ref Config.AutoTrashKey, ref EditAutoTrashKey);

            // 同步间隔设置
            ImGui.SameLine();
            ImGui.Text("回收间隔(帧):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.SliderInt("##AutoTrashInterval", ref AutoTrashSyncInterval, 1, 600, "%d fps");
            Config.TrashSyncInterval = AutoTrashSyncInterval;

            // 默认排除时间设置
            ImGui.Text("默认排除时间(秒):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.SliderInt("##DefaultExcludeTime", ref CustomTime, 1, 600, "%d 秒");

            // 自动垃圾桶物品列表
            ImGui.Separator();
            ImGui.TextColored(new Vector4(1, 0.5f, 0.5f, 1), "《自动垃圾桶表》");

            // 搜索区域
            ImGui.Text("搜索物品:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("##TrashSearchInput", "输入名称或ID", ref TrashSearchInput, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                TrashSearchInput = "";
            }

            // 获取所有垃圾桶物品并应用搜索过滤
            var trashItems = data.TrashList
                .Select(item => new
                {
                    Id = item.Key,
                    Name = Lang.GetItemNameValue(item.Key) ?? $"未知物品 ({item.Key})",
                    Amount = item.Value
                })
                .Where(item =>
                    string.IsNullOrWhiteSpace(TrashSearchInput) ||
                    item.Name.Contains(TrashSearchInput, StringComparison.OrdinalIgnoreCase) ||
                    item.Id.ToString().Contains(TrashSearchInput)
                )
                .ToList();

            // 显示物品数量信息 + 清空按钮
            ImGui.Text($"垃圾桶物品 (共 {trashItems.Count} 个物品)");
            ImGui.SameLine();

            // 添加全部返还按钮
            if (ImGui.Button("全部返还"))
            {
                if (trashItems.Count > 0)
                {
                    int totalItemsReturned = 0;
                    int totalTypesReturned = 0;

                    foreach (var item in trashItems)
                    {
                        int returned = ReturnItems(plr, item.Id, item.Amount);
                        totalItemsReturned += returned;
                        totalTypesReturned++;
                    }

                    // 清空垃圾桶列表
                    data.TrashList.Clear();
                    Config.Write();

                    ClientLoader.Chat.WriteLine($"已返还全部 {totalTypesReturned} 种物品，共 {totalItemsReturned} 个物品", Color.Yellow);
                }
            }

            ImGui.SameLine();

            // 垃圾桶清空按钮
            if (ImGui.Button("清空垃圾桶表"))
            {
                data.TrashList.Clear();
                Config.Write();
                ClientLoader.Chat.WriteLine("已清空自动垃圾桶表", Color.Yellow);
            }

            // 当前物品列表
            ImGui.BeginChild("TrashItemsList", new Vector2(0, 180), ImGuiChildFlags.Borders);

            // 使用索引显示所有物品
            for (int i = 0; i < trashItems.Count; i++)
            {
                var item = trashItems[i];
                string displayName = $"{i + 1}. {item.Name}"; // 添加连续索引前缀

                ImGui.PushID($"trash_{item.Id}");

                // 使用紧凑的5列布局
                ImGui.Columns(5, "trash_item_columns", false);
                ImGui.SetColumnWidth(0, 150); // 物品名称
                ImGui.SetColumnWidth(1, 80);  // 物品ID
                ImGui.SetColumnWidth(2, 120); // 返还数量滑块
                ImGui.SetColumnWidth(3, 100); // 按钮区域
                ImGui.SetColumnWidth(4, 120); // 临时排除时间（新增列）

                // 物品名称
                ImGui.Text(displayName);
                // 添加悬停提示
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    Item tempItem = new Item();
                    tempItem.SetDefaults(item.Id);
                    tempItem.stack = item.Amount;
                    TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
                }
                ImGui.NextColumn();

                // 物品ID
                ImGui.Text($"ID:{item.Id}");
                ImGui.NextColumn();

                // 初始化返还数量（默认为1）
                if (!ReturnAmounts.ContainsKey(item.Id))
                {
                    ReturnAmounts[item.Id] = Math.Max(1, item.Amount / 2); // 默认取一半数量
                }

                // 返还数量滑块
                int currentAmount = ReturnAmounts[item.Id];
                ImGui.SetNextItemWidth(110);
                ImGui.SliderInt($"##return_{item.Id}", ref currentAmount, 1, item.Amount, $"{currentAmount}/{item.Amount}");
                ReturnAmounts[item.Id] = currentAmount;
                ImGui.NextColumn();

                // 单个物品的返还按钮处理
                if (ImGui.Button("返还", new Vector2(40, 0)))
                {
                    // 首先检查物品是否还在垃圾桶中
                    if (!data.TrashList.ContainsKey(item.Id))
                    {
                        // 如果物品已不存在于垃圾桶中，重置所有相关状态
                        ClientLoader.Chat.WriteLine($"物品 [c/4C92D8:{item.Name}] 已不存在于垃圾桶中", Color.Yellow);

                        // 重置所有临时状态变量
                        if (WaitExcludeType == item.Id)
                        {
                            WaitExcludeType = 0;
                            ReturnAfterExclusion = false;
                            ShowExclusionWindows = false;
                        }
                        return;
                    }

                    // 检查物品是否在临时排除期内
                    if (AdventExcluded(item.Id))
                    {
                        string timeLeft = GetAdventTime(item.Id);
                        ClientLoader.Chat.WriteLine($"物品 [c/4C92D8:{item.Name}] 已被临时排除，剩余时间: {timeLeft}。正在返还...", Color.Yellow);
                        ExecuteReturn(plr, data, item.Id, item.Amount, currentAmount);
                    }
                    // 检查物品是否在排除表中
                    else if (data.ExcluItem.Contains(item.Id))
                    {
                        // 永久排除，直接返还
                        ExecuteReturn(plr, data, item.Id, item.Amount, currentAmount);
                    }
                    else
                    {
                        // 缓存返还数量
                        AmountCache[item.Id] = currentAmount;

                        // 如果不在排除表中，设置待处理状态
                        WaitExcludeType = item.Id;
                        TryExcludeTime = CustomTime; // 使用自定义默认时间
                        ReturnAfterExclusion = true; // 标记需要执行返还
                        ShowExclusionWindows = true;
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("删除", new Vector2(50, 0)))
                {
                    ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{item.Name}] 从自动垃圾桶删除", color);
                    data.TrashList.Remove(item.Id);
                    Config.Write();

                    // 如果删除的是等待排除的物品，重置状态
                    if (WaitExcludeType == item.Id)
                    {
                        WaitExcludeType = null;
                        ShowExclusionWindows = false;
                        ReturnAfterExclusion = false;
                    }

                    // 清除缓存
                    AmountCache.Remove(item.Id);
                    ReturnAmounts.Remove(item.Id);
                }

                ImGui.NextColumn();

                // 临时排除时间显示
                if (AdventExclusions != null)
                {
                    if (AdventExclusions.ContainsKey(item.Id) && AdventExclusions[item.Id] > DateTime.Now)
                    {
                        TimeSpan remaining = AdventExclusions[item.Id] - DateTime.Now;
                        int secondsLeft = (int)remaining.TotalSeconds;
                        ImGui.TextColored(new Vector4(1, 1, 0.5f, 1), $"剩余: {secondsLeft}秒");
                    }
                    else
                    {
                        ImGui.Text(""); // 空文本保持对齐
                    }
                }

                ImGui.Columns(1);
                ImGui.PopID();
            }

            // 如果没有物品显示提示
            if (trashItems.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(TrashSearchInput))
                {
                    ImGui.Text("垃圾桶列表为空,请将物品放入垃圾桶格子");
                }
                else
                {
                    ImGui.Text($"没有找到包含 '{TrashSearchInput}' 的物品");
                }
            }

            ImGui.EndChild();

            //排除表窗口
            ExclusionTableWindows(data);

            // 添加手持物品按钮
            if (ImGui.Button("添加手持物品到垃圾桶"))
            {
                if (!plr.HeldItem.IsAir)
                {
                    int itemId = plr.HeldItem.type;
                    if (!data.TrashList.ContainsKey(itemId))
                    {
                        string itemName = Lang.GetItemNameValue(itemId) ?? $"未知物品 ({itemId})";
                        ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{itemName}] 添加到自动垃圾桶", color);
                        data.TrashList.Add(itemId, 0);
                        Config.Write();
                    }
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("添加手持物品到排除表"))
            {
                if (!plr.HeldItem.IsAir)
                {
                    int itemId = plr.HeldItem.type;
                    if (!data.ExcluItem.Contains(itemId))
                    {
                        string itemName = Lang.GetItemNameValue(itemId) ?? $"未知物品 ({itemId})";
                        ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{itemName}] 添加到排除表", color);
                        data.ExcluItem.Add(itemId);
                        Config.Write();
                    }
                }
            }

            // 保存按钮
            if (ImGui.Button("保存设置"))
            {
                Config.Write();
                ClientLoader.Chat.WriteLine("自动垃圾桶设置已保存", Color.Green);
            }
        }
        ImGui.End();

        // 处理排除提示弹窗
        if (ShowExclusionWindows)
        {
            ExclusionWindows(plr);
        }
    }
    #endregion

    #region 垃圾桶排除表窗口
    private static void ExclusionTableWindows(TrashData? data)
    {
        if (data is null) return;

        // 排除物品列表
        ImGui.Separator();
        ImGui.TextColored(new Vector4(0.5f, 1, 0.5f, 1), "《排除物品表》");

        // 搜索区域
        ImGui.Text("搜索物品:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        ImGui.InputTextWithHint("##ExclusionSearchInput", "输入名称或ID", ref ExclusionSearchInput, 100);
        ImGui.SameLine();
        if (ImGui.Button("清空搜索##Exclusion"))
        {
            ExclusionSearchInput = "";
        }

        // 获取所有排除物品并应用搜索过滤
        var excluItems = data.ExcluItem
            .Select(id => new
            {
                Id = id,
                Name = Lang.GetItemNameValue(id) ?? $"未知物品 ({id})"
            })
            .Where(item =>
                string.IsNullOrWhiteSpace(ExclusionSearchInput) ||
                item.Name.Contains(ExclusionSearchInput, StringComparison.OrdinalIgnoreCase) ||
                item.Id.ToString().Contains(ExclusionSearchInput)
            )
            .ToList();

        // 显示物品数量信息 + 清空按钮
        ImGui.Text($"排除物品 (共 {excluItems.Count} 个物品)");
        ImGui.SameLine();
        // 添加排除表清空按钮
        if (ImGui.Button("清空排除表"))
        {
            data.ExcluItem.Clear();
            // 添加默认排除的钱币ID
            data.ExcluItem = new HashSet<int>() { 71, 72, 73, 74 };
            Config.Write();
            ClientLoader.Chat.WriteLine("已清空排除表并默认排除钱币", Color.Yellow);
        }

        // 当前排除列表
        ImGui.BeginChild("物品排除表", new Vector2(0, 180), ImGuiChildFlags.Borders);

        // 使用索引显示所有排除物品
        for (int i = 0; i < excluItems.Count; i++)
        {
            var item = excluItems[i];
            string displayName = $"{i + 1}. {item.Name}"; // 添加连续索引前缀

            ImGui.PushID($"exclu_{item.Id}");

            // 使用紧凑的3列布局
            ImGui.Columns(3, "exclusion_item_columns", false);
            ImGui.SetColumnWidth(0, 150); // 物品名称
            ImGui.SetColumnWidth(1, 80);  // 物品ID
            ImGui.SetColumnWidth(2, 120); // 删除按钮

            // 物品名称
            ImGui.Text(displayName);
            // 添加悬停提示
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
            {
                Item tempItem = new Item();
                tempItem.SetDefaults(item.Id);
                TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
            }
            ImGui.NextColumn();

            // 物品ID
            ImGui.Text($"ID:{item.Id}");
            ImGui.NextColumn();

            // 删除按钮
            if (ImGui.Button("删除", new Vector2(50, 0)))
            {
                ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{item.Name}] 从排除表中删除", color);
                data.ExcluItem.Remove(item.Id);
                Config.Write();
            }

            ImGui.Columns(1);
            ImGui.PopID();
        }

        // 如果没有物品显示提示
        if (excluItems.Count == 0)
        {
            if (string.IsNullOrWhiteSpace(ExclusionSearchInput))
            {
                ImGui.Text("排除列表为空");
            }
            else
            {
                ImGui.Text($"没有找到包含 '{ExclusionSearchInput}' 的排除物品");
            }
        }

        ImGui.EndChild();
    }
    #endregion

    #region 临时排除窗口
    private void ExclusionWindows(Player plr)
    {
        if (!WaitExcludeType.HasValue) return;

        int itemId = WaitExcludeType.Value;
        string itemName = Lang.GetItemNameValue(itemId);
        if (string.IsNullOrEmpty(itemName)) itemName = $"未知物品 ({itemId})";

        ImGui.OpenPopup("添加排除");
        if (ImGui.BeginPopupModal("添加排除", ref ShowExclusionWindows, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var data = Config.TrashItems.FirstOrDefault(x => x.Name == plr.name);
            if (data == null)
            {
                return;
            }

            ImGui.Text($"物品 '{itemName}' 不在排除表中，请选择操作：");

            // 时间输入框
            ImGui.Text("排除时间(秒):");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##ExcludeTime", ref TryExcludeTime);
            if (TryExcludeTime < 1) TryExcludeTime = 1;
            if (TryExcludeTime > 3600) TryExcludeTime = 3600; // 限制最大1小时

            ImGui.Spacing();

            // 按钮行
            if (ImGui.Button("加入排除表"))
            {
                if (!data.ExcluItem.Contains(itemId))
                {
                    data.ExcluItem.Add(itemId);
                    Config.Write();
                    ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{itemName}] 添加到排除表", Color.Green);
                }

                // 如果需要返还，执行返还操作
                if (ReturnAfterExclusion)
                {
                    // 使用缓存的返还数量
                    int cachedAmount = AmountCache.TryGetValue(itemId, out var amt) ? amt : 1;
                    ExecuteReturn(plr, data, itemId, data.TrashList[itemId], cachedAmount);
                    ReturnAfterExclusion = false;
                }
            }

            ImGui.SameLine();

            if (ImGui.Button($"临时排除({TryExcludeTime}秒)"))
            {
                AdventExclusions[itemId] = DateTime.Now.AddSeconds(TryExcludeTime);
                ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{itemName}] 临时排除{TryExcludeTime}秒", Color.Yellow);

                // 如果需要返还，执行返还操作
                if (ReturnAfterExclusion)
                {
                    // 使用缓存的返还数量
                    int cachedAmount = AmountCache.TryGetValue(itemId, out var amt) ? amt : 1;
                    ExecuteReturn(plr, data, itemId, data.TrashList[itemId], cachedAmount);
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("取消"))
            {
                // 取消时重置返还标志
                ReturnAfterExclusion = false;
                ShowExclusionWindows = false;
                WaitExcludeType = null;
                AmountCache.Remove(itemId); // 清除该物品的缓存
            }

            ImGui.EndPopup();
        }
    }
    #endregion

    #region 返还物品并同步服务器的方法
    private int ReturnItems(Player plr, int type, int amount)
    {
        int totalReturned = 0;

        // 获取物品的最大堆叠数量
        var item = new Item();
        item.SetDefaults(type);
        int maxStack = item.maxStack;

        // 分批返还物品
        while (amount > 0)
        {
            int stackSize = Math.Min(amount, maxStack);

            // 创建物品实体
            int Index = Item.NewItem(new EntitySource_DebugCommand(),
                                        (int)plr.position.X, (int)plr.position.Y,
                                        plr.width, plr.height, type, stackSize,
                                        noBroadcast: true, item.prefix, noGrabDelay: true);

            // 设置物品归属并同步
            Main.item[Index].playerIndexTheItemIsReservedFor = plr.whoAmI;
            NetMessage.SendData(MessageID.ItemOwner, plr.whoAmI, -1, null, Index);
            NetMessage.SendData(MessageID.SyncItem, plr.whoAmI, -1, null, Index, 1);

            amount -= stackSize;
            totalReturned += stackSize;
        }

        return totalReturned;
    }
    #endregion

    #region 执行返还操作的方法
    private void ExecuteReturn(Player plr, TrashData data, int itemKey, int itemValue, int currentAmount)
    {
        int returnAmount = Math.Min(currentAmount, itemValue);
        int returned = ReturnItems(plr, itemKey, returnAmount);

        // 更新垃圾桶中的物品数量
        int newAmount = itemValue - returnAmount;
        if (newAmount <= 0)
        {
            data.TrashList.Remove(itemKey);
            ClientLoader.Chat.WriteLine($"已将 [c/4C92D8:{Lang.GetItemNameValue(itemKey)}] 从[c/4C92D8:自动垃圾桶]移除", color);
        }
        else
        {
            data.TrashList[itemKey] = newAmount;
        }

        Config.Write();

        // 返还完成后重置相关状态
        if (WaitExcludeType == itemKey)
        {
            WaitExcludeType = null;
            ReturnAfterExclusion = false;
        }

        // 清除缓存
        AmountCache.Remove(itemKey);
        ReturnAmounts.Remove(itemKey);

        // 如果弹窗显示的是当前物品，关闭弹窗
        if (ShowExclusionWindows && WaitExcludeType == itemKey)
        {
            ShowExclusionWindows = false;
        }
    }
    #endregion

    #region 一键收藏所有物品
    private static bool isFavoriteMode = false; // 收藏模式开关（true为收藏，false为取消收藏）
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

    #region 物品编辑管理器窗口
    private static bool ShowItemManagerWindow = false; // 显示物品管理器窗口
    private static string SearchFilter = ""; // 物品搜索过滤器
    internal void DrawItemManagerWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(600, 450), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("物品编辑管理器", ref ShowItemManagerWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 搜索框和按钮
            ImGui.InputTextWithHint("##SearchFilter", "输入名称或ID搜索", ref SearchFilter, 100);
            ImGui.SameLine();
            if (ImGui.Button("清空搜索"))
            {
                SearchFilter = "";
            }
            ImGui.SameLine();
            DrawKeySelector("应用按键", ref Config.ItemModifyKey, ref EditItemManagerKey);

            // 获取所有物品并应用搜索过滤
            var AllItems = Config.ItemModifyList
                .Select(item => new
                {
                    Data = item,
                    Name = item.Name,
                    Type = item.Type,
                    ItemName = Lang.GetItemNameValue(item.Type) ?? $"未知物品 ({item.Type})"
                })
                .Where(item =>
                    string.IsNullOrWhiteSpace(SearchFilter) ||
                    item.Name.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                    item.ItemName.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                    item.Type.ToString().Contains(SearchFilter)
                )
                .ToList();

            // 计算总页数
            AllPages = (int)Math.Ceiling(AllItems.Count / (float)PageLimit);
            if (AllPages == 0) AllPages = 1;

            // 确保当前页在有效范围内
            if (NowPage >= AllPages) NowPage = AllPages - 1;
            if (NowPage < 0) NowPage = 0;

            // 按钮区域
            if (ImGui.Button($"添加(Alt+{Config.ItemModifyKey})"))
            {
                if (plr.HeldItem != null && !plr.HeldItem.IsAir)
                {
                    // 播放界面打开音效
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);

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

                    // 显示成功消息
                    ClientLoader.Chat.WriteLine($"已添加物品预设: {newItem.Name}", Color.Green);
                    ClientLoader.Chat.WriteLine($"使用 {Config.ItemModifyKey} 键应用此预设", Color.Yellow);

                    // 清空搜索以便显示新添加的物品
                    SearchFilter = "";
                }
                else
                {
                    ClientLoader.Chat.WriteLine($"请手持一个物品 使用{Config.ItemModifyKey}", Color.Red);
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("清空列表"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                ShowEditWindow = false;
                Config.ItemModifyList.Clear();
                Config.Write();
                SearchFilter = ""; // 清空搜索
            }

            ImGui.SameLine();
            ListPage(AllItems.Count); // 分页

            // 显示搜索结果信息
            if (!string.IsNullOrWhiteSpace(SearchFilter))
            {
                ImGui.TextColored(new Vector4(1, 1, 0.5f, 1), $"找到 {AllItems.Count} 个匹配项");
            }

            // 物品列表
            ImGui.Separator();
            ImGui.BeginChild("物品列表", new Vector2(0, 250), ImGuiChildFlags.Borders);

            // 获取当前页的物品
            var GetPage = AllItems.Skip(NowPage * PageLimit).Take(PageLimit).ToList();

            // 表头
            ImGui.Columns(5, "item_columns", false);
            ImGui.SetColumnWidth(0, 40);   // 索引
            ImGui.SetColumnWidth(1, 150);  // 预设名称
            ImGui.SetColumnWidth(2, 150);  // 物品名称
            ImGui.SetColumnWidth(3, 70);   // 物品ID
            ImGui.SetColumnWidth(4, 150);  // 操作按钮

            ImGui.Text("#"); ImGui.NextColumn();
            ImGui.Text("预设名称"); ImGui.NextColumn();
            ImGui.Text("物品名称"); ImGui.NextColumn();
            ImGui.Text("物品ID"); ImGui.NextColumn();
            ImGui.Text("操作"); ImGui.NextColumn();

            ImGui.Separator();
            ImGui.Columns(1);

            // 物品行
            for (int i = 0; i < GetPage.Count; i++)
            {
                var itemInfo = GetPage[i];
                var data = itemInfo.Data;

                ImGui.PushID(data.Name);

                // 使用5列布局
                ImGui.Columns(5, "item_columns", false);
                ImGui.SetColumnWidth(0, 40);   // 索引
                ImGui.SetColumnWidth(1, 150);  // 预设名称
                ImGui.SetColumnWidth(2, 150);  // 物品名称
                ImGui.SetColumnWidth(3, 70);   // 物品ID
                ImGui.SetColumnWidth(4, 150);  // 操作按钮

                // 索引号 (当前页的序号)
                ImGui.Text($"{NowPage * PageLimit + i + 1}");
                ImGui.NextColumn();

                // 预设名称
                ImGui.Text($"{data.Name}");
                ImGui.NextColumn();

                // 物品名称
                ImGui.Text($"{itemInfo.ItemName}");
                ImGui.NextColumn();

                // 物品ID
                ImGui.Text($"{data.Type}");
                ImGui.NextColumn();

                // 操作按钮
                if (ImGui.Button($"应用##{i}"))
                {
                    data.ApplyTo(Main.player[Main.myPlayer].HeldItem);
                    ClientLoader.Chat.WriteLine($"已应用物品预设: {data.Name}", Color.Green);
                }
                ImGui.SameLine();
                if (ImGui.Button($"编辑##{i}"))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    EditItem = data;
                    ShowEditWindow = true;
                }
                ImGui.SameLine();
                if (ImGui.Button($"删除##{i}"))
                {
                    SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                    ShowEditWindow = false;
                    Config.ItemModifyList.Remove(data);
                    Config.Write();
                    ClientLoader.Chat.WriteLine($"已删除物品预设: {data.Name}", Color.Yellow);
                }
                ImGui.NextColumn();

                ImGui.Columns(1);
                ImGui.PopID();

                // 悬停区域显示工具提示
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    Item tempItem = new Item();
                    tempItem.SetDefaults(data.Type);
                    data.ApplyTo(tempItem);
                    TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
                }
            }

            // 如果没有物品显示提示
            if (GetPage.Count == 0)
            {
                ImGui.Text("");
                if (string.IsNullOrWhiteSpace(SearchFilter))
                {
                    ImGui.Text($"没有物品预设 请使用Alt + {Config.ItemModifyKey} 添加");
                }
                else
                {
                    ImGui.Text($"没有找到包含 '{SearchFilter}' 的物品预设");
                }
            }

            ImGui.EndChild();
        }
        ImGui.End();

        // 显示物品编辑窗口
        if (ShowEditWindow && EditItem != null)
        {
            DrawItemEditWindow(plr);
        }
    }
    #endregion

    #region 分页与跳转功能
    private static int NowPage = 0; // 当前页码
    private const int PageLimit = 8; // 每页显示8个物品
    private static int AllPages = 0; // 总页数

    private static void ListPage(int totalItems)
    {
        // 显示分页信息和控件
        ImGui.Text($"第 {NowPage + 1} / {AllPages} 页 (共 {totalItems} 个物品)");
        ImGui.SameLine();

        // 上一页按钮
        if (ImGui.Button("上页") && NowPage > 0)
        {
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);
            NowPage--;
        }

        // 下一页按钮
        ImGui.SameLine();
        if (ImGui.Button("下页") && NowPage < AllPages - 1)
        {
            // 播放界面点击音效
            SoundEngine.PlaySound(SoundID.MenuTick);
            NowPage++;
        }
    }
    #endregion

    #region 物品编辑器窗口
    private static ItemData EditItem = null!; // 当前正在编辑的物品
    private static bool ShowEditWindow = false; // 是否显示编辑窗口
    private static void DrawItemEditWindow(Player plr)
    {
        ImGui.SetNextWindowSize(new Vector2(200, 200), ImGuiCond.FirstUseEver);
        if (ImGui.Begin($"编辑物品: {EditItem.Name}", ref ShowEditWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 基本信息
            ImGui.Text($"物品类型: {EditItem.Type} ({Lang.GetItemNameValue(EditItem.Type)})");

            // 添加悬停区域显示工具提示
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayNormal | ImGuiHoveredFlags.AllowWhenDisabled))
            {
                Item tempItem = new Item();
                tempItem.SetDefaults(EditItem.Type);
                EditItem.ApplyTo(tempItem);
                TerraAngel.Graphics.ImGuiUtil.ImGuiItemTooltip(tempItem);
            }

            ImGui.Separator();

            // 保存按钮
            if (ImGui.Button("保存更改"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                ShowEditWindow = false;
                Config.Write();
                EditItem.ApplyTo(Main.player[Main.myPlayer].HeldItem);
                ClientLoader.Chat.WriteLine($"已保存物品预设: {EditItem.Name}", Color.Green);
            }

            ImGui.SameLine();
            if (ImGui.Button("取消"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose, (int)plr.position.X / 16, (int)plr.position.Y / 16, 0, 5, 0);
                ShowEditWindow = false;
                ClientLoader.Chat.WriteLine($"已取消本次修改: {EditItem.Name}", Color.Green);
            }

            // 名称编辑
            string name = EditItem.Name;
            ImGui.InputText("名称", ref name, 100);
            EditItem.Name = name;

            // 基础属性
            ImGui.Separator();
            ImGui.Text("基础属性:");

            int damage = EditItem.Damage;
            ImGui.InputInt("伤害", ref damage);
            EditItem.Damage = damage;

            int defense = EditItem.Defense;
            ImGui.InputInt("防御", ref defense);
            EditItem.Defense = defense;

            int stack = EditItem.Stack;
            ImGui.InputInt("数量", ref stack);
            EditItem.Stack = stack;

            int prefix = (byte)EditItem.Prefix;
            ImGui.InputInt("前缀ID", ref prefix);
            EditItem.Prefix = (byte)prefix;

            int crit = EditItem.Crit;
            ImGui.InputInt("暴击率", ref crit);
            EditItem.Crit = crit;

            float knockBack = EditItem.KnockBack;
            ImGui.InputFloat("击退", ref knockBack);
            EditItem.KnockBack = knockBack;

            int bait = EditItem.bait; // 添加钓鱼饵属性
            ImGui.InputInt("渔饵力", ref bait);
            EditItem.bait = bait;

            int fishingPole = EditItem.fishingPole; // 添加钓鱼竿等级属性
            ImGui.InputInt("渔力", ref fishingPole);
            EditItem.fishingPole = fishingPole;

            int pick = EditItem.pick; // 添加镐力属性
            ImGui.InputInt("镐力", ref pick);
            EditItem.pick = pick;

            int axe = EditItem.axe; // 添加斧力属性
            ImGui.InputInt("斧力", ref axe);
            EditItem.axe = axe;

            int hammer = EditItem.hammer; // 添加锤力属性
            ImGui.InputInt("锤力", ref hammer);
            EditItem.hammer = hammer;

            int createTile = EditItem.createTile; // 创建的方块类型
            ImGui.InputInt("图格ID", ref createTile);
            EditItem.createTile = createTile;

            int createWall = EditItem.createWall; // 创建的墙类型
            ImGui.InputInt("墙壁ID", ref createWall);
            EditItem.createWall = createWall;

            int value = EditItem.Value;
            ImGui.InputInt("价值", ref value);
            EditItem.Value = value;

            // 射击属性
            ImGui.Separator();
            ImGui.Text("射击属性:");
            int shoot = EditItem.Shoot;
            ImGui.InputInt("弹幕ID", ref shoot);
            EditItem.Shoot = shoot;
            float shootSpeed = EditItem.ShootSpeed;
            ImGui.InputFloat("发射速度", ref shootSpeed);
            EditItem.ShootSpeed = shootSpeed;

            // 使用属性
            ImGui.Separator();
            ImGui.Text("使用属性:");

            int useTime = EditItem.UseTime;
            ImGui.InputInt("使用时间", ref useTime);
            EditItem.UseTime = useTime;

            int useAnimation = EditItem.UseAnimation;
            ImGui.InputInt("使用动画", ref useAnimation);
            EditItem.UseAnimation = useAnimation;

            int useAmmo = EditItem.UseAmmo;
            ImGui.InputInt("使用弹药", ref useAmmo);
            EditItem.UseAmmo = useAmmo;

            int ammo = EditItem.Ammo;
            ImGui.InputInt("是否弹药", ref ammo);
            EditItem.Ammo = ammo;

            int useStyle = EditItem.UseStyle;
            ImGui.InputInt("使用样式", ref useStyle);
            EditItem.UseStyle = useStyle;

            int healLife = EditItem.HealLife; // 物品使用时回复的生命值
            ImGui.InputInt("回复生命", ref healLife);
            EditItem.HealLife = healLife;

            int healMana = EditItem.HealMana; // 物品使用时回复的魔法值
            ImGui.InputInt("回复魔法", ref healMana);
            EditItem.HealMana = healMana;

            // 武器类型
            ImGui.Separator();
            ImGui.Text("物品类型:");
            bool melee = EditItem.Melee;
            ImGui.Checkbox("近战", ref melee);
            EditItem.Melee = melee;
            ImGui.SameLine();
            bool magic = EditItem.Magic;
            ImGui.Checkbox("魔法", ref magic);
            EditItem.Magic = magic;
            ImGui.SameLine();
            bool ranged = EditItem.Ranged;
            ImGui.Checkbox("远程", ref ranged);
            EditItem.Ranged = ranged;
            ImGui.SameLine();
            bool summon = EditItem.Summon;
            ImGui.Checkbox("召唤", ref summon);
            EditItem.Summon = summon;
            ImGui.SameLine();
            bool sentry = EditItem.sentry; // 是否为哨兵
            ImGui.SameLine();
            ImGui.Checkbox("哨兵", ref sentry);

            ImGui.Separator();
            bool accessory = EditItem.accessory; // 是否为饰品
            ImGui.Checkbox("饰品", ref accessory);
            EditItem.accessory = accessory;
            ImGui.SameLine();
            bool wornArmor = EditItem.wornArmor;
            ImGui.Checkbox("盔甲", ref wornArmor);
            EditItem.wornArmor = wornArmor;
            ImGui.SameLine();
            bool consumable = EditItem.consumable; // 是否为消耗品
            ImGui.Checkbox("消耗品", ref consumable);
            EditItem.consumable = consumable;
            ImGui.SameLine();
            bool material = EditItem.material; // 是否为材料
            ImGui.Checkbox("材料", ref material);
            EditItem.material = material;

            ImGui.Separator();
            int headSlot = EditItem.headSlot; // 头盔槽位
            ImGui.InputInt("头盔槽位", ref headSlot);
            EditItem.headSlot = headSlot;
            int bodySlot = EditItem.bodySlot; // 上衣槽位
            ImGui.InputInt("上衣槽位", ref bodySlot);
            EditItem.bodySlot = bodySlot;
            int legSlot = EditItem.legSlot; // 裤子槽位
            ImGui.InputInt("裤子槽位", ref legSlot);
            EditItem.legSlot = legSlot;

            ImGui.Separator();
            ImGui.Text("使用类型:");
            bool autoReuse = EditItem.AutoReuse;
            ImGui.Checkbox("自动挥舞", ref autoReuse);
            EditItem.AutoReuse = autoReuse;
            ImGui.SameLine();
            bool useTurn = EditItem.UseTurn;
            ImGui.Checkbox("使用转向", ref useTurn);
            EditItem.UseTurn = useTurn;
            ImGui.SameLine();
            bool channel = EditItem.Channel;
            ImGui.Checkbox("持续使用", ref channel);
            EditItem.Channel = channel;
            ImGui.SameLine();
            bool noMelee = EditItem.NoMelee;
            ImGui.Checkbox("无近战伤害", ref noMelee);
            EditItem.NoMelee = noMelee;
            ImGui.SameLine();
            bool noUseGraphic = EditItem.NoUseGraphic;
            ImGui.Checkbox("无使用图像", ref noUseGraphic);
            EditItem.NoUseGraphic = noUseGraphic;
        }
        ImGui.End();
    }
    #endregion

    #region 一键修改前缀窗口
    public static bool ShowEditPrefix = false; // 显示批量修改前缀窗口
    private static int PrefixId = 0; // 用于存储新前缀ID的变量
    private static void DrawEditPrefixWindow()
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 150), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));

        if (ImGui.Begin("批量修改饰品前缀", ref ShowEditPrefix, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Text("将修改玩家饰品栏的前缀");

            ImGui.Spacing();
            ImGui.InputInt("新前缀ID", ref PrefixId);

            // 限制前缀ID在有效范围内(0-84)
            if (PrefixId < 0) PrefixId = 0;
            if (PrefixId > 84) PrefixId = 84;

            ImGui.Spacing();
            if (ImGui.Button("应用"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose);
                ApplyPrefix();
                ShowEditPrefix = false;
            }

            ImGui.SameLine();
            if (ImGui.Button("取消"))
            {
                // 播放界面关闭音效
                SoundEngine.PlaySound(SoundID.MenuClose);
                ShowEditPrefix = false;
            }
        }
        ImGui.End();
    }
    #endregion

    #region 批量修改饰品
    private static void ApplyPrefix()
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

    #region 按键选择器辅助方法
    private void DrawKeySelector(string label, ref Keys key, ref bool editing)
    {
        // 显示按键标签和当前按键
        ImGui.Text($"{label}:");
        ImGui.SameLine();

        if (ImGui.Button($"{key}##{label}"))
        {
            editing = !editing;
        }

        // 如果正在编辑，显示提示
        if (editing)
        {
            ImGui.SameLine();
            ImGui.TextColored(new System.Numerics.Vector4(1, 1, 0, 1), "[按下新按键]");

            // 检测所有按键
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                if (k == Keys.None) continue;

                if (InputSystem.IsKeyPressed(k))
                {
                    // 播放按键选择音效
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    key = k;
                    editing = false;
                    break;
                }
            }
        }
    }
    #endregion
}