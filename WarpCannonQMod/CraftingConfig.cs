using System;
using System.Collections.Generic;
using System.IO;
using Oculus.Newtonsoft.Json;

namespace WarpCannonQMod
{
    public class CraftingConfig : Dictionary<string, CraftConfig>
    {
        public static CraftingConfig LoadFromConfig(string path)
        {
            if (!File.Exists(path))
                return null;
            var json = File.ReadAllText(path);
            var ingredients = JsonConvert.DeserializeObject<CraftingConfig>(json);
            return ingredients;
        }

        public static CraftingConfig GetDefaultIngredients()
        {
            var ingredients = new CraftingConfig();
            ingredients["WarpCannon"] = new CraftConfig();
            ingredients["WarpCannon"].Ingredients = new List<CraftIngredient>();
            ingredients["WarpCannon"].CraftAmount = 1;
            ingredients["WarpCannon"].Ingredients.Add(new CraftIngredient() { ItemName = "WarpBattery", Amount = 1 });
            ingredients["WarpCannon"].Ingredients.Add(new CraftIngredient() { ItemName = "AdvancedWiringKit", Amount = 1 });
            ingredients["WarpCannon"].Ingredients.Add(new CraftIngredient() { ItemName = "Magnetite", Amount = 1 });
            ingredients["WarpCannon"].Ingredients.Add(new CraftIngredient() { ItemName = "Kyanite", Amount = 1 });

            ingredients["WarpBattery"] = new CraftConfig();
            ingredients["WarpBattery"].Ingredients = new List<CraftIngredient>();
            ingredients["WarpBattery"].CraftAmount = 1;
            ingredients["WarpBattery"].Ingredients.Add(new CraftIngredient() { ItemName = "WarpScale", Amount = 2 });
            ingredients["WarpBattery"].Ingredients.Add(new CraftIngredient() { ItemName = "Battery", Amount = 1 });

            return ingredients;
        }

        public static void Save(CraftingConfig ingredients, string path)
        {
            var json = JsonConvert.SerializeObject(ingredients);
            File.WriteAllText(path, json);
        }
    }
}
