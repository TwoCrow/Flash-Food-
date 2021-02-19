using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public enum StationState { ORDER, PREP, TOPPING, SIDES, DRINK }

    public static string foodPath = Application.streamingAssetsPath + "/Food.xml";
    public static string ingredientsPath = Application.streamingAssetsPath + "/Ingredients.xml";
    public static string recipesPath = Application.streamingAssetsPath + "/Recipes.xml";

    public static int maxPrepStations = 6;
    public static int maxOrderStations = 10;

    public static int maxOrdersPerDay = 100;

    public static string defaultMeatAndCheeseText = "NO MEAT OR CHEESE";
    public static string defaultToppingsText = "NO TOPPINGS";
    public static string defaultSaucesText = "NO SAUCES";
    public static string defaultDeluxeToppingsText = "NO DELUXE TOPPINGS";
    public static string defaultBunsText = "NO BUNS";

    public static float entreeChance = 0.99f;
    public static float sideChance = 0.85f;
    public static float drinkChance = 0.85f;

    public static float requestChance = 0.4f;

    public static int maxRequests = 3;

    public static int entree = 0;
    public static int side = 1;
    public static int drink = 2;
    public static int none = -1;

    public static List<int> types = new List<int> { entree, side, drink };

    public static Dictionary<string, int> typeToInt = new Dictionary<string, int>() 
    {
        { "ENTREE", entree }, { "SIDE", side }, { "DRINK", drink }
    };

}
