using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Crafting;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class OutdoorPot : CustomPrefab {
    private readonly TechType pot;
    private readonly string prefabBase;

    private static readonly List<OutdoorPot> pots = [];

    [SetsRequiredMembers]
    internal OutdoorPot(TechType tt) : base(
        generateName(tt),
        "Outdoor " + tt.AsString(),
        "A " + tt.AsString() + " for use outdoors."
    ) {
        pot = tt;
        prefabBase = CraftData.GetClassIdForTechType(tt);
        pots.Add(this);

        Info.WithIcon(GetItemSprite());
        this.SetRecipe(GetBlueprintRecipe());
        this.SetPdaGroupCategory(GroupForPDA, CategoryForPDA);
        SetGameObject(GetGameObject);
    }

    private static string generateName(TechType tech) {
        var en = Enum.GetName(typeof(TechType), tech);
        return "outdoorpot_" + en.Substring(en.LastIndexOf('_') + 1);
    }

    public static void updateLocale() {
        foreach (var d in pots) {
            CustomLocaleKeyDatabase.registerKey(d.TechType.AsString(), "Outdoor " + Language.main.Get(d.pot));
            CustomLocaleKeyDatabase.registerKey(
                "Tooltip_" + d.TechType.AsString(),
                Language.main.Get("Tooltip_" + d.pot.AsString()) + " Designed for outdoor use."
            );
            SNUtil.Log("Relocalized " + d + " > " + Language.main.Get(d.TechType), AqueousEngineeringMod.modDLL);
        }
    }

    public void register() {
        this.Register();
        KnownTechHandler.SetAnalysisTechEntry(pot, new List<TechType>() { Info.TechType });
    }

    public bool UnlockedAtStart => false;

    public TechGroup GroupForPDA => TechGroup.ExteriorModules;

    public TechCategory CategoryForPDA => TechCategory.ExteriorModule;

    protected RecipeData GetBlueprintRecipe() {
        return RecipeUtil.getRecipe(pot); /*new TechData
        {
            Ingredients = new List<Ingredient>{new Ingredient(TechType.Titanium, 2)},
            craftAmount = 1
        };*/
    }

    protected Sprite GetItemSprite() {
        return SpriteManager.Get(pot); //TextureManager.getSprite("Textures/Items/"+ObjectUtil.formatFileName(this));
    }

    public GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject(prefabBase, true, false);
        if (world != null) {
            world.SetActive(false);
            world.EnsureComponent<TechTag>().type = Info.TechType;
            world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
            var c = world.EnsureComponent<Constructable>();
            c.techType = Info.TechType;
            c.allowedInBase = false;
            c.allowedInSub = false;
            c.allowedOutside = true;
            c.allowedOnGround = true;
            var p = world.EnsureComponent<Planter>();
            p.environment = Planter.PlantEnvironment.Dynamic;
            p.isIndoor = false;
            world.SetActive(true);
            return world;
        }

        SNUtil.WriteToChat("Could not fetch template GO for " + this);
        return null;
    }
}