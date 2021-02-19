using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// This class sets up many aspects of the game before the level starts, namely what orders will show up throughout the day and how many
// orders there will be.
public class GameSetup : MonoBehaviour
{
    public GameObject orderStation;
    public GameObject orderStationsPanel;
    public GameObject prepStationsPanel;

    public OrderStation[] orderStations;
    public OrderStation[] prepStations;

    public List<GameObject> orderStationsGO;
    public List<GameObject> prepStationsGO;

    List<OrderStation> availableStations;
    
    Queue<Order> orderQueue;
    Dictionary<string, Food> foods;

    System.Random random;

    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to the food and recipe lists and the order station manager.
        GameObject recipeParserGO = GameObject.Find("RecipeParser");
        RecipeParser recipeParser = recipeParserGO.GetComponent<RecipeParser>();

        GameObject orderStationManagerGO = GameObject.Find("OrderStationManager");
        OrderStationManager orderStationManager = orderStationManagerGO.GetComponent<OrderStationManager>();

        // Get a reference to StateManager
        GameObject stateManagerGO = GameObject.Find("StateManager");
        StateManager stateManager = stateManagerGO.GetComponent<StateManager>();

        random = new System.Random();

        // Create the lists and queues.
        orderStationsGO = new List<GameObject>();
        prepStationsGO = new List<GameObject>();
        availableStations = new List<OrderStation>();
        orderStations = new OrderStation[Constants.maxOrderStations];
        prepStations = new OrderStation[Constants.maxPrepStations];
        orderQueue = new Queue<Order>();
        foods = recipeParser.foods;

        // Initialize the various lists and queues.
        initStationObjects(orderStationsGO, orderStationsPanel, "Order");
        initStationObjects(prepStationsGO, prepStationsPanel, "Prep");
        initOrderStations();
        initPrepStations();
        initAvailableStations();

        // Setup the station states and the default starting station.
        stateManager.setupStationStates();

        // Generate orders for the day and enable the order station manager.
        generateOrders();
        orderStationManager.enableOrderStations(availableStations, orderStations, orderQueue, foods);
    }

    // Initializes all the order stations graphically.
    void initStationObjects(List<GameObject> stationList, GameObject panel, string type)
    {
        // Get the base position of the orderStation prefab.
        Vector3 pos = orderStation.transform.position;
        float x = orderStationsPanel.transform.position.x;
        float y = pos.y;

        int maxStations = type.Equals("Order") ? Constants.maxOrderStations : Constants.maxPrepStations;

        // Instantiate all stations.
        for (int i = 0; i < maxStations; i++)
        {
            // Instantiate the new order station and place it in the canvas.
            GameObject newStation = Instantiate(orderStation, new Vector3(x, y, 0), Quaternion.identity);
            newStation.transform.SetParent(panel.transform, false);
            newStation.name = type + " Station " + (i + 1);

            // Set the order station's various attributes.
            int stationNumber = i + 1;
            GameObject stationNumberGO = newStation.transform.GetChild(1).gameObject;
            GameObject keybindNumber = newStation.transform.GetChild(2).gameObject;
            GameObject ticket = newStation.transform.GetChild(3).gameObject;

            // Set the default text of the order station.
            Text stationNumberText = stationNumberGO.GetComponent<Text>();
            Text keybindNumberText = keybindNumber.GetComponent<Text>();

            stationNumberText.text = stationNumber.ToString();
            keybindNumberText.text = (stationNumber < 10) ? stationNumber.ToString() : "0";

            ticket.SetActive(false);

            // Move the y value down for the next order station.
            y = y - 100.0f;

            stationList.Add(newStation);
        }
    }

    // Initialize all of the available stations.
    void initAvailableStations()
    {
        for (int i = 0; i < Constants.maxOrderStations; i++)
        {
            availableStations.Add(orderStations[i]);
        }
    }

    // Iniitalize the stations within the orderStations array so that they will display properly.
    void initOrderStations()
    {
        for (int i = 0; i < Constants.maxOrderStations; i++)
        {
            int stationNumber = i + 1;
            orderStations[i] = new OrderStation(stationNumber, orderStationsGO[i]);
        }
    }

    // Initialize the prep stations within the prepStations array.
    void initPrepStations()
    {
        for (int i = 0; i < Constants.maxPrepStations; i++)
        {
            int stationNumber = i + 1;
            prepStations[i] = new OrderStation(stationNumber, prepStationsGO[i]);
        }

        // Disable all prep stations to start. These stations should only be active when the player is at the prep station.
        foreach (GameObject station in prepStationsGO)
        {
            station.SetActive(false);
        }
    }

    // Enqueues orders for the day in the orderQueue.
    void generateOrders()
    {
        // Generate as many random orders as the maxOrdersPerDay constant allows, and enqueue them within the orderQueue.
        for (int i = 0; i < Constants.maxOrdersPerDay; i++)
        {
            Recipe selectedEntree, selectedSide, selectedDrink;
            List<Recipe> selectedRecipes = selectRecipesForOrder();

            float sideValue = UnityEngine.Random.value;
            float drinkValue = UnityEngine.Random.value;

            selectedEntree = selectedRecipes[0];
            selectedSide = selectedRecipes[1];
            selectedDrink = selectedRecipes[2];

            Order newOrder = new Order(selectedEntree, selectedSide, selectedDrink);

            // Update if the order will ask for a side and/or drink. If not, a dummy
            // request will still be added, but inaccessible and not required to fulfill the order.
            if (sideValue > Constants.sideChance)
            {
                newOrder.wantsSide = false;
            }

            if (drinkValue > Constants.drinkChance)
            {
                newOrder.wantsDrink = false;
            }

            // If the order type exists in the new order, decide if it has any requests for
            // ingredient exceptions.
            foreach (int type in Constants.types)
            {
                Recipe selectedRecipe = selectedEntree;

                float requestValue = UnityEngine.Random.value;

                // Determine what sort of food type we're dealing with.
                // If the new order does not have the specified food type, exit early.
                if (type == Constants.side && newOrder.wantsSide)
                {
                    selectedRecipe = selectedSide;
                }
                
                if (type == Constants.drink && newOrder.wantsDrink)
                {
                    selectedRecipe = selectedDrink;
                }

                List<Ingredient> ingredients = new List<Ingredient>();

                // If the order has exceptions, decide what they are.
                foreach (Ingredient ingredient in selectedRecipe.ingredients)
                {
                    float ingredientValue = UnityEngine.Random.value;

                    // If the order has exceeded max requests, stop adding exceptions.
                    if (ingredients.Count >= Constants.maxRequests)
                    {
                        break;
                    }
                    // Otherwise, add a request
                    else if (ingredientValue > ingredient.occurrence)
                    {
                        ingredients.Add(ingredient);
                    }
                }

                newOrder.requests.Add(type, ingredients);
            }

            // Place the newly-created order in the queue.
            orderQueue.Enqueue(newOrder);
        }
    }

    List<Recipe> selectRecipesForOrder()
    {
        List<Recipe> selectedRecipes = new List<Recipe>();
        Recipe selectedRecipe = null;

        foreach (int type in Constants.types)
        {
            List<Food> foodList = foods.Values.ToList();
            foodList = foodList.Where(foodType => type == (Constants.typeToInt[foodType.type])).ToList();

            // Pick a random food and recipe.
            int foodIndex = random.Next(foodList.Count);
            int recipeIndex = random.Next(foodList[foodIndex].recipes.Count);

            // Set up the three indices of the selectedRecipes list.
            if (type == Constants.entree)
            {
                selectedRecipe = foodList[foodIndex].recipes[recipeIndex];
            }
            else if (type == Constants.side)
            {
                selectedRecipe = foodList[foodIndex].recipes[recipeIndex];
            }
            else if (type == Constants.drink)
            {
                selectedRecipe = foodList[foodIndex].recipes[recipeIndex];
            }

            // Add the selected recipe and reset it to null for the next pass.
            selectedRecipes.Add(selectedRecipe);
            selectedRecipe = null;
        }

        return selectedRecipes;
    }
}
