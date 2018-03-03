using SMLHelper.Patchers;
using UnityEngine;
using System.Collections.Generic;
using SMLHelper;
using System.IO;
using System.Linq;
using System;

namespace WarpCannonQMod
{
    public class EntryPoint
    {
        public const string WARP_CANNON_CLASS_ID = "WarpCannon";
        public const string WARP_BATTERY_CLASS_ID = "WarpBattery";
        public const string WARP_SCALE_CLASS_ID = "WarpScale";

        public static TechType warpCannonTechType;
        public static TechType warpScalesTechType;
        public static TechType warpBatteryTechType;

        public static AssetBundle AssetBundle;

        public static CraftingConfig IngredientsConfig;

        private static Dictionary<string, TechType> customTechTypes = new Dictionary<string, TechType>();

        public static void LoadConfig()
        {
            IngredientsConfig = CraftingConfig.LoadFromConfig(@"./QMods/WarpCannon/ingredients.json");
            if(IngredientsConfig == null)
            {
                IngredientsConfig = CraftingConfig.GetDefaultIngredients();
                CraftingConfig.Save(IngredientsConfig, @"./QMods/WarpCannon/ingredients.json");
            }
        }

        public static void OnGameStart()
        {
            LoadConfig();

            warpCannonTechType = TechTypePatcher.AddTechType(WARP_CANNON_CLASS_ID, "Warp Cannon", "A tool that allows you to warp 30 meters using Warper technology.");
            warpScalesTechType = TechTypePatcher.AddTechType(WARP_SCALE_CLASS_ID, "Warp Scale", "A resource obtained from warpers that can be used to craft Warp Batteries.");
            warpBatteryTechType = TechTypePatcher.AddTechType(WARP_BATTERY_CLASS_ID, "Warp Battery", "A battery that powers Warp Cannons.");

            customTechTypes.Add(WARP_CANNON_CLASS_ID, warpCannonTechType);
            customTechTypes.Add(WARP_BATTERY_CLASS_ID, warpBatteryTechType);
            customTechTypes.Add(WARP_SCALE_CLASS_ID, warpScalesTechType);

            var warpCannonData = new TechDataHelper();
            var warpBatteryData = new TechDataHelper();

            foreach(var techType in IngredientsConfig)
            {
                if(techType.Key == "WarpCannon")
                {
                    warpCannonData._craftAmount = techType.Value.CraftAmount;
                    warpCannonData._techType = warpCannonTechType;
                    warpCannonData._ingredients = new List<IngredientHelper>();

                    foreach(var ingredient in techType.Value.Ingredients)
                    {
                        var ingredientTech = default(TechType);
                        if (TechTypeExtensions.FromString(ingredient.ItemName, out ingredientTech, false))
                        {
                            warpCannonData._ingredients.Add(new IngredientHelper(ingredientTech, ingredient.Amount));
                        }
                        else if (customTechTypes.TryGetValue(ingredient.ItemName, out ingredientTech))
                        {
                            warpCannonData._ingredients.Add(new IngredientHelper(ingredientTech, ingredient.Amount));
                        }
                    }
                }
                
                if(techType.Key == "WarpBattery")
                {
                    warpBatteryData._craftAmount = techType.Value.CraftAmount;
                    warpBatteryData._techType = warpBatteryTechType;
                    warpBatteryData._ingredients = new List<IngredientHelper>();


                    foreach (var ingredient in techType.Value.Ingredients)
                    {
                        var ingredientTech = default(TechType);
                        if(TechTypeExtensions.FromString(ingredient.ItemName, out ingredientTech, false))
                        {
                            warpBatteryData._ingredients.Add(new IngredientHelper(ingredientTech, ingredient.Amount));
                        }
                        else if(customTechTypes.TryGetValue(ingredient.ItemName, out ingredientTech))
                        {
                            warpBatteryData._ingredients.Add(new IngredientHelper(ingredientTech, ingredient.Amount));
                        }
                    }
                }
            }

            CraftDataPatcher.customTechData.Add(warpCannonTechType, warpCannonData);
            CraftDataPatcher.customTechData.Add(warpBatteryTechType, warpBatteryData);

            CraftDataPatcher.customEquipmentTypes.Add(warpCannonTechType, EquipmentType.Hand);
            CraftDataPatcher.customItemSizes.Add(warpCannonTechType, new Vector2int(2, 2));
            CraftDataPatcher.customHarvestTypeList.Add(TechType.Warper, HarvestType.DamageAlive);
            CraftDataPatcher.customHarvestOutputList.Add(TechType.Warper, warpScalesTechType);

            CraftTreePatcher.customCraftNodes.Add("Personal/Tools/WarpCannon", warpCannonTechType);
            CraftTreePatcher.customCraftNodes.Add("Resources/Electronics/WarpBattery", warpBatteryTechType);

            // Load AssetBundle
            AssetBundle = AssetBundle.LoadFromFile(Path.Combine("./QMods/WarpCannon/", "warpcannon.assets"));
            if (AssetBundle == null)
            {
                return;
            }

            // Load GameObjects

            // Load Battery
            var warpCannonBattery = AssetBundle.LoadAsset<GameObject>("WarpBattery") as GameObject;
            Utility.AddBasicComponents(ref warpCannonBattery, "WarpBattery");
            warpCannonBattery.AddComponent<Pickupable>();
            warpCannonBattery.AddComponent<Battery>();
            warpCannonBattery.AddComponent<TechTag>().type = warpBatteryTechType;

            CustomPrefabHandler.customPrefabs.Add(new CustomPrefab("WarpBattery", "WorldEntities/Tools/WarpBattery", warpCannonBattery, warpBatteryTechType));

            // Load Warp Cannon
            var warpCannon = AssetBundle.LoadAsset<GameObject>("WarpCannon");
            Utility.AddBasicComponents(ref warpCannon, WARP_CANNON_CLASS_ID);
            warpCannon.AddComponent<Pickupable>();
            warpCannon.AddComponent<TechTag>().type = warpCannonTechType;

            var fabricating = warpCannon.FindChild("3rd person model").AddComponent<VFXFabricating>();
            fabricating.localMinY = -0.4f;
            fabricating.localMaxY = 0.2f;
            fabricating.posOffset = new Vector3(-0.054f, 0.223f, -0.06f);
            fabricating.eulerOffset = new Vector3(-44.86f, 90f, 0f);
            fabricating.scaleFactor = 1;

            var energyMixin = warpCannon.AddComponent<EnergyMixin>();
            energyMixin.defaultBattery = warpBatteryTechType;
            energyMixin.storageRoot = warpCannon.FindChild("3rd person model").AddComponent<ChildObjectIdentifier>();
            energyMixin.compatibleBatteries = new List<TechType> { warpBatteryTechType };
            energyMixin.allowBatteryReplacement = true;
            energyMixin.batteryModels = (new List<EnergyMixin.BatteryModels>()
            {
                new EnergyMixin.BatteryModels()
                {
                    techType = warpBatteryTechType,
                    model = warpCannonBattery
                }
            }).ToArray();

            var warpCannonComponent = warpCannon.AddComponent<WarpCannon>();
            warpCannonComponent.Init();
            warpCannonComponent.mainCollider = warpCannon.AddComponent<BoxCollider>();
            warpCannonComponent.ikAimRightArm = true;
            warpCannonComponent.useLeftAimTargetOnPlayer = true;

            CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(WARP_CANNON_CLASS_ID, "WorldEntities/Tools/WarpCannon", warpCannon, warpCannonTechType));

            // Load Sprites
            var warpCannonSprite = AssetBundle.LoadAsset<Sprite>("Warp_Cannon");
            var warpScalesSprite = AssetBundle.LoadAsset<Sprite>("Warp_Scale");
            var warpBatterySprite = AssetBundle.LoadAsset<Sprite>("Warp_Battery");

            CustomSpriteHandler.customSprites.Add(new CustomSprite(warpCannonTechType, warpCannonSprite));
            CustomSpriteHandler.customSprites.Add(new CustomSprite(warpScalesTechType, warpScalesSprite));
            CustomSpriteHandler.customSprites.Add(new CustomSprite(warpBatteryTechType, warpBatterySprite));
        }
    }
}
