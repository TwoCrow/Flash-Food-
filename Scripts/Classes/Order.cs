using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public Recipe entree { get; set; }
    public Recipe side { get; set; }
    public Recipe drink { get; set; }

    public HashSet<string> addedTypes { get; set; }

    public Dictionary<int, List<Ingredient>> requests { get; set; }
    public Dictionary<string, int> entreeAdditions { get; set; }
    public Dictionary<string, int> sideAdditions { get; set; }
    public Dictionary<string, int> drinkAdditions { get; set; }

    public List<Ingredient> entreeIngredients { get; set; }
    public List<Ingredient> sideIngredients { get; set; }
    public List<Ingredient> drinkIngredients { get; set; }

    public bool isRead { get; set; }
    public bool wantsSide { get; set; }
    public bool wantsDrink { get; set; }

    // Constructor
    public Order(Recipe entree, Recipe side, Recipe drink)
    {
        this.entree = entree;
        this.side = side;
        this.drink = drink;

        addedTypes = new HashSet<string>();

        requests = new Dictionary<int, List<Ingredient>>();
        entreeAdditions = new Dictionary<string, int>();
        sideAdditions = new Dictionary<string, int>();
        drinkAdditions = new Dictionary<string, int>();

        entreeIngredients = new List<Ingredient>();
        sideIngredients = new List<Ingredient>();
        drinkIngredients = new List<Ingredient>();

        isRead = false;
        wantsSide = wantsDrink = true;
    }

    // Adds the ingredient to the order with the specified ingredient list.
    public void addIngredient(Ingredient ingredient, int type)
    {
        if (type == Constants.entree)
        {
            entreeIngredients.Add(ingredient);
        }
        else if (type == Constants.side)
        {
            sideIngredients.Add(ingredient);
        }
        else if (type == Constants.drink)
        {
            drinkIngredients.Add(ingredient);
        }

        // Increment the ingredient count to know when to gray out an ingredient.
        incrementIngredientCount(ingredient, type);

        addIngredientType(ingredient.type);
    }

    // Add an ingredient of this type to know when to gray it out if it's mutually exclusive.
    public void addIngredientType(string type)
    {
        addedTypes.Add(type);
    }

    // Returns a bool to say whether the order already has an ingredient of this type.
    public bool hasType(string type)
    {
        return addedTypes.Contains(type);
    }

    // Returns a bool to say whether the given ingredient in this order is at max.
    public bool ingredientAtMax(Ingredient ingredient, int type)
    {
        Dictionary<string, int> additions = returnDictionaryOfType(type);

        if (!additions.ContainsKey(ingredient.name) || additions[ingredient.name] < ingredient.maxPerOrder)
        {
            return false;
        }
        else if (additions[ingredient.name] >= ingredient.maxPerOrder)
        {
            return true;
        }
        
        // We should never reach here, but it's necessary to prevent an error.
        return false;
    }

    // Increments the count of the given ingredient at this order.
    void incrementIngredientCount(Ingredient ingredient, int type)
    {
        Dictionary<string, int> additions = returnDictionaryOfType(type);

        // If the key is not within the dictionary, that means it was initially "at 0" (though it was unitilized)
        // so place a 1 there to indicate the ingredient was incremented.
        if (!additions.ContainsKey(ingredient.name))
        {
            additions.Add(ingredient.name, 1);
        }
        else if (!ingredientAtMax(ingredient, type))
        {
            additions[ingredient.name] = additions[ingredient.name] + 1;
        }
    }

    // Returns a bool to say whether the order already contains the given ingredient for the given type.
    public bool containsIngredient(Ingredient ingredient, int type)
    {
        Dictionary<string, int> additions = returnDictionaryOfType(type);

        if (additions.ContainsKey(ingredient.name))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    Dictionary<string, int> returnDictionaryOfType(int type)
    {
        Dictionary<string, int> additions = null;

        if (type == Constants.entree)
        {
            additions = entreeAdditions;
        }
        else if (type == Constants.side)
        {
            additions = sideAdditions;
        }
        else if (type == Constants.drink)
        {
            additions = drinkAdditions;
        }

        return additions;
    }
}
