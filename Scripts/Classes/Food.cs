using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food
{
    public string ID { get; set; }
    public string type { get; set; }
    public List<Recipe> recipes { get; set; }
    public List<string> ingredients { get; set; }

    public Food()
    {
        ID = type = "NULL";
        recipes = new List<Recipe>();
        ingredients = new List<string>();
    }
}
