using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class Drunk : PlayerMovementSpeedModifier {

	private static readonly DrunkVisual drunkVisual;

	static Drunk() {
		drunkVisual = new DrunkVisual();
		ScreenFXManager.instance.addOverride(drunkVisual);
	}

	private float nextSpeedRecalculation = -1;
	private float nextPushRecalculation = -1;
	private float nextShaderRecalculation = -1;
	private float lastVomitTime = -1;

	private float age;

	internal Vector3 currentPush;
	private float shaderIntensity;
	private float shaderIntensityTarget;
	private float shaderIntensityMoveSpeed;

	public float intensity = 1;

	internal Survival survivalObject;

	//private Rigidbody player;

	protected override void Update() {
		//if (!player)
		//	player = GetComponent<Rigidbody>();
		var dT = Time.deltaTime;
		age += dT;
		var time = DayNightCycle.main.timePassedAsFloat;
		if (time >= nextSpeedRecalculation) {
			nextSpeedRecalculation = time + Random.Range(0.5F, 2.5F);
			speedModifier = 1 - Random.Range(0.2F, 0.75F) * intensity;
		}
		if (time >= nextPushRecalculation) {
			nextPushRecalculation = time + Random.Range(0.5F, 1.5F);
			currentPush = Random.onUnitSphere * Random.Range(0.25F, 1.0F) * intensity;
			if (!Player.main.IsSwimming())
				currentPush = currentPush.setY(0);
		}
		if (time >= nextShaderRecalculation) {
			var dur = Random.Range(0.25F, 2.0F);
			nextShaderRecalculation = time + dur;
			shaderIntensityTarget = Random.Range(0.33F, 1.5F) * intensity;
			shaderIntensityMoveSpeed = Mathf.Abs(shaderIntensity - shaderIntensityTarget) / dur;
		}
		if (shaderIntensityTarget > shaderIntensity)
			shaderIntensity = Mathf.Min(shaderIntensityTarget, shaderIntensity + dT * shaderIntensityMoveSpeed);
		else if (shaderIntensityTarget < shaderIntensity)
			shaderIntensity = Mathf.Max(shaderIntensityTarget, shaderIntensity - dT * shaderIntensityMoveSpeed);
		drunkVisual.effect = 4 * shaderIntensity;
		//player.AddForce(currentPush, ForceMode.VelocityChange);
		if (Random.Range(0F, 1F) < 0.04F)
			SNUtil.shakeCamera(Random.Range(0.4F, 1.5F), Random.Range(0.25F, 0.75F), Random.Range(0.125F, 0.67F));
		if (age > 5F && time - lastVomitTime >= 5F / intensity && Random.Range(0F, 1F) < 0.001F) {
			lastVomitTime = time;
			SNUtil.vomit(survivalObject, 0, Random.Range(0F, 2F));
		}
		base.Update();
	}

	private void OnDisable() {
		drunkVisual.effect = 0;
	}

	private void OnDestroy() {
		OnDisable();
	}

	public override void saveToXML(XmlElement e) {
		base.saveToXML(e);
		e.addProperty("intensity", intensity);
	}

	public override void readFromXML(XmlElement e) {
		base.readFromXML(e);
		intensity = (float)e.getFloat("intensity", 0);
	}

	public static Drunk add(float duration) {
		var m = Player.main.gameObject.EnsureComponent<Drunk>();
		m.speedModifier = 1;
		m.elapseWhen = DayNightCycle.main.timePassedAsFloat + duration;
		return m;
	}

	internal static void manageDrunkenness(DIHooks.PlayerInput pi) {
		var d = Player.main.GetComponent<Drunk>();
		if (d)
			pi.SelectedInput += d.currentPush;
	}

}

internal class DrunkVisual : ScreenFXManager.ScreenFXOverride {

	internal float effect;

	internal DrunkVisual() : base(100) {

	}

	public override void onTick() {
		if (effect > 0) {
			ScreenFXManager.instance.registerOverrideThisTick(ScreenFXManager.instance.radialShader);
			ScreenFXManager.instance.radialShader.amount = effect;
		}
	}

}