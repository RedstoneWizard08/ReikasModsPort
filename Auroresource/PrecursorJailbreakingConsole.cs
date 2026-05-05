using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Auroresource;

public class PrecursorJailbreakingConsole : PrecursorStoryConsole {
    [SetsRequiredMembers]
    internal PrecursorJailbreakingConsole(XMLLocale.LocaleEntry e) : base(e, AuroresourceMod.LaserCutterJailbroken) {
        setPopup(TechType.LaserCutter);
    }

    public override bool isUsable(StoryConsoleTag tag) {
        var held = Inventory.main.GetHeld();
        return held && held.GetComponent<LaserCutter>(); // held.GetTechType() == TechType.LaserCutter;
    }
}