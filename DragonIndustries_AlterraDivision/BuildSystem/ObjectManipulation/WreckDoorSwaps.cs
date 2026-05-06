/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class WreckDoorSwaps : ManipulationBase {

	private static readonly string DOOR_FRAME = "055b3160-f57b-46ba-80f5-b708d0c8180e";

	private List<DoorSwap> swaps = [];

	public override void applyToObject(GameObject go) {
		var sw = go.EnsureComponent<WreckDoorSwapper>();
		sw.swaps = swaps;
		sw.Invoke(nameof(WreckDoorSwapper.applyDelayed), 2);
		SNUtil.Log("Queuing door swaps " + swaps.ToDebugString("\n")+" on wreck "+go.name+" @ "+go.transform.position);
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		swaps.Clear();
		foreach (var e2 in e.GetDirectElementsByTagName("door")) {
			var d = new DoorSwap(e2.GetVector("position").Value, e2.GetProperty("type"));
			swaps.Add(d);
		}
	}

	public override void saveToXML(XmlElement e) {
		foreach (var d in swaps) {
			var e2 = e.OwnerDocument.CreateElement("door");
			e2.AddProperty("position", d.position);
			e2.AddProperty("type", d.doorType);
			e.AppendChild(e2);
		}
	}

	public static void setupRepairableDoor(GameObject panel) {
		var weld = panel.EnsureComponent<WeldableWallPanelGeneric>();
		var lv = weld.GetComponentInChildren<LiveMixin>();
		lv.data.canResurrect = true;
	}

	public class DoorSwap {

		public readonly Vector3 position;
		public readonly string doorType;

		internal static readonly Dictionary<string, string> doorPrefabs = new() {
			{"Blocked", "d79ab37f-23b6-42b9-958c-9a1f4fc64cfd"},
			{"Handle", "d9524ffa-11cf-4265-9f61-da6f0fe84a3f"},
			{"Laser", "6f01d2df-03b8-411f-808f-b3f0f37b0d5c"},
			{"Repair", "b86d345e-0517-4f6e-bea4-2c5b40f623b4"},
			{"Openable", "b86d345e-0517-4f6e-bea4-2c5b40f623b4"},
			{"Delete", "b86d345e-0517-4f6e-bea4-2c5b40f623b4"},
		};

		internal static readonly HashSet<string> doorPrefabIDs = [..doorPrefabs.Values];

		public DoorSwap(Vector3 pos, string t) {
			position = pos;
			doorType = t;
		}

		public void applyTo(GameObject go) {
			SNUtil.Log("Matched to door "+go.transform.position+", converted to "+doorType, SNUtil.DiDLL);
			var par = go.transform.parent;
			var put = ObjectUtil.createWorldObject(doorPrefabs[doorType], true, true);
			if (put == null) {
				SNUtil.WriteToChat("Could not find prefab for door type " + doorType);
				return;
			}
			put.transform.position = go.transform.position;
			put.transform.rotation = go.transform.rotation;
			put.transform.parent = par;
			go.destroy();
			var d = put.GetComponent<StarshipDoor>();
			if (d) {
				if (doorType == "Delete") {
					put.removeChildObject("Starship_doors_manual_01/Starship_doors_automatic");
				}
				else if (doorType == "Openable") {
					d.UnlockDoor();
				}
				else if (doorType == "Repair") {
					d.LockDoor();
					var panel = ObjectUtil.createWorldObject("bb16d2bf-bc85-4bfa-a90e-ddc7343b0ac2", true, true);
					panel.transform.position = put.transform.position;
					panel.transform.rotation = put.transform.rotation;
					setupRepairableDoor(panel);
				}
			}
		}

		public override string ToString() {
			return $"[DoorSwap @ {position}, type={doorType}]";
		}



	}

	public static bool areWreckDoorSwapsPending(GameObject go) {
		var wr = go.GetComponent<WreckDoorSwapper>();
		return wr && wr.swaps.Count > 0;
	}

	private class WreckDoorSwapper : MonoBehaviour {
			
		internal List<DoorSwap> swaps = [];

		private void doSimpleSearch(GameObject doors, List<DoorSwap> unfound) {
			foreach (var d in swaps) {
				var found = false;
				if (doors) {
					foreach (Transform t in doors.transform) {
						if (!t || !t.gameObject)
							continue;
						var pos = t.position;
						//SNUtil.log("Checking door "+t.position);
						if (Vector3.Distance(d.position, pos) <= 0.5) {
							found = true;
							d.applyTo(t.gameObject);
						}
					}
				}
				if (!found) {
					unfound.Add(d);
				}
			}
		}

		private void doPrefabSearch(List<DoorSwap> unfound) {
			foreach (var pi in GetComponentsInChildren<PrefabIdentifier>(true)) {
				if (pi && (pi.ClassId == DOOR_FRAME || DoorSwap.doorPrefabIDs.Contains(pi.ClassId))) {
					try {
						for (var i = unfound.Count - 1; i >= 0; i--) {
							var d = unfound[i];
							if (Vector3.Distance(d.position, pi.transform.position) <= 0.5) {
								d.applyTo(pi.gameObject);
								unfound.RemoveAt(i);
								break;
							}
						}
						if (unfound.Count == 0)
							break;
					}
					catch (Exception e) {
						SNUtil.Log("Threw exception processing PI '"+pi+"': " + e);
						throw e;
					}
				}
			}
		}

		private void printCandidates(GameObject doors, List<DoorSwap> unfound) {
			var has = "Door candidates:{\n";
			if (doors) {
				foreach (Transform t in doors.transform) {
					if (t) {
						has += "[DOOR] "+t.name + " @ " + t.transform.position + "\n";
					}
				}
			}
			foreach (var pi in GetComponentsInChildren<PrefabIdentifier>()) {
				if (pi && (pi.ClassId == DOOR_FRAME || DoorSwap.doorPrefabIDs.Contains(pi.ClassId))) {
					has += "[PREFAB] "+pi.name + " [" + pi.ClassId + "] @ " + pi.transform.position + "\n";
				}
			}
			SNUtil.Log(has, SNUtil.DiDLL);
			SNUtil.Log("}\nTrying again in 2s", SNUtil.DiDLL);
		}

		public void applyDelayed() {
			var doors = gameObject.getChildObject("Doors");
			List<DoorSwap> unfound = [];
			try {
				doSimpleSearch(doors, unfound);
			}
			catch (Exception e) {
				SNUtil.Log("Threw exception doing simple search: " + e);
			}
			if (unfound.Count > 0) {
				SNUtil.Log("Some door swaps in wreck @ " + transform.position + " found no easy match, checking all PIs\n" + unfound.ToDebugString("\n"), SNUtil.DiDLL);
				try {
					doPrefabSearch(unfound);
				}
				catch (Exception e) {
					SNUtil.Log("Threw exception processing PIs: "+e);
				}
			}
			if (unfound.Count > 0) {
				SNUtil.Log("Some door swaps (" + unfound.Count + "/" + swaps.Count + ") for " + gameObject.name + " @ " + transform.position + " found no match!!\n" + unfound.ToDebugString("\n"), SNUtil.DiDLL);
				try {
					printCandidates(doors, unfound);
				}
				catch (Exception e) {
					SNUtil.Log("Threw exception printing candidates: " + e);
				}
				Invoke(nameof(applyDelayed), 2);
				swaps = unfound;
			}
			else {
				SNUtil.Log("Door swaps completed in "+ gameObject.name + " @ " + transform.position);
				this.destroy();
			}
		}
	}

}