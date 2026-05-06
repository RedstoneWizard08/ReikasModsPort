using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class SpawnedItemTracker : SerializedTracker<SpawnedItemTracker.SpawnedItemEvent> {

	public static readonly SpawnedItemTracker instance = new();

	private readonly Dictionary<string, SpawnedItemEvent> spawnedIDs = new();
	private readonly List<SpawnTagCallback> callbacks = [];

	private float lastTick;

	private SpawnedItemTracker() : base("CSpawns.dat", true, parse, SpawnedItemEvent.parseLegacy) {

	}

	private static SpawnedItemEvent parse(XmlElement s) {
		var e = new SpawnedItemEvent(SNUtil.GetTechType(s.GetProperty("item")), s.GetFloat("eventTime", -1));
		e.setObject(s);
		return e;
	}

	public SpawnedItemEvent addSpawn(TechType tt) {
		var e = new SpawnedItemEvent(tt, (int)DayNightCycle.main.timePassedAsFloat);
		add(e);
		return e;
	}

	public bool isSpawned(GameObject p) {
		return getSpawnEvent(p) != null;
	}

	public SpawnedItemEvent getSpawnEvent(GameObject p) {
		var pi = p.GetComponent<PrefabIdentifier>();
		return !pi ? null : spawnedIDs.ContainsKey(pi.Id) ? spawnedIDs[pi.Id] : null;
	}

	public bool isSpawned(Pickupable p) {
		return isSpawned(p.gameObject);
	}

	public SpawnedItemEvent getSpawnEvent(Pickupable p) {
		return getSpawnEvent(p.gameObject);
	}

	public string getDataMap() {
		return spawnedIDs.ToDebugString();
	}

	protected override void add(SpawnedItemEvent e) {
		base.add(e);
		if (!string.IsNullOrEmpty(e.objectID))
			spawnedIDs[e.objectID] = e;
	}

	protected override void clear() {
		base.clear();
		spawnedIDs.Clear();
	}

	public void tick() {
		var time = DayNightCycle.main.timePassedAsFloat;
		if (time - lastTick > 0.5F) {
			lastTick = time;

			for (var i = callbacks.Count - 1; i >= 0; i--) {
				var tag = callbacks[i];
				if (tag.isReady) {
					bool flag;
					if (tag.needsSearch) {
						tag.search();
						flag = true;
					}
					else {
						flag = tag.register();
					}
					if (flag)
						callbacks.RemoveAt(i);
				}
			}
		}
	}

	private class SpawnTagCallback {

		public readonly PrefabIdentifier prefab;
		public readonly SpawnedItemEvent entry;

		public readonly float creationTime;

		public bool isReady => DayNightCycle.main.timePassedAsFloat-creationTime >= 0.5F;

		public bool needsSearch { get; private set; }

		internal SpawnTagCallback(SpawnedItemEvent e, PrefabIdentifier pi) {
			prefab = pi;
			entry = e;

			creationTime = DayNightCycle.main.timePassedAsFloat;
		}

		internal bool register() {
			if (!prefab || string.IsNullOrEmpty(prefab.Id)) {
				SNUtil.Log("Skipping spawn tag callback for nulled ID: " + prefab + "; entry = " + entry, SNUtil.DiDLL);
				needsSearch = true;
				return false;
			}
			entry.attach(prefab);
			SNUtil.Log("Attached spawn tag callback " + entry, SNUtil.DiDLL);
			return true;
		}

		public void search() {
			var li = Inventory.main.container.GetItems(entry.itemType);
			if (li == null || li.Count == 0) {
				SNUtil.Log("Skipping spawn search tag callback, no matching items for " + entry, SNUtil.DiDLL);
				return;
			}
			var prefab = li[li.Count - 1].item.GetComponent<PrefabIdentifier>();
			if (!prefab || string.IsNullOrEmpty(prefab.Id)) {
				SNUtil.Log("Skipping spawn search tag callback for nulled ID: " + prefab + "; entry = " + entry, SNUtil.DiDLL);
				return;
			}
			entry.attach(prefab);
			SNUtil.Log("Attached spawn search tag callback " + entry, SNUtil.DiDLL);
		}

	}

	public class SpawnedItemEvent : SerializedTrackedEvent {

		public readonly TechType itemType;

		public string classID { get; private set; }
		public string objectID { get; private set; }

		public string tooltip =>
			//why does time start at 480
			"\n<color=#ffc500ff>This item was spawned by command " + Utils.PrettifyTime((int)eventTime-480) + " into the game.</color>";

		internal SpawnedItemEvent(TechType tt, double time) : base(time) {
			itemType = tt;
		}

		public override void saveToXML(XmlElement e) {
			e.AddProperty("item", itemType.AsString());
			if (!string.IsNullOrEmpty(objectID)) {
				e.AddProperty("class", classID);
				e.AddProperty("object", objectID);
			}
		}

		public void setObject(PrefabIdentifier pi) {
			var s = new SpawnTagCallback(this, pi);
			instance.callbacks.Add(s);
			SNUtil.Log("Registered callback for spawn event " + this, SNUtil.DiDLL);
		}

		internal void setObject(XmlElement s) {
			if (s.HasProperty("object")) {
				classID = s.GetProperty("class");
				objectID = s.GetProperty("object");
			}
		}

		internal void attach(PrefabIdentifier prefab) {
			classID = prefab.ClassId;
			objectID = prefab.Id;
			instance.spawnedIDs[objectID] = this;
		}

		internal static SpawnedItemEvent parseLegacy(string s) {
			var parts = s.Split(',');
			if (parts.Length != 2) {
				SNUtil.Log("Error parsing legacy item spawn event '" + s + "'");
				return null;
			}
			var e = new SpawnedItemEvent(SNUtil.GetTechType(parts[0]), int.Parse(parts[1]));
			if (parts.Length >= 4) {
				e.classID = parts[2];
				e.objectID = parts[3];
			}
			return e;
		}

		public override string ToString() {
			return
				$"[SpawnedItemEvent ItemType={itemType}, SpawnTime={eventTime}, ClassID={classID}, ObjectID={objectID}]";
		}


	}

}