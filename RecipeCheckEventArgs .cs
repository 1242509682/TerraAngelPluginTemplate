using MonoMod.RuntimeDetour;
using System.Linq;
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
    public static event Action? AddCustomRecipes; // 添加自定义配方(配方查找前调用)

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
        AddCustomRecipes = null;
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
        AddCustomRecipes?.Invoke();

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
            Recipe recipe = Main.recipe[i];

            // 跳过空配方
            if (recipe.createItem.type == 0) continue;

            // 检查工作站条件
            bool MeetsTileConditions = PlayerMeetsTileRequirements!(plr, recipe);

            // 检查环境条件
            bool MeetsEnvironmentConditions = PlayerMeetsEnvironmentConditions!(plr, recipe);

            // 检查材料是否足够
            bool MeetsMaterialConditions = Recipe.CollectedEnoughItemsToCraftRecipeNew(recipe);

            // 初始条件检查结果
            bool MeetsConditions = MeetsTileConditions && MeetsEnvironmentConditions && MeetsMaterialConditions;

            // 创建事件参数
            var args = new RecipeCheckEventArgs(recipe, i, plr, MeetsConditions, MeetsTileConditions, MeetsMaterialConditions, MeetsEnvironmentConditions, HasItems);

            // 触发自定义配方检查事件
            OnRecipeCheck?.Invoke(args);

            // 使用事件处理后的结果
            if (args.MeetsConditions)
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
    public static bool HasResultItemForRecipe(Recipe recipe, Dictionary<int, int> HasItems)
    {
        foreach (Item item in recipe.requiredItem)
        {
            if (item.type <= 0) continue;

            // 检查物品组
            if (RecipeGroup.recipeGroups != null)
            {
                foreach (var Group in RecipeGroup.recipeGroups.Values)
                {
                    if (Group.ValidItems.Contains(item.type))
                    {
                        int GroupId = Group.RegisteredId;
                        if (HasItems.TryGetValue(GroupId, out int GroupCount) && GroupCount >= item.stack)
                        {
                            return true;
                        }
                    }
                }
            }

            // 检查普通物品
            if (!HasItems.TryGetValue(item.type, out int itemCount) || itemCount < item.stack)
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
            if (tileId <= 0) continue; // 跳过无效ID（包括0）

            recipe.requiredTile[tileIndex] = tileId;
            tileIndex++;
        }

        // 炼金配方标记
        recipe.alchemy = data.IsAlchemyRecipe ||  data.RequiredTile.Contains(TileID.Bottles) || data.RequiredTile.Contains(TileID.AlchemyTable);

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
                    myRecipe.RequiredTile.Add(tileId);
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
    public int Index { get; } // 配方索引
    public Recipe Recipe { get; }
    public Player Player { get; }
    public bool MeetsConditions { get; set; } // 符合所有条件
    public bool MeetsTileConditions { get; set; } // 符合合成站条件
    public bool MeetsMaterialConditions { get; set; } //符合材料条件
    public bool MeetsEnvironmentConditions { get; set; } //符合环境条件
    public Dictionary<int, int> HasItem { get; } // 添加收集的物品数据
    public RecipeCheckEventArgs(Recipe recipe, int recipeIndex, Player player, bool meetsConditions, bool meetsTileConditions, bool meetsMaterialConditions, bool meetsEnvironmentConditions, Dictionary<int, int> hasItem)
    {
        this.Recipe = recipe;
        this.Index = recipeIndex;
        this.Player = player;
        this.MeetsConditions = meetsConditions;
        this.MeetsTileConditions = meetsTileConditions;
        this.MeetsMaterialConditions = meetsMaterialConditions;
        this.MeetsEnvironmentConditions = meetsEnvironmentConditions;
        this.HasItem = hasItem;
    }
}
#endregion

#region 配方数据结构
public class CustomRecipeData
{
    public string UniqueID { get; set; } = Guid.NewGuid().ToString();
    public int Index { get; set; } = -1; //配方索引
    public int ResultItem { get; set; } //合成物品
    public int ResultStack { get; set; } = 1; //合成物品数量
    public List<IngredientData> Ingredients { get; set; } = new List<IngredientData>(); //材料
    public List<int> RequiredTile { get; set; } = new List<int>(); //合成站
    public bool IsAlchemyRecipe { get; set; } //炼药标识
}

public class IngredientData
{
    public int ItemId { get; set; }
    public int Stack { get; set; } = 1;
}
#endregion