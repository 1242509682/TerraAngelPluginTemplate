using System.Reflection;
using MonoMod.RuntimeDetour;
using Newtonsoft.Json;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using static MyPlugin.MyPlugin;
using static MyPlugin.UITool;

namespace MyPlugin;

public static class RecipeHooks
{
    // 反射获取的私有方法委托
    private static Func<Player, Recipe, bool>? PlayerMeetsEnvironmentConditions;
    public static Func<Player, Recipe, bool>? PlayerMeetsTileRequirements;
    private static Action<Player>? CollectItemsToCraftWithFrom;

    // 钩子
    private static Hook? FindRecipesHook;
    public static event Action<RecipeCheckEventArgs>? OnRecipeCheck; // 配方检查事件
    public static event Action? BeforeRecipeCheck; // 添加自定义配方(配方查找前调用)

    #region 注册与卸载钩子方法
    public static void Register()
    {
        // 反射获取私有方法委托
        // 检查玩家是否满足环境条件（如血月、夜晚等）
        PlayerMeetsEnvironmentConditions = CreateDelegate<Func<Player, Recipe, bool>>(typeof(Recipe), "PlayerMeetsEnvironmentConditions", BindingFlags.NonPublic | BindingFlags.Static);

        // 检查玩家是否满足工作站条件
        PlayerMeetsTileRequirements = CreateDelegate<Func<Player, Recipe, bool>>(typeof(Recipe), "PlayerMeetsTileRequirements", BindingFlags.NonPublic | BindingFlags.Static);

        // 收集玩家可用于合成的物品
        CollectItemsToCraftWithFrom = CreateDelegate<Action<Player>>(typeof(Recipe), "CollectItemsToCraftWithFrom", BindingFlags.NonPublic | BindingFlags.Static);

        // 查找配方（用于决定配方是否显示）
        MethodInfo findRecipes = typeof(Recipe).GetMethod("FindRecipes", BindingFlags.Public | BindingFlags.Static, [typeof(bool)])!;
        FindRecipesHook = new Hook(findRecipes, OnFindRecipes);
    }
    #endregion

    #region  卸载事件钩子方法
    public static void Dispose()
    {
        FindRecipesHook?.Dispose();
        OnRecipeCheck = null;
        BeforeRecipeCheck = null;
    }
    #endregion

    #region 创建委托的辅助方法
    private static T? CreateDelegate<T>(Type type, string methodName, BindingFlags flags) where T : Delegate
    {
        try
        {
            MethodInfo method = type.GetMethod(methodName, flags)!;
            return method != null ? (T)Delegate.CreateDelegate(typeof(T), method) : null;
        }
        catch
        {
            return null;
        }
    }
    #endregion

    #region 自定义的 FindRecipes 挂钩 （配方检查事件）
    private delegate void orig_FindRecipes(bool canDelayCheck);
    private static void OnFindRecipes(orig_FindRecipes orig, bool canDelayCheck)
    {
        // 处理延迟检查情况
        if (canDelayCheck)
        {
            orig(canDelayCheck);
            return;
        }

        // 保存当前聚焦的配方信息
        int oldRecipe = Main.availableRecipe[Main.focusRecipe];
        float focusY = Main.availableRecipeY[Main.focusRecipe];

        // 清理可用配方
        Recipe.ClearAvailableRecipes();

        // 先添加自定义配方
        BeforeRecipeCheck?.Invoke();

        // 处理向导物品的配方
        if (!Main.guideItem.IsAir && !string.IsNullOrEmpty(Main.guideItem.Name))
        {
            CollectGuideRecipes();
            TryRefocusingRecipe(oldRecipe);
            VisuallyRepositionRecipes(focusY);
            return;
        }

        Player plr = Main.LocalPlayer;

        // 收集玩家可用于合成的物品
        CollectItemsToCraftWithFrom?.Invoke(plr);

        // 获取原版收集的物品数据
        var HasItems = GetCollectedItems();

        // 遍历所有配方
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Recipe Recipe = Main.recipe[i];
            int ActiveRecipe = Main.availableRecipe[i];
            float ActiveRecipeY = Main.availableRecipeY[i];

            // 跳过空配方
            if (Recipe.createItem.type == 0) continue;

            // 检查合成站条件
            bool MeetsTile = PlayerMeetsTileRequirements!(plr, Recipe);

            // 检查环境条件
            bool Environment = PlayerMeetsEnvironmentConditions!(plr, Recipe);

            // 检查材料是否足够
            bool Material = Recipe.CollectedEnoughItemsToCraftRecipeNew(Recipe);

            // 初始条件检查结果
            bool Conditions = MeetsTile && Environment && Material;

            // 创建事件参数
            var args = new RecipeCheckEventArgs(Recipe, i, plr, Conditions, MeetsTile, Material, Environment, HasItems, ActiveRecipe, ActiveRecipeY);

            // 触发自定义配方检查事件
            OnRecipeCheck?.Invoke(args);

            // 使用事件处理后的结果
            if (args.Conditions)
            {
                AddToAvailableRecipes(i);
            }
        }

        // 恢复配方聚焦和位置
        TryRefocusingRecipe(oldRecipe);
        VisuallyRepositionRecipes(focusY);
    }
    #endregion

    #region 从Recipe类里抄来的辅助方法
    // 添加可用配方
    public static void AddToAvailableRecipes(int recipeIndex)
    {
        Main.availableRecipe[Main.numAvailableRecipes] = recipeIndex;
        Main.numAvailableRecipes++;
    }

    // 尝试重新聚焦配方
    public static void TryRefocusingRecipe(int oldRecipe)
    {
        for (int i = 0; i < Main.numAvailableRecipes; i++)
        {
            if (oldRecipe == Main.availableRecipe[i])
            {
                Main.focusRecipe = i;
                break;
            }
        }

        if (Main.focusRecipe >= Main.numAvailableRecipes)
            Main.focusRecipe = Main.numAvailableRecipes - 1;

        if (Main.focusRecipe < 0)
            Main.focusRecipe = 0;
    }

    // 视觉定位配方
    public static void VisuallyRepositionRecipes(float focusY)
    {
        if (Main.numAvailableRecipes == 0) return;

        float num = Main.availableRecipeY[Main.focusRecipe] - focusY;
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Main.availableRecipeY[i] -= num;
        }
    }

    // 收集向导配方
    public static void CollectGuideRecipes()
    {
        int type = Main.guideItem.type;
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];
            if (recipe.createItem.type == 0)
            {
                break;
            }

            for (int j = 0; j < Recipe.maxRequirements; j++)
            {
                Item item = recipe.requiredItem[j];
                if (item.type == 0)
                {
                    break;
                }

                if (Main.guideItem.IsTheSameAs(item) || recipe.useWood(type, item.type) || recipe.useSand(type, item.type) || recipe.useIronBar(type, item.type) || recipe.useFragment(type, item.type) || recipe.AcceptedByItemGroups(type, item.type) || recipe.usePressurePlate(type, item.type))
                {
                    Main.availableRecipe[Main.numAvailableRecipes] = i;
                    Main.numAvailableRecipes++;
                    break;
                }
            }
        }
    }
    #endregion

    #region 配方增删改查方法
    // 查找第一个空配方槽位
    public static int FindEmptyRecipeSlot()
    {
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            if (Main.recipe[i].createItem.type == 0)
            {
                return i;
            }
        }
        return -1;
    }

    // 移除配方
    public static void RemoveRecipe(int Index)
    {
        if (Index >= 0 && Index < Recipe.maxRecipes)
        {
            Main.recipe[Index] = new Recipe();
            Main.availableRecipe[Index] = 0;
        }
    }

    // 查找配方的索引
    public static int FindRecipeIndex(int itemId)
    {
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            if (Main.recipe[i].createItem.type == itemId)
            {
                return i;
            }
        }
        return -1;
    }

    // 分配自定义配方索引
    public static void AddRecipeIndex(CustomRecipeData data, int index)
    {
        data.Index = index;
        CustomRecipeIndexes.Add(index);
        CustomRecipeItems.Add(data.ItemID);
    }

    // 重建配方方法
    public static void RebuildCustomRecipes()
    {
        // 先移除所有自定义配方
        foreach (var recipe in Config.CustomRecipes)
        {
            if (recipe.Index != -1)
            {
                RemoveRecipe(recipe.Index);
            }
        }

        // 清除缓存
        CustomRecipeItems.Clear();
        CustomRecipeIndexes.Clear();

        // 重新添加所有自定义配方
        foreach (var recipe in Config.CustomRecipes)
        {
            if (recipe.Index != -1)
            {
                AddToAvailableRecipes(recipe.Index);
            }
        }

        // 重新加载配方
        Recipe.FindRecipes();
        TryRefocusingRecipe(Main.availableRecipe[Main.focusRecipe]);
        VisuallyRepositionRecipes(Main.availableRecipeY[Main.focusRecipe]);
    }
    #endregion

    #region 复用已收集的物品
    public static bool HasMaterial(Recipe recipe, Dictionary<int, int> HasItems)
    {
        // 遍历配方中所有需要的材料
        foreach (Item item in recipe.requiredItem)
        {
            // 跳过无效材料
            if (item.type <= 0 || item.stack <= 0)
                continue;

            // 检查玩家是否拥有足够的该材料
            if (!HasItems.TryGetValue(item.type, out int stack) || stack < item.stack)
            {
                // 如果缺少任何一种材料，立即返回 false
                return false;
            }
        }

        // 所有材料都满足要求，返回 true
        return true;
    }
    #endregion

    #region 检查玩家是否有足够材料制作配方
    private static Dictionary<int, int> GetCollectedItems()
    {
        // 通过反射获取 Recipe._ownedItems 字段
        var ownedItems = typeof(Recipe).GetField("_ownedItems",
            BindingFlags.NonPublic | BindingFlags.Static);

        return (Dictionary<int, int>)ownedItems?.GetValue(null)! ?? new Dictionary<int, int>();
    }
    #endregion

    #region 检查是否存在与原版相同配方
    public static bool ExistsRecipe(CustomRecipeData data)
    {
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];
            if (recipe.createItem.type == 0) continue; // 跳过空槽位

            // 检查结果物品
            if (recipe.createItem.type != data.ItemID ||
                recipe.createItem.stack != data.Stack)
                continue;

            // 检查材料
            if (!AreIngredientsEqual(recipe, data.Material))
                continue;

            // 检查合成站
            if (!AreTilesEqual(recipe, data.Tile))
                continue;

            return true;
        }
        return false;
    }

    // 查找已存在的相同配方索引
    public static int FindExistingRecipeIndex(CustomRecipeData data)
    {
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];
            if (recipe.createItem.type == 0) continue;

            if (recipe.createItem.type == data.ItemID &&
                recipe.createItem.stack == data.Stack &&
                AreIngredientsEqual(recipe, data.Material) &&
                AreTilesEqual(recipe, data.Tile))
            {
                return i;
            }
        }
        return -1;
    }

    // 检查材料是否相同
    public static bool AreIngredientsEqual(Recipe recipe, List<MaterialData> ingredients)
    {
        // 收集配方中的有效材料
        var recipeIngredients = recipe.requiredItem
            .Where(item => item.type > 0)
            .Select(item => new { item.type, item.stack })
            .OrderBy(i => i.type)
            .ToList();

        // 收集自定义配方中的材料
        var customIngredients = ingredients
            .Where(i => i.type > 0)
            .Select(i => new { type = i.type, stack = i.stack })
            .OrderBy(i => i.type)
            .ToList();

        // 比较数量和内容
        if (recipeIngredients.Count != customIngredients.Count)
            return false;

        for (int i = 0; i < recipeIngredients.Count; i++)
        {
            if (recipeIngredients[i].type != customIngredients[i].type ||
                recipeIngredients[i].stack != customIngredients[i].stack)
            {
                return false;
            }
        }

        return true;
    }

    // 检查合成站是否相同
    public static bool AreTilesEqual(Recipe recipe, List<int> requiredTiles)
    {
        // 收集配方中的有效合成站
        var recipeTiles = recipe.requiredTile
            .Where(tile => tile > 0)
            .OrderBy(t => t)
            .ToList();

        // 收集自定义配方中的合成站
        var customTiles = requiredTiles
            .Where(t => t > 0)
            .OrderBy(t => t)
            .ToList();

        // 比较数量和内容
        if (recipeTiles.Count != customTiles.Count)
            return false;

        for (int i = 0; i < recipeTiles.Count; i++)
        {
            if (recipeTiles[i] != customTiles[i])
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region 从数据创建配方实例
    public static Recipe CreateRecipeFromData(CustomRecipeData data)
    {
        Recipe recipe = new Recipe();

        // 设置结果物品
        recipe.createItem.SetDefaults(data.ItemID);
        recipe.createItem.stack = data.Stack;

        // 使用 SetIngredients 设置材料 - 支持多个材料
        List<int> Ingredients = new List<int>();
        foreach (var ingredient in data.Material)
        {
            // 跳过无效材料
            if (ingredient.type <= 0 || ingredient.stack <= 0) continue;

            Ingredients.Add(ingredient.type);
            Ingredients.Add(ingredient.stack);
        }

        // 确保至少有一个有效材料
        if (Ingredients.Count > 0)
        {
            recipe.SetIngredients(Ingredients.ToArray());
        }

        // 使用 SetCraftingStation 设置合成站 - 支持多个合成站
        List<int> RequiredTile = new List<int>();
        foreach (int tileId in data.Tile)
        {
            // 跳过无效的 TileID (0 或负数)
            if (tileId <= 0) continue;

            // 跳过重复的合成站
            if (!RequiredTile.Contains(tileId))
            {
                RequiredTile.Add(tileId);
            }
        }

        // 确保至少有一个合成站
        if (RequiredTile.Count > 0)
        {
            recipe.SetCraftingStation(RequiredTile.ToArray());
        }

        // 配方合成环境
        foreach (string unlock in data.unlock)
        {
            switch (unlock)
            {
                case "水":
                    recipe.needWater = true;
                    break;
                case "岩浆":
                    recipe.needLava = true;
                    break;
                case "蜂蜜":
                    recipe.needHoney = true;
                    break;
                case "雪原":
                    recipe.needSnowBiome = true;
                    break;
                case "墓地":
                    recipe.needGraveyardBiome = true;
                    break;
                case "天顶":
                    recipe.needEverythingSeed = true;
                    break;
                case "腐化":
                    recipe.corruption = true;
                    break;
                case "猩红":
                    recipe.crimson = true;
                    break;
                case "炼药":
                    recipe.alchemy = true;
                    break;
                default:
                    break;
            }
        }

        return recipe;
    }
    #endregion

    #region 将原版配方转换为自定义配方
    public static void ConvertRecipe(Recipe recipe)
    {
        CustomRecipeData myRecipe = new()
        {
            ItemID = recipe.createItem.type,
            Stack = recipe.createItem.stack,
        };

        // 添加普通材料
        foreach (var item in recipe.requiredItem)
        {
            // 跳过无效材料
            if (item.IsAir || item.type <= 0 || item.stack <= 0)
            {
                continue;
            }

            // 添加材料到配方
            myRecipe.Material.Add(new MaterialData(item.type, item.stack));
        }

        // 添加所有合成站
        foreach (int tileId in recipe.requiredTile)
        {
            // 跳过无效的图格ID
            if (tileId <= 0) continue;

            // 确保不添加重复的合成站
            myRecipe.Tile.Add(tileId);
        }

        // 添加环境
        List<string> addedUnlocked = GetEnvironmentName(recipe);
        if (addedUnlocked.Count > 0)
        {
            myRecipe.unlock.AddRange(addedUnlocked);
        }

        // 添加到自定义配方列表
        Config.CustomRecipes.Add(myRecipe);
        Config.Write();

        // 设置为编辑状态
        UITool.EditingRecipe = myRecipe;
        UITool.IsNewRecipe = false;
    }
    #endregion

    #region 获取原版配方所需的环境名称
    private static List<string> GetEnvironmentName(Recipe recipe)
    {
        List<string> addedUnlocked = [];
        if (recipe.needWater)
        {
            addedUnlocked.Add("水");
        }
        if (recipe.needHoney)
        {
            addedUnlocked.Add("蜂蜜");
        }
        if (recipe.needLava)
        {
            addedUnlocked.Add("岩浆");
        }
        if (recipe.needSnowBiome)
        {
            addedUnlocked.Add("雪原");
        }
        if (recipe.needGraveyardBiome)
        {
            addedUnlocked.Add("墓地");
        }
        if (recipe.needEverythingSeed)
        {
            addedUnlocked.Add("天顶");
        }
        if (recipe.corruption)
        {
            addedUnlocked.Add("腐化");
        }
        if (recipe.crimson)
        {
            addedUnlocked.Add("猩红");
        }
        if (recipe.alchemy)
        {
            addedUnlocked.Add("炼药");
        }

        return addedUnlocked;
    }
    #endregion

    #region 获取合成站名称的辅助方法
    public static string GetTileName(int tileId)
    {
        // 优先使用基础列表
        if (BaseStations.TryGetValue(tileId, out var baseName))
            return baseName;

        // 使用自定义名称
        if (CustomStations.TryGetValue(tileId, out var customName))
            return customName;

        // 使用游戏内置的本地化名称
        string localizedName = TileID.Search.GetName(tileId);
        if (!string.IsNullOrEmpty(localizedName))
            return localizedName;

        return $"未知图格({tileId})";
    }
    #endregion

    #region 基础合成站列表
    public static readonly Dictionary<int, string> BaseStations = new Dictionary<int, string>
    {
        { TileID.WorkBenches, "工作台" },
        { TileID.TinkerersWorkbench, "工匠作坊" },
        { TileID.Furnaces, "熔炉" },
        { TileID.Anvils, "铁砧/铅砧" },
        { TileID.MythrilAnvil, "秘银砧/山铜砧" },
        { TileID.Hellforge, "地狱熔炉" },
        { TileID.Bottles, "玻璃瓶" },
        { TileID.AlchemyTable, "炼药桌" },
        { TileID.ImbuingStation, "灌注站" },
        { TileID.CrystalBall, "水晶球" },
        { TileID.Autohammer, "自动锤炼机" },
        { TileID.Loom, "织布机" },
        { TileID.Sawmill, "锯木机" },
        { TileID.Kegs, "酒桶" },
        { TileID.CookingPots, "烹饪锅" },
        { TileID.Blendomatic, "搅拌机" },
        { TileID.MeatGrinder, "绞肉机" },
        { TileID.Extractinator, "提炼机" },
        { TileID.LesionStation, "腐变室" },
        { TileID.SteampunkBoiler, "蒸汽朋克锅炉" },
        { TileID.GlassKiln, "玻璃窑" },
        { TileID.Solidifier, "固化机" },
        { TileID.BoneWelder, "骨头焊机" },
        { TileID.FleshCloningVat, "血肉克隆台" },
        { TileID.SkyMill, "天磨" },
        { TileID.LivingLoom, "生命木织机" },
        { TileID.IceMachine, "冰雪机" },
        { TileID.HeavyWorkBench, "重型工作台" },
        { TileID.LunarCraftingStation, "远古操纵机" },
        { TileID.DemonAltar, "恶魔祭坛" },
        { TileID.LihzahrdFurnace, "丛林蜥蜴熔炉" },
        { TileID.LihzahrdAltar, "丛林蜥蜴祭坛" },

    };
    #endregion

    #region 输出选择合成站列表名称
    public static Dictionary<int, string> StationList()
    {
        // 合并自定义合成站
        var mergedStations = new Dictionary<int, string>(BaseStations);
        foreach (var custom in CustomStations)
        {
            if (!mergedStations.ContainsKey(custom.Key))
            {
                mergedStations.Add(custom.Key, custom.Value);
            }
        }

        return mergedStations;
    }
    #endregion

    #region 获取环境条件的详细说明
    public static string GetEnvironmentTooltip(string condition)
    {
        switch (condition)
        {
            case "困难模式": return "击败肉山后解锁";
            case "血月": return "血月事件期间";
            case "白天": return "游戏时间4:30 AM - 7:30 PM";
            case "晚上": return "游戏时间7:30 PM - 4:30 AM";
            case "满月": return "月亮完全可见时";
            case "炼药": return "玩家站在玻璃瓶或炼药桌旁";
            case "水": return "玩家站在水中或任意水槽旁";
            case "蜂蜜": return "玩家站在蜂蜜中";
            case "岩浆": return "玩家站在熔岩中";
            case "森林": return "玩家处于森林生物群系";
            case "神圣": return "玩家处于神圣之地";
            case "腐化": return "玩家处于腐化之地";
            case "猩红": return "玩家处于猩红之地";
            case "地牢": return "玩家处于地牢中";
            case "神庙": return "玩家处于丛林神庙中";
            case "天空": return "玩家处于高空区域";
            case "沙尘暴": return "沙漠中发生沙尘暴时";
            case "一王后": return "击败毁灭者、双子眼、机械骷髅王任意一个时";
            case "三王后": return "击败毁灭者、双子眼、机械骷髅王所有时";
            case "一柱后": return "击败任意四柱之一时";
            case "四柱后": return "击败所有四柱时";
            case "2020": return "世界种子为醉酒世界(05162020)时";
            case "2021": return "世界种子为十周年(05162021)时";
            case "ftw": return "世界种子为真实世界(ForTheWorthy)时";
            case "ntb": return "世界种子为蜜蜂世界(Notthebees)时";
            case "dst": return "世界种子为永恒领域(Constant)时";
            case "颠倒": return "世界种子为颠倒世界(Don'tDigUp)时";
            case "陷阱": return "世界种子为陷阱世界(NoTraps)时";
            case "天顶": return "世界种子为天顶世界(Getfixedboi)时";
            case "邪恶boss": return "击败世界吞噬怪与克苏鲁之脑任意一个时";
            default: return $"满足 {condition} 条件时解锁配方";
        }
    }
    #endregion

    #region 合成环境名称
    public static readonly List<string> AllEnvironments = new List<string>
    {
        "炼药","水", "蜂蜜", "岩浆", "地下", "地下沙漠", "洞穴", "地狱", "海洋", "天空",
        "森林", "丛林", "沙漠", "雪原", "神圣", "蘑菇", "腐化", "陨石坑", "微光",
        "猩红", "地牢", "墓地", "蜂巢", "神庙", "沙尘暴", "宝石洞", "花岗岩","大理石",
        "和平蜡烛", "水蜡烛","影烛","星云环境","日耀环境","星尘环境","星旋环境",
        "下弦月", "残月", "新月", "娥眉月", "上弦月", "盈凸月","满月", "亏凸月",
        "哥布林入侵", "史莱姆王", "克苏鲁之眼", "鹿角怪", "克苏鲁之脑","世界吞噬怪","邪恶boss",
        "蜂王", "骷髅王", "困难模式", "海盗入侵", "史莱姆皇后", "一王后",
        "毁灭者", "双子魔眼", "机械骷髅王", "三王后", "世纪之花", "石巨人", "光之女皇", "猪龙鱼公爵", "拜月教邪教徒", "月亮领主",
        "哀木", "南瓜王", "常绿尖叫怪", "冰雪女王", "圣诞坦克", "火星飞碟", "小丑",
        "一柱后", "日耀柱", "星旋柱", "星云柱", "星尘柱", "四柱后",
        "霜月", "血月", "雨天", "白天", "晚上", "大风天", "万圣节", "圣诞节",
        "派对", "2020", "2021", "ftw", "ntb", "dst", "颠倒", "陷阱", "天顶",
    };
    #endregion
}

#region 事件参数类
public class RecipeCheckEventArgs : EventArgs
{
    public int Index { get; } // 配方索引
    public Recipe Recipe { get; set; }
    public Player Player { get; }
    public bool Conditions { get; set; } // 符合所有条件
    public bool MeetTile { get; set; } // 符合合成站条件
    public bool MeetMaterial { get; set; } //符合材料条件
    public bool MeetEnvironment { get; set; } //符合环境条件
    public Dictionary<int, int> HasItem { get; } // 添加收集的物品数据
    public int ActiveRecipe { get; set; }
    public float ActiveRecipeY { get; set; }
    public RecipeCheckEventArgs(Recipe recipe, int recipeIndex, Player player, bool conditions, bool meetsTile, bool mterial, bool environment, Dictionary<int, int> hasItem, int activeRecipe, float activeRecipeY)
    {
        this.Recipe = recipe;
        this.Index = recipeIndex;
        this.Player = player;
        this.Conditions = conditions;
        this.MeetTile = meetsTile;
        this.MeetMaterial = mterial;
        this.MeetEnvironment = environment;
        this.HasItem = hasItem;
        this.ActiveRecipe = activeRecipe;
        this.ActiveRecipeY = activeRecipeY;
    }
}
#endregion

#region 配方数据结构
public class CustomRecipeData
{
    [JsonProperty("UID", Order = 0)]
    public string UniqueID { get; set; } = Guid.NewGuid().ToString();
    [JsonProperty("配方索引", Order = 1)]
    public int Index { get; set; } = -1; //配方索引
    [JsonProperty("合成物品", Order = 2)]
    public int ItemID { get; set; } //合成物品
    [JsonProperty("合成物品数量", Order = 3)]
    public int Stack { get; set; } = 1; //合成物品数量
    [JsonProperty("组成材料", Order = 4)]
    public List<MaterialData> Material { get; set; } = new List<MaterialData>(); //材料
    [JsonProperty("合成站", Order = 5)]
    public List<int> Tile { get; set; } = new List<int>(); //合成站
    [JsonProperty("合成环境", Order = 6)]
    public List<string> unlock = new List<string>();

    #region 判断解锁配方
    public static bool IsRecipeUnlocked(CustomRecipeData recipe)
    {
        Player plr = Main.player[Main.myPlayer];
        foreach (string condition in recipe.unlock)
        {
            // 进度和环境条件检查
            if (!CheckCondition(condition, plr))
                return false;
        }

        return true;
    }
    #endregion

    #region 检查条件
    public static bool CheckCondition(string condition, Player plr)
    {
        switch (condition)
        {
            case string s when s.StartsWith("自定义:"):
                // 解析自定义格式："物品名(ID)"
                string cPart = s.Substring(4); // 去掉"自定义:"前缀

                // 检查格式是否正确（包含括号）
                int openParen = cPart.LastIndexOf('(');
                int stopParen = cPart.LastIndexOf(')');

                if (openParen > 0 && stopParen > openParen)
                {
                    // 提取括号内的ID部分
                    string idString = cPart.Substring(openParen + 1, stopParen - openParen - 1);

                    if (int.TryParse(idString, out var tileID))
                    {
                        return plr.adjTile[tileID];
                    }
                }
                return false;
            case "炼药":
                return plr.adjTile[TileID.AlchemyTable] || plr.adjTile[TileID.Bottles]; // 炼药桌与玻璃瓶的图格ID
            case "水":
                return plr.adjWater || plr.adjTile[TileID.Sinks]; // 水槽的图格ID
            case "蜂蜜":
                return plr.adjHoney;
            case "岩浆":
                return plr.adjLava;
            case "史莱姆王":
            case "史王":
                return NPC.downedSlimeKing;
            case "克眼":
            case "克苏鲁之眼":
                return NPC.downedBoss1;
            case "巨鹿":
            case "鹿角怪":
                return NPC.downedDeerclops;
            case "邪恶boss":
                return NPC.downedBoss2;
            case "世吞":
            case "世界吞噬者":
                return NPC.downedBoss2 && Utils.BestiaryEntry2(NPCID.EaterofWorldsHead);
            case "克苏鲁之脑":
            case "世界吞噬怪":
                return NPC.downedBoss2 && Utils.BestiaryEntry2(NPCID.BrainofCthulhu);
            case "蜂王":
                return NPC.downedQueenBee;
            case "骷髅王":
                return NPC.downedBoss3;
            case "困难模式":
            case "肉后":
            case "血肉墙":
                return Main.hardMode;
            case "毁灭者":
                return NPC.downedMechBoss1;
            case "双子魔眼":
                return NPC.downedMechBoss2;
            case "机械骷髅王":
                return NPC.downedMechBoss3;
            case "世纪之花":
            case "花后":
            case "世花":
                return NPC.downedPlantBoss;
            case "石后":
            case "石巨人":
                return NPC.downedGolemBoss;
            case "史后":
            case "史莱姆皇后":
                return NPC.downedQueenSlime;
            case "光之女皇":
            case "光女":
                return NPC.downedEmpressOfLight;
            case "猪鲨":
            case "猪龙鱼公爵":
                return NPC.downedFishron;
            case "教徒":
            case "拜月教邪教徒":
                return NPC.downedAncientCultist;
            case "月亮领主":
                return NPC.downedMoonlord;
            case "哀木":
                return NPC.downedHalloweenTree;
            case "南瓜王":
                return NPC.downedHalloweenKing;
            case "常绿尖叫怪":
                return NPC.downedChristmasTree;
            case "冰雪女王":
                return NPC.downedChristmasIceQueen;
            case "圣诞坦克":
                return NPC.downedChristmasSantank;
            case "火星飞碟":
                return NPC.downedMartians;
            case "小丑":
                return NPC.downedClown;
            case "日耀柱":
                return NPC.downedTowerSolar;
            case "星旋柱":
                return NPC.downedTowerVortex;
            case "星云柱":
                return NPC.downedTowerNebula;
            case "星尘柱":
                return NPC.downedTowerStardust;
            case "一王后":
                return NPC.downedMechBossAny;
            case "三王后":
                return NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
            case "一柱后":
                return NPC.downedTowerNebula || NPC.downedTowerSolar || NPC.downedTowerStardust || NPC.downedTowerVortex;
            case "四柱后":
                return NPC.downedTowerNebula && NPC.downedTowerSolar && NPC.downedTowerStardust && NPC.downedTowerVortex;
            case "哥布林入侵":
                return NPC.downedGoblins;
            case "海盗入侵":
                return NPC.downedPirates;
            case "霜月":
                return NPC.downedFrost;
            case "血月":
                return Main.bloodMoon;
            case "雨天":
                return Main.raining;
            case "白天":
                return Main.dayTime;
            case "晚上":
                return !Main.dayTime;
            case "大风天":
                return Main.IsItAHappyWindyDay;
            case "万圣节":
                return Main.halloween;
            case "圣诞节":
                return Main.xMas;
            case "派对":
                return BirthdayParty.PartyIsUp;
            case "2020":
                return Main.drunkWorld;
            case "2021":
                return Main.tenthAnniversaryWorld;
            case "ftw":
                return Main.getGoodWorld;
            case "ntb":
                return Main.notTheBeesWorld;
            case "dst":
                return Main.dontStarveWorld;
            case "颠倒":
                return Main.remixWorld;
            case "陷阱":
                return Main.noTrapsWorld;
            case "天顶":
                return Main.zenithWorld;
            case "森林":
                return plr.ShoppingZone_Forest;
            case "丛林":
                return plr.ZoneJungle;
            case "沙漠":
                return plr.ZoneDesert;
            case "雪原":
                return plr.ZoneSnow;
            case "宝石洞":
                return plr.ZoneGemCave;
            case "花岗岩":
                return plr.ZoneGranite;
            case "大理石":
                return plr.ZoneMarble;
            case "陨石坑":
                return plr.ZoneMeteor;
            case "和平蜡烛":
                return plr.ZonePeaceCandle;
            case "水蜡烛":
                return plr.ZoneWaterCandle;
            case "影烛":
                return plr.ZoneShadowCandle;
            case "微光":
                return plr.ZoneShimmer;
            case "星云环境":
                return plr.ZoneTowerNebula;
            case "日耀环境":
                return plr.ZoneTowerSolar;
            case "星尘环境":
                return plr.ZoneTowerStardust;
            case "星旋环境":
                return plr.ZoneTowerVortex;
            case "地下沙漠":
                return plr.ZoneUndergroundDesert;
            case "地下":
                return plr.ZoneDirtLayerHeight;
            case "洞穴":
                return plr.ZoneRockLayerHeight;
            case "地狱":
                return plr.ZoneUnderworldHeight;
            case "海洋":
                return plr.ZoneBeach;
            case "神圣":
                return plr.ZoneHallow;
            case "蘑菇":
                return plr.ZoneGlowshroom;
            case "腐化":
            case "腐化之地":
                return plr.ZoneCorrupt;
            case "猩红":
            case "猩红之地":
                return plr.ZoneCrimson;
            case "地牢":
                return plr.ZoneDungeon;
            case "墓地":
                return plr.ZoneGraveyard;
            case "蜂巢":
                return plr.ZoneHive;
            case "神庙":
                return plr.ZoneLihzhardTemple;
            case "沙尘暴":
                return plr.ZoneSandstorm;
            case "天空":
                return plr.ZoneSkyHeight;
            case "满月":
                return Main.moonPhase == 0;
            case "亏凸月":
                return Main.moonPhase == 1;
            case "下弦月":
                return Main.moonPhase == 2;
            case "残月":
                return Main.moonPhase == 3;
            case "新月":
                return Main.moonPhase == 4;
            case "娥眉月":
                return Main.moonPhase == 5;
            case "上弦月":
                return Main.moonPhase == 6;
            case "盈凸月":
                return Main.moonPhase == 7;
            default:
                return false;
        }
    }
    #endregion
}

public class MaterialData
{
    [JsonProperty("物品ID", Order = 0)]
    public int type { get; set; }
    [JsonProperty("物品数量", Order = 1)]
    public int stack { get; set; }

    public MaterialData() { }
    public MaterialData(int itemId, int stack = 1)
    {
        type = itemId;
        this.stack = stack;
    }
}
#endregion