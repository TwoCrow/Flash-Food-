using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientManager : MonoBehaviour
{
    public GameObject panel;
    public GameObject ingredientGO;

    public Text ingredientTypeName;

    public string recentType;

    RecipeParser recipeParser;
    OrderStationManager orderStationManager;
    StateManager stateManager;

    Food food;
    Dictionary<(string, string), List<Ingredient>> ingredients;

    public Dictionary<string, Ingredient> keybinds;

    int baseIndex, currentIndex;

    // Get references to the other necessary managers.
    void Start()
    {
        // Get a reference to GameSetup
        GameObject recipeParserGO = GameObject.Find("RecipeParser");
        recipeParser = recipeParserGO.GetComponent<RecipeParser>();

        // Get a reference to OrderStationManager
        GameObject orderStations = GameObject.Find("OrderStationManager");
        orderStationManager = orderStations.GetComponent<OrderStationManager>();

        // Get a reference to StateManager
        GameObject stateManagerGO = GameObject.Find("StateManager");
        stateManager = stateManagerGO.GetComponent<StateManager>();

        keybinds = new Dictionary<string, Ingredient>();

        ingredients = recipeParser.ingredientsPerStation;
    }

    // Access the next page of the ingredient list.
    public void nextPage(string station)
    {
        foreach (string ingredient in food.ingredients)
        {
            currentIndex++;

            // If the current index is greater than the total, go back to the first index where
            // ingredients start appearing.
            if (currentIndex >= food.ingredients.Count)
            {
                currentIndex = baseIndex;
            }

            // Set up the next ingredient page if the ingredient dictionary contains a list at the current station and the current index
            // and that the list located there has a count greater than 0
            if (ingredients.ContainsKey((station, food.ingredients[currentIndex])) && ingredients[(station, food.ingredients[currentIndex])].Count > 0)
            {
                initIngredientPage(station, food.ingredients[currentIndex]);
                break;
            }
        }
    }

    // Initialize the first ingredient page whenever the player first opens an order.
    public void initFirstIngredientPage(string station, Food selectedFood)
    {
        baseIndex = currentIndex = 0;
        food = selectedFood;

        // Find the first instance where there exists at least one ingredient at this station.
        for (int i = 0; i < food.ingredients.Count; i++)
        {
            string type = food.ingredients[i];

            if (ingredients.ContainsKey((station, type)) && ingredients[(station, type)].Count > 0)
            {
                baseIndex = currentIndex = i;
                break;
            }
        }

        initIngredientPage(station, food.ingredients[baseIndex]);
    }

    // Initialize a subsequent ingredient page when the player moves to a new one.
    public void initIngredientPage(string station, string type)
    {
        ingredientTypeName.text = type.ToUpper() + "S";
        recentType = type;

        // Clean up any objects that are currently in the panel.
        revertRecipeCard();

        Order order = orderStationManager.orderStations[stateManager.activeOrderIndex].order;

        // Debug.Log("Trying to access (" + station + ", " + type + ")");

        // Initialize all ingredients as prefab objects within the panel and set their information accordingly.
        foreach (Ingredient ingredient in ingredients[(station, type)])
        {
            // Instantiate the new order station and place it in the canvas.
            GameObject newIngredient = Instantiate(ingredientGO, new Vector3(0, 0, 0), Quaternion.identity);
            newIngredient.transform.SetParent(panel.transform, false);

            // Get references to the two text fields for the ingredient.
            Text ingredientName = newIngredient.transform.GetChild(0).gameObject.GetComponent<Text>();
            Text keybindText = newIngredient.transform.GetChild(1).gameObject.GetComponent<Text>();

            ingredientName.text = ingredient.name.ToUpper();
            keybindText.text = ingredient.keybind.ToUpper();

            int orderType = orderStationManager.getTypeBasedOnStation();

            // If the order does not have a type added and the ingredient being added is not mutually exclusive, or the ingredient being added
            // has not reached its max for the order, add it to the keybind dictionary.
            if (!((order.hasType(type) && ingredient.isMutuallyExclusive) || order.ingredientAtMax(ingredient, orderType)))
            {
                keybinds.Add(ingredient.keybind.ToLower(), ingredient);
            }
            // Otherwise, the ingredient should not be added to the list of active 
            else
            {
                Image background = newIngredient.GetComponent<Image>();
                // 0.1960784f repeated thrice is the color code for dark gray.
                Color darkGray = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1.0f);

                background.color = darkGray;
                ingredientName.color = darkGray;
                keybindText.color = Color.gray;
            }
        }
    }

    // Clear the recipe card and keybinds dictionary.
    public void revertRecipeCard()
    {
        foreach (Transform child in panel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        keybinds.Clear();
    }
}
