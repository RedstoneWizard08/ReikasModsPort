using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class GlowOilNatural : PickedUpAsOtherItem {
    [SetsRequiredMembers]
    internal GlowOilNatural() : base("NaturalGlowOil", EcoceanMod.glowOil.TechType) {
        Info.WithIcon(GetItemSprite());
    }

    protected Sprite GetItemSprite() {
        return EcoceanMod.glowOil.getSprite();
    }

    public override GameObject GetGameObject() {
        GameObject world = EcoceanMod.glowOil.GetGameObject().clone();
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.EnsureComponent<Pickupable>().SetTechTypeOverride(Info.TechType);
        world.EnsureComponent<GlowOilTag>().enabled = true;
        world.fullyEnable();
        return world;
    }

    public override int getNumberCollectedAs() {
        return EcoceanMod.config.getInt(ECConfig.ConfigEntries.GLOWCOUNT);
    }

    public void register() {
        this.Register();
        PDAManager.PDAPage p = EcoceanMod.glowOil.getPDAEntry();
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, new List<TechType>() { template });
        var e = new PDAScanner.EntryData {
            key = Info.TechType,
            locked = true,
            scanTime = 3,
            encyclopedia = p.id,
        };
        PDAHandler.AddCustomScannerEntry(e);
    }
}