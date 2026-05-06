using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class CustomEgg : CustomPrefab {
    public readonly TechType CreatureToSpawn;
    private readonly TechType _template;
    private TechType _undiscoveredTechType;

    private readonly string _creatureID;

    public float EggScale = 1;

    private string _eggTexture;
    public string CreatureHeldDesc;
    private Action<GameObject> _objectModify;

    public int EggSize = 2;
    public int CreatureSize = 3;

    public readonly WaterParkCreatureData EggProperties;

    private readonly Assembly _ownerMod;

    private static readonly Dictionary<TechType, CustomEgg> Eggs = new();

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
        _ownerMod = a != null ? a : SNUtil.TryGetModDLL();

        CreatureToSpawn = c;
        _template = t.GetEgg();
        if (_template == TechType.None)
            throw new Exception("Failed to find egg for creature techtype " + t.AsString());

        _creatureID = id;

        var prefab = PrefabUtil.GetPrefabForTechType(t);
        var creature = prefab.GetComponent<WaterParkCreature>();
        var wpp = creature.data;

        EggProperties = ScriptableObject.CreateInstance<WaterParkCreatureData>();
        EggProperties.initialSize = wpp.initialSize;
        EggProperties.maxSize = wpp.maxSize;
        EggProperties.outsideSize = wpp.outsideSize;
        EggProperties.daysToGrow = wpp.daysToGrow;
        EggProperties.isPickupableOutside = wpp.isPickupableOutside;

        AddOnRegister(OnPatched);

        Eggs[CreatureToSpawn] = this;
        SetGameObject(GetGameObject);
        Info.WithIcon(GetItemSprite());
    }

    private void OnPatched() {
        if (_ownerMod == null)
            throw new Exception("Egg item " + _creatureID + "/" + Info.TechType + " has no source mod!");

        CraftDataHandler.SetItemSize(CreatureToSpawn, new Vector2int(CreatureSize, CreatureSize));

        var prefab = PrefabUtil.GetPrefabForTechType(CreatureToSpawn);
        var creature = prefab.GetComponent<WaterParkCreature>();
        creature.data = EggProperties;

        _undiscoveredTechType = EnumHandler.AddEntry<TechType>(Info.ClassID + "_undiscovered").WithPdaInfo("", "");
        SpriteHandler.RegisterSprite(_undiscoveredTechType, GetItemSprite());
        CraftDataHandler.SetItemSize(_undiscoveredTechType, SizeInInventory);

        //WaterParkCreatureData data = ScriptableObject.CreateInstance<WaterParkCreatureData>();

        SNUtil.Log("Constructed custom egg for " + _creatureID + ": " + Info.TechType.AsString(), _ownerMod);
    }

    public Vector2int SizeInInventory => new(EggSize, EggSize);

    public CustomEgg SetTexture(string tex) {
        _eggTexture = tex;
        SpriteHandler.RegisterSprite(
            CreatureToSpawn,
            TextureManager.getSprite(_ownerMod, _eggTexture + _creatureID + "_Hatched")
        );
        return this;
    }

    public CustomEgg ModifyGo(Action<GameObject> a) {
        _objectModify = a;
        return this;
    }

    private Sprite GetItemSprite() {
        return TextureManager.getSprite(_ownerMod, "Textures/Items/Egg_" + _creatureID);
    }

    public GameObject GetGameObject() {
        var pfb = ObjectUtil.createWorldObject(_template);
        var egg = pfb.EnsureComponent<CreatureEgg>();
        egg.eggType = Info.TechType;
        egg.overrideEggType = _undiscoveredTechType; //undiscovered
        egg.creatureType = CreatureToSpawn;
        // egg.explodeOnHatch = false;
        pfb.fullyEnable();
        pfb.transform.localScale = Vector3.one * EggScale;
        RenderUtil.swapTextures(_ownerMod, pfb.GetComponentInChildren<Renderer>(), _eggTexture + _creatureID);
        _objectModify?.Invoke(pfb);
        return pfb;
    }

    public static void UpdateLocale() {
        foreach (var e in Eggs.Values) {
            var cname = Language.main.Get(e.CreatureToSpawn);
            CustomLocaleKeyDatabase.registerKey(e.Info.TechType.AsString(), cname + " Egg");
            CustomLocaleKeyDatabase.registerKey("Tooltip_" + e.Info.TechType.AsString(), "Hatches a " + cname);

            CustomLocaleKeyDatabase.registerKey(
                e._undiscoveredTechType.AsString(),
                Language.main.Get(TechType.BonesharkEggUndiscovered)
            );
            CustomLocaleKeyDatabase.registerKey(
                "Tooltip_" + e._undiscoveredTechType.AsString(),
                Language.main.Get("Tooltip_" + TechType.BonesharkEggUndiscovered.AsString())
            );

            SNUtil.Log("Relocalized " + e + " > " + Language.main.Get(e.Info.TechType), e._ownerMod);
            if (!string.IsNullOrEmpty(e.CreatureHeldDesc)) {
                CustomLocaleKeyDatabase.registerKey(
                    "Tooltip_" + e.CreatureToSpawn.AsString(),
                    e.CreatureHeldDesc + "\nRaised in containment."
                );
            }
        }
    }

    public static CustomEgg GetEgg(TechType creature) {
        return Eggs.TryGetValue(creature, out var egg) ? egg : null;
    }

    public static CustomEgg CreateAndRegisterEgg(
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
        RegisterEgg(egg, scale, grownHeldDesc, isBig, modify, eggSpawnRate, spawn);
        return egg;
    }

    public static CustomEgg CreateAndRegisterEgg(
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
        RegisterEgg(egg, scale, grownHeldDesc, isBig, modify, eggSpawnRate, spawn);
        return egg;
    }

    private static void RegisterEgg(
        CustomEgg egg,
        float scale,
        string grownHeldDesc,
        bool isBig,
        Action<CustomEgg> modify,
        float eggSpawnRate,
        params BiomeType[] spawn
    ) {
        egg.SetTexture("Textures/Eggs/");
        egg.CreatureHeldDesc = grownHeldDesc;
        egg.EggScale = scale;
        if (!isBig) {
            egg.CreatureSize = 2;
            egg.EggSize = 1;
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

    public bool Includes(TechType tt) {
        return tt == Info.TechType || tt == _undiscoveredTechType;
    }
}