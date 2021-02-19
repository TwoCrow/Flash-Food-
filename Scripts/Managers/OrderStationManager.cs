using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OrderStationManager : MonoBehaviour
{
    public float durationBetweenOrders = 5f;

    public GameObject orderStation;

    public Text ordersRemaining;

    // Text fields from the RecipeCard panel
    public Text recipeName;
    public Text instructionText;
    public Text recipeDefaultText;
    public Text ingredientTypeText;
    public Text ingredientDefaultText;

    public Text orderDisplay;

    List<OrderStation> availableStations;
    Queue<Order> orderQueue;
    Dictionary<string, Food> foods;

    GameSetup gameSetup;
    StateManager stateManager;
    IngredientManager ingredientManager;
    RecipeParser recipeParser;

    public OrderStation[] orderStations { get; set; }

    // This function is what is called by GameSetup to start enabling order stations.
    public void enableOrderStations(List<OrderStation> availableStations, OrderStation[] orderStations, Queue<Order> orderQueue, Dictionary<string, Food> foods)
    {
        // Get a reference to GameSetup. Yes I realize I just passed a ton of stuff from there to here.
        GameObject gameSetupGO = GameObject.Find("GameSetup");
        gameSetup = gameSetupGO.GetComponent<GameSetup>();

        // Get a reference to StateManager
        GameObject stateManagerGO = GameObject.Find("StateManager");
        stateManager = stateManagerGO.GetComponent<StateManager>();

        // Get a reference to IngredientManager
        GameObject ingredientManagerGO = GameObject.Find("IngredientManager");
        ingredientManager = ingredientManagerGO.GetComponent<IngredientManager>();

        // Get a reference to RecipeParser
        GameObject recipeParserGO = GameObject.Find("RecipeParser");
        recipeParser = recipeParserGO.GetComponent<RecipeParser>();

        // Get the references for the various lists and queues for use later.
        this.availableStations = availableStations;
        this.orderStations = orderStations;
        this.orderQueue = orderQueue;
        this.foods = foods;

        // Set the default state of the recipe and ingredient windows.
        toggleDefaultText(true);

        // Start pushing orders to the order station.
        StartCoroutine("pushOrders");
    }

    // =-----------------------------------------------------------------------------------------------------------------=
    // Order Pushing - All methods associated with generating / displaying new orders or dismissing them.
    // =-----------------------------------------------------------------------------------------------------------------=

    // Slowly push out orders onto avaialable stations.
    IEnumerator pushOrders()
    {
        while (orderQueue.Count > 0)
        {
            yield return new WaitForSeconds(durationBetweenOrders);

            if (availableStations.Count > 0)
            {
                dequeueOrder();
            }
        }
    }

    // Dequeues an order from the orderQueue and displays its information properly.
    void dequeueOrder()
    {
        OrderStation orderStation = availableStations[0];
        GameObject station = orderStation.station;

        // Place the newly-generated order at the first available station, and remove that station from the list.
        availableStations.RemoveAt(0);

        // Update the order station's ticket to reflect the new order.
        GameObject ticket = station.transform.GetChild(3).gameObject;

        // Get the various text fields for the order's ticket.
        Text recipeName = ticket.transform.GetChild(0).gameObject.GetComponent<Text>();
        Text requests = ticket.transform.GetChild(1).gameObject.GetComponent<Text>();

        recipeName.text = requests.text = "???";

        orderStation.order = orderQueue.Dequeue();

        ticket.SetActive(true);

        // Update the orders remaining ticker.
        ordersRemaining.text = orderQueue.Count.ToString();
    }

    // Dismisses an active order, and places that station back in the orderQueue.
    public void serveOrder(int stationIndex)
    {
        // Get a reference to this order station.
        OrderStation orderStation = orderStations[stationIndex];
        GameObject station = orderStation.station;

        // Deactive the ticket.
        GameObject ticket = station.transform.GetChild(2).gameObject;
        ticket.SetActive(false);

        // Re-sort the available stations list so that the lowest station is at the front.
        if (!availableStations.Contains(orderStations[stationIndex]))
        {
            availableStations.Add(orderStations[stationIndex]);
            availableStations.Sort((x, y) => x.stationNumber.CompareTo(y.stationNumber));
        }

        // Check the order to see if it was made correctly.
        checkOrder(stationIndex);
    }

    // =-----------------------------------------------------------------------------------------------------------------=
    // Station Input - All methods associated with managing input for each order and prep station.
    // =-----------------------------------------------------------------------------------------------------------------=

    // Serves as a switchboard for the order stations. When the player presses an order station key, a different event will occur depending
    // on the current state - the station where the player currently is.
    public void orderStationEvent(int stationIndex)
    {
        switch (stateManager.currentStation)
        {
            case (Constants.StationState.ORDER):
                orderWindowActive(stationIndex);
                break;
            case (Constants.StationState.PREP):
                prepStationActive(stationIndex);
                break;
            case (Constants.StationState.TOPPING):
                orderBuildStationActive(stationIndex);
                break;
            case (Constants.StationState.SIDES):
                orderBuildStationActive(stationIndex);
                break;
            case (Constants.StationState.DRINK):
                orderBuildStationActive(stationIndex);
                break;
        }
    }

    // Determines what the order station buttons will do when pressed at the Order Window.
    void orderWindowActive(int stationIndex)
    {
        // Get a reference to the order station and its order at this index.
        OrderStation orderStation = orderStations[stationIndex];
        Order order = orderStation.order;

        // If the order is read, serve it. If not, reveal what the customer wants.
        if (order.isRead)
        {
            serveOrder(stationIndex);
        }
        else
        {
            revealOrder(stationIndex);
        }
    }

    // Enacts various procedures when a prep station is selected.
    void prepStationActive(int stationIndex)
    {
        // If the stationIndex is greater than the max number of prep stations,
        // return early to prevent an error.
        if (stationIndex >= Constants.maxPrepStations)
            return;

        stateManager.activePrepIndex = stationIndex;
    }

    // Enacts various checks and procedures when an order station is selected.
    void orderBuildStationActive(int stationIndex)
    {
        if (orderUnavailable(stationIndex))
            return;

        stateManager.activeOrderIndex = stationIndex;

        displayOrderRecipe(stationIndex);
        highlightSelectedOrder(stationIndex);
    }

    // =-----------------------------------------------------------------------------------------------------------------=
    // Ticket Display - All methods in charge of updating the information on tickets.
    // =-----------------------------------------------------------------------------------------------------------------=

    // Updates all order tickets upon a station change.
    public void updateAllTickets()
    {
        // Reset all order highlights in case a side or drink was grayed out previously.
        resetHighlights();

        // If we're not at the prep station, show active order tickets instead of prep tickets.
        if (stateManager.currentStation != (Constants.StationState)1)
        {
            for (int i = 0; i < Constants.maxOrderStations; i++)
            {
                // If the order has been read, then it's alright to reveal the information.
                if (orderStations[i].order.isRead)
                {
                    updateOrderTicket(i);
                }
            }
        }
        // Otherwise, we can update the prep tickets.
        else
        {
            for (int i = 0; i < Constants.maxPrepStations; i++)
            {
                updatePrepTicket(i);
            }
        }
    }

    // Updates the order ticket based on what state the game currently is in.
    void updateOrderTicket(int stationIndex)
    {
        // Get a reference to this order station.
        OrderStation orderStation = orderStations[stationIndex];
        GameObject ticket = orderStation.station.transform.GetChild(3).gameObject;

        // Get the various text fields for the order's ticket.
        Text recipeName = ticket.transform.GetChild(0).gameObject.GetComponent<Text>();
        Text requests = ticket.transform.GetChild(1).gameObject.GetComponent<Text>();

        // Get the type of food info that should be shown on the ticket.
        // This changes based on the current station.
        int type = getTypeBasedOnStation();

        if (type == Constants.entree)
        {
            recipeName.text = updateTicketForEntree(orderStation.order);
        }
        else if (type == Constants.side)
        {
            recipeName.text = updateTicketForSide(orderStation.order);
        }
        else if (type == Constants.drink)
        {
            recipeName.text = updateTicketForDrink(orderStation.order);
        }

        // Return early if the order does not call for a side or drink when
        // we need to display that information.
        if (!orderStation.order.wantsSide && type == Constants.side)
        {
            recipeName.text = "NO SIDE";
            requests.text = "";

            changeOrderColor(stationIndex, Color.gray);

            return;
        }
        else if (!orderStation.order.wantsDrink && type == Constants.drink)
        {
            recipeName.text = "NO DRINK";
            requests.text = "";
            changeOrderColor(stationIndex, Color.gray);

            return;
        }

        // With all hazard checks passed, update the special requests to exclude
        // certain ingredients.
        List<Ingredient> requestList = orderStation.order.requests[type];
        string exceptionText = "";

        // Display the ingredient exceptions for this order on the ticket.
        foreach (Ingredient ingredient in requestList)
        {
            exceptionText += ingredient.name.ToUpper() + ", ";
        }

        if (requestList.Count > 0)
        {
            exceptionText = exceptionText.Remove(exceptionText.Length - 2);
            requests.text = "NO " + exceptionText;
        }
        else
        {
            requests.text = "";
        }
    }

    void updatePrepTicket(int stationIndex)
    {

    }

    // Reveals the order when the player selects it at the order window.
    void revealOrder(int stationIndex)
    {
        // Update the order station's ticket to reflect the new order.
        OrderStation orderStation = orderStations[stationIndex];

        updateOrderTicket(stationIndex);

        // Make sure to show that the order has been read.
        orderStation.order.isRead = true;
    }

    // =-----------------------------------------------------------------------------------------------------------------=
    // Ticket Color - All methods in charge of changing the coloring of a ticket for highlighting.
    // =-----------------------------------------------------------------------------------------------------------------=

    // Change the color of the order.
    void changeOrderColor(int stationIndex, Color color)
    {
        // Get the images that need to be grayed out.
        Image stationBackground = orderStations[stationIndex].station.transform.GetChild(0).gameObject.GetComponent<Image>();
        Image ticketBackground = orderStations[stationIndex].station.transform.GetChild(3).gameObject.GetComponent<Image>();

        stationBackground.color = color;
        ticketBackground.color = color;
    }

    // Resets the changes made by highlightSelectedOrder() so that all stations look the same again.
    public void resetHighlights()
    {
        int type = getTypeBasedOnStation();

        for (int i = 0; i < orderStations.Length; i++)
        {
            Order order = orderStations[i].order;

            if ((type == Constants.side && !order.wantsSide) || (type == Constants.drink && !order.wantsDrink))
                continue;

            changeOrderColor(i, Color.white);
        }
    }

    // Highlights the order station that has been selected by making all other order stations gray.
    void highlightSelectedOrder(int stationIndex)
    {
        for (int i = 0; i < orderStations.Length; i++)
        {
            if (i == stationIndex)
                continue;

            changeOrderColor(i, Color.gray);
        }
    }

    // =-----------------------------------------------------------------------------------------------------------------=
    // Recipe Display - All methods in charge of managing and updating the recipe card.
    // =-----------------------------------------------------------------------------------------------------------------=

    // Displays the recipe for the selected order in the recipe card.
    void displayOrderRecipe(int stationIndex)
    {
        // Get a reference to the recipe tied to this order station.
        OrderStation orderStation = orderStations[stationIndex];
        Order order = orderStation.order;
        Recipe selectedFood = order.entree;

        // Toggle the text fields of the recipe and ingredient cards.
        toggleDefaultText(false);

        // Get the correct type of food to show in the ingredient card.
        int type = getTypeBasedOnStation();

        if (type == Constants.entree)
        {
            selectedFood = order.entree;
        }
        else if (type == Constants.side)
        {
            selectedFood = order.side;
        }
        else if (type == Constants.drink)
        {
            selectedFood = order.drink;
        }

        string instructions = "";

        recipeName.text = selectedFood.name;

        List<Ingredient> ingredients = selectedFood.ingredients;
        Food ingredientTypes = foods[selectedFood.type];

        // Loop through all ingredient types.
        foreach (string ingredientType in ingredientTypes.ingredients)
        {
            // Extract ingredient types that are only what we are currently viewing.
            List<Ingredient> relatedIngredients = ingredients.Where(ingredient => ingredient.type.Equals(ingredientType)).ToList();

            if (relatedIngredients.Count <= 0)
            {
                continue;
            }

            // Loop through those ingredients to build a line containing only those ingredient types.
            foreach (Ingredient ingredient in relatedIngredients)
            {
                if (ingredient.type.Equals("BUN"))
                {
                    instructions += "SANDWICHED BETWEEN A ";
                }
                else if (ingredient.amount > 1)
                {
                    instructions += "(" + ingredient.amount + ") ";
                }

                instructions += ingredient.name.ToUpper() + ", ";
            }

            // Remove the straggling comma.
            instructions = instructions.Remove(instructions.Length - 2);
            instructions += "\n\n";
        }

        instructionText.text = instructions;

        Food recipe = foods[selectedFood.type];

        ingredientManager.initFirstIngredientPage(stateManager.currentStation.ToString(), recipe);
    }

    // =-----------------------------------------------------------------------------------------------------------------=
    // Add to Selection - All methods in charge of adding an ingredient to an order or prep.
    // =-----------------------------------------------------------------------------------------------------------------=

    // Adds the specified ingredient to the active station.
    public void addIngredientToOrder(Ingredient ingredient)
    {
        Order order = orderStations[stateManager.activeOrderIndex].order;

        order.addIngredient(ingredient, getTypeBasedOnStation());

        // TODO: The following lines should be removed once graphical representations are implemented.
        int type = getTypeBasedOnStation();
        List<Ingredient> ingredients;

        if (type == Constants.entree)
        {
            ingredients = order.entreeIngredients;
        }
        else if (type == Constants.side)
        {
            ingredients = order.sideIngredients;
        }
        else
        {
            ingredients = order.drinkIngredients;
        }

        string output = "";

        foreach (Ingredient yea in ingredients)
        {
            output += yea.name + ", ";
        }

        output = output.Remove(output.Length - 2);

        orderDisplay.text = output;
    }

    // =-----------------------------------------------------------------------------------------------------------------=
    // Order Checking - All methods in charge of checking an order to ensure it's correct.
    // =-----------------------------------------------------------------------------------------------------------------=

    // Checks the order that is being submitted at the given station index to see if it was made correctly.
    void checkOrder(int stationIndex)
    {
        Order order = orderStations[stationIndex].order;

        List<Ingredient> entree = order.entree.ingredients;
        List<Ingredient> side = order.side.ingredients;
        List<Ingredient> drink = order.drink.ingredients;

        bool entreeCorrect = checkOrderBasedOnType(order, Constants.entree, order.entreeIngredients, entree);
        bool sideCorrect = checkOrderBasedOnType(order, Constants.side, order.sideIngredients, side);
        bool drinkCorrect = checkOrderBasedOnType(order, Constants.drink, order.drinkIngredients, drink);

        // TODO This is debug info to be removed once graphics are properly implemented.
        string check = "";

        if (entreeCorrect)
        {
            check += "Entree correct!\n";
        }
        else
        {
            check += "Entree INCORRECT!\n";
        }

        if ((sideCorrect && order.wantsSide))
        {
            check += "Side correct!\n";
        }
        else if (order.wantsSide)
        {
            check += "Side INCORRECT!\n";
        }

        if ((drinkCorrect && order.wantsDrink))
        {
            check += "Drink correct!\n";
        }
        else if (order.wantsDrink)
        {
            check += "Drink INCORRECT!\n";
        }

        orderDisplay.text = check;
    }

    // Returns a bool based on if the order is correct or not.
    bool checkOrderBasedOnType(Order order, int type, List<Ingredient> currentIngredients, List<Ingredient> correctRecipe)
    {
        // If the order is asking for a food of the current type, then expand the ingredient list for truthful checking.
        if (order.requests.ContainsKey(type))
        {
            correctRecipe = expandIngredients(correctRecipe);
            correctRecipe = correctRecipe.Where(ingredient => !order.requests[type].Contains(ingredient)).ToList();
        }
        // Otherwise return true, since a player automatically gets an order correct by virtue of the customer not wanting
        // a specific item.
        else
        {
            return true;
        }

        // Sort the various ingredient lists to ensure that all checking is done correctly.
        currentIngredients = currentIngredients.OrderBy(ingredient => ingredient.type).ToList();
        correctRecipe = correctRecipe.OrderBy(ingredient => ingredient.type).ToList();

        // Return what the order checking function finds.
        return noErrorsFound(currentIngredients, correctRecipe);
    }

    // Returns true if no errors are found between the current order and what the order should contain.
    // Returns false if either the sizes of the two lists are not the same, or a discrepancy is found between the current and comparison lists.
    bool noErrorsFound(List<Ingredient> current, List<Ingredient> comparison)
    {
        // If the count is not the same, then we know immediately that the order is incorrect because it is either
        // missing ingredients or has too many.
        if (current.Count == comparison.Count)
        {
            // Loop through both ingredient lists to see if they match each other exactly.
            for (int i = 0; i < current.Count; i++)
            {
                if (!current[i].ID.Equals(comparison[i].ID))
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    // =-----------------------------------------------------------------------------------------------------------------=
    // Auxiliary Methods - All methods that serve a useful, auxiliary purpose in support of other methods.
    // =-----------------------------------------------------------------------------------------------------------------=

    // Returns a boolean value indicative of whether the order can be selected.
    bool orderUnavailable(int stationIndex)
    {
        int type = getTypeBasedOnStation();

        return (orderNotRead(stationIndex) || orderUnavailableDueToChoice(stationIndex, type));
    }

    // Returns a boolean indicative of whether can be selected due to the guest's choices.
    bool orderUnavailableDueToChoice(int stationIndex, int type)
    {
        Order order = orderStations[stationIndex].order;

        return ((type == Constants.side && !order.wantsSide) || (type == Constants.drink && !order.wantsDrink));
    }

    // Returns a boolean value indicative of whether the order has been read from the Order Window yet.
    bool orderNotRead(int stationIndex)
    {
        return !orderStations[stationIndex].order.isRead;
    }

    // Returns a string based on the current active station state.
    // The string is either ENTREE, SIDE, or DRINK
    public int getTypeBasedOnStation()
    {
        string current = stateManager.currentStation.ToString();

        if (current.Equals("ORDER") || current.Equals("GRILL") || current.Equals("TOPPING"))
        {
            return Constants.entree;
        }
        else if (current.Equals("SIDES"))
        {
            return Constants.side;
        }
        else if (current.Equals("DRINK"))
        {
            return Constants.drink;
        }
        else
        {
            return Constants.none;
        }
    }

    // Expands the ingredient list of a specified recipe for use in error checking.
    List<Ingredient> expandIngredients(List<Ingredient> recipe)
    {
        List<Ingredient> expandedList = new List<Ingredient>();

        // For each ingredient in the list, add it to the expandedList as many times
        // as its amount specifies. This will usually only be 1.
        foreach (Ingredient ingredient in recipe)
        {
            for (int i = 0; i < ingredient.amount; i++)
            {
                expandedList.Add(ingredient);
            }
        }

        return expandedList;
    }

    // Toggles the state of the recipe and ingredient cards.
    // Passing true will revert the cards to their default text.
    // Passing false will enable the recipe and ingredient text.
    public void toggleDefaultText(bool state)
    {
        recipeName.enabled = ingredientTypeText.enabled = instructionText.enabled = !state;
        recipeDefaultText.enabled = ingredientDefaultText.enabled = state;
    }

    // These three functions are used to easily get access to the names of
    // the entree, side, and drink that are part of the order.
    string updateTicketForEntree(Order order)
    {
        return order.entree.name;
    }

    string updateTicketForSide(Order order)
    {
        return order.side.name;
    }

    string updateTicketForDrink(Order order)
    {
        return order.drink.name;
    }
}
