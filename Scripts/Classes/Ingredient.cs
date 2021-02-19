using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient
{
    // Used to identify the ingredient in various dictionaries.
    public string ID { get; set; }
    // The string that displays in-game.
    public string name { get; set; }
    // The type of ingredient. Ties it to an ingredient page for recipes.
    public string type { get; set; }
    // The station this ingredient appears at exclusively.
    public string station { get; set; }
    // The keybind that is used to place this ingredient.
    public string keybind { get; set; }
    // The amount of ingredient required for an order to be successful.
    public int amount { get; set; }
    // The chance this ingredient will appear in an order from 0.0 to 1.0
    public float occurrence { get; set; }
    // Whether this ingredient can appear in an order with others of its type.
    public bool isMutuallyExclusive { get; set; }
    // The maximum amount of this ingredient that can appear in an order.
    public int maxPerOrder { get; set; }

    public Ingredient()
    {
        ID = name = station = keybind = type = "NULL";
        amount = maxPerOrder = 0;
        occurrence = 0.0f;
        isMutuallyExclusive = false;
    }

    public Ingredient ShallowCopy()
    {
        return (Ingredient)this.MemberwiseClone();
    }

}
