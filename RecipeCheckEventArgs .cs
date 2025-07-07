using MonoMod.RuntimeDetour;
using System.Reflection;
using Terraria;
using Terraria.ID;
using static MyPlugin.MyPlugin;

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
    public static event Action? OnPostFindRecipes; //  配方查找后事件

    #region 注册与卸载钩子方法
    public static void Register()
    {
        // 反射获取私有方法委托
        // 检查玩家是否满足环境条件（如血月、夜晚等）
        PlayerMeetsEnvironmentConditions = CreateDelegate<Func<Player, Recipe, bool>>(
            typeof(Recipe), "PlayerMeetsEnvironmentConditions",
            BindingFlags.NonPublic | BindingFlags.Static);

        // 检查玩家是否满足工作站条件
        PlayerMeetsTileRequirements = CreateDelegate<Func<Player, Recipe, bool>>(
            typeof(Recipe), "PlayerMeetsTileRequirements",
            BindingFlags.NonPublic | BindingFlags.Static);

        // 收集玩家可用于合成的物品
        CollectItemsToCraftWithFrom = CreateDelegate<Action<Player>>(
            typeof(Recipe), "CollectItemsToCraftWithFrom",
            BindingFlags.NonPublic | BindingFlags.Static);

        // 查找配方（用于决定配方是否显示）
        MethodInfo findRecipes = typeof(Recipe).GetMethod("FindRecipes",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(bool)])!;

        if (findRecipes != null)
        {
            FindRecipesHook = new Hook(findRecipes, OnFindRecipes);
        }
    }
    #endregion

    #region  卸载事件钩子方法
    public static void Dispose()
    {
        FindRecipesHook?.Dispose();
        OnRecipeCheck = null;
        OnPostFindRecipes = null;
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
        if (canDelayCheck)
        {
            orig(canDelayCheck);
            return;
        }

        int oldRecipe = Main.availableRecipe[Main.focusRecipe];
        float focusY = Main.availableRecipeY[Main.focusRecipe];
        Recipe.ClearAvailableRecipes(); // 清理可用配方

        // 先添加自定义配方
        OnPostFindRecipes?.Invoke();

        if (!Main.guideItem.IsAir && Main.guideItem.Name != "")
        {
            CollectGuideRecipes(); // 处理向导物品的配方
            TryRefocusingRecipe(oldRecipe); // 尝试重新聚焦配方
            VisuallyRepositionRecipes(focusY); // 视觉定位配方
            return;
        }

        Player plr = Main.LocalPlayer;

        // 收集玩家可用于合成的物品
        CollectItemsToCraftWithFrom?.Invoke(plr);

        // 获取原版收集的物品数据
        var collectedItems = GetCollectedItems();

        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];
            if (recipe.createItem.type == 0) continue; // 跳过空配方

            // 检查原始条件
            bool meetsConditions = true;
            bool meetsStationConditions = true;

            // 使用反射得到的委托检查条件
            if (PlayerMeetsTileRequirements != null)
            {
                meetsConditions &= PlayerMeetsTileRequirements(plr, recipe);
                meetsStationConditions = meetsConditions; // 单独记录工作站条件
            }

            // 检查环境条件（如血月、夜晚等）
            if (PlayerMeetsEnvironmentConditions != null)
            {
                meetsConditions &= PlayerMeetsEnvironmentConditions(plr, recipe);
            }

            // 检查材料是否足够
            meetsConditions &= Recipe.CollectedEnoughItemsToCraftRecipeNew(recipe);

            // 创建事件参数
            var args = new RecipeCheckEventArgs(recipe, plr, meetsConditions, meetsStationConditions, i, collectedItems);

            // 触发自定义配方检查事件
            OnRecipeCheck?.Invoke(args);

            // 使用事件处理后的结果
            if (args.MeetsConditions)
            {
                AddToAvailableRecipes(i);
            }
        }

        TryRefocusingRecipe(oldRecipe);
        VisuallyRepositionRecipes(focusY);
    }
    #endregion

    #region 从Recipe类里抄来的辅助方法
    // 添加可用配方
    private static void AddToAvailableRecipes(int recipeIndex)
    {
        Main.availableRecipe[Main.numAvailableRecipes] = recipeIndex;
        Main.numAvailableRecipes++;
    }

    // 尝试重新聚焦配方
    private static void TryRefocusingRecipe(int oldRecipe)
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
    private static void VisuallyRepositionRecipes(float focusY)
    {
        if (Main.numAvailableRecipes == 0) return;

        float num = Main.availableRecipeY[Main.focusRecipe] - focusY;
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Main.availableRecipeY[i] -= num;
        }
    }

    // 收集向导配方
    private static void CollectGuideRecipes()
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
    public static void RemoveRecipe(int recipeIndex)
    {
        if (recipeIndex >= 0 && recipeIndex < Recipe.maxRecipes)
        {
            Main.recipe[recipeIndex] = new Recipe();
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
    #endregion

    #region 复用已收集的物品
    public static bool HasResultItemForRecipe(Recipe recipe, Dictionary<int, int> collectedItems)
    {
        foreach (Item item in recipe.requiredItem)
        {
            if (item.type <= 0) continue;

            // 检查物品组（如果适用）
            if (RecipeGroup.recipeGroups != null)
            {
                foreach (var group in RecipeGroup.recipeGroups.Values)
                {
                    if (group.ValidItems.Contains(item.type))
                    {
                        int groupId = group.RegisteredId;
                        if (collectedItems.TryGetValue(groupId, out int groupCount) && groupCount >= item.stack)
                        {
                            return true;
                        }
                    }
                }
            }

            // 检查普通物品
            if (!collectedItems.TryGetValue(item.type, out int availableCount) ||
                availableCount < item.stack)
            {
                return false;
            }
        }
        return true;
    } 
    #endregion

    #region 检查玩家是否有足够材料制作配方
    private static Dictionary<int, int> GetCollectedItems()
    {
        // 通过反射获取 Recipe._ownedItems 字段
        var field = typeof(Recipe).GetField("_ownedItems",
            BindingFlags.NonPublic | BindingFlags.Static);

        return (Dictionary<int, int>)field?.GetValue(null)! ?? new Dictionary<int, int>();
    }
    #endregion

    #region 检查是否存在相同配方
    public static bool ExistsRecipe(CustomRecipeData data)
    {
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];
            if (recipe.createItem.type == 0) continue; // 跳过空槽位

            // 检查结果物品
            if (recipe.createItem.type != data.ResultItem ||
                recipe.createItem.stack != data.ResultStack)
                continue;

            // 检查材料
            if (!AreIngredientsEqual(recipe, data.Ingredients))
                continue;

            // 检查合成站
            if (!AreTilesEqual(recipe, data.RequiredTile))
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

            if (recipe.createItem.type == data.ResultItem &&
                recipe.createItem.stack == data.ResultStack &&
                AreIngredientsEqual(recipe, data.Ingredients) &&
                AreTilesEqual(recipe, data.RequiredTile))
            {
                return i;
            }
        }
        return -1;
    }

    // 检查材料是否相同
    public static bool AreIngredientsEqual(Recipe recipe, List<IngredientData> ingredients)
    {
        // 收集配方中的有效材料
        var recipeIngredients = recipe.requiredItem
            .Where(item => item.type > 0)
            .Select(item => new { item.type, item.stack })
            .OrderBy(i => i.type)
            .ToList();

        // 收集自定义配方中的材料
        var customIngredients = ingredients
            .Where(i => i.ItemId > 0)
            .Select(i => new { type = i.ItemId, stack = i.Stack })
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
        recipe.createItem.SetDefaults(data.ResultItem);
        recipe.createItem.stack = data.ResultStack;

        // 初始化材料数组
        recipe.requiredItem = new Item[Recipe.maxRequirements];
        recipe.requiredTile = new int[Recipe.maxRequirements];
        for (int i = 0; i < Recipe.maxRequirements; i++)
        {
            recipe.requiredItem[i] = new Item();
        }

        // 设置材料
        int itemIndex = 0;
        foreach (var ingredient in data.Ingredients)
        {
            if (itemIndex >= Recipe.maxRequirements) break;

            recipe.requiredItem[itemIndex] = new Item();
            recipe.requiredItem[itemIndex].SetDefaults(ingredient.ItemId);
            recipe.requiredItem[itemIndex].stack = ingredient.Stack;
            itemIndex++;
        }

        // 设置合成站
        int tileIndex = 0;
        foreach (int tileId in data.RequiredTile)
        {
            if (tileIndex >= Recipe.maxRequirements) break;

            recipe.requiredTile[tileIndex] = tileId;
            tileIndex++;
        }

        // 炼金配方标记
        recipe.alchemy = data.RequiredTile.Contains(TileID.Bottles);

        return recipe;
    }
    #endregion

    #region 将原版配方转换为自定义配方
    public static void ConvertRecipe(Recipe recipe)
    {
        CustomRecipeData myRecipe = new()
        {
            ResultItem = recipe.createItem.type,
            ResultStack = recipe.createItem.stack
        };

        // 添加材料
        foreach (var ingredient in recipe.requiredItem)
        {
            if (ingredient.type > 0 && ingredient.stack > 0)
            {
                myRecipe.Ingredients.Add(new IngredientData
                {
                    ItemId = ingredient.type,
                    Stack = ingredient.stack
                });
            }
        }

        // 设置合成站
        if (recipe.requiredTile != null && recipe.requiredTile.Length > 0)
        {
            foreach (int tileId in recipe.requiredTile)
            {
                if (tileId > 0)
                {
                    myRecipe.RequiredTile.Append(tileId);
                    break;
                }
            }
        }

        // 如果是炼金配方
        myRecipe.IsAlchemyRecipe = recipe.alchemy;

        // 添加到自定义配方列表
        Config.CustomRecipes.Add(myRecipe);
        Config.Write();

        // 设置为编辑状态
        UITool.EditingRecipe = myRecipe;
        UITool.IsNewRecipe = false;
    }
    #endregion
}

#region 事件参数类
public class RecipeCheckEventArgs : EventArgs
{
    public Recipe Recipe { get; }
    public Player Player { get; }
    public bool MeetsConditions { get; set; }
    public bool MeetsStationConditions { get; set; }
    public int RecipeIndex { get; }
    public Dictionary<int, int> CollectedItems { get; } // 添加收集的物品数据

    public RecipeCheckEventArgs(Recipe recipe, Player player, bool meetsConditions, bool meetsStationConditions, int recipeIndex, Dictionary<int, int> collectedItems)
    {
        Recipe = recipe;
        Player = player;
        MeetsConditions = meetsConditions;
        MeetsStationConditions = meetsStationConditions;
        RecipeIndex = recipeIndex;
        CollectedItems = collectedItems;
    }
}
#endregion

#region 配方数据结构

// 用于存储自定义配方数据
public class CustomRecipeData
{
    public string UniqueID { get; set; } = Guid.NewGuid().ToString();
    public int Index { get; set; } = -1;
    public int ResultItem { get; set; }
    public int ResultStack { get; set; } = 1;
    public List<IngredientData> Ingredients { get; set; } = new List<IngredientData>();
    public List<int> RequiredTile { get; set; } = new List<int>();
    public bool IsAlchemyRecipe { get; set; }
}

public class IngredientData
{
    public int ItemId { get; set; }
    public int Stack { get; set; } = 1;
}
#endregion