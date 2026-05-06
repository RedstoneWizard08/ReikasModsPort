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
		e.AddProperty("endTime", elapseWhen);
		e.AddProperty("modifier", damageFactor);
	}

	public virtual void readFromXML(XmlElement e) {
		elapseWhen = (float)e.GetFloat("endTime", 0);
		damageFactor = (float)e.GetFloat("modifier", 0);
	}

}