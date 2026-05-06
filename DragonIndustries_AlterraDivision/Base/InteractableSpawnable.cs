using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Handlers;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class InteractableSpawnable : CustomPrefab {
    public readonly XMLLocale.LocaleEntry locale;

    public float scanTime = 1;

    public Action<PDAScanner.EntryData> scanEntryModifier;

    public readonly Assembly ownerMod;

    public int fragmentCount { get; private set; }
    public int scanCount { get; private set; }

    public static TechType fragmentUnlock { get; private set; }

    [SetsRequiredMembers]
    protected InteractableSpawnable(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
        locale = e;
        ownerMod = SNUtil.TryGetModDLL();
        SetGameObject(GetGameObject);
    }

    public abstract GameObject GetGameObject();

    public void countGen<G>(WorldgenDatabase worldgen) where G : WorldGenerator {
        fragmentCount = worldgen.getCount<G>();
        SNUtil.Log("Found " + fragmentCount + " " + Info.ClassID + " to use as fragments", ownerMod);
    }

    public void countGen(WorldgenDatabase worldgen, string id = null) {
        if (id == null)
            id = Info.ClassID;
        fragmentCount = worldgen.getCount(id);
        SNUtil.Log("Found " + fragmentCount + " " + id + " to use as fragments", ownerMod);
    }

    public void setFragment(TechType unlock, int count, bool delete = false) {
        scanCount = count;
        fragmentUnlock = unlock;
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, new List<TechType>() { fragmentUnlock });
        var old = scanEntryModifier;
        scanEntryModifier = e => {
            old?.Invoke(e);
            e.isFragment = true;
            e.blueprint = fragmentUnlock;
            e.totalFragments = scanCount;
            e.destroyAfterScan = delete;
        };
    }

    public void registerEncyPage() {
        SNUtil.AddPdaEntry(
            this,
            scanTime,
            locale.getString("category"),
            locale.pda,
            locale.getString("header"),
            scanEntryModifier
        );
        /*
        PDAManager.PDAPage page = PDAManager.createPage("ency_"+ClassID, FriendlyName, locale.pda, locale.getString("category"));
        page.setHeaderImage(TextureManager.getTexture(ownerMod, locale.getString("header")));
        page.register();
        PDAScanner.EntryData e = new PDAScanner.EntryData();
        e.key = TechType;
        e.scanTime = scanTime;
        e.locked = true;
        if (scanEntryModifier != null)
            scanEntryModifier.Invoke(e);
        e.encyclopedia = page.id;
        PDAHandler.AddCustomScannerEntry(e);
        */
    }
}