using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    OrderStationManager orderStationManager;
    GameSetup gameSetup;
    StateManager stateManager;
    IngredientManager ingredientManager;

    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to GameSetup
        GameObject gameSetupGO = GameObject.Find("GameSetup");
        gameSetup = gameSetupGO.GetComponent<GameSetup>();

        // Get a reference to OrderStationManager
        GameObject orderStations = GameObject.Find("OrderStationManager");
        orderStationManager = orderStations.GetComponent<OrderStationManager>();

        // Get a reference to StateManager
        GameObject stateManagerGO = GameObject.Find("StateManager");
        stateManager = stateManagerGO.GetComponent<StateManager>();

        // Get a reference to IngredientManager
        GameObject ingredientManagerGO = GameObject.Find("IngredientManager");
        ingredientManager = ingredientManagerGO.GetComponent<IngredientManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only allow for order station and change station buttons if no order is active.
        if (stateManager.activeOrderIndex < 0)
        {
            orderStationButtons();
            changeStationButtons();
        }
        // If an order is active, enable inputs for ingredients.
        else if (stateManager.activeOrderIndex >= 0)
        {
            addIngredientButtons();
            ingredientPageButtons();
        }
    }

    // Takes input to change the state of what station the player is currently at.
    void changeStationButtons()
    {
        if (Input.GetKeyDown("j"))
        {
            stateManager.nextStation(orderStationManager);
        }
        else if (Input.GetKeyDown("f"))
        {
            stateManager.previousStation(orderStationManager);
        }
    }

    // Takes input pertaining to the active ingredients, and adds that ingredient to the active order.
    void addIngredientButtons()
    {
        Dictionary<string, Ingredient> keybinds = ingredientManager.keybinds;

        // Checks for all active ingredient keybinds and if pressed, adds that ingredient to the order.
        // The double for-loop has to be done here to prevent a "Collection was modified" error.
        // If there's any runtime considerations to be held, I think here would be the first place.
        foreach (string key in keybinds.Keys.ToList())
        {
            if (keybinds.ContainsKey(key))
            {
                Ingredient ingredient = keybinds[key];

                if (Input.GetKeyDown(key))
                {
                    orderStationManager.addIngredientToOrder(ingredient);
                    ingredientManager.initIngredientPage(stateManager.currentStation.ToString(), ingredientManager.recentType);
                }
            }
        }
    }

    // Takes input to flip between ingredient types and to add an ingredient to an order.
    void ingredientPageButtons()
    {
        if (Input.GetKeyDown("space"))
        {
            ingredientManager.nextPage(stateManager.currentStation.ToString());
        }
        else if (Input.GetKeyDown("return"))
        {
            stateManager.activeOrderIndex = -1;
            orderStationManager.toggleDefaultText(true);
            orderStationManager.resetHighlights();
            ingredientManager.revertRecipeCard();
        }
    }

    // All input associated with interacting with the main order stations.
    void orderStationButtons()
    {
        if (Input.GetKeyDown("1"))
        {
            orderStationManager.orderStationEvent(0);
        }
        else if (Input.GetKeyDown("2"))
        {
            orderStationManager.orderStationEvent(1);
        }
        else if (Input.GetKeyDown("3"))
        {
            orderStationManager.orderStationEvent(2);
        }
        else if (Input.GetKeyDown("4"))
        {
            orderStationManager.orderStationEvent(3);
        }
        else if (Input.GetKeyDown("5"))
        {
            orderStationManager.orderStationEvent(4);
        }
        else if (Input.GetKeyDown("6"))
        {
            orderStationManager.orderStationEvent(5);
        }
        else if (Input.GetKeyDown("7"))
        {
            orderStationManager.orderStationEvent(6);
        }
        else if (Input.GetKeyDown("8"))
        {
            orderStationManager.orderStationEvent(7);
        }
        else if (Input.GetKeyDown("9"))
        {
            orderStationManager.orderStationEvent(8);
        }
        else if (Input.GetKeyDown("0"))
        {
            orderStationManager.orderStationEvent(9);
        }
    }
}
