using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Prep denotes an ingredient that has to be prepped in order to be available for use in an order, like beef patties, french fries, or bacon.
public class Prep : Ingredient
{
    // All prep ingredients are special, so this helps the parser realize that they are separate from regular ingredients.
    public bool isPrep { get; set; }
    // Used to denote if an ingredient is raw. If it is, it will need 
    public bool isRaw { get; set; }
    // The amount of time that must elapse in order for this ingredient to be considered cook, if it is a raw ingredient.
    public float cookTime { get; set; }
    // The amount of time that must elapse after the ingredient is finished cooking for it to burn.
    public float burnTime { get; set; }
    // The ID of the ingredient this ingredient becomes when cooked.
    public string cookedID { get; set; }
    // Tracks the stock of this ingredient presently available for use in the restaurant.
    public int stock { get; set; }
    
    public Prep()
    {
        isPrep = isRaw = false;
        cookTime = burnTime = 0.0f;
        cookedID = "";
        stock = 0;
    }
}
