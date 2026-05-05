using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class VanillaBiomes : BiomeBase {
    public static readonly VanillaBiomes Shallows = new(0, "Safe Shallows", 0.25F, "safe", "safeShallows");
    public static readonly VanillaBiomes Kelp = new(50, "Kelp Forest", 0.4F, "kelp", "kelpForest");

    public static readonly VanillaBiomes Redgrass = new(
        100,
        "Grassy Plateaus",
        0.33F,
        "grassy",
        "grassyPlateaus",
        "GrassyPlateaus_Tower"
    );

    public static readonly VanillaBiomes Mushroom = new(
        150,
        "Mushroom Forest",
        0.67F,
        "mushroom",
        "mushroomForest",
        "tree"
    );

    public static readonly VanillaBiomes Jellyshroom = new(
        250,
        "Jellyshroom Caves",
        1F,
        "jellyshroom",
        "JellyshroomCaves"
    );

    public static readonly VanillaBiomes Grandreef = new(300, "Grand Reef", 0.75F, "grandReef", "smokers");
    public static readonly VanillaBiomes Deepgrand = new(500, "Deep Grand Reef", 0.75F, "deepgrand", "deepGrandReef");
    public static readonly VanillaBiomes Koosh = new(300, "Bulb Zone", 0.33F, "koosh", "kooshZone");
    public static readonly VanillaBiomes Dunes = new(350, "Dunes", 0.25F, "dunes", "Dunes_ThermalVents");

    public static readonly VanillaBiomes Crash = new(
        200,
        "Crash Zone",
        -0.5F,
        "crash",
        "crashZone",
        "crashZone_Mesa",
        "CrashZone_Trench",
        "CrashZone_NoLoot"
    );

    public static readonly VanillaBiomes Crag = new(200, "Crag Field", 0.25F, "crag", "cragField");

    public static readonly VanillaBiomes Sparse = new(
        200,
        "Sparse Reef",
        0F,
        "sparse",
        "sparseReef",
        "sparseReef_Deep",
        "sparseReef_spike"
    );

    public static readonly VanillaBiomes Mountains = new(350, "Mountains", 0F, "mountains");
    public static readonly VanillaBiomes Treader = new(300, "Sea Treader's Path", 0.1F, "seaTreaderPath");

    public static readonly VanillaBiomes Underislands = new(
        200,
        "Underwater Islands",
        0.33F,
        "Underwaterislands",
        "UnderwaterIslands_ValleyFloor",
        "Underwaterislands_Island",
        "Underwaterislands_IslandCave"
    );

    public static readonly VanillaBiomes Bloodkelp = new(
        400,
        "Blood Kelp Trench",
        0.4F,
        "bloodkelp",
        "bloodkelp_trench",
        "bloodkelp_deeptrench"
    );

    public static readonly VanillaBiomes Bloodkelpnorth = new(400, "Northern Blood Kelp", 0.4F, "bloodkelptwo");

    public static readonly VanillaBiomes Lostriver = new(
        700,
        "Lost River",
        0.67F,
        "LostRiver_BonesField_Corridor",
        "LostRiver_BonesField_Corridor_Stream",
        "LostRiver_BonesField",
        "LostRiver_BonesField_Lake",
        "LostRiver_BonesField_LakePit",
        "LostRiver_Corridor",
        "LostRiver_Junction",
        "LostRiver_GhostTree_Lower",
        "LostRiver_GhostTree",
        "LostRiver_Canyon",
        "LostRiver_SkeletonCave",
        "Precursor_LostRiverBase"
    );

    public static readonly VanillaBiomes Cove = new(900, "Tree Cove", 1F, "LostRiver_TreeCove");

    public static readonly VanillaBiomes Ilz = new(
        1200,
        "Inactive Lava Zone",
        0.5F,
        "ILZCorridor",
        "ILZCorridorDeep",
        "ILZChamber",
        "ILZChamber_Dragon",
        "LavaPit",
        "LavaFalls",
        "LavaCastle",
        "ILZCastleTunnel",
        "ilzLava"
    );

    public static readonly VanillaBiomes Alz = new(1400, "Active Lava Zone", 0.4F, "LavaLakes", "LavaLakes_LavaPool");
    public static readonly VanillaBiomes Aurora = new(0, "Aurora", 0F, "crashedShip"); //not a distinct biome
    public static readonly VanillaBiomes Floatisland = new(0, "Floating Island", 0F, "FloatingIsland");

    public static readonly VanillaBiomes
        Mountisland = new(0, "Mountain Island", 0F, "MountainIsland"); //not a distinct biome

    public static readonly VanillaBiomes Void = new(8192, "Crater Edge", 0F, "void" /*, ""*/);

    public readonly float AverageDepth;

    private VanillaBiomes(float dp, string d, float deco, params string[] ids) : base(d, deco, ids) {
        AverageDepth = dp;
    }

    public override bool IsCaveBiome() {
        return this == Alz || this == Ilz || this == Cove || this == Lostriver || this == Jellyshroom ||
               this == Deepgrand;
    }

    public override bool ExistsInSeveralPlaces() {
        return this == Shallows || this == Kelp || this == Redgrass || this == Mushroom;
    }

    public override bool IsVoidBiome() {
        return this == Void;
    }

    public override bool IsInBiome(Vector3 pos) {
        return GetBiome(pos) == this;
    }

    public static int Compare(VanillaBiomes b1, VanillaBiomes b2) {
        return b1.AverageDepth.CompareTo(b2.AverageDepth);
    }
}