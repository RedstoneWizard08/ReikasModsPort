using System.Diagnostics.CodeAnalysis;

namespace ReikaKalseki.Auroresource;

public class DrillableMeteorite : DrillableResourceArea {
    [SetsRequiredMembers]
    public DrillableMeteorite() : base(AuroresourceMod.Locale.getEntry("DrillableMeteorite"), 24) {
        addDrop(TechType.Titanium, 400);
        addDrop(TechType.Copper, 250);
        addDrop(TechType.Nickel, 180);
        addDrop(TechType.Lead, 180);
        addDrop(TechType.Silver, 150);
        addDrop(TechType.Gold, 100);
        addDrop(TechType.MercuryOre, 40);
    }
}