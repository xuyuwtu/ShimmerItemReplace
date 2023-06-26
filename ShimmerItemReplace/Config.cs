using Newtonsoft.Json;
using System.Security.Cryptography.Pkcs;
using Terraria;
using Terraria.ID;

namespace ShimmerItemReplace;

internal class Config
{
    public bool AddToReload = false;
    public string[] CommandNames = new string[] { "sirc" };
    public string CommandPermission = "sirc";
    public TransformInfo[] Replace = new TransformInfo[]
    {
        new(ItemID.RodofDiscord,    ItemID.RodOfHarmony, 22),
        new(ItemID.Clentaminator,   ItemID.Clentaminator2, 22),
        new(ItemID.BottomlessBucket,ItemID.BottomlessShimmerBucket,22),
        new(ItemID.BottomlessShimmerBucket,ItemID.BottomlessBucket,22),
        new(ItemID.JungleKey,       ItemID.PiranhaGun,13),
        new(ItemID.CorruptionKey,   ItemID.ScourgeoftheCorruptor,13),
        new(ItemID.CrimsonKey,      ItemID.VampireKnives,13),
        new(ItemID.HallowedKey,     ItemID.RainbowGun,13),
        new(ItemID.FrozenKey,       ItemID.StaffoftheFrostHydra,13),
        new(ItemID.DungeonDesertKey,ItemID.StormTigerStaff,13)
    };
    public RecipeInfo[] Recipe = Array.Empty<RecipeInfo>();
}
public class TransformInfo
{
    [JsonProperty("src")]
    public short srcType;
    [JsonProperty("dest")]
    public short destType = -1;
    [JsonProperty("pg")]
    public byte progress = 0;
    public bool clear = false;
    public TransformInfo() { }
    public TransformInfo(short srcType, short destType, byte progress)
    {
        this.srcType = srcType;
        this.destType = destType;
        this.progress = progress;
    }
}
public class RecipeInfo
{
    [JsonIgnore]
    public int RecipeId;
    public ItemInfo createItem = new();
    public ItemInfo[] requiredItems = Array.Empty<ItemInfo>();
    public bool crimson = false;
    public bool corruption = false;
    public static readonly int numRecipes = Recipe.numRecipes;
    private void AddRecipe()
    {
        var currentRecipe = Recipe.currentRecipe;
        currentRecipe.createItem.SetDefaults(createItem.type);
        currentRecipe.createItem.stack = createItem.stack;
        for (int i = 0; i < Math.Min(Recipe.maxRequirements, requiredItems.Length); i++) 
        {
            currentRecipe.requiredItem[i].SetDefaults(requiredItems[i].type);
            currentRecipe.requiredItem[i].stack = requiredItems[i].stack;
        }
        currentRecipe.crimson = crimson;
        currentRecipe.corruption = corruption;
        RecipeId = Recipe.numRecipes;
        Recipe.AddRecipe();
        Console.WriteLine($"add type:{createItem.type} RecipeId:{RecipeId}");
    }
    private void ModifyRecipe(int decraftingRecipeIndex)
    {
        var recipe = Main.recipe[decraftingRecipeIndex];
        recipe.createItem.SetDefaults(createItem.type);
        recipe.createItem.stack = createItem.stack;
        var setCount = Math.Min(Recipe.maxRequirements, requiredItems.Length);
        for (int i = 0; i < setCount; i++)
        {
            recipe.requiredItem[i].SetDefaults(requiredItems[i].type);
            recipe.requiredItem[i].stack = requiredItems[i].stack;
        }
        for (int i = setCount; i < Recipe.maxRequirements; i++)
        {
            if (recipe.requiredItem[i].type != 0)
            {
                recipe.requiredItem[i] = new Item();
            }
        }
        RecipeId = decraftingRecipeIndex;
        Console.WriteLine($"modify type:{createItem.type} RecipeId:{RecipeId}");
    }
    public void UpdateRecipe()
    {
        var decraftingRecipeIndex = Terraria.GameContent.ShimmerTransforms.GetDecraftingRecipeIndex(ItemID.Sets.ShimmerCountsAsItem[createItem.type] != -1 ? ItemID.Sets.ShimmerCountsAsItem[createItem.type] : createItem.type);
        if (decraftingRecipeIndex == -1)
        {
            AddRecipe();
        }
        else
        {
            var add = false;
            if (crimson)
            {
                decraftingRecipeIndex = ItemID.Sets.IsCraftedCrimson[createItem.type];
                if (decraftingRecipeIndex == -1) 
                {
                    AddRecipe();
                    add = true;
                }
            }
            if (corruption)
            {
                decraftingRecipeIndex = ItemID.Sets.IsCraftedCorruption[createItem.type];
                if (decraftingRecipeIndex == -1)
                {
                    if (!crimson)
                    {
                        AddRecipe();
                        add = true;
                    }
                }
            }
            if (!add)
            {
                ModifyRecipe(decraftingRecipeIndex);
            }
        }
    }
}
public class ItemInfo
{
    public int type;
    public int stack = 1;
}