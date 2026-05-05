using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class UsableItemRegistry {

	public static readonly UsableItemRegistry instance = new();

	private readonly Dictionary<TechType, Func<Survival, GameObject, bool>> actions = new();

	private float lastUse = -1;

	private UsableItemRegistry() {/*
	    addUsableItem(TechType.Bladderfish, (s, go) => {
		    Player.main.GetComponent<OxygenManager>().AddOxygen(15f);
	        return true;
		});*/
		addUsableItem(TechType.FirstAidKit, (s, go) => {
			return Player.main.GetComponent<LiveMixin>().AddHealth(50f) > 0.1f;
		});
		addUsableItem(TechType.EnzymeCureBall, (s, go) => {
			Debug.LogWarningFormat(s, "Code should be unreachable for the time being.", Array.Empty<object>());
			var component2 = Utils.GetLocalPlayer().gameObject.GetComponent<InfectedMixin>();
			if (component2.IsInfected()) {
				component2.RemoveInfection();
				Utils.PlayFMODAsset(s.curedSound, s.transform, 20f);
				return true;
			}
			return false;
		});
	}

	public void addUsableItem(TechType item, Func<Survival, GameObject, bool> onUse) {
		actions[item] = onUse;
	}

	public bool isUsable(TechType tt) {
		return actions.ContainsKey(tt);
	}

	public bool use(TechType tt, Survival s, GameObject go) {
		if (DayNightCycle.main.timePassedAsFloat - lastUse < 0.5) {
			SNUtil.writeToChat("Prevented duplicate use of item " + tt);
			return false;
		}
		lastUse = DayNightCycle.main.timePassedAsFloat;
		var ret = actions[tt].Invoke(s, go);
		if (ret) {
			ConsumableTracker.instance.onConsume(go, false);
			Inventory.main.container.RemoveItem(go.GetComponent<Pickupable>(), true);
		}
		return ret;
	}
}