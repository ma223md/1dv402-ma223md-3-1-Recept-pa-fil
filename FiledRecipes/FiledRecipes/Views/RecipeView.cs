using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView
    {
        public void Show(IRecipe recipe)
        {
            // Variabler för att centrera receptets namn
            string source = recipe.Name;
            int length = 38;
            int spaces = length - source.Length;
            int padLeft = spaces / 2 + source.Length;
            // Skriv ut receptets namn
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(" ╔══════════════════════════════════════╗ ");
            Console.WriteLine(" ║" + source.PadLeft(padLeft).PadRight(length) + "║ ");
            Console.WriteLine(" ╚══════════════════════════════════════╝ ");
            Console.ResetColor();
            Console.WriteLine("");

            // Skriv ut receptets ingredienser
            Console.WriteLine("Ingredienser");
            Console.WriteLine("-----------------");
            foreach (Ingredient i in recipe.Ingredients)
            {
                Console.Write(i.Amount + " ");
                Console.Write(i.Measure + " ");
                Console.WriteLine(i.Name);
            }
            Console.WriteLine("");

            // Skriv ut receptets instruktioner
            Console.WriteLine("Gör såhär:");
            Console.WriteLine("-----------------");
            foreach (string line in recipe.Instructions)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("");
        }

        public void Show(IEnumerable<IRecipe> recipes)
        {
                foreach (Recipe recipe in recipes)
                {
                    Show(recipe);

                    // Tryck knapp för att plussa på n och se nästa recept
                    Console.WriteLine("Tryck på valfri tangent för att se nästa recept");
                    Console.WriteLine("");
                    Console.ReadKey();
                    Console.Clear();
            }
        }
    }
}
