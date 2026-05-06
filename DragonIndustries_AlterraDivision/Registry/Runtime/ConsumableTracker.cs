using System.Collections.ObjectModel;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class ConsumableTracker : SerializedTracker<ConsumableTracker.ConsumeItemEvent> {

	public static readonly ConsumableTracker instance = new();

	private ConsumableTracker() : base("Consumption.dat", true, parseConsumption, null) {

	}

	public void onConsume(GameObject go, bool isEating) {
		onConsume(CraftData.GetTechType(go), isEating);
	}

	public void onConsume(TechType tt, bool isEating) {
		//SNUtil.writeToChat("Log eat of "+tt.AsString());
		if (tt != TechType.None)
			add(new ConsumeItemEvent(tt, DayNightCycle.main.timePassedAsFloat, isEating));
	}

	private static ConsumeItemEvent parseConsumption(XmlElement s) {
		return new ConsumeItemEvent(SNUtil.GetTechType(s.GetProperty("itemType")), s.GetFloat("eventTime", -1), s.GetBoolean("isEat"));
	}

	public ReadOnlyCollection<ConsumeItemEvent> getEvents() {
		return data.AsReadOnly();
	}

	public class ConsumeItemEvent : SerializedTrackedEvent {

		public readonly TechType itemType;
		public readonly bool isEating;

		internal ConsumeItemEvent(TechType tt, double time, bool eat) : base(time) {
			itemType = tt;
			isEating = eat;
		}

		public override void saveToXML(XmlElement e) {
			e.AddProperty("itemType", itemType.AsString());
			e.AddProperty("isEat", isEating);
		}

		public override string ToString() {
			return $"[ConsumeItemEvent Tech={itemType}, Time={eventTime}, Eat={isEating}]";
		}


	}

}