using System; //For data read/write methods
using System.Collections.Generic; //Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections
using System.Reflection;
using BepInEx;
using Nautilus.Handlers;
using ReikaKalseki.AqueousEngineering;
using ReikaKalseki.DIAlterra;
using UnityEngine; //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Ecocean;

[BepInPlugin(MOD_KEY, "Ecocean", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency(DIMod.MOD_KEY)]
public class EcoceanMod : BaseUnityPlugin {
    public const string MOD_KEY = "ReikaKalseki.Ecocean";

    //public static readonly ModLogger logger = new ModLogger();
    public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();

    public static readonly Config<ECConfig.ConfigEntries> config = new Config<ECConfig.ConfigEntries>(modDLL);
    internal static readonly XMLLocale locale = new XMLLocale(modDLL, "XML/locale.xml");

    public static readonly WorldgenDatabase worldgen = new WorldgenDatabase();

    public static GlowOilMushroom glowShroom;
    public static GlowOil glowOil;
    public static GlowOilNatural naturalOil;

    public static LavaBombMushroom lavaShroom;
    public static LavaBomb lavaBomb;

    public static PlanktonCloud plankton;
    public static PlanktonItem planktonItem;
    public static WorldCollectedItem treeMushroomSpores;

    public static SeamothPlanktonScoop planktonScoop;

    public static PiezoCrystal piezo;
    public static SonarFlora sonarFlora;

    public static VoidBubble voidBubble;
    public static VoidTongue tongue;

    public static HeatBubble heatBubble;

    //public static HeatColumnFog heatColumnFog;
    public static HeatColumnShell heatColumnShell;
    public static VoidOrganic voidOrganic;
    public static readonly Dictionary<string, HeatColumnBone> heatColumnBones = new();

    //public static TreeBud mushTreeResource;

    public static MushroomStack mushroomStack;
    public static PinkBulbStack pinkBulbStack;
    public static MushroomVaseStrand mushroomVaseStrand;

    public static PinkLeaves pinkLeaves;

    //public static readonly MushroomTendril[] mushroomTendrils = new MushroomTendril[2];
    public static DeadPlant deadBlighted;

    private static TerrainLootSpawner ilzOreSpawner;

    public static TechType waterCurrentCommon;
    public static TechType celeryTree;

    internal static readonly Vector3 reaperlessTripleVent = new(-1150, -240, -258);
    internal static readonly Vector3 northDuneBit = new(-1151, -340, 1444);

    internal static readonly List<Vector3> ilzOreSpawners = [
        new Vector3(-92, -1250, 343),
        new Vector3(-108.6F, -1256, 379.7F),
        new Vector3(-358, -1052, 109),
        new Vector3(-165, -1278, 267),
    ];

    public void Awake() {
        config.load();

        var harmony = new HarmonySystem(MOD_KEY, modDLL, typeof(ECPatches));
        harmony.apply();

        ModVersionCheck.getFromGitVsInstall("Ecocean", modDLL, "Ecocean").register();

        locale.load();

        glowOil = new GlowOil(locale.getEntry("GlowOil"));
        glowOil.register();
        naturalOil = new GlowOilNatural();
        naturalOil.register();

        lavaBomb = new LavaBomb(locale.getEntry("LavaBomb"));
        lavaBomb.Register();

        plankton = new PlanktonCloud(locale.getEntry("plankton"));
        plankton.register();
        planktonItem = new PlanktonItem(locale.getEntry("planktonItem"));
        planktonItem.Register();
        treeMushroomSpores = new WorldCollectedItem(
            locale.getEntry("treeMushroomSpores"),
            "1ce074ee-1a58-439b-bb5b-e5e3d9f0886f"
        );
        treeMushroomSpores.sprite = TextureManager.getSprite(modDLL, "Textures/Items/TreeMushroomSpores");
        treeMushroomSpores.inventorySize = new Vector2int(2, 1);
        treeMushroomSpores.Register();

        deadBlighted = new DeadPlant();
        deadBlighted.Register();

        WaterCurrent.register();

        piezo = new PiezoCrystal(locale.getEntry("piezoCrystal"));
        piezo.register();

        sonarFlora = new SonarFlora(locale.getEntry("sonarFlora"));
        sonarFlora.Register();

        mushroomStack = new MushroomStack(locale.getEntry("mushroomStack"));
        mushroomStack.Register();

        pinkLeaves = new PinkLeaves(locale.getEntry("pinkLeaves"));
        pinkLeaves.Register();

        pinkBulbStack = new PinkBulbStack(locale.getEntry("pinkBulbStack"));
        pinkBulbStack.Register();
        CraftData.entClassTechTable[DecoPlants.PINK_BULB_STACK.prefab] = pinkBulbStack.TechType;

        mushroomVaseStrand = new MushroomVaseStrand(locale.getEntry("mushroomVaseStrand"));
        mushroomVaseStrand.Register();
        CraftData.entClassTechTable[DecoPlants.MUSHROOM_VASE_STRANDS.prefab] = mushroomVaseStrand.TechType;

        XMLLocale.LocaleEntry e;
        /*
        e = locale.getEntry("mushroomTendril");
        mushroomTendrils[0] = new MushroomTendril(e, MushroomTendril.color1);
        mushroomTendrils[0].Patch();
        mushroomTendrils[1] = new MushroomTendril(e, MushroomTendril.color2);
        mushroomTendrils[1].Patch();
        */
        e = locale.getEntry("celeryTree");
        celeryTree = SNUtil.AddTechTypeToVanillaPrefabs(e, DecoPlants.CELERY_TREE.prefab);
        SNUtil.AddPdaEntry(celeryTree, e.key, e.name, 10, "Lifeforms/Flora/Land", e.pda, e.getString("header"));

        voidBubble = new VoidBubble(locale.getEntry("VoidBubble"));
        voidBubble.register();
        tongue = new VoidTongue(locale.getEntry("VoidTongue"));
        tongue.register();

        heatBubble = new HeatBubble();
        heatBubble.Register();
        //heatColumnFog = new HeatColumnFog();
        //heatColumnFog.Patch();
        heatColumnShell = new HeatColumnShell(locale.getEntry("HeatColumnShell"));
        heatColumnShell.register();
        voidOrganic = new VoidOrganic(locale.getEntry("voidOrganic"));
        voidOrganic.sprite = TextureManager.getSprite(modDLL, "Textures/Items/VoidOrganics");
        voidOrganic.Register();
        foreach (var s in HeatColumnBone.boneProps.Keys) {
            heatColumnBones[s] = new HeatColumnBone(s);
            heatColumnBones[s].Register();
        }

        //mushTreeResource = new TreeBud(locale.getEntry("TreeBud"));
        //mushTreeResource.Patch();

        planktonScoop = new SeamothPlanktonScoop();
        planktonScoop.register();

        glowShroom = new GlowOilMushroom();
        glowShroom.Register();
        e = locale.getEntry(glowShroom.ClassID);
        glowShroom.addPDAEntry(e.pda, 15F, e.getString("header"));
        SNUtil.Log(" > " + glowShroom);
        GenUtil.registerPrefabWorldgen(
            glowShroom,
            EntitySlot.Type.Medium,
            LargeWorldEntity.CellLevel.Far,
            BiomeType.Dunes_Grass,
            1,
            0.25F
        );

        lavaShroom = new LavaBombMushroom();
        lavaShroom.Register();
        e = locale.getEntry(lavaShroom.ClassID);
        lavaShroom.addPDAEntry(e.pda, 20F, e.getString("header"));
        SNUtil.Log(" > " + lavaShroom);
        GenUtil.registerPrefabWorldgen(
            lavaShroom,
            EntitySlot.Type.Medium,
            LargeWorldEntity.CellLevel.Far,
            BiomeType.InactiveLavaZone_Chamber_Floor,
            1,
            0.08F
        );
        GenUtil.registerPrefabWorldgen(
            lavaShroom,
            EntitySlot.Type.Medium,
            LargeWorldEntity.CellLevel.Far,
            BiomeType.InactiveLavaZone_Chamber_Floor_Far,
            1,
            0.08F
        );
        GenUtil.registerPrefabWorldgen(
            lavaShroom,
            EntitySlot.Type.Medium,
            LargeWorldEntity.CellLevel.Far,
            BiomeType.InactiveLavaZone_Corridor_Floor_Far,
            1,
            0.067F
        );
        GenUtil.registerPrefabWorldgen(
            lavaShroom,
            EntitySlot.Type.Medium,
            LargeWorldEntity.CellLevel.Far,
            BiomeType.InactiveLavaZone_Corridor_Floor,
            1,
            0.04F
        );

        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.DIAMOND.prefab,
            BiomeType.MushroomForest_GiantTreeInteriorFloor,
            3F,
            1
        );
        LootDistributionHandler.EditLootDistributionData(
            VanillaResources.LITHIUM.prefab,
            BiomeType.MushroomForest_GiantTreeInteriorFloor,
            8F,
            1
        );

        //GenUtil.registerSlotWorldgen(mushTreeResource.ClassID, mushTreeResource.PrefabFileName, mushTreeResource.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Near, BiomeType.MushroomForest_GiantTreeInteriorRecess, 1, 3F);
        //GenUtil.registerSlotWorldgen(mushTreeResource.ClassID, mushTreeResource.PrefabFileName, mushTreeResource.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Near, BiomeType.MushroomForest_GiantTreeInteriorSpecial, 1, 5F);

        //GenUtil.registerSlotWorldgen(pinkLeaves.ClassID, pinkLeaves.PrefabFileName, pinkLeaves.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.UnderwaterIslands_IslandPlants, 1, 1F);
        //GenUtil.registerSlotWorldgen(pinkLeaves.ClassID, pinkLeaves.PrefabFileName, pinkLeaves.TechType, EntitySlot.Type.Medium, LargeWorldEntity.CellLevel.Medium, BiomeType.CrashZone_TrenchSand, 1, 2F);

        BioReactorHandler.SetBioReactorCharge(
            lavaShroom.seed.TechType,
            BaseBioReactor.GetCharge(TechType.SnakeMushroomSpore) * 3
        );
        BioReactorHandler.SetBioReactorCharge(glowOil.TechType, BaseBioReactor.GetCharge(TechType.BloodOil) * 2);
        BioReactorHandler.SetBioReactorCharge(
            mushroomStack.seed.TechType,
            BaseBioReactor.GetCharge(TechType.GarryFish) * 0.6F
        );

        GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.REAPER.prefab, reaperlessTripleVent.SetY(-200)));
        GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.REAPER.prefab, northDuneBit.SetY(-320)));

        var wr = new TerrainLootSpawner.WeightedTerrainLootSpawn();
        wr.addEntry(VanillaResources.DIAMOND.prefab, 20);
        wr.addEntry(VanillaResources.GOLD.prefab, 30);
        wr.addEntry(VanillaResources.LITHIUM.prefab, 10);
        wr.addEntry(VanillaResources.MAGNETITE.prefab, 40);
        wr.addEntry(VanillaResources.NICKEL.prefab, 40);
        wr.addEntry(VanillaResources.RUBY.prefab, 10);
        wr.addEntry(VanillaResources.SILVER.prefab, 20);
        wr.addEntry(VanillaResources.URANIUM.prefab, 20);
        ilzOreSpawner = new TerrainLootSpawner("ilzOreSpawner", wr);
        ilzOreSpawner.Register();

        foreach (var vec in ilzOreSpawners)
            GenUtil.registerWorldgen(
                new PositionedPrefab(ilzOreSpawner.ClassID, vec, Quaternion.identity, new Vector3(0.1F, 32, 40))
            );

        //ConsoleCommandsHandler.RegisterConsoleCommand<Action<int>>("currentFlowVec", MountainCurrentSystem.instance.registerFlowVector);
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<float>>(
            "attackBase",
            r => {
                ECHooks.AttractCreaturesToBase(
                    Player.main.currentSub,
                    r,
                    c => c is GhostLeviathan or GhostLeviatanVoid or ReaperLeviathan or SeaDragon or Shocker or CrabSquid or BoneShark
                );
            }
        );

        worldgen.load();

        FoodEffectSystem.instance.register();

        /*
        GenUtil.ContainerPrefab pfb = GenUtil.getOrCreateDatabox(planktonScoop.TechType);
        if (QModManager.API.QModServices.Main.ModPresent("SeaToSea")) {
            GenUtil.registerWorldgen(new PositionedPrefab(pfb.ClassID, new Vector3());
        }*/

        glowShroom.addNativeBiome(VanillaBiomes.Dunes);
        lavaShroom.addNativeBiome(VanillaBiomes.Ilz);
        pinkBulbStack.addNativeBiome(VanillaBiomes.Grandreef);
        pinkBulbStack.addNativeBiome(VanillaBiomes.Redgrass, true);
        pinkBulbStack.addNativeBiome(VanillaBiomes.Koosh);
        mushroomStack.addNativeBiome(VanillaBiomes.Mountains);
        pinkLeaves.addNativeBiome(VanillaBiomes.Crash);
        mushroomVaseStrand.addNativeBiome(VanillaBiomes.Mushroom);
        //lavaLily.addNativeBiome(VanillaBiomes.ALZ);

        e = locale.getEntry("Mouseovers");
        foreach (var kvp in e.getFields()) {
            if (!string.IsNullOrEmpty(kvp.Value))
                CustomLocaleKeyDatabase.registerKey(kvp.Key, kvp.Value);
        }

        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ECHooks).TypeHandle);

        // PostLoad
        if (InstructionHandlers.GetTypeBySimpleName("ReikaKalseki.AqueousEngineering.BaseSonarPinger") != null) {
            //AE is loaded
            BaseSonarPinger.onBaseSonarPingedEvent += go =>
                ECHooks.PingSonarFromObject(go.gameObject.FindAncestor<SubRoot>(), 0.67F);

            BaseRoomSpecializationSystem.instance.registerModdedObject(glowOil, 0.25F);
            BaseRoomSpecializationSystem.instance.registerModdedObject(glowShroom, 0.4F); //jellyshroom is 0.4
            BaseRoomSpecializationSystem.instance.registerModdedObject(lavaShroom, 0.4F);
            BaseRoomSpecializationSystem.instance.registerModdedObject(mushroomStack, 0.15F);
            BaseRoomSpecializationSystem.instance.registerModdedObject(pinkBulbStack, -0.05F);
            BaseRoomSpecializationSystem.instance.registerModdedObject(pinkLeaves, 0.75F);
            BaseRoomSpecializationSystem.instance.registerModdedObject(mushroomVaseStrand, 0.25F);

            ACUEcosystems.AddFood(new ACUEcosystems.PlantFood(glowShroom, 0.25F, BiomeRegions.Other));
            ACUEcosystems.AddFood(new ACUEcosystems.PlantFood(lavaShroom, 0.25F, BiomeRegions.LavaZone));
            ACUEcosystems.AddFood(new ACUEcosystems.PlantFood(mushroomStack, 0.02F, BiomeRegions.Other));
            ACUEcosystems.AddFood(
                new ACUEcosystems.PlantFood(pinkBulbStack, 0.1F, BiomeRegions.Koosh, BiomeRegions.GrandReef)
            );
            ACUEcosystems.AddFood(new ACUEcosystems.PlantFood(pinkLeaves, 0.1F, BiomeRegions.Other));
            ACUEcosystems.AddFood(new ACUEcosystems.PlantFood(mushroomVaseStrand, 0.3F, BiomeRegions.Mushroom));

            MushroomVaseStrand.filterDrops.addEntry(TechType.Salt, 50);
            MushroomVaseStrand.filterDrops.addEntry(TechType.Copper, 10);
            MushroomVaseStrand.filterDrops.addEntry(TechType.Gold, 5);
            MushroomVaseStrand.filterDrops.addEntry(TechType.Lead, 8);
            MushroomVaseStrand.filterDrops.addEntry(TechType.Silver, 6);
            MushroomVaseStrand.filterDrops.addEntry(TechType.Quartz, 15);
            MushroomVaseStrand.filterDrops.addEntry(TechType.Lithium, 8);
        }

        foreach (BiomeType b in Enum.GetValues(typeof(BiomeType)))
            LootDistributionHandler.EditLootDistributionData("0e67804e-4a59-449d-929a-cd3fc2bef82c", b, 0, 0);
    }
}