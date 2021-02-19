using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class RecipeParser : MonoBehaviour
{
    public Dictionary<string, Ingredient> ingredients;
    public Dictionary<string, Recipe> recipes;
    public Dictionary<string, Food> foods;

    public Dictionary<(string, string), List<Ingredient>> ingredientsPerStation;
    public HashSet<string> ingredientTypes;

    // Start is called before the first frame update
    void Awake()
    {
        ingredients = new Dictionary<string, Ingredient>();
        recipes = new Dictionary<string, Recipe>();
        foods = new Dictionary<string, Food>();

        ingredientsPerStation = new Dictionary<(string, string), List<Ingredient>>();
        ingredientTypes = new HashSet<string>();

        processFiles("ingredients");
        processFiles("recipes");
        processFiles("foods");

        initIngredientsPerStation();
    }

    void processFiles(string selection)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;

        switch (selection)
        {
            case "ingredients":
                processIngredients(readerSettings, Constants.ingredientsPath);
                break;
            case "recipes":
                processRecipes(readerSettings, Constants.recipesPath);
                break;
            case "foods":
                processFoods(readerSettings, Constants.foodPath);
                break;
        }
    }

    // Processes the Ingredients.xml file
    void processIngredients(XmlReaderSettings readerSettings, string path)
    {
        XmlReader reader = XmlReader.Create(path, readerSettings);

        XmlDocument doc = new XmlDocument();
        doc.Load(reader);

        XmlNode root = doc.FirstChild;

        if (root.HasChildNodes)
        {
            // Process all Ingredient nodes.
            foreach (XmlNode node in root.ChildNodes)
            {
                Ingredient ingredient = new Ingredient();

                ingredient.ID = node.Attributes[0].Value;

                // Process all inner attributes of each individual node, e.g. the name and keybind.
                foreach (XmlNode attribute in node.ChildNodes)
                {
                    if (attribute.Name.Equals("Name"))
                    {
                        ingredient.name = attribute.InnerText;
                    }
                    else if (attribute.Name.Equals("Type"))
                    {
                        ingredient.type = attribute.InnerText;
                        ingredientTypes.Add(ingredient.type);
                    }
                    else if (attribute.Name.Equals("Station"))
                    {
                        ingredient.station = attribute.InnerText;
                    }
                    else if (attribute.Name.Equals("Keybind"))
                    {
                        ingredient.keybind = attribute.InnerText;
                    }
                    else if (attribute.Name.Equals("MutuallyExclusive"))
                    {
                        ingredient.isMutuallyExclusive = bool.Parse(attribute.InnerText);
                    }
                    else if (attribute.Name.Equals("MaxPerOrder"))
                    {
                        ingredient.maxPerOrder = Int32.Parse(attribute.InnerText);
                    }
                }

                // Have to do this song-and-dance because apparently TryAdd() doesn't exist?
                if (!ingredients.ContainsKey(ingredient.ID))
                {
                    ingredients.Add(ingredient.ID, ingredient);
                }
            }
        }
    }

    // Processes the Recipes.xml file
    void processRecipes(XmlReaderSettings readerSettings, string path)
    {
        XmlReader reader = XmlReader.Create(path, readerSettings);

        XmlDocument doc = new XmlDocument();
        doc.Load(reader);

        XmlNode root = doc.FirstChild;

        if (root.HasChildNodes)
        {
            // Process all Recipe nodes.
            foreach (XmlNode node in root.ChildNodes)
            {
                Recipe recipe = new Recipe();

                recipe.ID = node.Attributes[0].Value;

                // Process all inner attributes of each Recipe node (e.g. Name, Type)
                foreach (XmlNode attribute in node.ChildNodes)
                {
                    if (attribute.Name.Equals("Name"))
                    {
                        recipe.name = attribute.InnerText;
                    }
                    else if (attribute.Name.Equals("Type"))
                    {
                        recipe.type = attribute.InnerText;
                    }
                    else if (attribute.Name.Equals("Ingredients"))
                    {
                        getIngredientsForRecipe(attribute, recipe);
                    }
                }

                // Have to do this song-and-dance because apparently TryAdd() doesn't exist?
                if (!recipes.ContainsKey(recipe.ID))
                {
                    recipes.Add(recipe.ID, recipe);
                }
            }
        }
    }

    // Runs through the ingredient list for the associated recipe, filling in all of its attributes.
    void getIngredientsForRecipe(XmlNode recipeList, Recipe recipe)
    {
        // Process each Ingredient within the Ingredient list.
        foreach (XmlNode recipeNode in recipeList.ChildNodes)
        {
            Ingredient ingredient = new Ingredient();

            // Process all attributes for the given Ingredient.
            foreach (XmlNode recipeAttribute in recipeNode.ChildNodes)
            {
                if (recipeAttribute.Name.Equals("IngredientID"))
                {
                    ingredient.ID = recipeAttribute.InnerText;
                }
                else if (recipeAttribute.Name.Equals("Amount"))
                {
                    ingredient.amount = Int32.Parse(recipeAttribute.InnerText);
                }
                else if (recipeAttribute.Name.Equals("Occurrence"))
                {
                    ingredient.occurrence = float.Parse(recipeAttribute.InnerText);
                }
            }

            setupIngredient(ingredient);

            recipe.ingredients.Add(ingredient);
        }
    }

    // Fills in remaining attributes from the Ingredient dictionary.
    void setupIngredient(Ingredient ingredient)
    {
        Ingredient attributes = ingredients[ingredient.ID];

        ingredient.name = attributes.name;
        ingredient.type = attributes.type;
        ingredient.station = attributes.station;
        ingredient.keybind = attributes.keybind;
    }

    // Processes the Food.xml file, which contains the groupings of food types and their associated recipes.
    void processFoods(XmlReaderSettings readerSettings, string path)
    {
        XmlReader reader = XmlReader.Create(path, readerSettings);

        XmlDocument doc = new XmlDocument();
        doc.Load(reader);

        XmlNode root = doc.FirstChild;

        if (root.HasChildNodes)
        {
            // Process all Food nodes.
            foreach (XmlNode node in root.ChildNodes)
            {
                Food food = new Food();

                food.ID = node.Attributes[0].Value;

                // Process all child nodes in the Food node, like assocaited Recipes.
                foreach (XmlNode attributes in node.ChildNodes)
                {
                    if (attributes.Name.Contains("Type"))
                    {
                        food.type = attributes.InnerText;
                    }
                    else if (attributes.Name.Contains("Recipes"))
                    {
                        // Process all Recipes associated with this food type.
                        foreach (XmlNode recipe in attributes.ChildNodes)
                        {
                            string ID = "";

                            if (recipe.Name.Contains("RecipeID"))
                            {
                                ID = recipe.InnerText;
                            }

                            // Add the recipe to the Food's recipe list.
                            food.recipes.Add(recipes[ID]);
                        }
                    }
                    else if (attributes.Name.Contains("Ingredients"))
                    {
                        foreach (XmlNode ingredient in attributes.ChildNodes)
                        {
                            string ID = "";

                            if (ingredient.Name.Contains("Ingredient"))
                            {
                                ID = ingredient.InnerText;
                            }

                            // Add the ingredient to the Food's ingredient list.
                            food.ingredients.Add(ID);
                        }
                    }
                    
                }

                foods.Add(food.ID, food);
            }
        }
    }

    // Initialize the dictionary to contain all ingredients, grouped first by station then by ingredient type.
    void initIngredientsPerStation()
    {
        // Convert the dictionary to a list to make sorting easier.
        List<Ingredient> ingredientsList = ingredients.Values.ToList();

        // Sort the list first by station and then by type.
        ingredientsList.OrderBy(ingredient => ingredient.station).ThenBy(ingredient => ingredient.type);

        // Loop through all stations and ingredient types, dividing the ingredient list up into chunks associated by station and ingredient type.
        // Add these chunks to the ingredientsPerStation dictionary, with a tuple of the station and type as the key.
        foreach (Constants.StationState state in Enum.GetValues(typeof(Constants.StationState)))
        {
            string stationType = state.ToString();

            foreach (string ingredientType in ingredientTypes)
            {
                List<Ingredient> relatedIngredients = ingredientsList.Where(ingredient => 
                (ingredient.station.Equals(stationType)) && (ingredient.type.Equals(ingredientType))).ToList();

                // Ensure the key doesn't already exist before adding it to the dictionary.
                if (!ingredientsPerStation.ContainsKey((stationType, ingredientType)) && relatedIngredients.Count > 0)
                {
                    //print("Adding | Station: " + stationType + " Type: " + ingredientType);
                    ingredientsPerStation.Add((stationType, ingredientType), relatedIngredients);
                }
            }
        }
    }
}
