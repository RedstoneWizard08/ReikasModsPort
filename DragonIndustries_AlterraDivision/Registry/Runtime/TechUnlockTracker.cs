using System.Collections.Generic;
using System.Xml;

namespace ReikaKalseki.DIAlterra;

public class TechUnlockTracker : SerializedTracker<TechUnlockTracker.TechUnlock> {

	public static readonly TechUnlockTracker instance = new();

	private readonly Dictionary<TechType, TechUnlock> unlocks = new();

	private TechUnlockTracker() : base("Unlocks.dat", true, parseUnlock, parseLegacyUnlock) {

	}

	public void onUnlock(TechType tt) {
		add(new TechUnlock(tt, DayNightCycle.main.timePassedAsFloat));
	}

	public void onScan(PDAScanner.EntryData scan) {
		add(new TechUnlock(scan.key, DayNightCycle.main.timePassedAsFloat, true));
	}

	public TechUnlock getUnlock(TechType tt) {
		return unlocks.ContainsKey(tt) ? unlocks[tt] : null;
	}

	private static TechUnlock parseUnlock(XmlElement s) {
		return new TechUnlock(SNUtil.GetTechType(s.GetProperty("tech")), s.GetFloat("eventTime", -1), s.GetBoolean("isScan"));
	}

	private static TechUnlock parseLegacyUnlock(string s) {
		var parts = s.Split(',');
		var tt = SNUtil.GetTechType(parts[0]);
		return new TechUnlock(tt, float.Parse(parts[1]), bool.Parse(parts[2]));
	}

	protected override void add(TechUnlock e) {
		base.add(e);
		unlocks[e.tech] = e;
	}

	protected override void clear() {
		base.clear();
		unlocks.Clear();
	}

	public class TechUnlock : SerializedTrackedEvent {

		public readonly TechType tech;
		public readonly bool isScan;

		internal TechUnlock(TechType tt, double time, bool scan = false) : base(time) {
			tech = tt;
			isScan = scan;
		}

		public override void saveToXML(XmlElement e) {
			e.AddProperty("tech", tech.AsString());
			e.AddProperty("isScan", isScan);
		}

		public override string ToString() {
			return $"[TechUnlock Tech={tech}, IsScan={isScan}, Time={eventTime}]";
		}


	}

}