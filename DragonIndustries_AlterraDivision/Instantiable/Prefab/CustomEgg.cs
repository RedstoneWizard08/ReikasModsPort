using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class CustomEgg : CustomPrefab {
    public readonly TechType creatureToSpawn;
    private readonly TechType template;
    private TechType undiscoveredTechType;

    private readonly string creatureID;

    public float eggScale = 1;

    private string eggTexture;
    public string creatureHeldDesc;
    private Action<GameObject> objectModify;

    public int eggSize = 2;
    public int creatureSize = 3;

    public readonly WaterParkCreatureData eggProperties;

    private readonly Assembly ownerMod;

    private static readonly Dictionary<TechType, CustomEgg> eggs = new();

    [SetsRequiredMembers]
    public CustomEgg(CustomPrefab pfb, TechType t) : this(pfb.Info.TechType, t, pfb.Info.ClassID) {
    }

    [SetsRequiredMembers]
    public CustomEgg(TechType c, TechType t) : this(c, t, c.AsString()) {
    }

    [SetsRequiredMembers]
    private CustomEgg(TechType c, TechType t, string id, Assembly a = null) : base(
        id + "_Egg",
        id + " Egg",
        "Hatches a " + id
    ) {
        ownerMod = a != null ? a : SNUtil.tryGetModDLL();

        creatureToSpawn = c;
        template = t.getEgg();
        if (template == TechType.None)
            throw new Exception("Failed to find egg for creature techtype " + t.AsString());

        creatureID = id;

        var prefab = CraftData.GetPrefabForTechTypeAsync(t).GetResult();
        var creature = prefab.GetComponent<WaterParkCreature>();
        var wpp = creature.data;
        eggProperties = ScriptableObject.CreateInstance<WaterParkCreatureData>();
        eggProperties.initialSize = wpp.initialSize;
        eggProperties.maxSize = wpp.maxSize;
        eggProperties.outsideSize = wpp.outsideSize;
        eggProperties.daysToGrow = wpp.daysToGrow;
        eggProperties.isPickupableOutside = wpp.isPickupableOutside;

        AddOnRegister(onPatched);

        eggs[creatureToSpawn] = this;
        SetGameObject(GetGameObject);
        Info.WithIcon(GetItemSprite());
    }

    private void onPatched() {
        if (ownerMod == null)
            throw new Exception("Egg item " + creatureID + "/" + Info.TechType + " has no source mod!");

        CraftDataHandler.SetItemSize(creatureToSpawn, new Vector2int(creatureSize, creatureSize));

        var prefab = CraftData.GetPrefabForTechTypeAsync(creatureToSpawn).GetResult();
        var creature = prefab.GetComponent<WaterParkCreature>();
        creature.data = eggProperties;

        undiscoveredTechType = EnumHandler.AddEntry<TechType>(Info.ClassID + "_undiscovered").WithPdaInfo("", "");
        SpriteHandler.RegisterSprite(undiscoveredTechType, GetItemSprite());
        CraftDataHandler.SetItemSize(undiscoveredTechType, SizeInInventory);

        //WaterParkCreatureData data = ScriptableObject.CreateInstance<WaterParkCreatureData>();

        SNUtil.log("Constructed custom egg for " + creatureID + ": " + Info.TechType.AsString(), ownerMod);
    }

    public Vector2int SizeInInventory => new(eggSize, eggSize);

    public CustomEgg setTexture(string tex) {
        eggTexture = tex;
        SpriteHandler.RegisterSprite(
            creatureToSpawn,
            TextureManager.getSprite(ownerMod, eggTexture + creatureID + "_Hatched")
        );
        return this;
    }

    public CustomEgg modifyGO(Action<GameObject> a) {
        objectModify = a;
        return this;
    }

    protected Sprite GetItemSprite() {
        return TextureManager.getSprite(ownerMod, "Textures/Items/Egg_" + creatureID);
    }

    public GameObject GetGameObject() {
        var pfb = ObjectUtil.createWorldObject(template);
        var egg = pfb.EnsureComponent<CreatureEgg>();
        egg.eggType = Info.TechType;
        egg.overrideEggType = undiscoveredTechType; //undiscovered
        egg.creatureType = creatureToSpawn;
        // egg.explodeOnHatch = false;
        pfb.fullyEnable();
        pfb.transform.localScale = Vector3.one * eggScale;
        RenderUtil.swapTextures(ownerMod, pfb.GetComponentInChildren<Renderer>(), eggTexture + creatureID);
        objectModify?.Invoke(pfb);
        return pfb;
    }

    public static void updateLocale() {
        foreach (var e in eggs.Values) {
            var cname = Language.main.Get(e.creatureToSpawn);
            CustomLocaleKeyDatabase.registerKey(e.Info.TechType.AsString(), cname + " Egg");
            CustomLocaleKeyDatabase.registerKey("Tooltip_" + e.Info.TechType.AsString(), "Hatches a " + cname);

            CustomLocaleKeyDatabase.registerKey(
                e.undiscoveredTechType.AsString(),
                Language.main.Get(TechType.BonesharkEggUndiscovered)
            );
            CustomLocaleKeyDatabase.registerKey(
                "Tooltip_" + e.undiscoveredTechType.AsString(),
                Language.main.Get("Tooltip_" + TechType.BonesharkEggUndiscovered.AsString())
            );

            SNUtil.log("Relocalized " + e + " > " + Language.main.Get(e.Info.TechType), e.ownerMod);
            if (!string.IsNullOrEmpty(e.creatureHeldDesc)) {
                CustomLocaleKeyDatabase.registerKey(
                    "Tooltip_" + e.creatureToSpawn.AsString(),
                    e.creatureHeldDesc + "\nRaised in containment."
                );
            }
        }
    }

    public static CustomEgg getEgg(TechType creature) {
        return eggs.ContainsKey(creature) ? eggs[creature] : null;
    }

    public static CustomEgg createAndRegisterEgg(
        CustomPrefab creature,
        TechType basis,
        float scale,
        string grownHeldDesc,
        bool isBig,
        Action<CustomEgg> modify,
        float eggSpawnRate = 1,
        params BiomeType[] spawn
    ) {
        var egg = new CustomEgg(creature, basis);
        registerEgg(egg, scale, grownHeldDesc, isBig, modify, eggSpawnRate, spawn);
        return egg;
    }

    public static CustomEgg createAndRegisterEgg(
        TechType creature,
        TechType basis,
        float scale,
        string grownHeldDesc,
        bool isBig,
        Action<CustomEgg> modify,
        float eggSpawnRate = 1,
        params BiomeType[] spawn
    ) {
        var egg = new CustomEgg(creature, basis);
        registerEgg(egg, scale, grownHeldDesc, isBig, modify, eggSpawnRate, spawn);
        return egg;
    }

    private static void registerEgg(
        CustomEgg egg,
        float scale,
        string grownHeldDesc,
        bool isBig,
        Action<CustomEgg> modify,
        float eggSpawnRate,
        params BiomeType[] spawn
    ) {
        egg.setTexture("Textures/Eggs/");
        egg.creatureHeldDesc = grownHeldDesc;
        egg.eggScale = scale;
        if (!isBig) {
            egg.creatureSize = 2;
            egg.eggSize = 1;
        }

        modify?.Invoke(egg);

        foreach (var b in spawn)
            GenUtil.registerSlotWorldgen(
                egg.Info.ClassID,
                egg.Info.PrefabFileName,
                egg.Info.TechType,
                EntitySlot.Type.Small,
                LargeWorldEntity.CellLevel.Medium,
                b,
                1,
                0.2F * eggSpawnRate
            );
    }

    public bool includes(TechType tt) {
        return tt == Info.TechType || tt == undiscoveredTechType;
    }
}