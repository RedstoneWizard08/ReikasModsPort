using System;
using System.Collections.Generic;
using System.Reflection;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public static class CustomMaterials {
    private static readonly Dictionary<Materials, BasicCustomOre>
        mappings = new();

    private static readonly Dictionary<TechType, BasicCustomOre> techs = new();

    static CustomMaterials() {
        foreach (Materials m in Enum.GetValues(typeof(Materials))) {
            var id = Enum.GetName(typeof(Materials), m);
            SNUtil.log("Registering material " + id);
            var attr = getMaterial(m);
            var e = SeaToSeaMod.ItemLocale.getEntry(id);
            var template =
                (VanillaResources)typeof(VanillaResources).GetField(attr.templateName).GetValue(null);
            var item = (BasicCustomOre)Activator.CreateInstance(
                attr.itemClass,
                new object[] { id, e.name, e.desc, template }
            );
            item.glowIntensity = attr.glow;
            switch (m) { //since has no custom class
                case Materials.IRIDIUM:
                    item.collectSound = "event:/loot/pickup_copper";
                    break;
            }

            mappings[m] = item;
            item.Register();
            techs[item.Info.TechType] = item;
            item.addPDAEntry(e.pda, m == Materials.PRESSURE_CRYSTALS ? 5 : 2, e.getString("header"));
            SNUtil.log(" > " + item);
        }
    }

    public static Material getMaterial(Materials key) {
        var info = typeof(Materials).GetField(Enum.GetName(typeof(Materials), key));
        return (Material)Attribute.GetCustomAttribute(info, typeof(Material));
    }

    public static BasicCustomOre getItem(Materials key) {
        return mappings[key];
    }

    public static BasicCustomOre getItemByTech(TechType tt) {
        return techs.ContainsKey(tt) ? techs[tt] : null;
    }

    public static TechType getIngot(Materials key) {
        return C2CItems.getIngot(getItem(key).Info.TechType).ingot;
    }

    public enum Materials {
        [Material(typeof(Azurite), "URANIUM", 4F)]
        VENT_CRYSTAL, //forms when superheated water is injected into cold water
        [Material(typeof(Platinum), "GOLD")] PLATINUM,

        [Material(typeof(PressureCrystals), "TITANIUM", 1.2F)]
        PRESSURE_CRYSTALS,

        [Material(typeof(Avolite), "KYANITE", 0.75F)]
        PHASE_CRYSTAL,

        [Material(typeof(BasicCustomOre), "SILVER")]
        IRIDIUM,

        [Material(typeof(Calcite), "MAGNETITE", 0.2F)]
        CALCITE,

        [Material(typeof(Obsidian), "KYANITE")]
        OBSIDIAN,

        [Material(typeof(Oxygenite), "QUARTZ", 1F)]
        OXYGENITE,
    }

    public class Material : Attribute {
        internal readonly Type itemClass;
        internal readonly string templateName;
        internal readonly float glow;

        public Material(Type item, string t, float g = 0) {
            itemClass = item;
            templateName = t;
            glow = g;
        }
    }
}