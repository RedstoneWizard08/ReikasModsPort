using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class CustomBiome : BiomeBase {

	private static float nextMusicChoiceTime = -1;
	private static VanillaMusic currentMusic;
	private static CustomBiome currentBiomeForMusic;

	public readonly string biomeName;
	private readonly System.Reflection.Assembly ownerMod;

	public Story.BiomeGoal discoveryGoal { get; private set; }

	protected CustomBiome(string name, float deco) : base(name, deco, name) {
		biomeName = name;
		ownerMod = SNUtil.tryGetModDLL();
	}

	public abstract double getDistanceToBiome(Vector3 vec);

	public abstract void register();

	public void onLoad() {

	}

	public void createDiscoveryStoryGoal(float minStayTime, XMLLocale.LocaleEntry e) {
		if (e.pda == "#NULL")
			SNUtil.log("Error - XML entry '" + e.key + "' is missing a sound field: " + e.dump());
		createDiscoveryStoryGoal(minStayTime, e.desc, SoundManager.registerPDASound(SNUtil.tryGetModDLL(), "BiomeGoal_" + biomeName, e.pda).asset);
	}

	public void createDiscoveryStoryGoal(float minStayTime, string text, FMODAsset sound) {
		discoveryGoal = new Story.BiomeGoal {
			key = "Goal_Biome" + biomeName,
			goalType = Story.GoalType.PDA,
			delay = 0,
			biome = biomeName,
			minStayDuration = minStayTime,
		};
		SNUtil.addVOLine(discoveryGoal, text, sound);
		StoryHandler.instance.registerTickedGoal(discoveryGoal);
		SNUtil.log("Created BiomeGoal for " + this);
	}

	public virtual float getNextMusicSilencePadding() {
		return 0;
	}

	public abstract mset.Sky getSky();

	public abstract VanillaMusic[] getMusicOptions();

	public virtual float getMurkiness(float orig) {
		return orig;
	}

	public virtual float getScatteringFactor(float orig) {
		return orig;
	}

	public virtual Vector3 getColorFalloff(Vector3 orig) {
		return orig;
	}

	public virtual float getFogStart(float orig) {
		return orig;
	}

	public virtual float getScatterFactor(float orig) {
		return orig;
	}

	public virtual Color getWaterColor(Color orig) {
		return orig;
	}

	public virtual float getSunScale(float orig) {
		return orig;
	}

	public virtual Vector4 getEmissiveVector(Vector4 orig) {
		return orig;
	}

	public static void tickMusic(DayNightCycle cyc) {
		if (cyc.timePassedAsFloat >= nextMusicChoiceTime) {
			var ep = Player.main;
			if (ep) {
				var pos = ep.transform.position;
				var b = GetBiome(pos) as CustomBiome;
				var changed = b != currentBiomeForMusic;
				currentBiomeForMusic = b;
				if (changed) {
					if (currentMusic != null)
						currentMusic.stop();
					nextMusicChoiceTime = cyc.timePassedAsFloat + 1;
					return;
				}
				if (b != null) {
					var mus = b.getMusicOptions();
					//SNUtil.writeToChat(b.biomeName+" > "+(mus != null ? string.Join(",", (object[])mus) : "null"));
					if (mus != null) {
						foreach (var vm in VanillaMusic.getAll()) {
							vm.stop();
						}
						if (mus.Length > 0) {/*
							foreach (VanillaMusic vm in VanillaMusic.getAll()) {
								vm.disable();
							}
							foreach (VanillaMusic vm in mus) {
								vm.enable();
								vm.setToBiome(biome);
							}*/
							var track = mus[Random.Range(0, mus.Length)];
							track.play();
							nextMusicChoiceTime = cyc.timePassedAsFloat + track.getLength() + b.getNextMusicSilencePadding();
							currentMusic = track;
							//SNUtil.writeToChat("Selected play of track "+track+" @ "+cyc.timePassedAsFloat+" > "+nextMusicChoiceTime);
						}
					}
				}
				else {/*
					foreach (VanillaMusic vm in VanillaMusic.getAll()) {
						vm.reset();
					}*/
					nextMusicChoiceTime = cyc.timePassedAsFloat + 1;
				}
			}
		}
	}
}