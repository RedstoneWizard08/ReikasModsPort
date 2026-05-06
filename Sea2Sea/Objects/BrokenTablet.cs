using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class BrokenTablet : CustomPrefab {
    public readonly TechType tablet;

    private static readonly List<BrokenTablet> tablets = [];

    [SetsRequiredMembers]
    internal BrokenTablet(TechType tt) : base(
        generateName(tt),
        "Broken " + tt.AsString(),
        "Pieces of " + tt.AsString()
    ) {
        tablet = tt;
        tablets.Add(this);

        SetGameObject(GetGameObject);
    }

    private static string generateName(TechType tech) {
        var en = Enum.GetName(typeof(TechType), tech);
        return "brokentablet_" + en.Substring(en.LastIndexOf('_') + 1);
    }

    public static void updateLocale() {
        foreach (var d in tablets) {
            CustomLocaleKeyDatabase.registerKey(
                d.Info.TechType.AsString(),
                "Broken " + Language.main.Get(d.tablet)
            );
            CustomLocaleKeyDatabase.registerKey(
                "Tooltip_" + d.Info.TechType.AsString(),
                "A shattered " + Language.main.Get(d.tablet) + ". Not very useful directly."
            );
            SNUtil.Log(
                "Relocalized broken tablet " + d + " > " + d.tablet.AsString() + " > " +
                Language.main.Get(d.Info.TechType),
                SNUtil.DiDLL
            );
        }
    }

    public void register() {
        var tabPfb = ObjectUtil.lookupPrefab(CraftData.GetClassIdForTechType(tablet));
        //tabPfb.SetActive(false);
        var fab = tabPfb.getChildObject("Model").EnsureComponent<VFXFabricating>();
        fab.localMaxY = 0.1F;
        fab.localMinY = -0.1F;
        KnownTechHandler.SetAnalysisTechEntry(Info.TechType, new List<TechType>() { tablet });
        var e = new PDAScanner.EntryData {
            key = Info.TechType,
            blueprint = tablet,
            destroyAfterScan = false,
            locked = true,
            totalFragments = 1,
            isFragment = true,
            scanTime = tablet == TechType.PrecursorKey_Orange ? 10 : 15,
        };
        //e.encyclopedia = Enum.GetName(typeof(TechType), tablet);//PDAScanner.mapping.ContainsKey(tablet) ? PDAScanner.mapping[tablet].encyclopedia : null;
        PDAHandler.AddCustomScannerEntry(e);
    }

    public GameObject GetGameObject() {
        var tabRef = ObjectUtil.lookupPrefab(CraftData.GetClassIdForTechType(tablet));
        var world = ObjectUtil.createWorldObject("83b61f89-1456-4ff5-815a-ecdc9b6cc9e4", true, false);
        //GameObject sparker = ObjectUtil.createWorldObject("ff8e782e-e6f3-40a6-9837-d5b6dcce92bc");
        tabRef.SetActive(false);
        if (world != null) {
            world.SetActive(false);
            world.EnsureComponent<TechTag>().type = Info.TechType;
            world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
            var m1a = tabRef.GetComponentInChildren<MeshRenderer>().materials[1];
            foreach (var r in world.GetComponentsInChildren<Renderer>()) {
                if (r.materials.Length !=
                    2) //any other renderers, like the VFXVoluLight added on purple and thus inherited here
                    continue;
                var idx = 0;
                var m1b = r.materials[1];
                foreach (var m in r.materials) {
                    foreach (var tex in m.GetTexturePropertyNames()) {
                        m1b.SetTexture(tex, m1a.GetTexture(tex));
                        m1b.SetTextureOffset(tex, m1a.GetTextureOffset(tex));
                        m1b.SetTextureScale(tex, m1a.GetTextureScale(tex));
                    }

                    idx++;
                }
            }

            //fetch existing light, added by C2CHooks skyapplier for purple
            var l = world.GetComponentInChildren<Light>();
            var f = l.GetComponent<FlickeringLight>();
            switch (tablet) {
                case TechType.PrecursorKey_Orange: {
                    l.intensity = 0.4F;
                    l.range = 18F;
                    l.color = new Color(1F, 0.63F, 0F, 1);
                    l.transform.localPosition = new Vector3(0, 0.03F, 0);
                    l.shadows = LightShadows.Soft;
                    f.dutyCycle = 0.8F;
                    f.updateRate = 0.67F;
                    f.fadeRate = 8F;
                    break;
                }
                case TechType.PrecursorKey_Red: {
                    l.intensity = 1F;
                    l.range = 10F;
                    l.color = new Color(1F, 0.33F, 0.33F, 1);
                    l.transform.localPosition = new Vector3(-0.25F, 0.3F, 0);
                    f.dutyCycle = 0.67F;
                    f.updateRate = 0.25F;
                    f.fadeRate = 5F;
                    break;
                }
                case TechType.PrecursorKey_White: {
                    l.intensity = 0.9F;
                    l.range = 45F;
                    l.color = new Color(216F / 255F, 247F / 255F, 1F, 1);
                    l.shadows = LightShadows.Soft;
                    l.transform.localPosition = new Vector3(0, 1.25F, 0);
                    f.dutyCycle = 0.3F;
                    f.updateRate = 0.08F;
                    f.fadeRate = 500F;
                    break;
                }
            } /*
            sparker.transform.SetParent(world.transform);
            sparker.removeChildObject("ElecLight");
            sparker.removeChildObject("xElec");
            foreach (ParticleSystemRenderer r in sparker.GetComponentsInChildren<ParticleSystemRenderer>()) {
                foreach (Material m in r.materials)
                    m.SetColor("_Color", l.color);
            }
            sparker.removeComponent<DamagePlayerInRadius>();
            */

            return world;
        } else {
            SNUtil.WriteToChat("Could not fetch template GO for " + this);
            return null;
        }
    }
}