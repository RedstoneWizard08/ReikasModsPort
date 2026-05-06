using System;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class C2CLifepod : MonoBehaviour {

	private Simplex1DGenerator pathVariation;

	private EscapePod pod;
	private Rigidbody body;
	private Stabilizer stabilizer;
	private WorldForces forces;
	private LiveMixin live;

	private Vector3 rotationSpeed;

	private Vector3 lastPosition;
	private MovingAverage movementSpeedXZ = new(90);

	private BiomeBase currentBiome;

	private static readonly float MAX_ROTATE_SPEED = 2F;

	private void FixedUpdate() {
		if (C2CHooks.skipPodTick)
			return;
		if (!pod)
			pod = GetComponent<EscapePod>();
		if (!body)
			body = GetComponent<Rigidbody>();
		if (!stabilizer)
			stabilizer = GetComponent<Stabilizer>();
		if (!forces)
			forces = GetComponent<WorldForces>();
		if (!live)
			live = GetComponent<LiveMixin>();
		if (body && SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.PODFAIL)) {
			currentBiome = BiomeBase.GetBiome(transform.position.SetY(Mathf.Min(-3, transform.position.y - 10)));
			var sp = getMovementSpeed();
			var dT = Time.fixedDeltaTime;
			if (pathVariation == null)
				getOrCreateNoise();
			if (sp > 0) {
				body.constraints = RigidbodyConstraints.None;
				body.drag = 0;
				body.angularDrag = 0;
				stabilizer.enabled = false;
				forces.enabled = false;
				var sp2 = sp < 1 ? sp*sp : sp;
				if (currentBiome == VanillaBiomes.Dunes) {
					sp2 *= 2F;
				}
				else if (currentBiome == VanillaBiomes.Sparse || currentBiome == VanillaBiomes.Grandreef || currentBiome == VanillaBiomes.Treader) {
					sp2 *= 1.5F;
				}
				else if (currentBiome == VanillaBiomes.Mushroom) {
					sp2 *= 1.25F;
				}
				var force = 0.2F*new Vector3(-1F, 0, 0.25F+1.8F*(float)pathVariation.getValue(new Vector3(DayNightCycle.main.timePassedAsFloat, 0, 0)))*sp2;
				//SNUtil.writeToChat(sp.ToString("0.000")+">"+force.ToString("F4"));
				body.velocity = force;

				live.TakeDamage(2 * sp * dT, transform.position, DamageType.Normal);

				if (transform.position.y < -5) {
					body.transform.Rotate(rotationSpeed * dT, Space.Self);
					rotationSpeed += new Vector3(UnityEngine.Random.Range(-0.15F, 0.15F), UnityEngine.Random.Range(-0.15F, 0.15F), UnityEngine.Random.Range(-0.15F, 0.15F));
					rotationSpeed.x = Mathf.Clamp(rotationSpeed.x, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
					rotationSpeed.y = Mathf.Clamp(rotationSpeed.y, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
					rotationSpeed.z = Mathf.Clamp(rotationSpeed.z, -MAX_ROTATE_SPEED, MAX_ROTATE_SPEED);
				}
			}
			var depth = -transform.position.y;
			var tgt = getTargetDepth();
			//SNUtil.writeToChat(depth.ToString("000.0")+"/"+tgt.ToString("000.0"));
			if (tgt > 0.1F && tgt > depth) {
				var sink = 0.25F;
				if (tgt - depth > 80 || tgt >= 150)
					sink = 0.75F;
				else if (tgt - depth > 40 || tgt >= 80)
					sink = 0.5F;
				if (currentBiome == VanillaBiomes.Void)
					sink = 2.5F;
				if (currentBiome == VanillaBiomes.Dunes)
					sink = 1F;
				body.velocity = body.velocity.SetY(-sink);
				//SNUtil.writeToChat(body.velocity.ToString("F4"));
			}
			else if (depth > 10 && tgt < depth) {
				var rise = 0.2F;
				if (isStuck())
					rise = 1.5F * (float)Math.Max(0.1, 1 - movementSpeedXZ.getAverage() * 100);
				body.velocity = body.velocity.SetY(rise);
			}
		}
		if (transform.position.y < -900) {
			var delay = 0F;
			if (Player.main.currentEscapePod == pod) {
				//delay = 2.5F;
				SoundManager.playSoundAt(SoundManager.buildSound("event:/sub/cyclops/explode"), transform.position);
				Player.main.liveMixin.Kill(DamageType.Explosive);
			}
			gameObject.destroy(false, delay);
		}
		movementSpeedXZ.addValue(Vector3.Distance(transform.position.SetY(lastPosition.y), lastPosition));
		//SNUtil.writeToChat(movementSpeedXZ.getAverage().ToString("00.0000"));
		lastPosition = transform.position;
	}

	private void getOrCreateNoise() {
		var use = SaveLoadManager.main.firstStart;
		if (pathVariation == null || pathVariation.seed != use) {
			pathVariation = (Simplex1DGenerator)new Simplex1DGenerator(use).setFrequency(0.004F);
		}
	}

	private float getPassedDays() {
		if (!Story.StoryGoalManager.main.completedGoals.Contains("Goal_Builder"))
			return 0;
		var time = DayNightCycle.main.timePassedAsFloat-0.4F;
		//SNUtil.writeToChat((time*10/DayNightCycle.kDayLengthSeconds).ToString("00.00"));
		return time / DayNightCycle.kDayLengthSeconds - 3; //subtract 3 days as that is the assumed time to build a builder tool
	}

	private float getMovementSpeed() {
		var days = getPassedDays();
		return days < 3 ? 0 : days < 8 ? 1 : days < 20 ? 1 + 1.5F * (days - 8) / 12F : Mathf.Min(5F, 2.5F + (days - 20) * 0.5F);
	}

	private float getTargetDepth() {
		var days = getPassedDays();
		if (days < 20)
			return 0;
		days -= 20;
		if (isStuck())
			return 0;
		if (currentBiome == VanillaBiomes.Shallows)
			return Mathf.Min(1, days / 15F) * 10;
		if (currentBiome == VanillaBiomes.Kelp)
			return Mathf.Min(1, days / 10F) * 36;
		return currentBiome == VanillaBiomes.Redgrass
			? Mathf.Min(1, days / 10F) * 80
			: currentBiome == VanillaBiomes.Bloodkelp
				? Mathf.Min(1, days / 8F) * 120
				: currentBiome == VanillaBiomes.Mushroom
					? Mathf.Min(1, days / 6F) * 150
					: currentBiome == VanillaBiomes.Sparse
						? Mathf.Min(1, days / 6F) * 180
						: currentBiome == VanillaBiomes.Dunes || currentBiome == VanillaBiomes.Sparse || currentBiome == VanillaBiomes.Grandreef || currentBiome == VanillaBiomes.Treader
							? Mathf.Min(1, days / 4F) * 220
							: currentBiome == VanillaBiomes.Void ? 9999 : -transform.position.y;
	}

	private bool isStuck() {
		return movementSpeedXZ.getAverage() <= 0.01F;
	}

}