using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class SanctuaryJellyray : RetexturedFish {
    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal SanctuaryJellyray(XMLLocale.LocaleEntry e) : base(e, VanillaCreatures.JELLYRAY.prefab) {
        locale = e;
        glowIntensity = 0.5F;

        ScanTime = 5;
        EggBase = TechType.Jellyray;
        EggMaturationTime = 3600;
        //eggSpawnRate = 0.25F;
        //eggSpawns.Add(BiomeType.GrandReef_TreaderPath);
    }

    public override void prepareGameObject(GameObject world, Renderer[] r0) {
        var kc = world.EnsureComponent<PurpleJellyrayTag>();
        foreach (var r in r0) {
            r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
            RenderUtil.disableTransparency(r.materials[0]);
            r.materials[0].SetFloat("_EmissionLM", 0.1F);
            r.materials[0].SetFloat("_EmissionLMNight", 0.1F);
        }
    }

    public override BehaviourType GetBehavior() {
        return BehaviourType.MediumFish;
    }
}

internal class PurpleJellyrayTag : MonoBehaviour {
    private Renderer[] renders;

    private float lastEyeFlameCheckTime = -1;
    private float lastEyeFlameEatTime = DayNightCycle.main.timePassedAsFloat - Random.Range(0, 1200);

    private SanctuaryPlantTag currentTarget;

    private SwimRandom swimmer;

    private void Update() {
        if (renders == null)
            renders = GetComponentsInChildren<Renderer>();
        if (!swimmer)
            swimmer = GetComponentInChildren<SwimRandom>();

        var time = DayNightCycle.main.timePassedAsFloat;
        if (time - lastEyeFlameCheckTime >= 10 && time - lastEyeFlameEatTime >= 600 &&
            !GetComponent<WaterParkCreature>()) { //10 min each
            lastEyeFlameCheckTime = time;
            WorldUtil.getObjectsNear(
                transform.position,
                180,
                tryTarget,
                go => go.GetComponent<SanctuaryPlantTag>()
            );
        }

        if (currentTarget && swimmer) {
            var dist = (currentTarget.transform.position - transform.position).sqrMagnitude;
            if (dist < 5) {
                if (currentTarget.tryHarvest()) { //does not spawn item
                    lastEyeFlameEatTime = time;
                    SoundManager.playSoundAt(
                        SoundManager.buildSound(TechData.GetSoundPickup(TechType.SeaTreaderPoop)),
                        transform.position
                    );
                    var jr = GetComponent<Jellyray>();
                    jr.Hunger.Value = 0;
                    jr.Happy.Add(0.5F);
                    currentTarget = null;
                }
            } else {
                swimmer.swimBehaviour.SwimTo(currentTarget.transform.position, 2.5F);
            }
        }
    }

    private bool tryTarget(SanctuaryPlantTag sp) {
        if (!currentTarget || (sp.transform.position - transform.position).sqrMagnitude <
            (currentTarget.transform.position - transform.position).sqrMagnitude)
            currentTarget = sp;
        return false;
    }
}