using System; //For data read/write methods
//Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections
using System.Reflection;
using BepInEx;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine; //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Auroresource;

[BepInPlugin(ModKey, "Auroresource", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
public class AuroresourceMod : BaseUnityPlugin {
    public const string ModKey = "ReikaKalseki.Auroresource";

    //public static readonly ModLogger logger = new ModLogger();
    public static readonly Assembly ModDLL = Assembly.GetExecutingAssembly();

    public static readonly Config<ARConfig.ConfigEntries> ModConfig = new(ModDLL);

    public static readonly XMLLocale Locale = new(ModDLL, "XML/locale.xml");
    public static readonly XMLLocale PdaLocale = new(ModDLL, "XML/pda.xml");
    public static readonly XMLLocale VoLocale = new(ModDLL, "XML/vo.xml");

    public static readonly Vector3 JailbreakPedestalLocation = new(420, -93.3F, 1153);

    public static DrillableMeteorite DunesMeteor;
    public static LavaDome LavaPitCenter;
    public static PrecursorJailbreakingConsole Console;
    public static ScannerRoomMeteorPlanner MeteorDetector;

    public const string LaserCutterJailbroken = "lasercutterjailbreak";

    public static TechType DetectorUnlock = TechType.BaseMapRoom;

    public void Awake() {
        ModConfig.load();

        var harmony = new HarmonySystem(ModKey, ModDLL, typeof(ARPatches));
        harmony.apply();

        ModVersionCheck.getFromGitVsInstall("Auroresource", ModDLL, "Auroresource").register();
        SNUtil.checkModHash(ModDLL);

        Locale.load();
        PdaLocale.load();
        VoLocale.load();

        AddPdaEntries();

        DunesMeteor = new DrillableMeteorite();
        DunesMeteor.register();
        LavaPitCenter = new LavaDome();
        LavaPitCenter.register(10);
        Console = new PrecursorJailbreakingConsole(Locale.getEntry("JailBreakConsole"));
        Console.register(JailbreakPedestalLocation, 20);

        MeteorDetector = new ScannerRoomMeteorPlanner();
        MeteorDetector.Register();

        FallingMaterialSystem.Instance.Register();

        PDAMessagePrompts.instance.addPDAMessage(VoLocale.getEntry("auroracut"));
        PDAMessagePrompts.instance.addPDAMessage(VoLocale.getEntry("jailbreak"));

        GenUtil.registerWorldgen(new PositionedPrefab(DunesMeteor.ClassID, WorldUtil.DUNES_METEOR + Vector3.down * 29));
        GenUtil.registerWorldgen(new PositionedPrefab(LavaPitCenter.ClassID, WorldUtil.LAVA_DOME + Vector3.down * 56));
        GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.REAPER.prefab, new Vector3(-1125, -209, 1130)));

        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ARHooks).TypeHandle);

        DIHooks.OnWorldLoadedEvent += () => {
            SNUtil.log("Adding resource data to motherlode PDA pages.", ModDLL);
            DunesMeteor.updateLocale();
            LavaPitCenter.updateLocale();
        };

        StoryHandler.instance.addListener(s => {
                if (s == LaserCutterJailbroken)
                    PDAMessagePrompts.instance.trigger("jailbreak");
            }
        );

        CustomLocaleKeyDatabase.registerKey(Locale.getEntry("AuroraLaserCut"));
        CustomLocaleKeyDatabase.registerKey(Locale.getEntry("AuroraLaserCutNeedsUnlock"));

        TechTypeMappingConfig<float>.loadInline(
            "falling_materials",
            TechTypeMappingConfig<float>.FloatParser.instance,
            FallingMaterialSystem.Instance.AddMaterial
        );

        ConsoleCommandsHandler.RegisterConsoleCommand<Action>(
            "triggerFallingDebris",
            FallingMaterialSystem.Instance.SpawnItem
        );
        ConsoleCommandsHandler.RegisterConsoleCommand(
            "queueFallingDebris",
            FallingMaterialSystem.Instance.QueueSpawn
        );

        // PostLoad
        if (DetectorUnlock != TechType.None)
            TechnologyUnlockSystem.instance.addDirectUnlock(DetectorUnlock, MeteorDetector.TechType);
    }

    public static void AddPdaEntries() {
        foreach (var e in PdaLocale.getEntries()) {
            var page = PDAManager.createPage(e);
            if (e.hasField("audio"))
                page.setVoiceover(e.getString("audio"));
            if (e.hasField("header"))
                page.setHeaderImage(TextureManager.getTexture(ModDLL, "Textures/PDA/" + e.getString("header")));
            page.register();
        }
    }
}