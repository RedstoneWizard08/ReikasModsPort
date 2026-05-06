using System; //For data read/write methods
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Nautilus.Assets;
using Nautilus.Handlers; //Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections
using UnityEngine; //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.DIAlterra;

[BepInPlugin(MOD_KEY, "DIAlterra", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class DIMod : BaseUnityPlugin {
    private static DIMod Instance;
    private ManualLogSource BaseLogger => base.Logger;
    public new static ManualLogSource Logger => Instance.BaseLogger;

    public const string MOD_KEY = "ReikaKalseki.DIAlterra";

    public static readonly XMLLocale locale = new(SNUtil.DiDLL, "XML/locale.xml");
    /*
    private static readonly List<SNMod> mods = new List<SNMod>();

    public static void addMod(SNMod mod) {
        mods.Add(mod);
    }
    */
    //public static readonly ModLogger logger = new ModLogger();

    public static readonly Config<DIConfig.ConfigEntries> config = new(SNUtil.DiDLL);

    internal static readonly Dictionary<TechType, CustomPrefab> machineList = new();

    public static SignalManager.ModSignal areaOfInterestMarker { get; private set; }
    public static TemporaryFloatingLocker floatingLocker { get; private set; }

    public void Start() {
        Instance = this;
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PlacedObject).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(DICustomPrefab).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WorldGenerator).TypeHandle);
        //mods.Insert(0, new SNMod(MOD_KEY));
    }

    public void Awake() {
        SNUtil.Log("Start DI Main Init", SNUtil.DiDLL);
        config.load();

        var harmony = new HarmonySystem(MOD_KEY, SNUtil.DiDLL, typeof(DIPatches));
        harmony.apply();

        ModVersionCheck.getFromGitVsInstall("Dragon Industries", SNUtil.DiDLL, "DragonIndustries_AlterraDivision")
            .register();

        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SaveSystem).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SpawnedItemTracker).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(TechUnlockTracker).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SurvivalEventTracker).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CommandTracker).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ConsumableTracker).TypeHandle);

        new ObjectDeleter().Register();
        floatingLocker = new TemporaryFloatingLocker();
        floatingLocker.Register();

        locale.load();

        CustomLocaleKeyDatabase.registerKey(locale.getEntry("ItemNotDroppable"));
        CustomLocaleKeyDatabase.registerKey(locale.getEntry("BulkheadInoperable"));
        CustomLocaleKeyDatabase.registerKey(locale.getEntry("PrecursorCraftPrompt"));
        CustomLocaleKeyDatabase.registerKey(locale.getEntry("PrecursorCraftNoIngredients"));

        KnownTech.onAdd += (tt, vb) => TechUnlockTracker.instance.onUnlock(tt);

        var spineEel = createEgg(
            TechType.SpineEel,
            TechType.BoneShark,
            1,
            "SpineEelDesc",
            true,
            0.16F,
            4,
            0.5F,
            BiomeType.BonesField_Ground,
            BiomeType.LostRiverJunction_Ground
        ).ModifyGo(e => {
                List<Renderer> li = [];
                foreach (var r in e.GetComponentsInChildren<Renderer>()) {
                    RenderUtil.makeTransparent(r);
                    RenderUtil.setGlossiness(r.material, 10, 6, 0.5F);
                    r.material.SetColor("_SpecColor", new Color(1, 1, 0.8F, 1));
                    r.material.SetFloat("_SrcBlend", 5);
                    r.material.SetFloat("_DstBlend", 10);
                    r.material.SetFloat("_SrcBlend2", 2);
                    r.material.SetFloat("_DstBlend2", 10);
                    li.Add(r);
                }

                foreach (var r in li) {
                    for (var i = 0; i < 3; i++) {
                        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).setName("EggGlow_" + i);
                        sphere.transform.localScale = Vector3.one * 0.1F;
                        sphere.transform.SetParent(r.transform.parent);
                        sphere.transform.localPosition =
                            MathUtil.getRandomVectorAround(new Vector3(-0.3F, 0.24F, 0), 0.15F);
                        sphere.transform.localRotation = Quaternion.identity;
                        sphere.removeComponent<Collider>();
                        // ECCHelpers.ApplySNShaders(sphere, new UBERMaterialProperties(0, 0, 5));
                        var r2 = sphere.GetComponentInChildren<Renderer>();
                        r2.material.SetColor("_GlowColor", new Color(1, 0.75F, 0.33F, 1));
                        RenderUtil.setEmissivity(r2, 0.8F);
                        RenderUtil.setGlossiness(r2, 4, 0, 0);
                    }
                }
            }
        );
        createEgg(
            TechType.GhostRayBlue,
            TechType.Jumper,
            1.75F,
            "GhostRayDesc",
            true,
            0.12F,
            2,
            0.75F,
            BiomeType.TreeCove_LakeFloor
        );
        createEgg(
            TechType.GhostRayRed,
            TechType.Crabsnake,
            1.25F,
            "CrimsonRayDesc",
            true,
            0.6F,
            2,
            0.8F,
            BiomeType.InactiveLavaZone_Chamber_Floor_Far
        );
        createEgg(
            TechType.Biter,
            TechType.RabbitRay,
            1F,
            "BiterDesc",
            false,
            0.6F,
            2,
            1,
            BiomeType.GrassyPlateaus_CaveFloor,
            BiomeType.Mountains_CaveFloor
        );
        createEgg(
            TechType.Blighter,
            TechType.RabbitRay,
            1F,
            "BlighterDesc",
            false,
            0.6F,
            2,
            1,
            BiomeType.BloodKelp_CaveFloor
        );
        GenUtil.registerSlotWorldgen(
            "b5d6cf1a-7d42-45f2-a0f3-0e05ff707502",
            "WorldEntities/Eggs/JumperEgg.prefab",
            TechType.JumperEgg,
            EntitySlot.Type.Small,
            LargeWorldEntity.CellLevel.Medium,
            BiomeType.Kelp_CaveFloor,
            1,
            0.36F
        );

        /*
        dispatchLoadPhase("loadConfig");
        dispatchLoadPhase("afterConfig");
        dispatchLoadPhase("doPatches");
        dispatchLoadPhase("addItems");
        dispatchLoadPhase("loadMain");
        dispatchLoadPhase("loadConfig");
        */
        addCommands();

        SpriteHandler.RegisterSprite(
            TechType.PDA,
            TextureManager.getSprite(SNUtil.DiDLL, "Textures/ScannerSprites/PDA")
        );
        SpriteHandler.RegisterSprite(
            TechType.Databox,
            TextureManager.getSprite(SNUtil.DiDLL, "Textures/ScannerSprites/Databox")
        );
        SpriteHandler.RegisterSprite(
            TechType.ReaperLeviathan,
            TextureManager.getSprite(SNUtil.DiDLL, "Textures/ScannerSprites/Reaper")
        );

        var le = locale.getEntry("AreaOfInterest");
        SNUtil.AllowDidll = true;
        areaOfInterestMarker = SignalManager.createSignal("AreaOfInterest", le.desc, le.desc, "", "");
        areaOfInterestMarker.register(
            null,
            TextureManager.getSprite(SNUtil.DiDLL, "Textures/AreaOfInterestMarker"),
            Vector3.zero
        );
        SNUtil.AllowDidll = false;

        SNUtil.Log("Finish DI Main Init", SNUtil.DiDLL);

        // POST LOAD

        //dispatchLoadPhase("loadModInteract");
        //dispatchLoadPhase("loadFinal");
        BiomeBase.InitializeBiomeHoles();

        ModVersionCheck.fetchRemoteVersions();
    }

    private static void killSelf() {
        var v = Player.main.GetVehicle();
        if (v)
            v.GetComponent<LiveMixin>().TakeDamage(99999);
        if (Player.main.currentSub && Player.main.currentSub.isCyclops)
            Player.main.currentSub.GetComponent<LiveMixin>().TakeDamage(99999);
        Player.main.GetComponent<LiveMixin>().TakeDamage(99999);
    }

    private static CustomEgg createEgg(
        TechType creature,
        TechType basis,
        float scale,
        string locKey,
        bool isBig,
        float grownScale,
        float daysToGrow,
        float rate,
        params BiomeType[] spawn
    ) {
        Action<CustomEgg> a = e => {
            e.EggProperties.maxSize = grownScale;
            if (!isBig)
                e.EggProperties.initialSize = Mathf.Max(e.EggProperties.initialSize, 0.2F);
            e.EggProperties.daysToGrow = daysToGrow;
        };
        SNUtil.AllowDidll = true;
        var egg = CustomEgg.CreateAndRegisterEgg(
            creature,
            basis,
            scale,
            locale.getEntry(locKey).desc,
            isBig,
            a,
            rate,
            spawn
        );
        SNUtil.AllowDidll = false;
        return egg;
    }

    private static void dispatchLoadPhase(string phase) { /*
    foreach (SNMod mod in mods) {

    }*/
    }

    private static void addCommands() {
        BuildingHandler.instance.addCommand<string, PlacedObject>("pfb", BuildingHandler.instance.spawnPrefabAtLook);
        //BuildingHandler.instance.addCommand<string>("btt", BuildingHandler.instance.spawnTechTypeAtLook);
        BuildingHandler.instance.addCommand<bool>("bden", BuildingHandler.instance.setEnabled);
        BuildingHandler.instance.addCommand("bdsa", BuildingHandler.instance.selectAll);
        BuildingHandler.instance.addCommand("bdslp", BuildingHandler.instance.selectLastPlaced);
        BuildingHandler.instance.addCommand<string>("bdspid", BuildingHandler.instance.selectOfID);
        BuildingHandler.instance.addCommand("bdsync", BuildingHandler.instance.syncObjects);
        BuildingHandler.instance.addCommand<string>("bdexs", BuildingHandler.instance.saveSelection);
        BuildingHandler.instance.addCommand<string>("bdexa", BuildingHandler.instance.saveAll);
        BuildingHandler.instance.addCommand<string>("bdld", BuildingHandler.instance.loadFile);
        BuildingHandler.instance.addCommand("bdcc", BuildingHandler.instance.clearCache);
        BuildingHandler.instance.addCommand("bdcache", BuildingHandler.instance.cache);
        BuildingHandler.instance.addCommand("bdlc", BuildingHandler.instance.loadCache);
        BuildingHandler.instance.addCommand<string>("bdexc", BuildingHandler.instance.saveCache);
        BuildingHandler.instance.addCommand("bdinfo", BuildingHandler.instance.selectedInfo);
        BuildingHandler.instance.addCommand("bdtex", BuildingHandler.instance.dumpTextures);
        BuildingHandler.instance.addCommand("bdact", BuildingHandler.instance.activateObject);
        BuildingHandler.instance.addCommand<float>("bdsc", BuildingHandler.instance.setScale);
        BuildingHandler.instance.addCommand<float, float, float>("bdscxyz", BuildingHandler.instance.setScaleXYZ);
        ConsoleCommandsHandler.RegisterConsoleCommand<Action<string, bool>>("sound", SoundManager.playSound);
        ConsoleCommandsHandler.RegisterConsoleCommand("dumpBiomeTex", DIHooks.DumpWaterscapeTextures);
        ConsoleCommandsHandler.RegisterConsoleCommand("biomeAt", printBiomeData);
        ConsoleCommandsHandler.RegisterConsoleCommand("killSelf", killSelf);
        ConsoleCommandsHandler.RegisterConsoleCommand("clear000", clear000);
        ConsoleCommandsHandler.RegisterConsoleCommand("clearLoose", clearUnparentedItem);
        ConsoleCommandsHandler.RegisterConsoleCommand("particle", spawnParticle);
        //ConsoleCommandsHandler.RegisterConsoleCommand<Action>("hideVersions", DIHooks.hideVersions);
        //ConsoleCommandsHandler.RegisterConsoleCommand<Action>("autoUpdate", DIHooks.autoUpdate);
        //ConsoleCommandsHandler.RegisterConsoleCommand<Action<string, string, string>>("exec", DebugExec.run);
        ConsoleCommandsHandler.RegisterConsoleCommand("vehicleToMe", bringVehicleToPlayer);
        ConsoleCommandsHandler.RegisterConsoleCommand("savePhysProps", PhysicsSettlingProp.export);
        ConsoleCommandsHandler.RegisterConsoleCommand("dumpPrefabs", dumpPrefabsOfType);
        ConsoleCommandsHandler.RegisterConsoleCommand("deleteNear", deletePrefabNear);
        ConsoleCommandsHandler.RegisterConsoleCommand("fullbright", setFullbright);
    }

    public static void setFullbright(bool on) {
        if (on) {
            var l = Player.main.gameObject.addLight();
            l.intensity = 0.5F;
            l.range = 2500F;
            l.color = Color.white;
            l.gameObject.name = "FullbrightLight";
            PlayerMovementSpeedModifier.add(5, 999999);
        } else {
            Player.main.gameObject.removeChildObject("FullbrightLight");
            Player.main.gameObject.removeComponent<PlayerMovementSpeedModifier>();
        }

        foreach (var waterscapeVolume in FindObjectsOfType<WaterscapeVolume>()) {
            waterscapeVolume.enabled = !on;
        }
    }

    private static void dumpPrefabsOfType(string id) {
        List<PositionedPrefab> li = [];
        foreach (var pi in FindObjectsOfType<PrefabIdentifier>()) {
            if (pi && pi.ClassId == id) {
                li.Add(new PositionedPrefab(pi));
            }
        }

        var file = BuildingHandler.instance.dumpPrefabs(id, li);
        SNUtil.WriteToChat("Exported " + li.Count + " prefabs of id '" + id + "' to " + file);
    }

    private static void deletePrefabNear(float r, string id) {
        var pos = Player.main.transform.position;
        var pis = 0;
        var found = 0;
        foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(pos, r)) {
            pis++;
            if (SNUtil.Match(pi, id)) {
                pi.gameObject.destroy(false);
                found++;
            }

            SNUtil.WriteToChat("Found " + pis + " objects near " + pos + ", deleted " + found);
        }
    }

    private static void bringVehicleToPlayer(string type) {
        MonoBehaviour v = null;
        switch (type.ToLowerInvariant()) {
            case "seamoth":
                v = WorldUtil.getClosest<SeaMoth>(Player.main.transform.position);
                break;
            case "prawn":
            case "exosuit":
                v = WorldUtil.getClosest<Exosuit>(Player.main.transform.position);
                break;
            case "cyclops":
                v = WorldUtil.getClosest<SubRoot>(Player.main.transform.position);
                break;
        }

        if (v) {
            var pos = Camera.main.transform.position + Camera.main.transform.forward * 10;
            if (v is Vehicle vehicle) {
                vehicle.TeleportVehicle(pos, vehicle.transform.rotation);
            } else if (v is SubRoot root && root.isCyclops) {
                root.transform.position = pos;
            }
        }
    }

    private static void printBiomeData() {
        SNUtil.WriteToChat(
            "Current native biome: " + WaterBiomeManager.main.GetBiome(Player.main.transform.position, false)
        );
        SNUtil.WriteToChat("Localized DI name: " + BiomeBase.GetBiome(Player.main.transform.position).DisplayName);
    }

    private static void spawnParticle(string pfb, float dur) {
        WorldUtil.spawnParticlesAt(
            Camera.main.transform.position + Camera.main.transform.forward.normalized * 10,
            pfb,
            dur,
            true
        );
    }

    private static void clear000() {
        foreach (var go in WorldUtil.getObjectsNear(Vector3.zero, 0.2F)) {
            if (go && go.activeInHierarchy && go.transform.position.magnitude < 0.02F) {
                var pi = go.FindAncestor<PrefabIdentifier>();
                if (pi && !pi.GetComponentInChildren<Vehicle>() && !pi.GetComponentInChildren<Player>() &&
                    !pi.GetComponentInChildren<SubRoot>())
                    pi.gameObject.destroy(false);
            }
        }
    }

    private static void clearUnparentedItem(string id) {
        var found = 0;
        foreach (var pi in FindObjectsOfType<PrefabIdentifier>()) {
            if (pi.ClassId == id && !pi.gameObject.FindAncestor<StorageContainer>()) {
                pi.gameObject.destroy(false);
                found++;
            }
        }

        SNUtil.WriteToChat("Destroyed " + found + " items of type '" + id + "' not in StorageContainers.");
    }

    public static void restartGame() {
        var svc = PlatformUtils.main.services;
        if (svc is PlatformServicesSteam steam)
            steam.RestartInSteam();
    }
}