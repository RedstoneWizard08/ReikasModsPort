//For data read/write methods
//Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections
using System.Reflection;
using BepInEx;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine; //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Exscansion;

[BepInPlugin(MOD_KEY, "Exscansion", Nautilus.PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
[BepInDependency(DIMod.MOD_KEY)]
public class ExscansionMod : BaseUnityPlugin {
    public const string MOD_KEY = "ReikaKalseki.Exscansion";

    public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();

    public static readonly Config<ESConfig.ConfigEntries> config = new(modDLL);

    public static ScannerRoomMarker abandonedBase;
    public static ScannerRoomMarker alienBase;
    public static ScannerRoomMarker fossils;

    private void Awake() {
        config.load();

        var harmony = new HarmonySystem(MOD_KEY, modDLL, typeof(ESPatches));
        harmony.apply();

        ModVersionCheck.getFromGitVsInstall("Exscansion", modDLL, "Exscansion").register();

        abandonedBase = new ScannerRoomMarker(
            EnumHandler.AddEntry<TechType>("AbandonedBase").WithPdaInfo("Titanium Mass", "")
        );
        alienBase = new ScannerRoomMarker(
            EnumHandler.AddEntry<TechType>("AlienBase").WithPdaInfo("Unidentifiable Object", "")
        );
        fossils = new ScannerRoomMarker(
            EnumHandler.AddEntry<TechType>("Fossils").WithPdaInfo("Fossilized Remains", "")
        );
        abandonedBase.Register();
        alienBase.Register();
        fossils.Register();

        /*
        if (config.getBoolean(ESConfig.ConfigEntries.BASES)) {
            GenUtil.registerWorldgen(new PositionedPrefab(abandonedBase.ClassID, new Vector3(0, 0, 0)));
            GenUtil.registerWorldgen(new PositionedPrefab(abandonedBase.ClassID, new Vector3(0, 0, 0)));
            GenUtil.registerWorldgen(new PositionedPrefab(abandonedBase.ClassID, new Vector3(0, 0, 0)));

            GenUtil.registerWorldgen(new PositionedPrefab(abandonedBase.ClassID, new Vector3(0, 0, 0)));
            GenUtil.registerWorldgen(new PositionedPrefab(abandonedBase.ClassID, new Vector3(0, 0, 0)));
        }*/
        if (config.getBoolean(ESConfig.ConfigEntries.ALIEN)) {
            GenUtil.registerWorldgen(new PositionedPrefab(alienBase.Info.ClassID, new Vector3(-56, -1211, 116)));
            GenUtil.registerWorldgen(new PositionedPrefab(alienBase.Info.ClassID, new Vector3(265, -1440, -347)));
            GenUtil.registerWorldgen(
                new PositionedPrefab(alienBase.Info.ClassID, new Vector3(-252, -814, 316))
            ); //drf
            GenUtil.registerWorldgen(
                new PositionedPrefab(alienBase.Info.ClassID, new Vector3(-890, -311, -816))
            ); //sparse reef
            GenUtil.registerWorldgen(
                new PositionedPrefab(alienBase.Info.ClassID, new Vector3(-1224, -395, 1072.5F))
            ); //meteor
            GenUtil.registerWorldgen(
                new PositionedPrefab(alienBase.Info.ClassID, new Vector3(-628.5F, -559, 1485))
            ); //nbkelp
            GenUtil.registerWorldgen(
                new PositionedPrefab(alienBase.Info.ClassID, new Vector3(-1119, -685, -692))
            ); //lr lab cache
        }

        if (config.getBoolean(ESConfig.ConfigEntries.FOSSILS)) {
            GenUtil.registerWorldgen(
                new PositionedPrefab(fossils.Info.ClassID, new Vector3(-481, -798, 13))
            ); //lr steps ribs
        }

        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(ESHooks).TypeHandle);

        TechTypeMappingConfig<Color>.loadInline(
            "scanner_ping_colors",
            TechTypeMappingConfig<Color>.ColorParser.instance,
            TechTypeMappingConfig<Color>.dictionaryAssign(ESHooks.pingColors)
        );
    }
}