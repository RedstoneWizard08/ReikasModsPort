using System.Xml;

namespace ReikaKalseki.DIAlterra;

public class HealthModifier : SelfRemovingComponent, CustomSerializedComponent {

	public float damageFactor = 1;

	public static void add(float dmg, float duration) {
		var m = Player.main.gameObject.AddComponent<HealthModifier>();
		m.damageFactor = dmg;
		m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
	}

	public virtual void saveToXML(XmlElement e) {
		e.addProperty("endTime", elapseWhen);
		e.addProperty("modifier", damageFactor);
	}

	public virtual void readFromXML(XmlElement e) {
		elapseWhen = (float)e.getFloat("endTime", 0);
		damageFactor = (float)e.getFloat("modifier", 0);
	}

}