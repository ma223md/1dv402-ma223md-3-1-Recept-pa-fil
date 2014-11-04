using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        public void Load()
        {
            // Skapa lista som kan innehålla referenser till receptobjekt
            List<IRecipe> recipeNames = new List<IRecipe>(100);

            // variabel som definierar radens status?
            RecipeReadStatus status = RecipeReadStatus.Indefinite;

            try
            {
                // Öppna fil för läsning
                using (StreamReader reader = new StreamReader(@"App_Data\Recipes.txt"))
                {
                    // Läser filen rad för rad.
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // om tom rad  
                        if (line == "")
                        {
                            // fortsätt till nästa rad
                            continue;
                        }

                        // om avdelning för nytt recept 
                        if (line == "[Recept]")
                        {
                            // sätt status till att nästa rad som läses kommer att vara receptets namn
                            status = RecipeReadStatus.New;
                        }
                        // eller om det är avdelningen för ingredienser 
                        else if (line == "[Ingredienser]")
                        {
                            // sätt status till att kommande rader kommer att vara receptets ingredienser
                            status = RecipeReadStatus.Ingredient;
                        }
                        // eller om det är avdelningen för instruktioner 
                        else if (line == "[Instruktioner]")
                        {
                            // sätt status till att kommande rader kommer vara receptets instruktioner
                            status = RecipeReadStatus.Instruction;
                        }
                        // annars är det ett namn, en ingrediens eller en instruktion
                        else
                        {
                            // Om status är satt att raden ska tolkas som namn..
                            if (status == RecipeReadStatus.New)
                            {
                                // ..skapa nytt receptobjekt med receptets namn
                                Recipe recipe = new Recipe(line);
                                // lägg till recept till receptlista
                                recipeNames.Add(recipe);
                            }

                            // eller om status är satt att raden ska tolkas som en ingridiens
                            else if (status == RecipeReadStatus.Ingredient)
                            {
                                // dela upp raden i tre delar
                                string[] ingredientSplit = line.Split(';');
                                // om antalet delar inte är tre, kasta FileFormatException
                                if (ingredientSplit.Length != 3)
                                {
                                    throw new FileFormatException();
                                }
                                else
                                {
                                    string amount = ingredientSplit[0];
                                    string measure = ingredientSplit[1];
                                    string name = ingredientSplit[2];
                                    // skapa ett ingrediensobjekt och initiera det med de tre delarna för mängd, mått och namn
                                    Ingredient ingredient = new Ingredient();
                                    ingredient.Amount = amount;
                                    ingredient.Measure = measure;
                                    ingredient.Name = name;
                                    // lägg till ingrediensen till receptets lista med ingredienser
                                    IRecipe recipe = recipeNames[recipeNames.Count - 1];
                                    recipe.Add(ingredient);
                                }
                            }

                            // eller om status är satt att raden ska tolkas som en instruktion..
                            else if (status == RecipeReadStatus.Instruction)
                            {
                                // ..lägg till raden till receptets lista med instruktioner
                                IRecipe recipe = recipeNames[recipeNames.Count - 1];
                                recipe.Add(line);
                            }

                            else
                            {
                                // annars - kasta FileFormatException
                                throw new FileFormatException();
                            }
                        }

                        // sortera listan med recept med avseende på receptens namn
                        // Tilldela avsett fält i klassen, _recipes, en referens till listan
                        _recipes = recipeNames.OrderBy(x => x.Name).ToList();

                        // Tilldela avsedd egenskap i klassen, IsModified, ett värde som indikerar att listan med recept är oförändrad
                        IsModified = false;

                        // Utlös händelse om att recept har lästs genom att anropa metoden OnRecipesChanged och skicka med parametern EventArgs.Empty.
                        OnRecipesChanged(EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ett oväntat fel inträffade.\n{0}",
                ex.Message);
            }
        }

        public void Save()
        {
            // öppna textfil och skriv över den med det som lästs in
            if (IsModified == true)
            {
                try
                {
                    // Skapar en StreamWriter-objekt och skriver strängar.
                    using (StreamWriter writer = new StreamWriter(@"App_Data\Recipes.txt"))
                    {
                        foreach (Recipe r in _recipes)
                        {
                            writer.WriteLine("[Recept]");
                            writer.WriteLine(r.Name);
                            writer.WriteLine("[Ingredienser]");
                            foreach (Ingredient i in r.Ingredients)
                            {
                                writer.Write(i.Amount + ";");
                                writer.Write(i.Measure + ";");
                                writer.WriteLine(i.Name);
                            }
                            writer.WriteLine("[Instruktioner]");
                            foreach (string line in r.Instructions)
                            {
                                writer.WriteLine(line);
                            }
                        }
                        writer.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ett oväntat fel inträffade.\n{0}",
                    ex.Message);
                } 
            }
        }
    }
}
