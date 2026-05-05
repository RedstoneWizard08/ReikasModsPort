namespace ReikaKalseki.DIAlterra;

public class TemporaryBreathPrevention : SelfRemovingComponent {

	public static void add(float duration) {
		var m = Player.main.gameObject.AddComponent<TemporaryBreathPrevention>();
		m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
	}

}