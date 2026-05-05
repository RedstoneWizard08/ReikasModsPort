using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CVoyager : MonoBehaviour {

	private static readonly float MIN_VOID_TIME = 10;
	private static readonly float STRONG_VOID_TIME = 20;

	private Rigidbody body;
	private LiveMixin live;
	private PowerRelay power;

	private float voidTime;

	private void Update() {
		if (!body)
			body = GetComponent<Rigidbody>();
		if (!live)
			live = GetComponent<LiveMixin>();
		if (!power)
			power = GetComponent<PowerRelay>();

		if (shouldSink())
			voidTime += Time.deltaTime;
		else
			voidTime = 0;

		if (voidTime >= MIN_VOID_TIME) {
			var f2 = (float)MathUtil.linterpolate(voidTime-STRONG_VOID_TIME, 0, 10, 0, 0.5F, true);
			var f = (float)MathUtil.linterpolate(voidTime-MIN_VOID_TIME, 0, STRONG_VOID_TIME-MIN_VOID_TIME, 0, 50, true);
			if (voidTime < STRONG_VOID_TIME)
				f *= (float)MathUtil.linterpolate(-transform.position.y, 0, 25, 1, f2, true);
			body.AddForce(Vector3.down * Time.deltaTime * f, ForceMode.VelocityChange);
		}
	}

	private void Start() {
		InvokeRepeating(nameof(tick), 0f, 0.5F);
		InvokeRepeating(nameof(slowTick), 0f, 5F);

		foreach (var c in GetComponentsInChildren<Constructable>()) {
			c.gameObject.destroy(false);
		}

		Renderer decals = gameObject.getChildObject("Model/Exterior/Decals").GetComponent<MeshRenderer>();
		var decal = decals.materials[4];
		decal.SetTexture("_MainTex", TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/SeaVoyagerDecal_MainTex"));
		decal.SetTexture("_SpecTex", TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/SeaVoyagerDecal_MainTex"));
		decal.SetTexture("_EmissionMap", TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/SeaVoyagerDecal_Illum"));
		decal.SetTexture("_Illum", TextureManager.getTexture(SeaToSeaMod.ModDLL, "Textures/SeaVoyagerDecal_Illum"));
	}

	private void OnKill() {
		this.destroy(false);
	}

	private void OnDisable() {
		CancelInvoke(nameof(tick));
		CancelInvoke(nameof(slowTick));
	}

	internal void tick() {
		if (voidTime >= MIN_VOID_TIME) {
			var sunk = transform.position.y < -20;
			if (live)
				live.TakeDamage(sunk ? 50 : 10);
			if (power)
				power.ConsumeEnergy(sunk ? 5 : 2, out var trash);
		}
	}

	internal void slowTick() {
		Ecocean.ECHooks.attractToSoundPing(this, true, 1);
	}

	private bool shouldSink() {
		return VanillaBiomes.Void.IsInBiome(transform.position.setY(-5)) || transform.position.y < -16; //once sunk stay sunk
	}

}