using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using Story;

namespace ReikaKalseki.SeaToSea;

public class BiomeDiscoverySystem : IStoryGoalListener {

	public static readonly BiomeDiscoverySystem instance = new();

	//private readonly HashSet<string> biomes = new HashSet<string>();

	private readonly Dictionary<BiomeBase, string> basicEntryGoal = new();
	private readonly Dictionary<BiomeBase, string> exploreGoal = new();
	private readonly Dictionary<string, BiomeBase> goalMap = new();

	private BiomeDiscoverySystem() {
		var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
	}

	public void register() {
		mapBiome(VanillaBiomes.Shallows, "Goal_Lifepod2"); //"Aurora suffered orbital hull failure"
		mapBiome(VanillaBiomes.Kelp, "Goal_BiomeKelpForest");
		mapBiome(VanillaBiomes.Redgrass, "Goal_BiomeGrassyPlateaus");
		mapBiome(VanillaBiomes.Mushroom, "Goal_BiomeMushroomForest");
		mapBiome(VanillaBiomes.Jellyshroom, "Goal_BiomeJellyCave");
		mapBiome(VanillaBiomes.Deepgrand, "Goal_BiomeDeepGrandReef");
		mapBiome(VanillaBiomes.Koosh, "Goal_BiomeKooshZone");
		mapBiome(VanillaBiomes.Dunes, "Goal_BiomeDunes");
		mapBiome(VanillaBiomes.Crash, "Goal_BiomeCrashedShip");
		mapBiome(VanillaBiomes.Sparse, "Goal_BiomeSparseReef");
		mapBiome(VanillaBiomes.Mountains, "Goal_BiomeMountains");
		mapBiome(VanillaBiomes.Treader, "Goal_BiomeSeaTreaderPath");
		mapBiome(VanillaBiomes.Underislands, "Goal_BiomeUnderwaterIslands");
		mapBiome(VanillaBiomes.Bloodkelp, "Goal_BiomeBloodKelp");
		mapBiome(VanillaBiomes.Bloodkelpnorth, "Goal_BiomeBloodKelp2");
		mapBiome(VanillaBiomes.Lostriver, "Goal_BiomeLostRiver");
		mapBiome(VanillaBiomes.Ilz, "ILZChamber_Dragon"); //"energy in structure in center of chamber"
		mapBiome(VanillaBiomes.Alz, "Emperor_Telepathic_Contact3");
		mapBiome(VanillaBiomes.Aurora, "Goal_LocationAuroraEntry");
		mapBiome(VanillaBiomes.Floatisland, "Goal_BiomeFloatingIsland");
		mapBiome(VanillaBiomes.Void, "Goal_BiomeVoid");

		//no triggers in vanilla
		createTrigger(VanillaBiomes.Grandreef, "grandreef");
		createTrigger(VanillaBiomes.Crag, "CragField");
		createTrigger(VanillaBiomes.Cove, "LostRiver_TreeCove");

		StoryHandler.instance.registerTickedGoal(StoryHandler.instance.createLocationGoal(WorldUtil.SUNBEAM_SITE, 1000, "Goal_BiomeMountainIsland", WorldUtil.isMountainIsland));

		foreach (var cb in BiomeBase.GetCustomBiomes()) {
			if (cb.discoveryGoal != null) {
				mapBiome(cb, cb.discoveryGoal.key);
			}
			else {
				createTrigger(cb, cb.biomeName);
			}
		}

		StoryHandler.instance.addListener(this);
	}
	/*
	private void generateBiomeGoalList() {
		/*
		foreach (string biome in biomes) {
			foreach (BiomeGoal bg in BiomeGoalTracker.main.goalData.goals) {
				if (bg.biome == biome) {
					biomeGoals.Add(bg);
					break;
				}
			}
		}*//*

		foreach (BiomeGoal bg in BiomeGoalTracker.main.goalData.goals) {
			if (bg.key.StartsWith("Goal_Biome", StringComparison.InvariantCultureIgnoreCase)) {
				BiomeBase bb = BiomeBase.getBiome(bg.biome);
				if (bb == null) {
					SNUtil.log("Skipping handling of biome goal '"+bg.key+"', unrecognized biome '"+bg.biome+"'");
					continue;
				}
				if (basicEntryGoal.ContainsKey(bb)) {
					SNUtil.log("Multiple biome goals '"+bg.key+"' + '"+basicEntryGoal[bb].key+"', for biome '"+bg.biome+"'");
				}
				basicEntryGoal[bb] = bg;
				goalMap[bg.key] = bb;
			}
		}

		foreach (BiomeBase bb in BiomeBase.getAllBiomes()) {
			if (!basicEntryGoal.ContainsKey(bb)) {
				basicEntryGoal[bb] = new BiomeGoal();
				basicEntryGoal[bb].biome = bb.displayName;
			}
		}
	}*/

	private void mapBiome(BiomeBase bb, string goal) {
		basicEntryGoal[bb] = goal;
		goalMap[goal] = bb;
	}

	private void createTrigger(BiomeBase bb, string id) {
		var bg = new BiomeGoal {
			key = "Goal_Biome" + id,
			biome = id,
			delay = 0,
			goalType = Story.GoalType.Story,
			minStayDuration = 2,
		};
		mapBiome(bb, bg.key);
		StoryHandler.instance.registerTickedGoal(bg);
	}

	public void NotifyGoalComplete(string key) {
		if (goalMap.ContainsKey(key)) {
			LifeformScanningSystem.instance.onBiomeDiscovered();
		}
	}

	public void forceDiscovery(BiomeBase bb) {
		if (bb == null || bb == BiomeBase.Unrecognized)
			return;
		if (!basicEntryGoal.ContainsKey(bb)) {
			SNUtil.log("No cached biome goal to apply for biome " + bb);
			return;
		}
		StoryGoal.Execute(basicEntryGoal[bb], Story.GoalType.Story);
	}

	public bool isDiscovered(BiomeBase bb) {
		if (bb == null || bb == BiomeBase.Unrecognized)
			return true;
		//if (basicEntryGoal.Count == 0) {
		//	generateBiomeGoalList();
		//}
		if (!basicEntryGoal.ContainsKey(bb)) {
			SNUtil.log("No cached biome goal to check for biome " + bb);
			return true;
		}
		return StoryGoalManager.main.IsGoalComplete(basicEntryGoal[bb]);
	}

	public bool visitedAllBiomes() {
		//if (basicEntryGoal.Count == 0) {
		//	generateBiomeGoalList();
		//}
		foreach (var kvp in basicEntryGoal) {
			if (!StoryGoalManager.main.IsGoalComplete(kvp.Value)) {
				SNUtil.writeToChat("Missing biome goal '" + kvp.Value + " for biome " + kvp.Key);
				return false;
			}
		}
		return true;
	}

	public bool checkIfVisitedAllBiomes() {
		return C2CUtil.checkConditionAndShowPDAAndVoicelogIfNot(visitedAllBiomes(), "notvisitedallbiomes", PDAMessages.Messages.NotSeenBiomesMessage);
	}

}