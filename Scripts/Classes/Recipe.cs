using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipe
{
    public string ID { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public List<Ingredient> ingredients { get; set; }

    public Recipe()
    {
        ID = name = type = "NULL";
        ingredients = new List<Ingredient>();
    }
}
