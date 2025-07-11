using ImGuiNET;
using Microsoft.Xna.Framework;
using System.Numerics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using static MyPlugin.MyPlugin;

namespace MyPlugin;

public class NPCShopEditorUI
{
    private static bool showShopEditor = false;
    private static bool showItemEditWindow = false;
    private static int selectedNPCType = NPCID.Merchant;
    private static int selectedItemIndex = -1;
    private static bool addingNewItem = false;
    private static bool editingItem = false;
    private static CShopItemInfo newItem = new CShopItemInfo();
    private static Dictionary<int, string> npcNameCache = new Dictionary<int, string>();
    private static string unlockSearch = "";

    #region 切换窗口方法
    public static void ToggleWindow()
    {
        showShopEditor = !showShopEditor;

        if (showShopEditor)
        {
            // 初始化UI状态
            selectedItemIndex = -1;
            addingNewItem = false;
            editingItem = false;
            showItemEditWindow = false; // 重置编辑窗口状态
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
    } 
    #endregion

    #region 渲染主窗口
    public static void Draw()
    {
        if (!showShopEditor) return;

        ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);
        ImGui.Begin("NPC商店编辑器", ref showShopEditor, ImGuiWindowFlags.NoCollapse);

        // 上半部分：NPC选择区
        DrawNPCSelection();

        ImGui.Separator();

        // 下半部分：物品编辑区
        DrawItemEditor();

        ImGui.End();

        // 单独绘制物品编辑窗口
        DrawItemEditWindow();
    } 
    #endregion

    #region NPC选择器
    private static void DrawNPCSelection()
    {
        ImGui.BeginChild("NPCSelection", new Vector2(0, 120), ImGuiChildFlags.Borders);

        // 标题
        ImGui.TextColored(Color.Gold.ToVector4(), "选择NPC:");

        // NPC类型下拉菜单
        ImGui.SetNextItemWidth(250);
        if (ImGui.BeginCombo("##NPCType", GetNPCName(selectedNPCType)))
        {
            foreach (var npcType in GetShopNPCs())
            {
                if (ImGui.Selectable(GetNPCName(npcType), selectedNPCType == npcType))
                {
                    selectedNPCType = npcType;
                    selectedItemIndex = -1;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();
        if (ImGui.Button("刷新配置"))
        {
            ReloadConfig();
        }

        // 显示当前NPC的商店信息
        int shopIndex = Config.FindShopItem(selectedNPCType);
        if (shopIndex != -1)
        {
            var shop = Config.Shop[shopIndex];
            bool enabled = shop.Enabled;
            bool ReplaceOriginal = shop.ReplaceOriginal;

            ImGui.Text($"商店状态: {(enabled ? "已启用" : "已禁用")}");
            if (ImGui.Checkbox("启用此商店", ref enabled))
            {
                shop.Enabled = enabled;
                Config.Write();
            }

            ImGui.SameLine();
            if (ImGui.Checkbox("清理原版商店", ref ReplaceOriginal))
            {
                shop.ReplaceOriginal = ReplaceOriginal;
                Config.Write();
            }

            ImGui.TextColored(new Vector4(1f, 0.8f, 0.6f, 1f), "按Esc退出对话时立即刷新商店");
        }
        else
        {
            ImGui.TextColored(Color.Red.ToVector4(), "未找到此NPC的商店配置");
            if (ImGui.Button("创建新商店配置"))
            {
                CreateNewShopForNPC(selectedNPCType);
            }
        }

        ImGui.EndChild();
    } 
    #endregion

    #region 物品列表区
    private static void DrawItemEditor()
    {
        int shopIndex = Config.FindShopItem(selectedNPCType);
        if (shopIndex == -1 || !Config.Shop[shopIndex].Enabled)
        {
            ImGui.TextColored(Color.Red.ToVector4(), "请先启用或创建商店配置");
            return;
        }

        var shop = Config.Shop[shopIndex];
        var items = shop.item;

        ImGui.BeginChild("Item Editor", new Vector2(0, 0), ImGuiChildFlags.Borders);
        ImGui.TextColored(Color.Gold.ToVector4(), "商店物品列表:");
        ImGui.SameLine();
        if (ImGui.Button("添加新物品"))
        {
            addingNewItem = true;
            editingItem = false;
            newItem = new CShopItemInfo();

            // 设置默认价格（100铜币）
            coinPlatinum = 0;
            coinGold = 0;
            coinSilver = 1;
            coinCopper = 0;
            newItem.price = 100; // 100铜币
            newItem.stack = 1;   // 默认数量为1

            showItemEditWindow = true;
        }

        // 物品列表
        if (ImGui.BeginTable("ItemsTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupColumn("物品", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("前缀", ImGuiTableColumnFlags.WidthFixed, 80);
            ImGui.TableSetupColumn("数量", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("价格", ImGuiTableColumnFlags.WidthFixed, 150);
            ImGui.TableSetupColumn("解锁条件", ImGuiTableColumnFlags.WidthFixed, 150); // 新增
            ImGui.TableSetupColumn("操作", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableHeadersRow();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                ImGui.TableNextRow();

                // 物品名称 (只覆盖第一列)
                ImGui.TableSetColumnIndex(0);
                string itemName = Lang.GetItemNameValue(item.id) ?? $"物品ID: {item.id}";
                if (ImGui.Selectable(itemName, selectedItemIndex == i, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 0)))
                {
                    selectedItemIndex = i;
                    editingItem = false;
                }

                // 前缀 (第二列)
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(item.prefix > 0 ? Lang.prefix[item.prefix].Value : "无");

                // 数量 (第三列)
                ImGui.TableSetColumnIndex(2);
                ImGui.Text(item.stack.ToString());

                // 价格 (第四列)
                ImGui.TableSetColumnIndex(3);
                if (item.price > 0)
                {
                    ImGui.Text($"{item.price} 铜币");
                }
                else
                {
                    ImGui.Text("默认价格");
                }

                // 解锁条件 (第五列)
                ImGui.TableSetColumnIndex(4);
                if (item.unlock.Count > 0)
                {
                    ImGui.TextWrapped(string.Join(", ", item.unlock));
                }
                else
                {
                    ImGui.Text("无");
                }

                // 操作 (第六列)
                ImGui.TableSetColumnIndex(5);
                if (ImGui.SmallButton($"编辑##{i}"))
                {
                    selectedItemIndex = i;
                    editingItem = true;
                    newItem = item.Clone();

                    // 将物品价格分解为不同币种
                    int price = newItem.price;
                    coinPlatinum = price / 1000000;
                    price %= 1000000;
                    coinGold = price / 10000;
                    price %= 10000;
                    coinSilver = price / 100;
                    coinCopper = price % 100;

                    showItemEditWindow = true;
                }

                ImGui.SameLine();
                if (ImGui.SmallButton($"删除##{i}"))
                {
                    items.RemoveAt(i);
                    Config.Write();
                    selectedItemIndex = -1;
                    i--;
                }
            }
            ImGui.EndTable();
        }

        ImGui.EndChild();
    }
    #endregion

    #region 物品编辑窗口
    private static int coinPlatinum = 0;
    private static int coinGold = 0;
    private static int coinSilver = 0;
    private static int coinCopper = 0;
    private static bool showSetTotalPrice = false;
    private static int tempTotalPrice = 0;
    public static bool showItemSearch = false;
    private static string itemSearchText = "";
    private static void DrawItemEditWindow()
    {
        if (!showItemEditWindow) return;

        int shopIndex = Config.FindShopItem(selectedNPCType);
        if (shopIndex == -1 || !Config.Shop[shopIndex].Enabled) return;

        var shop = Config.Shop[shopIndex];
        var items = shop.item;

        string title = addingNewItem ? "添加新物品" : "编辑物品";
        ImGui.SetNextWindowSize(new Vector2(600, 700), ImGuiCond.FirstUseEver);
        if (ImGui.Begin(title, ref showItemEditWindow, ImGuiWindowFlags.NoCollapse))
        {
            // 物品ID选择部分 - 添加了搜索功能
            ImGui.Text("物品ID:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt("##ItemID", ref newItem.id);

            // 添加物品搜索按钮
            ImGui.SameLine();
            if (ImGui.Button("搜索物品"))
            {
                showItemSearch = true;
                itemSearchText = "";
            }

            // 显示物品名称
            ImGui.SameLine();
            string itemName = Lang.GetItemNameValue(newItem.id) ?? "未知物品";
            ImGui.Text(itemName);

            // 物品搜索窗口
            if (showItemSearch)
            {
                ShowItemSearch(shop);
            }

            // 前缀选择
            ImGui.Text("前缀:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            if (ImGui.BeginCombo("##Prefix", newItem.prefix > 0 ? Lang.prefix[newItem.prefix].Value : "无"))
            {
                if (ImGui.Selectable("无", newItem.prefix == 0))
                {
                    newItem.prefix = 0;
                }

                // 只显示前79个标准前缀（避免显示mod内容）
                for (int i = 1; i < 84; i++)
                {
                    if (ImGui.Selectable(Lang.prefix[i].Value, newItem.prefix == i))
                    {
                        newItem.prefix = i;
                    }
                }
                ImGui.EndCombo();
            }

            // 数量输入
            ImGui.Text("数量:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##Stack", ref newItem.stack);
            newItem.stack = Math.Max(1, newItem.stack);

            // 价格输入部分 - 重构为多币种输入
            ImGui.Separator();
            ImGui.TextColored(Color.Gold.ToVector4(), "价格设置:");

            // 计算当前总价
            int currentPrice = coinPlatinum * 1000000 + coinGold * 10000 + coinSilver * 100 + coinCopper;

            // 总价显示
            ImGui.Text("总价格: ");
            ImGui.SameLine();
            ImGui.TextColored(Color.LightGreen.ToVector4(), $"{currentPrice * newItem.stack} 铜币");
            ImGui.SameLine();
            if (ImGui.Button("设置单价"))
            {
                tempTotalPrice = currentPrice;
                showSetTotalPrice = true;
            }
            if (ImGui.IsItemHovered())
            {
                var text = "注意价格为单价(数量 × 值 = 总价)，并随着NPC好感度变化";
                ImGui.SetTooltip(text);
            }

            // 各币种输入
            ImGui.Text("铂金币:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##Platinum", ref coinPlatinum);
            coinPlatinum = Math.Max(0, coinPlatinum);

            ImGui.SameLine();
            ImGui.Text("金币:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##Gold", ref coinGold);
            coinGold = Math.Max(0, coinGold);

            ImGui.SameLine();
            ImGui.Text("银币:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##Silver", ref coinSilver);
            coinSilver = Math.Max(0, coinSilver);

            ImGui.SameLine();
            ImGui.Text("铜币:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##Copper", ref coinCopper);
            coinCopper = Math.Max(0, coinCopper);

            // 设置总价窗口
            if (showSetTotalPrice)
            {
                ImGui.OpenPopup("设置单价");
                if (ImGui.BeginPopupModal("设置单价", ref showSetTotalPrice, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Text("输入单价(铜币):");
                    ImGui.InputInt("##TotalPrice", ref tempTotalPrice);
                    tempTotalPrice = Math.Max(0, tempTotalPrice);

                    if (ImGui.Button("确定"))
                    {
                        // 分解总价到各币种
                        int remaining = tempTotalPrice;
                        coinPlatinum = remaining / 1000000;
                        remaining %= 1000000;
                        coinGold = remaining / 10000;
                        remaining %= 10000;
                        coinSilver = remaining / 100;
                        coinCopper = remaining % 100;

                        showSetTotalPrice = false;
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("取消"))
                    {
                        showSetTotalPrice = false;
                    }

                    ImGui.EndPopup();
                }
            }

            // 解锁条件部分
            ImGui.Separator();
            ImGui.TextColored(Color.LightSkyBlue.ToVector4(), "解锁条件:");

            // 搜索框
            ImGui.Text("搜索条件:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("##UnlockSearch", ref unlockSearch, 50);

            // 使用分组实现左右布局
            ImGui.BeginGroup();

            // 左侧：条件选择列表
            ImGui.BeginChild("UnlockConditions", new Vector2(300, 150), ImGuiChildFlags.Borders);
            var allConditions = GetAllUnlockConditions();
            foreach (var condition in allConditions)
            {
                if (!string.IsNullOrEmpty(unlockSearch) &&
                    !condition.Contains(unlockSearch, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool hasCondition = newItem.unlock.Contains(condition);
                if (ImGui.Checkbox(condition, ref hasCondition))
                {
                    if (hasCondition)
                    {
                        if (!newItem.unlock.Contains(condition))
                        {
                            newItem.unlock.Add(condition);
                        }
                    }
                    else
                    {
                        newItem.unlock.Remove(condition);
                    }
                }
            }
            ImGui.EndChild();

            ImGui.EndGroup(); // 结束左侧分组

            ImGui.SameLine(); // 切换到右侧

            // 右侧：当前解锁条件
            ImGui.BeginGroup();
            ImGui.Text("当前解锁条件:");
            ImGui.BeginChild("CurrentUnlockConditions", new Vector2(0, 150), ImGuiChildFlags.Borders);
            foreach (var condition in newItem.unlock)
            {
                ImGui.Text($"- {condition}");
            }
            ImGui.EndChild();
            ImGui.EndGroup(); // 结束右侧分组

            // 保存/取消按钮
            ImGui.Separator();
            if (ImGui.Button("保存"))
            {
                newItem.price = coinPlatinum * 1000000 +
                 coinGold * 10000 +
                 coinSilver * 100 +
                 coinCopper;

                if (addingNewItem)
                {
                    items.Add(newItem);
                }
                else if (editingItem && selectedItemIndex >= 0)
                {
                    items[selectedItemIndex] = newItem;
                }

                Config.Write();
                addingNewItem = false;
                editingItem = false;
                showItemEditWindow = false;
            }

            ImGui.SameLine();
            if (ImGui.Button("取消"))
            {
                addingNewItem = false;
                editingItem = false;
                showItemEditWindow = false;
            }

            ImGui.End();
        }
    }
    #endregion

    #region 搜索物品方法
    private static void ShowItemSearch(ShopItem shop)
    {
        ImGui.SetNextWindowSize(new Vector2(450, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("搜索物品", ref showItemSearch, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.TextColored(new Vector4(1f, 0.8f, 0.6f, 1f), "搜索物品:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("##ItemSearch", ref itemSearchText, 100);

            ImGui.BeginChild("ItemList", new Vector2(0, 400), ImGuiChildFlags.Borders);

            // 获取当前商店的所有物品ID用于排除
            var existingItemIds = shop.item.Select(i => i.id).ToHashSet();

            // 使用提取的搜索逻辑
            var filteredItems = ContentSamples.ItemsByType.Where(kvp =>
            {
                if (kvp.Value == null || kvp.Key <= 0) return false;
                if (string.IsNullOrWhiteSpace(itemSearchText)) return true;

                return kvp.Value.Name.Contains(itemSearchText, StringComparison.OrdinalIgnoreCase) ||
                       kvp.Key.ToString().Contains(itemSearchText);
            });

            foreach (var kvp in filteredItems)
            {
                Item item = kvp.Value;
                if (item.type == 0 || item.Name == null) continue;

                // 检查物品是否已存在于当前商店
                bool alreadyExists = existingItemIds.Contains(item.type);

                // 设置颜色提示
                if (alreadyExists)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.5f, 0.5f, 1f));
                }

                // 允许选择已存在物品（因为可能需要编辑），但视觉上标记
                if (ImGui.Selectable($"{item.Name} (ID:{item.type})##ItemSearch", newItem.id == item.type,
                    alreadyExists ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.None))
                {
                    newItem.id = item.type;
                    showItemSearch = false;
                }

                if (alreadyExists)
                {
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    ImGui.TextColored(new Vector4(1f, 0.5f, 0.5f, 1f), "(已存在)");
                }

                // 工具提示显示物品信息
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"ID: {item.type}");
                    ImGui.Text($"名称: {item.Name}");
                    ImGui.Text($"类型: {item.type}");
                    ImGui.Text($"稀有度: {item.rare}");
                    ImGui.EndTooltip();
                }
            }
            ImGui.EndChild();
        }
        ImGui.End();
    } 
    #endregion

    #region 获取NPC名称
    private static string GetNPCName(int npcType)
    {
        if (!npcNameCache.TryGetValue(npcType, out var name))
        {
            name = Lang.GetNPCNameValue(npcType) ?? $"NPC ID: {npcType}";
            npcNameCache[npcType] = name;
        }
        return name;
    } 
    #endregion

    #region 获取拥有商店的NPCID
    private static List<int> GetShopNPCs()
    {
        return new List<int>
        {
            NPCID.Merchant,
            NPCID.ArmsDealer,
            NPCID.Demolitionist,
            NPCID.Clothier,
            NPCID.Wizard,
            NPCID.Mechanic,
            NPCID.SantaClaus,
            NPCID.Truffle,
            NPCID.Steampunker,
            NPCID.DyeTrader,
            NPCID.Cyborg,
            NPCID.WitchDoctor,
            NPCID.Pirate,
            NPCID.TravellingMerchant,
            NPCID.SkeletonMerchant,
            NPCID.Golfer,
            NPCID.BestiaryGirl,
            NPCID.Princess,
            NPCID.Dryad,
            NPCID.DD2Bartender,
            NPCID.PartyGirl,
            NPCID.Stylist,
            NPCID.GoblinTinkerer,
            NPCID.Painter
        };
    }
    #endregion

    #region 获取所有解锁条件名称
    private static List<string> GetAllUnlockConditions()
    {
        return new List<string>
        {
            "史莱姆王", "克眼", "巨鹿", "克脑", "蜂王", "骷髅王",
            "困难模式", "毁灭者", "双子魔眼", "机械骷髅王", "世纪之花",
            "石巨人", "史莱姆皇后", "光之女皇", "猪鲨", "教徒",
            "月亮领主", "哀木", "南瓜王", "常绿尖叫怪", "冰雪女王",
            "圣诞坦克", "火星飞碟", "小丑", "日耀柱", "星旋柱",
            "星云柱", "星尘柱", "一王后", "三王后", "一柱后",
            "四柱后", "哥布林入侵", "海盗入侵", "霜月", "血月",
            "雨天", "白天", "晚上", "大风天", "万圣节",
            "圣诞节", "派对", "2020", "2021", "ftw",
            "ntb", "dst", "森林", "丛林", "沙漠",
            "雪原", "洞穴", "海洋", "神圣", "蘑菇",
            "腐化之地", "猩红之地", "地牢", "墓地", "蜂巢",
            "神庙", "沙尘暴", "天空", "满月", "亏凸月",
            "下弦月", "残月", "新月", "娥眉月", "上弦月",
            "盈凸月"
        };
    }
    #endregion

    #region 创建新自定义商店到指定NPC
    private static void CreateNewShopForNPC(int npcType)
    {
        var newShop = new ShopItem
        {
            Enabled = true,
            Name = Lang.GetNPCNameValue(npcType) + "(商店)",
            NpcType = npcType,
            item = new List<CShopItemInfo>()
        };

        Config.Shop.Add(newShop);
        Config.Write();
    } 
    #endregion
}