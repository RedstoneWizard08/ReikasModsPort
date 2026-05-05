using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class FlickeringLight : MonoBehaviour {

	public float dutyCycle = 0.5F;
	public float updateRate = 0.5F;

	//takes 1/N seconds to fade
	public float fadeRate = 99999;

	public float maxIntensity = -1;
	public float minIntensity;

	public float currentIntensity { get; private set; }

	private float targetIntensity;

	private Light light;

	private float lastUpdate = -1;

	private void Update() {
		if (!light)
			light = GetComponent<Light>();
		if (!light)
			return;
		if (maxIntensity < 0)
			maxIntensity = light.intensity;
		var time = DayNightCycle.main.timePassedAsFloat;
		var dT = Time.deltaTime;
		if (currentIntensity > targetIntensity) {
			currentIntensity = Mathf.Max(targetIntensity, currentIntensity - dT * fadeRate);
		}
		else if (currentIntensity < targetIntensity) {
			currentIntensity = Mathf.Min(targetIntensity, currentIntensity + dT * fadeRate);
		}
		light.intensity = currentIntensity;
		if (time - lastUpdate >= updateRate) {
			targetIntensity = Random.Range(0F, 1F) <= dutyCycle ? maxIntensity : minIntensity;
			lastUpdate = time;
		}
	}

}