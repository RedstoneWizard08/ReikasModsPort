using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class HealingOverTime : MonoBehaviour, CustomSerializedComponent {

	private static readonly float TICK_RATE = 0.25F;

	private float totalToHeal;
	private float healingRemaining;
	private float totalDuration;

	private float healRate;
	private float startTime;

	public HealingOverTime setValues(float total, float seconds) {
		totalToHeal = total;
		totalDuration = seconds;
		healingRemaining = total;
		healRate = totalToHeal / seconds * TICK_RATE;
		return this;
	}

	public void activate() {
		CancelInvoke(nameof(tick));
		startTime = Time.time;
		InvokeRepeating(nameof(tick), 0f, TICK_RATE);
	}

	internal void tick() {
		var amt = Mathf.Min(healingRemaining, healRate);
		Player.main.GetComponent<LiveMixin>().AddHealth(amt);
		healingRemaining -= amt;
		if (healingRemaining <= 0)
			this.destroy(false);
	}

	private void OnKill() {
		this.destroy(false);
	}

	public virtual void saveToXML(XmlElement e) {
		e.addProperty("total", totalToHeal);
		e.addProperty("remaining", healingRemaining);
		e.addProperty("duration", totalDuration);
		e.addProperty("rate", healRate);
		e.addProperty("time", startTime);
	}

	public virtual void readFromXML(XmlElement e) {
		totalToHeal = (float)e.getFloat("total", 0);
		healingRemaining = (float)e.getFloat("remaining", 0);
		totalDuration = (float)e.getFloat("duration", 0);
		healRate = (float)e.getFloat("rate", 0);
		activate();
		startTime = (float)e.getFloat("time", 0);
	}

}