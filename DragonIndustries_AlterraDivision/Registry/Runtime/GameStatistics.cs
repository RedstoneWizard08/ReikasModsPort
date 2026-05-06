using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class GameStatistics {
	//TODO add cheat commands, etc

	private List<SubRoot> bases = [];
	private List<Vehicle> vehicles = [];
	private List<SubRoot> cyclops = [];

	private XmlDocument xmlDoc;

	private GameStatistics() {

	}

	public static GameStatistics collect() {
		var gs = new GameStatistics();
		gs.populate();
		return gs;
	}

	private void populate() {
		cyclops = [];
		bases = [];
		//hasSeamoth = GameInfoIcon.Has(TechType.Seamoth);
		//hasCyclops = GameInfoIcon.Has(TechType.Cyclops);
		//hasPrawn = GameInfoIcon.Has(TechType.Exosuit);
		foreach (var s in Object.FindObjectsOfType<Vehicle>()) {
			vehicles.Add(s);
		}
		foreach (var s in Object.FindObjectsOfType<SubRoot>()) {
			if (s.isBase)
				bases.Add(s);
			else
			if (s.isCyclops)
				cyclops.Add(s);
		}

		bases.Sort((b1, b2) => b2.transform.position.y.CompareTo(b1.transform.position.y));

		xmlDoc = new XmlDocument();
		xmlDoc.AppendChild(xmlDoc.CreateElement("Root"));

		var e = xmlDoc.DocumentElement.AddChild("Player");
		e.AddProperty("health", Player.main.liveMixin.health);
		var sv = Player.main.GetComponent<Survival>();
		if (sv) {
			e.AddProperty("food", sv.food);
			e.AddProperty("water", sv.water);
		}
		collectStorage(e.AddProperty("inventory"), Inventory.main.container);
		collectStorage(e.AddProperty("equipment"), Inventory.main.equipment);

		e = xmlDoc.DocumentElement.AddChild("Bases");
		foreach (var sub in bases) {
			var e2 = e.AddChild("Base");
			var pos = sub.transform.position;
			e2.AddProperty("centerX", pos.x);
			e2.AddProperty("centerY", pos.y);
			e2.AddProperty("centerZ", pos.z);
			e2.AddProperty("biome", WorldUtil.getRegionalDescription(pos, false));
			e2.AddProperty("cellSize", sub.GetComponentsInChildren<BaseCell>(true).Length);
			e2.AddProperty("scannerCount", sub.GetComponentsInChildren<MapRoomFunctionality>(true).Length);
			e2.AddProperty("moonpoolCount", sub.GetComponentsInChildren<VehicleDockingBay>(true).Length);
			e2.AddProperty("acuCount", sub.GetComponentsInChildren<WaterPark>(true).Length);
			e2.AddProperty("currentPower", sub.powerRelay.GetPower());
			e2.AddProperty("maxPower", sub.powerRelay.GetMaxPower());
			collectStorage(e2, sub.gameObject);
		}

		e = xmlDoc.DocumentElement.AddChild("Cyclopses");
		foreach (var sub in cyclops) {
			var e2 = e.AddChild("Cyclops");
			foreach (var tt in sub.getCyclopsUpgrades()) {
				e2.AddProperty("module", tt.AsString());
			}
			collectStorage(e2, sub.gameObject);
		}

		e = xmlDoc.DocumentElement.AddChild("Vehicles");
		foreach (var v in vehicles) {
			var e2 = e.AddChild("Vehicle");
			e2.AddProperty("type", getObjectType(v));
			foreach (var tt in v.getVehicleUpgrades()) {
				e2.AddProperty("module", tt.AsString());
			}
		}

		e = xmlDoc.DocumentElement.AddChild("StoryGoals");
		/*
		foreach (string goal in StoryGoalManager.main.completedGoals) {
			XmlElement e2 = e.addChild("Goal");
			e2.addProperty("key", goal);
			e2.addProperty("unlockTime", 0);
		}*/
		StoryHandler.instance.forAllNewerThan(9999999, g => {
			var e2 = e.AddChild("Unlock");
			e2.AddProperty("tech", g.goal);
			e2.AddProperty("unlockTime", g.eventTime);
		});

		e = xmlDoc.DocumentElement.AddChild("TechUnlocks");
		TechUnlockTracker.instance.forAllNewerThan(9999999, u => {
			var e2 = e.AddChild("Unlock");
			e2.AddProperty("tech", u.tech.AsString());
			e2.AddProperty("unlockTime", u.eventTime);
		});

		e = xmlDoc.DocumentElement.AddChild("Cheats");
		{
			var e2 = e.AddChild("SpawnedItems");
			SpawnedItemTracker.instance.forAll(s => {
				var e3 = e2.AddChild("SpawnedItem");
				e3.AddProperty("item", s.itemType.AsString());
				e3.AddProperty("spawnTime", s.eventTime);
			});
			e2 = e.AddChild("Commands");
			CommandTracker.instance.forAll(s => {
				var e3 = e2.AddChild("Command");
				e3.AddProperty("command", s.command);
				e3.AddProperty("runTime", s.eventTime);
			});
		}
	}

	private void collectStorage(XmlElement root, GameObject from) {
		var e = root.AddChild("inventories");
		foreach (var sc in from.GetComponentsInChildren<StorageContainer>(true)) {
			var e3 = e.AddChild("storage");
			e3.AddProperty("type", getObjectType(sc));
			collectStorage(e3, sc.container);
		}
	}

	private void collectStorage(XmlElement e3, Equipment sc) {
		var items = e3.AddChild("items");
		foreach (var kvp in sc.equipment) {
			if (kvp.Value != null) {
				var tt = kvp.Value.item.GetTechType();
				var added = items.AddProperty(kvp.Key, tt.AsString());
				added.SetAttribute("displayName", Language.main.Get(tt));
			}
		}
	}

	private void collectStorage(XmlElement e3, ItemsContainer sc) {
		var counts = new Dictionary<TechType, int>();
		sc.forEach(ii => {
			var tt = ii.item.GetTechType();
			var has = counts.ContainsKey(tt) ? counts[tt] : 0;
			counts[tt] = has + 1;
		});
		if (counts.Count == 0)
			return;
		var items = e3.AddChild("items");
		foreach (var kvp in counts) {
			var added = items.AddProperty(kvp.Key.AsString(), kvp.Value);
			added.SetAttribute("displayName", Language.main.Get(kvp.Key));
		}
	}

	private string getObjectType(Component c) {
		var tt = CraftData.GetTechType(c.gameObject);
		return tt == TechType.None ? c.gameObject.name : tt.AsString();
	}

	public void writeToFile(string file) {
		Directory.CreateDirectory(Path.GetDirectoryName(file));
		xmlDoc.Save(file);
	}

	public void submit() {
		var file = Path.Combine(SNUtil.GetCurrentSaveDir(), "finalStatistics.xml");
		writeToFile(file);
	}

}