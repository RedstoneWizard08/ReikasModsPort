namespace ReikaKalseki.Auroresource;

public class PrecursorJailbreakingConsole : PrecursorStoryConsole {

	internal PrecursorJailbreakingConsole(XMLLocale.LocaleEntry e) : base(e, AuroresourceMod.laserCutterJailbroken) {
		setPopup(TechType.LaserCutter);
	}

	public override bool isUsable(StoryConsoleTag tag) {
		Pickupable held = Inventory.main.GetHeld();
		return held && held.GetComponent<LaserCutter>();// held.GetTechType() == TechType.LaserCutter;
	}

}