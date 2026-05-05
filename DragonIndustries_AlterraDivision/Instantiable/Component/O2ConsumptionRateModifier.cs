using System;

namespace ReikaKalseki.DIAlterra;

[Obsolete("Unimplemented")]
public class O2ConsumptionRateModifier : SelfRemovingComponent {

	public float consumptionFactor = 1;

	[Obsolete("Effect is unimplemented")]
	public static void add(float f, float duration) {
		var m = Player.main.gameObject.AddComponent<O2ConsumptionRateModifier>();
		m.consumptionFactor = f;
		m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
	}

}