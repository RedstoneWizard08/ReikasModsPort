using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class LargeOxygenite : CustomPrefab {
    public readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    public LargeOxygenite(XMLLocale.LocaleEntry e) : base("Large_" + e.key, e.name, e.desc) {
        locale = e;
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var go = ObjectUtil.createWorldObject(VanillaResources.LARGE_QUARTZ.prefab);
        Oxygenite.setupOxygeniteRender(go, 2.5F);
        var dr = go.GetComponent<Drillable>();
        dr.Start();
        dr.minResourcesToSpawn = 1;
        dr.maxResourcesToSpawn = 1;
        dr.primaryTooltip = locale.name;
        // dr.kChanceToSpawnResources = 1;
        var ox = CustomMaterials.getItem(CustomMaterials.Materials.OXYGENITE).Info.TechType;
        dr.resources = [new() { techType = ox, chance = 1 }];
        var rt = go.EnsureComponent<ResourceTracker>();
        rt.techType = ox;
        rt.overrideTechType = ox;
        return go;
    }

    public void postRegister() {
        /*
        PDAManager.PDAPage page = PDAManager.createPage("ency_"+ClassID, FriendlyName, locale.pda, locale.getString("category"));
        page.setHeaderImage(TextureManager.getTexture(SeaToSeaMod.modDLL, locale.getString("header")));
        page.register();
        PDAScanner.EntryData e = new PDAScanner.EntryData();
        e.key = TechType;
        e.destroyAfterScan = false;
        e.locked = true;
        e.scanTime = 4;
        e.encyclopedia = page.id;
        PDAHandler.AddCustomScannerEntry(e);
        */
    }
}