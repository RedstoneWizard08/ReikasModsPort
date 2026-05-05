using ReikaKalseki.DIAlterra;

using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CReaper : MonoBehaviour {

	private SphereCollider mouthObject;
	private ReaperLeviathan creature;
	private ReaperMeleeAttack attack;
	private AggressiveWhenSeeTarget[] aggression;
	private SwimBehaviour swim;

	private static readonly float VANILLA_RADIUS = 4.7F;
	private static readonly float WIDE_RADIUS = 7.5F;

	public float increasedAggroFactor;

	private void Start() {
		mouthObject = gameObject.getChildObject("reaper_leviathan/root/neck/head/mouth_damage_trigger").GetComponent<SphereCollider>();
		creature = GetComponent<ReaperLeviathan>();
		attack = GetComponent<ReaperMeleeAttack>();
		swim = GetComponent<SwimBehaviour>();
		aggression = GetComponents<AggressiveWhenSeeTarget>();
	}

	private void Update() {
		if (increasedAggroFactor > 0) {
			mouthObject.radius = Mathf.Lerp(VANILLA_RADIUS, WIDE_RADIUS, increasedAggroFactor);
			increasedAggroFactor = Mathf.Clamp01(increasedAggroFactor - Time.deltaTime * 0.25F);
			if (increasedAggroFactor > 0.8F) {
				creature.Aggression.Add(0.75F);
				var v = Player.main.GetVehicle();
				target(v ? v.gameObject : Player.main.gameObject);
			}
		}
		else {
			mouthObject.radius = VANILLA_RADIUS;
		}
	}

	internal void forceAggression(GameObject hit, Vector3 pos) {
		if ((transform.position - pos).sqrMagnitude >= 1600)
			swim.SwimTo(pos, 20);
		creature.Aggression.Add(0.75F);
		creature.leashPosition = pos;
		target(hit);
		increasedAggroFactor = 1;
	}

	private void target(GameObject hit) {
		attack.lastTarget.SetTarget(hit);
		foreach (var a in aggression)
			a.lastTarget.SetTarget(hit);
	}
}