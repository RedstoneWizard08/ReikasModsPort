using System.Diagnostics.CodeAnalysis;

namespace ReikaKalseki.Auroresource;

public class LavaDome : DrillableResourceArea {
    [SetsRequiredMembers]
    public LavaDome() : base(AuroresourceMod.Locale.getEntry("DrillableLavaDome"), 69) {
        addDrop(TechType.Quartz, 180);
        addDrop(TechType.AluminumOxide, 120);
        addDrop(TechType.Diamond, 80);
        addDrop(TechType.Magnetite, 50);
        addDrop(TechType.Sulphur, 30);
        addDrop(TechType.UraniniteCrystal, 30);
        addDrop(TechType.Kyanite, 10);
    }
}