using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public class PrecursorPipeFastTravelConsole : PrecursorStoryConsole {

	internal PrecursorPipeFastTravelConsole(XMLLocale.LocaleEntry e) : base(e) {
		setPopup(TechType.PrecursorSurfacePipe);
	}

	public override bool isUsable(StoryConsoleTag tag) {
		return true;
	}

}