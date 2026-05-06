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
		e.AddProperty("endTime", elapseWhen);
		e.AddProperty("modifier", speedModifier);
	}

	public virtual void readFromXML(XmlElement e) {
		elapseWhen = (float)e.GetFloat("endTime", 0);
		speedModifier = (float)e.GetFloat("modifier", 0);
	}

}