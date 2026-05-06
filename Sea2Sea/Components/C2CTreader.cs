using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CTreader : MonoBehaviour {

	private readonly List<DeepStalkerTag> stalkers = [];

	private void Start() {
		InvokeRepeating(nameof(tick), 0f, 1);
	}

	private void Update() {

	}

	private void OnKill() {
		this.destroy(false);
	}

	private void OnDisable() {
		CancelInvoke(nameof(tick));
	}

	internal void attachStalker(DeepStalkerTag s) {
		if (!stalkers.Contains(s))
			stalkers.Add(s);
	}

	internal void removeStalker(DeepStalkerTag s) {
		stalkers.Remove(s);
	}

	internal void tick() {
		if (C2CHooks.skipTreaderTick)
			return;
		var ep = Player.main;
		if (ep) {
			var dist = Vector3.Distance(ep.transform.position, transform.position+transform.forward*10+transform.up*0);
			if (dist <= 12) {
				var amt = Inventory.main.GetPickupCount(TechType.SeaTreaderPoop);
				if (amt > 0) {
					var df = Mathf.Clamp01(1.5F/dist);
					var chance = Mathf.Clamp(0.25F*amt, 0, 0.8F)*df;
					//SNUtil.writeToChat(dist+" x "+amt+" > "+df+" > "+chance);
					if (chance > 0 && Random.Range(0F, 1F) <= chance) {
						gameObject.GetComponent<SeaTreaderMeleeAttack>().OnAttackTriggerEnter(Player.main.GetComponentInChildren<Collider>());
					}
				}
			}
			if (dist <= 120) {
				var amt = DeepStalkerTag.countDeepStalkersNear(transform);
				//int amt = stalkers.Count;
				for (var i = amt; i < 3; i++) {
					var go = ObjectUtil.createWorldObject(C2CItems.deepStalker.ClassID, true, true);
					go.transform.position = MathUtil.getRandomVectorAround(transform.position, 12).SetY(transform.position.y + 2);
					go.GetComponent<DeepStalkerTag>().bindToTreader(GetComponent<SeaTreader>());
				}
			}
		}
	}

}