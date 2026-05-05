using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class BasicCustomOre : CustomPrefab, DIPrefab<VanillaResources> {
    public readonly bool isLargeResource;

    public string collectSound = null;
    public Vector2int inventorySize = new(1, 1);

    public float glowIntensity { get; set; }
    public VanillaResources baseTemplate { get; set; }

    private readonly Assembly ownerMod;

    [SetsRequiredMembers]
    public BasicCustomOre(XMLLocale.LocaleEntry e, VanillaResources template) : this(e.key, e.name, e.desc, template) {
    }

    [SetsRequiredMembers]
    public BasicCustomOre(string id, string name, string desc, VanillaResources template) : base(id, name, desc) {
        ownerMod = SNUtil.tryGetModDLL();
        // typeof(ModPrefab).GetField("Mod", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, ownerMod);
        baseTemplate = template;

        AddOnRegister(() => {
                ItemRegistry.instance.addItem(this);
                if (collectSound != null) {
                    CraftDataHandler.SetPickupSound(Info.TechType, collectSound);
                }
            }
        );
        SetGameObject(GetGameObject);
        Info.WithIcon(GetItemSprite()).WithSizeInInventory(SizeInInventory);
    }

    public void registerWorldgen(BiomeType biome, int amt, float chance) {
        SNUtil.log("Adding worldgen " + biome + " x" + amt + " @ " + chance + "% to " + this, ownerMod);
        GenUtil.registerOreWorldgen(this, biome, amt, chance);
    }

    public void addPDAEntry(string text, float scanTime = 2, string header = null) {
        SNUtil.addPDAEntry(this, scanTime, "PlanetaryGeology", text, header, null);
    }

    protected Sprite GetItemSprite() {
        return TextureManager.getSprite(ownerMod, "Textures/Items/" + ObjectUtil.formatFileName(this));
    }

    public virtual void prepareGameObject(GameObject go, Renderer[] r) {
    }

    public sealed override string ToString() {
        return base.ToString() + " [" + Info.TechType + "] / " + ClassID + " / " + Info.PrefabFileName;
    }

    public GameObject GetGameObject() {
        return ObjectUtil.getModPrefabBaseObject(this);
    }

    public string ClassID => Info.ClassID;

    public virtual bool isResource() {
        return true;
    }

    public Assembly getOwnerMod() {
        return ownerMod;
    }

    public virtual string getTextureFolder() {
        return "Resources";
    }

    public Sprite getIcon() {
        return GetItemSprite();
    }

    public Vector2int SizeInInventory => inventorySize;
}