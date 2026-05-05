using System;
using System.Xml;

namespace ReikaKalseki.DIAlterra;

[Obsolete]
public class SurvivalEventTracker : SerializedTracker<SurvivalEventTracker.SurvivalEvent> {

	public static readonly SurvivalEventTracker instance = new SurvivalEventTracker();

	private SurvivalEventTracker() : base("SurvivalEvents.dat", true, parse, null) {

	}

	private static SurvivalEvent parse(XmlElement s) {
		string type = s.getProperty("type");
		if (string.IsNullOrEmpty(type))
			return null;
		switch (type) {
			default:
				return null;
		}
	}

	public abstract class SurvivalEvent : SerializedTrackedEvent {

		internal SurvivalEvent(float time) : base(time) {

		}

		public override void saveToXML(XmlElement e) {

		}

	}

}