using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using Story;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class SNUtil {
    private static FMODAsset unlockSound;

    public static readonly Assembly diDLL = Assembly.GetExecutingAssembly();
    public static readonly Assembly smlDLL = Assembly.GetAssembly(typeof(CustomPrefab));
    public static readonly Assembly gameDLL = Assembly.GetAssembly(typeof(BoneShark));
    public static readonly Assembly gameDLL2 = Assembly.GetAssembly(typeof(FMODAsset));

    public static readonly string
        gameDir = Directory.GetParent(gameDLL.Location).Parent.Parent.FullName; //managed -> _Data -> root

    public static readonly string savesDir = Path.Combine(gameDir, "SNAppData/SavedGames");

    public static bool allowDIDLL = false;

    private static bool checkedReikaPC;
    private static bool savedIsReikaPC;

    private static readonly HashSet<Assembly> assembliesToSkip = [
        diDLL,
        smlDLL,
        gameDLL,
        gameDLL2,
    ];

    static SNUtil() {
    }

    private static bool evaluateReikaPC() {
        return false; // TODO
        // try {
        //     OperatingSystem os = Environment.OSVersion;
        //     return os.Platform == PlatformID.Win32NT && os.Version.Major == 10 &&
        //            System.Security.Principal.WindowsIdentity.GetCurrent().Name.EndsWith(
        //                "\\Reika",
        //                StringComparison.InvariantCultureIgnoreCase
        //            ) && System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture ==
        //            System.Runtime.InteropServices.Architecture.X64 &&
        //            Steamworks.SteamUser.GetSteamID().m_SteamID == 76561198068058411;
        // } catch (Exception e) {
        //     log("Error evaluating PC: " + e.ToString(), diDLL);
        //     return false;
        // }
    }

    public static bool isReikaPC() {
        if (!checkedReikaPC)
            savedIsReikaPC = evaluateReikaPC();
        checkedReikaPC = true;
        return savedIsReikaPC;
    }

    public static void checkModHash(Assembly mod) {
        using (var md5 = MD5.Create()) {
            using (var stream = File.OpenRead(mod.Location)) {
                var hash = md5.ComputeHash(stream);
                var hashfile = Path.Combine(Path.GetDirectoryName(mod.Location), "mod.hash");
                //if (!File.Exists(hashfile))
                //	File.WriteAllBytes(hashfile, hash);
                var stored = File.ReadAllBytes(hashfile);
                if (stored.SequenceEqual(hash))
                    log("Mod " + mod.Location + " hash check passed with hash " + hash.toDebugString(), mod);
                else
                    throw new Exception(
                        "Your mod assembly has been modified! Redownload it.\nExpected: " + stored.toDebugString() +
                        "\nActual: " + hash.toDebugString()
                    );
            }
        }
    }

    public static string getStacktrace(StackFrame[] sf = null) {
        if (sf == null)
            sf = new StackTrace().GetFrames();
        return string.Join(
            "\n",
            sf.Skip(1).Select(s => s.GetMethod() + " in " + s.GetMethod().DeclaringType)
        );
    }

    internal static Assembly tryGetModDLL(bool acceptDI = false) {
        try {
            var di = Assembly.GetExecutingAssembly();
            var sf = new StackTrace().GetFrames();
            if (sf == null || sf.Length == 0)
                return Assembly.GetCallingAssembly();
            foreach (var f in sf) {
                var a = f.GetMethod().DeclaringType.Assembly;
                if ((a != di || acceptDI || allowDIDLL) && a != smlDLL && a != gameDLL && a != gameDLL2 &&
                    a.Location.Contains("QMods"))
                    return a;
            }

            log("Could not find valid mod assembly: " + getStacktrace(sf), diDLL);
        } catch (Exception e) {
            log("Failed to find a DLL due to an exception: " + e, diDLL);
            return diDLL;
        }

        return Assembly.GetCallingAssembly();
    }

    public static void log(string s, Assembly a = null, int indent = 0) {
        while (s.Length > 4096) {
            var part = s.Substring(0, 4096);
            log(part, a);
            s = s.Substring(4096);
        }

        var id = (a != null ? a : tryGetModDLL()).GetName().Name.ToUpperInvariant().Replace("PLUGIN_", "");
        if (indent > 0) {
            s = s.PadLeft(s.Length + indent, ' ');
        }

        UnityEngine.Debug.Log(id + ": " + s);
    }

    public static bool canUseDebug() {
        return GameModeUtils.currentGameMode == GameModeOption.Creative || isReikaPC();
    }

    public static string getCurrentSaveDir() {
        return Path.Combine(savesDir, SaveLoadManager.main.currentSlot);
    }

    public static int getInstallSeed() {
        var seed = diDLL.Location.GetHashCode();
        seed &= ~(1 << Environment.ProcessorCount);
        var n = Environment.MachineName;
        if (string.IsNullOrEmpty(n))
            n = Environment.UserName;
        seed ^= n != null ? n.GetHashCode() : 0;
        seed ^= Environment.OSVersion.VersionString.GetHashCode();
        return seed;
    }

    public static int getWorldSeedInt() {
        var seed = getWorldSeed();
        return unchecked((int)((seed & 0xFFFFFFFFL) ^ (seed >> 32)));
    }

    public static long getWorldSeed() {
        var path = SaveUtils.GetCurrentSaveDataDir();
        var seed = SaveLoadManager._main.firstStart;
        seed ^= path.GetHashCode();
        seed ^= unchecked((long)diDLL.Location.GetHashCode() << 32);
        return seed;
    }

    public static TechType getTechType(string tech) {
        if (!Enum.TryParse<TechType>(tech, false, out var ret)) {
            if (EnumHandler.TryGetValue(tech, out ret)) {
                return ret;
            } else {
                log("TechType '" + tech + "' not found!");
                //log("Tech list: "+string.Join(", ", Enum.GetNames(typeof(TechType))));
                return TechType.None;
            }
        }

        return ret;
    }

    public static void writeToChat(string s) {
        while (s.Length >= 4096) {
            var part = s.Substring(0, 4096);
            ErrorMessage.AddMessage(part);
            s = s.Substring(4096);
        }

        ErrorMessage.AddMessage(s);
    }

    public static void showPDANotification(string text, string soundPath) {
        var pda = Player.main.gameObject.AddComponent<PDANotification>();
        pda.enabled = true;
        pda.text = text;
        pda.sound = SoundManager.getSound(soundPath);
        pda.Play();
        pda.destroy(false, 15);
    }

    public static void addSelfUnlock(TechType tech, PDAManager.PDAPage page = null) {
        KnownTechHandler.SetAnalysisTechEntry(tech, new List<TechType>() { tech });
        if (page != null) {
            var e = new PDAScanner.EntryData {
                key = tech,
                scanTime = 5,
                locked = true,
            };
            page.register();
            e.encyclopedia = page.id;
            PDAHandler.AddCustomScannerEntry(e);
        }
    }

    public static StoryGoal addRadioMessage(string key, string text, string soundPath) {
        return addRadioMessage(
            key,
            text,
            SoundManager.registerPDASound(tryGetModDLL(), "radio_" + key, soundPath).asset
        );
    }

    public static StoryGoal addRadioMessage(string key, string text, FMODAsset sound) {
        var sg = new StoryGoal(key, Story.GoalType.Radio, 0);
        addVOLine(sg, text, sound);
        return sg;
    }

    public static void addVOLine(StoryGoal type, string text, FMODAsset sound) {
        addVOLine<StoryGoal>(type, text, sound);
    }

    public static void addVOLine<G>(G goal, string text, FMODAsset sound) where G : StoryGoal {
        PDAHandler.AddLogEntry(goal.key, goal.key, sound);
        CustomLocaleKeyDatabase.registerKey(goal.key, text);
    }

    public static void teleportPlayer(Player ep, Vector3 to) {
        if (ep.currentMountedVehicle != null) {
            ep.currentMountedVehicle.transform.position =
                to + (ep.currentMountedVehicle.transform.position - ep.transform.position);
        }

        ep.transform.position = to;
    }

    public static PDAManager.PDAPage addPDAEntry(
        ICustomPrefab pfb,
        float scanTime,
        string pageCategory = null,
        string pageText = null,
        string pageHeader = null,
        Action<PDAScanner.EntryData> modify = null
    ) {
        return addPDAEntry(
            pfb.Info.TechType,
            pfb.Info.ClassID,
            pfb.Info.PrefabFileName,
            scanTime,
            pageCategory,
            pageText,
            pageHeader,
            modify
        );
    }

    public static PDAManager.PDAPage addPDAEntry(
        TechType pfb,
        string id,
        string desc,
        float scanTime,
        string pageCategory = null,
        string pageText = null,
        string pageHeader = null,
        Action<PDAScanner.EntryData> modify = null
    ) {
        PDAManager.PDAPage page = null;
        if (pageCategory != null && !string.IsNullOrEmpty(pageText)) {
            page = PDAManager.createPage("ency_" + id, desc, pageText, pageCategory);
            if (pageHeader != null)
                page.setHeaderImage(TextureManager.getTexture(tryGetModDLL(), "Textures/PDA/" + pageHeader));
            page.register();
        }

        if (scanTime >= 0 && pfb != TechType.None)
            addScanUnlock(pfb, desc, scanTime, page, modify);
        return page;
    }

    public static void addScanUnlock(
        TechType pfb,
        string desc,
        float scanTime,
        PDAManager.PDAPage page = null,
        Action<PDAScanner.EntryData> modify = null
    ) {
        var e = new PDAScanner.EntryData {
            key = pfb,
            scanTime = scanTime,
            locked = true,
        };
        modify?.Invoke(e);

        if (page != null) {
            e.encyclopedia = page.id;
            log("Bound scanner entry for " + desc + " to " + page.id);
        } else {
            log("Scanner entry for " + desc + " had no ency page.");
        }

        PDAHandler.AddCustomScannerEntry(e);
    }

    public static void addMultiScanUnlock(
        TechType toScan,
        float scanTime,
        TechType unlock,
        int total,
        bool remove
    ) {
        var e = new PDAScanner.EntryData {
            key = toScan,
            scanTime = scanTime,
            locked = true,
            blueprint = unlock,
            isFragment = true,
            totalFragments = total,
            destroyAfterScan = remove,
        };
        PDAHandler.AddCustomScannerEntry(e);
    }

    public static void triggerTechPopup(TechType tt, Sprite spr = null) {
        var at = new KnownTech.AnalysisTech {
            techType = tt,
            unlockMessage = "NotificationBlueprintUnlocked",
            unlockSound = getUnlockSound(),
        };
        if (spr == null)
            spr = getTechPopupSprite(tt);
        if (spr != null)
            at.unlockPopup = spr;
        uGUI_PopupNotification.main.OnAnalyze(at, true);
    }

    public static void triggerMultiTechPopup(IEnumerable<TechType> tt) {
        var pd = new PopupData(
            "New Blueprints Unlocked",
            "<color=#74C8F8FF>" + string.Join(
                ",\n",
                tt.Select(tc => Language.main.Get(tc.AsString()))
            ) + "</color>"
        ) {
            sound = getUnlockSound().path,
        };
        triggerUnlockPopup(pd);
    }

    public static FMODAsset getUnlockSound() {
        if (unlockSound == null) {
            foreach (var kt in KnownTech.analysisTech) {
                if (kt.unlockMessage == "NotificationBlueprintUnlocked") {
                    unlockSound = kt.unlockSound;
                    break;
                }
            }
        }

        return unlockSound;
    }

    public static Sprite getTechPopupSprite(TechType tt) {
        foreach (var kt in KnownTech.analysisTech) {
            if (kt.techType == tt) {
                return kt.unlockPopup;
            }
        }

        return null;
    }

    public static void triggerUnlockPopup(PopupData data) {
        var entry = new uGUI_PopupNotification.Entry() {
            id = string.Empty,
            sound = null,
            controls = data.controlText == null ? null : data.controlText.Replace("\\n", "\n"),
            text = data.text.Replace("\\n", "\n"),
            skin = PopupNotificationSkin.Unlock,
            sprite = data.graphic != null ? data.graphic() : null,
            title = data.title.Replace("\\n", "\n"),
        };
        uGUI_PopupNotification.main.Show(entry);
        log("Showing progression popup " + data, diDLL);
        if (!string.IsNullOrEmpty(data.sound))
            SoundManager.playSound(data.sound);
    }

    public class PopupData {
        public readonly string title;
        public readonly string text;

        public string controlText = null;
        public Func<Sprite> graphic = null;
        public string sound = "event:/tools/scanner/scan_complete";
        public Action onUnlock;

        public PopupData(string t, string d) {
            title = t;
            text = d;
        }

        public override string ToString() {
            return string.Format(
                "[PopupData Title='{0}'; Text='{1}'; ControlText='{2}'; Graphic={3}; Sound={4}]",
                title,
                text,
                controlText,
                graphic,
                sound
            );
        }
    }

    public static void shakeCamera(float duration, float intensity, float frequency = 1) {
        //Camera.main.gameObject.EnsureComponent<CameraShake>().fire(intensity, duration, falloff);
        var cam = Player.main.GetComponentInChildren<MainCameraControl>();
        cam.ShakeCamera(intensity, duration, MainCameraControl.ShakeMode.BuildUp, frequency);
    }
    /*
    private class CameraShake : MonoBehaviour {

        private float duration;
        private float intensity;
        private float falloff;

        private Vector3 originalPosition;
        private float durationToGo;

        internal void fire(float i, float d, float f) {
            originalPosition = transform.position;
            duration = d;
            durationToGo = d;
            intensity = i;
            falloff = f;
        }

        void Update() {
            if (durationToGo > 0) {
                float i = Mathf.Lerp(0, intensity, durationToGo/duration);
                transform.position = originalPosition+UnityEngine.Random.insideUnitSphere*i;
                durationToGo -= Time.deltaTime*falloff;
            }
            else {
                transform.position = originalPosition;
                this.destroy(false);
            }
        }
    }*/

    public static int getFragmentScanCount(TechType tt) {
        return PDAScanner.GetPartialEntryByKey(tt, out var entry)
            ? entry == null ? 0 : entry.unlocked
            : 0;
    }

    public static void addEncyNotification(string id, float duration = 3) {
        NotificationManager.main.Add(NotificationManager.Group.Encyclopedia, id, duration);
    }

    public static void addBlueprintNotification(TechType recipe, float duration = 3) {
        NotificationManager.main.Add(NotificationManager.Group.Blueprints, recipe.EncodeKey(), duration);
    }

    public static void addInventoryNotification(Pickupable item, float duration = 3) {
        NotificationManager.main.Add(
            NotificationManager.Group.Inventory,
            item.GetComponent<UniqueIdentifier>().Id,
            duration
        );
    }

    public static void createPopupWarning(string msg, bool makeBlue) {
        /*
                        QModManager.Patching.Patcher.Dialogs.Add(new Dialog
                        {
                            message = text,
                            leftButton = Dialog.Button.SeeLog,
                            rightButton = Dialog.Button.Close,
                            color = Dialog.DialogColor.Red
                        });
        */
        var patcher = InstructionHandlers.getTypeBySimpleName("QModManager.Patching.Patcher");
        var dlgType = InstructionHandlers.getTypeBySimpleName("QModManager.Utility.Dialog");
        var btnType = dlgType.GetNestedType("Button", BindingFlags.NonPublic);
        var dialogs = (IList)patcher.GetProperty("Dialogs", BindingFlags.Static | BindingFlags.NonPublic)
            .GetValue(null);
        var dlg = Activator.CreateInstance(dlgType);
        dlgType.GetField("message", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dlg, msg);
        dlgType.GetField("color", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dlg, makeBlue ? 1 : 0);
        dlgType.GetField("leftButton", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
            dlg,
            btnType.GetField("SeeLog", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)
        );
        dlgType.GetField("rightButton", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
            dlg,
            btnType.GetField("Close", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)
        );
        dialogs.Add(dlg);
    }

    public static bool checkPiracy() {
        HashSet<string> files = [
            "steam_api64.cdx", "steam_api64.ini", "steam_emu.ini", "valve.ini", "chuj.cdx", "SteamUserID.cfg",
            "Achievements.bin", "steam_settings", "user_steam_id.txt", "account_name.txt", "ScreamAPI.dll",
            "ScreamAPI32.dll", "ScreamAPI64.dll", "SmokeAPI.dll", "SmokeAPI32.dll", "SmokeAPI64.dll",
            "Free Steam Games Pre-installed for PC.url", "Torrent-Igruha.Org.URL", "oalinst.exe",
        ];
        foreach (var file in files) {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, file)))
                return true;
        }

        return false;
    }

    public static void vomit(Survival s, float food, float water) {
        s.food = Mathf.Max(1, s.food - food);
        s.water = Mathf.Max(1, s.water - water);
        SoundManager.playSoundAt(
            SoundManager.buildSound(
                Player.main.IsUnderwater() ? "event:/player/Puke_underwater" : "event:/player/Puke"
            ),
            Player.main.transform.position,
            false,
            12
        );
        PlayerMovementSpeedModifier.add(0.15F, 1.25F);
        MainCameraControl.main.ShakeCamera(
            2F,
            1.0F,
            MainCameraControl.ShakeMode.Linear,
            0.25F
        ); //SNUtil.shakeCamera(1.2F, 0.5F, 0.2F);
    }

    // public static CustomPrefab getModPrefabByTechType(this TechType tt) {
    //     Dictionary<string, ModPrefab> dict = (Dictionary<string, ModPrefab>)typeof(ModPrefab).GetField(
    //         "ClassIdDictionary",
    //         BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
    //     ).GetValue(null);
    //     return dict.ContainsKey(tt.AsString()) ? dict[tt.AsString()] : null;
    // }

    // public static Sprite getPrefabSprite(this CustomPrefab mod) {
    //     return (Sprite)typeof(Spawnable)
    //         .GetMethod(
    //             "GetItemSprite",
    //             BindingFlags.Instance | BindingFlags.NonPublic,
    //             null,
    //             CallingConventions.HasThis,
    //             new Type[0],
    //             null
    //         ).Invoke(mod, BindingFlags.Default, null, new object[0], null);
    // }

    public static WaterParkCreatureData getModifiedACUParams(
        TechType basis,
        float initSizeScale,
        float maxSizeScale,
        float outsideSizeScale,
        float growTimeScale
    ) {
        var prefab = CraftData.GetPrefabForTechTypeAsync(basis).GetResult();
        var creature = prefab.GetComponent<WaterParkCreature>();
        var baseP = creature.data;

        var mod = ScriptableObject.CreateInstance<WaterParkCreatureData>();

        mod.initialSize = baseP.initialSize * initSizeScale;
        mod.maxSize = baseP.maxSize * maxSizeScale;
        mod.outsideSize = baseP.outsideSize * maxSizeScale;
        mod.daysToGrow = baseP.daysToGrow * 1200f * growTimeScale / 1200f;
        mod.isPickupableOutside = baseP.isPickupableOutside;

        return mod;
    }

    public static string getDescriptiveEncyPageCategoryName(PDAEncyclopedia.EntryData data) {
        var lifeform = data.nodes.Length >= 2 && data.nodes[0] == "Lifeforms";
        return lifeform
            ? Language.main.Get("EncyPath_" + data.nodes[0] + "/" + data.nodes[1])
            : Language.main.Get("EncyPath_" + data.nodes[0]);
    }

    public static MapRoomCamera getControllingCamera(Player ep) {
        foreach (MapRoomCamera cam in MapRoomCamera.cameras) {
            // if (cam && cam.controllingPlayer == ep)
            //     return cam;

            if (cam && cam.active)
                return cam;
        }

        return null;
    }

    public static TechType addTechTypeToVanillaPrefabs(XMLLocale.LocaleEntry e, params string[] prefabs) {
        return addTechTypeToVanillaPrefabs(e.key, e.name, e.desc, prefabs);
    }

    public static TechType addTechTypeToVanillaPrefabs(
        string key,
        string name,
        string desc,
        params string[] prefabs
    ) {
        TechType ret = EnumHandler.AddEntry<TechType>(key).WithPdaInfo(name, desc);
        foreach (var pfb in prefabs)
            CraftData.entClassTechTable[pfb] = ret;
        return ret;
    }

    public static void setBlueprintUnlockProgress(PDAScanner.EntryData entryData, int steps) {
        if (!PDAScanner.GetPartialEntryByKey(entryData.key, out var entry)) {
            entry = PDAScanner.Add(entryData.key, 0);
        }

        if (entry == null)
            return;
        entry.unlocked = steps;
        if (entry.unlocked >= entryData.totalFragments) {
            PDAScanner.partial.Remove(entry);
            PDAScanner.complete.Add(entry.techType);
            PDAScanner.NotifyRemove(entry);
        } else {
            PDAScanner.NotifyProgress(entry);
        }
    }

    public static bool match(string s, string seek) {
        return s == seek ||
               (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(seek) && seek[0] == '*' && s.EndsWith(
                   seek.Substring(1),
                   StringComparison.InvariantCulture
               )) || (seek[seek.Length - 1] == '*' && s.StartsWith(
                   seek.Substring(0, seek.Length - 1),
                   StringComparison.InvariantCulture
               ));
    }

    public static bool match(PrefabIdentifier pi, string id) {
        return pi && match(pi.ClassId, id);
    }

    public static bool match(GameObject go, TechType tt) {
        return CraftData.GetTechType(go) == tt;
    }

    public static bool match(GameObject go, params TechType[] tts) {
        var has = CraftData.GetTechType(go);
        foreach (var tt in tts)
            if (has == tt)
                return true;
        return false;
    }

    public static UnityEngine.UI.Button createPDAUIButtonUnderTab<T>(Texture2D ico, Action onClick)
        where T : uGUI_PDATab {
        return createPDAUIButton(ico, onClick, uGUI_PDA.main ? uGUI_PDA.main.GetComponentInChildren<T>() : null);
    }

    public static UnityEngine.UI.Button createPDAUIButton(Texture2D ico, Action onClick, uGUI_PDATab tab = null) {
        if (!uGUI_PDA.main)
            return null;
        var go = uGUI_PDA.main.gameObject.getChildObject("Content/PingManagerTab/Content/ButtonAll");
        var go2 = go.clone();
        var t = uGUI_PDA.main.transform;
        if (tab) {
            var content = tab.gameObject.getChildObject("Content");
            t = content ? content.transform : tab.transform;
        }

        go2.transform.SetParent(t, false);
        var tg = go2.GetComponent<UnityEngine.UI.Toggle>();
        var sprs = tg.spriteState;
        var hover = sprs.highlightedSprite;
        var tr = tg.transition;
        tg.destroy();
        var img = go2.GetComponent<UnityEngine.UI.Image>();
        var icon = img.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
        if (ico != null)
            icon.sprite = Sprite.Create(ico, new Rect(0, 0, ico.width, ico.height), Vector2.zero);
        var b = go2.EnsureComponent<UnityEngine.UI.Button>();
        b.image = img;
        b.onClick.AddListener(() => onClick.Invoke());
        var sprs2 = b.spriteState;
        sprs2.highlightedSprite = hover;
        sprs2.selectedSprite = hover;
        b.spriteState = sprs2;
        b.transition = tr;
        var rt = b.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(ico.width, ico.height);
        rt = icon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(ico.width, ico.height);
        return b;
    }

    public static bool isPlayerCured() {
        return StoryGoalManager.main.completedGoals.Contains(StoryGoals.CURED);
    }

    public static bool isSunbeamExpected() {
        return StoryGoalManager.main.IsGoalComplete(StoryGoals.getRadioPlayGoal(StoryGoals.SUNBEAM_TIMER_START)) &&
               !StoryGoalManager.main.IsGoalComplete(StoryGoals.SUNBEAM_DESTROY_FAR) &&
               !StoryGoalManager.main.IsGoalComplete(StoryGoals.SUNBEAM_DESTROY_NEAR);
    }

    public static void migrateSaveDataFolder(string oldSaveDir, string ext, string saveFileName) {
        if (Directory.Exists(oldSaveDir) && Directory.Exists(savesDir)) {
            log("Migrating save data from " + oldSaveDir + " to " + savesDir);
            var all = true;
            foreach (var dat in Directory.GetFiles(oldSaveDir)) {
                if (dat.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)) {
                    var save = Path.Combine(savesDir, Path.GetFileNameWithoutExtension(dat));
                    if (Directory.Exists(save)) {
                        log("Moving save data " + dat + " to " + save);
                        File.Move(dat, Path.Combine(save, saveFileName));
                    } else {
                        log("No save found for '" + dat + ", skipping");
                        all = false;
                    }
                }
            }

            log("Migration complete.");
            if (all) {
                log("All files moved, deleting old folder.");
                Directory.Delete(oldSaveDir);
            } else {
                log("Some files could not be moved so the old folder will not be deleted.");
            }
        }
    }
}