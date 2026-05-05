using System.Xml;

namespace ReikaKalseki.DIAlterra;

public class PlayerMovementSpeedModifier : SelfRemovingComponent, CustomSerializedComponent {

	public float speedModifier = 1;

	public static void add(float modifier, float duration) {
		var m = Player.main.gameObject.AddComponent<PlayerMovementSpeedModifier>();
		m.speedModifier = modifier;
		m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
	}

	public virtual void saveToXML(XmlElement e) {
		e.addProperty("endTime", elapseWhen);
		e.addProperty("modifier", speedModifier);
	}

	public virtual void readFromXML(XmlElement e) {
		elapseWhen = (float)e.getFloat("endTime", 0);
		speedModifier = (float)e.getFloat("modifier", 0);
	}

}