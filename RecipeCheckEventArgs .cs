using MonoMod.RuntimeDetour;
using System.Reflection;
using Terraria;
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
    private static Hook? CollectGuideRecipesHook;
    private static Hook? SetupRecipesHook;

    public static event Action<RecipeCheckEventArgs>? OnRecipeCheck; // 配方检查事件
    public static event Action? OnPostFindRecipes; //  配方查找后事件

    // 存储原始配方数据
    public static List<RecipeData> OriginalRecipes { get; } = new List<RecipeData>();

    #region 注册与卸载钩子方法
    public static void Register()
    {
        // 通过反射获取私有静态方法
        PlayerMeetsEnvironmentConditions = CreateDelegate<Func<Player, Recipe, bool>>(
            typeof(Recipe), "PlayerMeetsEnvironmentConditions",
            BindingFlags.NonPublic | BindingFlags.Static);

        PlayerMeetsTileRequirements = CreateDelegate<Func<Player, Recipe, bool>>(
            typeof(Recipe), "PlayerMeetsTileRequirements",
            BindingFlags.NonPublic | BindingFlags.Static);

        CollectItemsToCraftWithFrom = CreateDelegate<Action<Player>>(
            typeof(Recipe), "CollectItemsToCraftWithFrom",
            BindingFlags.NonPublic | BindingFlags.Static);

        // 挂钩 FindRecipes 方法（
        MethodInfo findRecipes = typeof(Recipe).GetMethod("FindRecipes",
            BindingFlags.Public | BindingFlags.Static,
            [typeof(bool)])!;

        if (findRecipes != null)
        {
            FindRecipesHook = new Hook(findRecipes, OnFindRecipes);
        }

        // 挂钩 CollectGuideRecipes 方法
        MethodInfo collectGuideRecipes = typeof(Recipe).GetMethod("CollectGuideRecipes",BindingFlags.NonPublic | BindingFlags.Static,Type.EmptyTypes)!;

        if (collectGuideRecipes != null)
        {
            CollectGuideRecipesHook = new Hook(collectGuideRecipes, OnCollectGuideRecipes);
        }

        // 挂钩 SetupRecipes 方法 (用于原版配方
        MethodInfo setupRecipes = typeof(Recipe).GetMethod("SetupRecipes",
            BindingFlags.Public | BindingFlags.Static)!;

        if (setupRecipes != null)
        {
            SetupRecipesHook = new Hook(setupRecipes, OnSetupRecipes);
        }
    }
    #endregion

    #region  卸载事件钩子方法
    public static void Dispose()
    {
        FindRecipesHook?.Dispose();
        CollectGuideRecipesHook?.Dispose();
        SetupRecipesHook?.Dispose();
        OnRecipeCheck = null;
        OnPostFindRecipes = null;
        OriginalRecipes.Clear(); // 清理原始配方备份
    }
    #endregion

    #region 原版设置配方方法钩子
    private delegate void orig_SetupRecipes();
    private static void OnSetupRecipes(orig_SetupRecipes orig)
    {
        // 备份原始配方
        BackupOrigRecipes();

        // 执行原始方法
        orig();
    }
    #endregion

    #region 备份原始配方
    private static void BackupOrigRecipes()
    {
        OriginalRecipes.Clear();

        // 遍历所有可能的配方槽位
        for (int i = 0; i < Recipe.maxRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];

            // 遇到空配方槽位时停止
            if (recipe.createItem.type == 0) break;

            // 创建配方数据副本
            var recipeData = new RecipeData
            {
                RecipeIndex = i,
                CreateItem = recipe.createItem.Clone(),
                RequiredItems = recipe.requiredItem.Select(item => item.Clone()).ToArray(),
                RequiredTiles = recipe.requiredTile.ToArray(),
                AcceptedGroups = recipe.acceptedGroups.ToArray(),
                Alchemy = recipe.alchemy,
                NeedHoney = recipe.needHoney,
                NeedLava = recipe.needLava,
                NeedWater = recipe.needWater,
                AnyWood = recipe.anyWood,
                AnyIronBar = recipe.anyIronBar,
                AnyPressurePlate = recipe.anyPressurePlate,
                AnySand = recipe.anySand,
                AnyFragment = recipe.anyFragment,
                NeedSnowBiome = recipe.needSnowBiome,
                NeedGraveyardBiome = recipe.needGraveyardBiome,
                NeedEverythingSeed = recipe.needEverythingSeed,
                NotDecraftable = recipe.notDecraftable,
                Crimson = recipe.crimson,
                Corruption = recipe.corruption,
                CustomShimmerResults = recipe.customShimmerResults,
            };

            OriginalRecipes.Add(recipeData);
        }
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
            // 处理向导物品的配方
            var originalMethod = typeof(Recipe).GetMethod("CollectGuideRecipes", BindingFlags.NonPublic | BindingFlags.Static);
            originalMethod?.Invoke(null, null);

            TryRefocusingRecipe(oldRecipe); // 尝试重新聚焦配方
            VisuallyRepositionRecipes(focusY); // 视觉定位配方
            //OnPostFindRecipes?.Invoke();
            return;
        }

        Player plr = Main.LocalPlayer;

        // 收集玩家可用于合成的物品
        CollectItemsToCraftWithFrom?.Invoke(plr);

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
            var args = new RecipeCheckEventArgs(recipe, plr, meetsConditions, meetsStationConditions, i);

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
        //OnPostFindRecipes?.Invoke();
    }
    #endregion

    #region 自定义的 CollectGuideRecipes 挂钩（配方查找后事件）
    private delegate void orig_CollectGuideRecipes();
    private static void OnCollectGuideRecipes(orig_CollectGuideRecipes orig)
    {
        orig(); // 先执行原始方法

        // 触发后处理事件
        OnPostFindRecipes?.Invoke();
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

    // 修改现有配方
    public static void ModifyRecipe(int Index, Action<Recipe> modifier)
    {
        if (Index >= 0 && Index < Recipe.maxRecipes)
        {
            modifier(Main.recipe[Index]);
        }
    }

    // 添加新配方
    public static int AddNewRecipe(Action<Recipe> initializer)
    {
        int slot = FindEmptyRecipeSlot();
        if (slot == -1) return -1;

        Recipe recipe = Main.recipe[slot];
        initializer(recipe);
        return slot;
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

    #region 检查玩家是否有足够材料制作配方
    public static bool HasResultItemForRecipe(Player player, Recipe recipe)
    {
        // 检查每个材料是否足够
        for (int i = 0; i < recipe.requiredItem.Count(); i++)
        {
            Item item = recipe.requiredItem[i];
            if (item.type <= 0) continue; // 跳过空槽位

            int totalCount = 0;

            // 检查背包
            for (int j = 0; j < player.inventory.Length; j++)
            {
                if (player.inventory[j].type == item.type)
                {
                    totalCount += player.inventory[j].stack;
                }
            }

            for (int j = 0; j < player.armor.Length; j++)
            {
                if (player.armor[j].type == item.type)
                {
                    totalCount += player.armor[j].stack;
                }
            }

            for (int j = 0; j < player.bank.item.Length; j++)
            {
                if (player.bank.item[j].type == item.type)
                {
                    totalCount += player.bank.item[j].stack;
                }
            }

            for (int j = 0; j < player.bank2.item.Length; j++)
            {
                if (player.bank2.item[j].type == item.type)
                {
                    totalCount += player.bank2.item[j].stack;
                }
            }

            for (int j = 0; j < player.bank3.item.Length; j++)
            {
                if (player.bank3.item[j].type == item.type)
                {
                    totalCount += player.bank3.item[j].stack;
                }
            }

            for (int j = 0; j < player.bank4.item.Length; j++)
            {
                if (player.bank4.item[j].type == item.type)
                {
                    totalCount += player.bank4.item[j].stack;
                }
            }

            for (int j = 0; j < player.miscEquips.Length; j++)
            {
                if (player.miscEquips[j].type == item.type)
                {
                    totalCount += player.miscEquips[j].stack;
                }
            }

            for (int j = 0; j < player.miscDyes.Length; j++)
            {
                if (player.miscDyes[j].type == item.type)
                {
                    totalCount += player.miscDyes[j].stack;
                }
            }

            // 如果材料不足
            if (totalCount < item.stack)
            {
                return false;
            }
        }

        return true;
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

    public RecipeCheckEventArgs(Recipe recipe, Player player, bool meetsConditions, bool meetsStationConditions, int recipeIndex)
    {
        Recipe = recipe;
        Player = player;
        MeetsConditions = meetsConditions;
        MeetsStationConditions = meetsStationConditions;
        RecipeIndex = recipeIndex;
    }
}
#endregion

#region 配方数据结构
// 用于存储原版配方数据
public class RecipeData
{
    public int RecipeIndex { get; set; }
    public Item CreateItem { get; set; } = new Item();
    public Item[] RequiredItems { get; set; } = Array.Empty<Item>();
    public int[] RequiredTiles { get; set; } = Array.Empty<int>();
    public int[] AcceptedGroups { get; set; } = Array.Empty<int>();
    public bool Alchemy { get; set; }
    public bool NeedHoney { get; set; }
    public bool NeedLava { get; set; }
    public bool NeedWater { get; set; }
    public bool AnyWood { get; set; }
    public bool AnyIronBar { get; set; }
    public bool AnyPressurePlate { get; set; }
    public bool AnySand { get; set; }
    public bool AnyFragment { get; set; }
    public bool NeedSnowBiome { get; set; }
    public bool NeedGraveyardBiome { get; set; }
    public bool NeedEverythingSeed { get; set; }
    public bool NotDecraftable { get; set; }
    public bool Crimson { get; set; }
    public bool Corruption { get; set; }
    public List<Item> CustomShimmerResults { get; set; } = new List<Item>();
}

// 用于存储自定义配方数据
public class CustomRecipeData
{
    public string UniqueID { get; set; } = Guid.NewGuid().ToString();
    public int Index { get; set; } = -1;
    public int ResultItem { get; set; }
    public int ResultStack { get; set; } = 1;
    public List<IngredientData> Ingredients { get; set; } = new List<IngredientData>();
    public List<int> RequiredTile { get; set; } =  new List<int>();
    public bool IsAlchemyRecipe { get; set; }
}

public class IngredientData
{
    public int ItemId { get; set; }
    public int Stack { get; set; } = 1;
} 
#endregion