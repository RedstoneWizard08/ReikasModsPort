using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Handlers;
using Nautilus.Utility;
using Story;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class SNUtil {
    private static FMODAsset _unlockSound;

    public static readonly Assembly DiDLL = Assembly.GetExecutingAssembly();
    public static readonly Assembly NautilusDLL = Assembly.GetAssembly(typeof(CustomPrefab));
    public static readonly Assembly GameDLL = Assembly.GetAssembly(typeof(BoneShark));
    public static readonly Assembly GameDLL2 = Assembly.GetAssembly(typeof(FMODAsset));

    public static readonly string
        GameDir = Directory.GetParent(GameDLL.Location)!.Parent!.Parent!.FullName; //managed -> _Data -> root

    public static readonly string SavesDir = Path.Combine(GameDir, "SNAppData/SavedGames");

    public static bool AllowDidll = false;

    private static bool _checkedReikaPC;
    private static bool _savedIsReikaPC;

    private static readonly HashSet<Assembly> AssembliesToSkip = [
        DiDLL,
        NautilusDLL,
        GameDLL,
        GameDLL2,
    ];

    static SNUtil() {
    }

    private static bool EvaluateReikaPC() {
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

    public static bool IsReikaPC() {
        if (!_checkedReikaPC)
            _savedIsReikaPC = EvaluateReikaPC();
        _checkedReikaPC = true;
        return _savedIsReikaPC;
    }

    public static string GetStacktrace(StackFrame[] sf = null) {
        sf ??= new StackTrace().GetFrames();

        return string.Join(
            "\n",
            sf?.Skip(1).Select(s => s.GetMethod() + " in " + s.GetMethod().DeclaringType) ?? []
        );
    }

    internal static Assembly TryGetModDLL(bool acceptDi = false) {
        try {
            var di = Assembly.GetExecutingAssembly();
            var frames = new StackTrace().GetFrames();
            
            if (frames == null || frames.Length == 0)
                return Assembly.GetCallingAssembly();

            foreach (var frame in frames) {
                var asm = frame.GetMethod().DeclaringType?.Assembly;
                var diCheck = asm != di || acceptDi || AllowDidll;
                var libCheck = asm != NautilusDLL && asm != GameDLL && asm != GameDLL2;
                var loc = (asm?.Location ?? "").ToLower();
                
                if (diCheck && libCheck && loc.Contains("bepinex") && loc.Contains("plugins"))
                    return asm;
            }

            Log("Could not find valid mod assembly: " + GetStacktrace(frames), DiDLL);
        } catch (Exception e) {
            Log("Failed to find a DLL due to an exception: " + e, DiDLL);
            return DiDLL;
        }

        return Assembly.GetCallingAssembly();
    }

    public static void Log(string s, Assembly a = null, int indent = 0) {
        while (s.Length > 4096) {
            var part = s[..4096];
            Log(part, a);
            s = s[4096..];
        }

        var id = (a != null ? a : TryGetModDLL(true)).GetName().Name.ToUpperInvariant().Replace("PLUGIN_", "");
        if (indent > 0) {
            s = s.PadLeft(s.Length + indent, ' ');
        }

        UnityEngine.Debug.Log(id + ": " + s);
    }

    public static bool CanUseDebug() {
        return GameModeUtils.currentGameMode == GameModeOption.Creative || IsReikaPC();
    }

    public static string GetCurrentSaveDir() {
        return Path.Combine(SavesDir, SaveLoadManager.main.currentSlot);
    }

    public static int GetInstallSeed() {
        var seed = DiDLL.Location.GetHashCode();
        seed &= ~(1 << Environment.ProcessorCount);
        var n = Environment.MachineName;
        if (string.IsNullOrEmpty(n))
            n = Environment.UserName;
        seed ^= n.GetHashCode();
        seed ^= Environment.OSVersion.VersionString.GetHashCode();
        return seed;
    }

    public static int GetWorldSeedInt() {
        var seed = GetWorldSeed();
        return unchecked((int)((seed & 0xFFFFFFFFL) ^ (seed >> 32)));
    }

    public static long GetWorldSeed() {
        var path = SaveUtils.GetCurrentSaveDataDir();
        var seed = SaveLoadManager._main.firstStart;
        seed ^= path.GetHashCode();
        seed ^= (long)DiDLL.Location.GetHashCode() << 32;
        return seed;
    }

    public static TechType GetTechType(string tech) {
        if (Enum.TryParse<TechType>(tech, false, out var ret)) return ret;
        if (EnumHandler.TryGetValue(tech, out ret)) {
        } else {
            Log("TechType '" + tech + "' not found!");
            //log("Tech list: "+string.Join(", ", Enum.GetNames(typeof(TechType))));
            return TechType.None;
        }

        return ret;
    }

    public static void WriteToChat(string s) {
        while (s.Length >= 4096) {
            var part = s[..4096];
            ErrorMessage.AddMessage(part);
            s = s[4096..];
        }

        ErrorMessage.AddMessage(s);
    }

    public static void ShowPdaNotification(string text, string soundPath) {
        var pda = Player.main.gameObject.AddComponent<PDANotification>();
        pda.enabled = true;
        pda.text = text;
        pda.sound = SoundManager.getSound(soundPath);
        pda.Play();
        pda.destroy(false, 15);
    }

    public static void AddSelfUnlock(TechType tech, PDAManager.PDAPage page = null) {
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

    public static StoryGoal AddRadioMessage(string key, string text, string soundPath) {
        return AddRadioMessage(
            key,
            text,
            SoundManager.registerPDASound(TryGetModDLL(), "radio_" + key, soundPath).asset
        );
    }

    public static StoryGoal AddRadioMessage(string key, string text, FMODAsset sound) {
        var sg = new StoryGoal(key, Story.GoalType.Radio, 0);
        AddVoLine(sg, text, sound);
        return sg;
    }

    public static void AddVoLine(StoryGoal type, string text, FMODAsset sound) {
        AddVoLine<StoryGoal>(type, text, sound);
    }

    public static void AddVoLine<TG>(TG goal, string text, FMODAsset sound) where TG : StoryGoal {
        PDAHandler.AddLogEntry(goal.key, goal.key, sound);
        CustomLocaleKeyDatabase.registerKey(goal.key, text);
    }

    public static void TeleportPlayer(Player ep, Vector3 to) {
        if (ep.currentMountedVehicle != null) {
            ep.currentMountedVehicle.transform.position =
                to + (ep.currentMountedVehicle.transform.position - ep.transform.position);
        }

        ep.transform.position = to;
    }

    public static PDAManager.PDAPage AddPdaEntry(
        ICustomPrefab pfb,
        float scanTime,
        string pageCategory = null,
        string pageText = null,
        string pageHeader = null,
        Action<PDAScanner.EntryData> modify = null
    ) {
        return AddPdaEntry(
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

    public static PDAManager.PDAPage AddPdaEntry(
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
                page.setHeaderImage(TextureManager.getTexture(TryGetModDLL(), "Textures/PDA/" + pageHeader));
            page.register();
        }

        if (scanTime >= 0 && pfb != TechType.None)
            AddScanUnlock(pfb, desc, scanTime, page, modify);
        return page;
    }

    public static void AddScanUnlock(
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
            Log("Bound scanner entry for " + desc + " to " + page.id);
        } else {
            Log("Scanner entry for " + desc + " had no ency page.");
        }

        PDAHandler.AddCustomScannerEntry(e);
    }

    public static void AddMultiScanUnlock(
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

    public static void TriggerTechPopup(TechType tt, Sprite spr = null) {
        var at = new KnownTech.AnalysisTech {
            techType = tt,
            unlockMessage = "NotificationBlueprintUnlocked",
            unlockSound = GetUnlockSound(),
        };
        if (spr == null)
            spr = GetTechPopupSprite(tt);
        if (spr != null)
            at.unlockPopup = spr;
        uGUI_PopupNotification.main.OnAnalyze(at, true);
    }

    public static void TriggerMultiTechPopup(IEnumerable<TechType> tt) {
        var pd = new PopupData(
            "New Blueprints Unlocked",
            "<color=#74C8F8FF>" + string.Join(
                ",\n",
                tt.Select(tc => Language.main.Get(tc.AsString()))
            ) + "</color>"
        ) {
            Sound = GetUnlockSound().path,
        };
        TriggerUnlockPopup(pd);
    }

    public static FMODAsset GetUnlockSound() {
        if (_unlockSound != null) return _unlockSound;
        foreach (var kt in KnownTech.analysisTech.Where(kt => kt.unlockMessage == "NotificationBlueprintUnlocked")) {
            _unlockSound = kt.unlockSound;
            break;
        }

        return _unlockSound;
    }

    public static Sprite GetTechPopupSprite(TechType tt) {
        return (from kt in KnownTech.analysisTech where kt.techType == tt select kt.unlockPopup).FirstOrDefault();
    }

    public static void TriggerUnlockPopup(PopupData data) {
        var entry = new uGUI_PopupNotification.Entry() {
            id = string.Empty,
            sound = null,
            controls = data.ControlText?.Replace("\\n", "\n"),
            text = data.Text.Replace("\\n", "\n"),
            skin = PopupNotificationSkin.Unlock,
            sprite = data.Graphic?.Invoke(),
            title = data.Title.Replace("\\n", "\n"),
        };
        uGUI_PopupNotification.main.Show(entry);
        Log("Showing progression popup " + data, DiDLL);
        if (!string.IsNullOrEmpty(data.Sound))
            SoundManager.playSound(data.Sound);
    }

    public class PopupData(string t, string d) {
        public readonly string Title = t;
        public readonly string Text = d;

        public string ControlText = null;
        public Func<Sprite> Graphic = null;
        public string Sound = "event:/tools/scanner/scan_complete";
        public Action OnUnlock;

        public override string ToString() {
            return string.Format(
                "[PopupData Title='{0}'; Text='{1}'; ControlText='{2}'; Graphic={3}; Sound={4}]",
                Title,
                Text,
                ControlText,
                Graphic,
                Sound
            );
        }
    }

    public static void ShakeCamera(float duration, float intensity, float frequency = 1) {
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

    public static int GetFragmentScanCount(TechType tt) {
        return PDAScanner.GetPartialEntryByKey(tt, out var entry)
            ? entry?.unlocked ?? 0
            : 0;
    }

    public static void AddEncyNotification(string id, float duration = 3) {
        NotificationManager.main.Add(NotificationManager.Group.Encyclopedia, id, duration);
    }

    public static void AddBlueprintNotification(TechType recipe, float duration = 3) {
        NotificationManager.main.Add(NotificationManager.Group.Blueprints, recipe.EncodeKey(), duration);
    }

    public static void AddInventoryNotification(Pickupable item, float duration = 3) {
        NotificationManager.main.Add(
            NotificationManager.Group.Inventory,
            item.GetComponent<UniqueIdentifier>().Id,
            duration
        );
    }

    public static void CreatePopupWarning(string msg, bool makeBlue) {
        /*
                        QModManager.Patching.Patcher.Dialogs.Add(new Dialog
                        {
                            message = text,
                            leftButton = Dialog.Button.SeeLog,
                            rightButton = Dialog.Button.Close,
                            color = Dialog.DialogColor.Red
                        });
        */
        var patcher = InstructionHandlers.GetTypeBySimpleName("QModManager.Patching.Patcher");
        var dlgType = InstructionHandlers.GetTypeBySimpleName("QModManager.Utility.Dialog");
        var btnType = dlgType.GetNestedType("Button", BindingFlags.NonPublic);
        var dialogs = (IList)patcher.GetProperty("Dialogs", BindingFlags.Static | BindingFlags.NonPublic)
            ?.GetValue(null);
        var dlg = Activator.CreateInstance(dlgType);
        dlgType.GetField("message", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dlg, msg);
        dlgType.GetField("color", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dlg, makeBlue ? 1 : 0);
        dlgType.GetField("leftButton", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(
                dlg,
                btnType.GetField("SeeLog", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)
            );
        dlgType.GetField("rightButton", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(
                dlg,
                btnType.GetField("Close", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)
            );
        dialogs?.Add(dlg);
    }

    public static bool CheckPiracy() {
        HashSet<string> files = [
            "steam_api64.cdx", "steam_api64.ini", "steam_emu.ini", "valve.ini", "chuj.cdx", "SteamUserID.cfg",
            "Achievements.bin", "steam_settings", "user_steam_id.txt", "account_name.txt", "ScreamAPI.dll",
            "ScreamAPI32.dll", "ScreamAPI64.dll", "SmokeAPI.dll", "SmokeAPI32.dll", "SmokeAPI64.dll",
            "Free Steam Games Pre-installed for PC.url", "Torrent-Igruha.Org.URL", "oalinst.exe",
        ];
        return files.Any(file => File.Exists(Path.Combine(Environment.CurrentDirectory, file)));
    }

    public static void Vomit(Survival s, float food, float water) {
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

    public static WaterParkCreatureData GetModifiedAcuParams(
        TechType basis,
        float initSizeScale,
        float maxSizeScale,
        float outsideSizeScale,
        float growTimeScale
    ) {
        var prefab = PrefabUtil.GetPrefabForTechType(basis);
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

    public static string GetDescriptiveEncyPageCategoryName(PDAEncyclopedia.EntryData data) {
        var lifeform = data.nodes.Length >= 2 && data.nodes[0] == "Lifeforms";
        return lifeform
            ? Language.main.Get("EncyPath_" + data.nodes[0] + "/" + data.nodes[1])
            : Language.main.Get("EncyPath_" + data.nodes[0]);
    }

    public static MapRoomCamera GetControllingCamera(Player ep) {
        foreach (var cam in MapRoomCamera.cameras) {
            // if (cam && cam.controllingPlayer == ep)
            //     return cam;

            if (cam && cam.active)
                return cam;
        }

        return null;
    }

    public static TechType AddTechTypeToVanillaPrefabs(XMLLocale.LocaleEntry e, params string[] prefabs) {
        return AddTechTypeToVanillaPrefabs(e.key, e.name, e.desc, prefabs);
    }

    public static TechType AddTechTypeToVanillaPrefabs(
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

    public static void SetBlueprintUnlockProgress(PDAScanner.EntryData entryData, int steps) {
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

    public static bool Match(string s, string seek) {
        return s == seek ||
               (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(seek) && seek[0] == '*' && s.EndsWith(
                   seek[1..],
                   StringComparison.InvariantCulture
               )) || (seek[^1] == '*' && s.StartsWith(
                   seek[..^1],
                   StringComparison.InvariantCulture
               ));
    }

    public static bool Match(PrefabIdentifier pi, string id) {
        return pi && Match(pi.ClassId, id);
    }

    public static bool Match(GameObject go, TechType tt) {
        return CraftData.GetTechType(go) == tt;
    }

    public static bool Match(GameObject go, params TechType[] tts) {
        var has = CraftData.GetTechType(go);
        foreach (var tt in tts)
            if (has == tt)
                return true;
        return false;
    }

    public static UnityEngine.UI.Button CreatePdauiButtonUnderTab<T>(Texture2D ico, Action onClick)
        where T : uGUI_PDATab {
        return CreatePdauiButton(ico, onClick, uGUI_PDA.main ? uGUI_PDA.main.GetComponentInChildren<T>() : null);
    }

    public static UnityEngine.UI.Button CreatePdauiButton(Texture2D ico, Action onClick, uGUI_PDATab tab = null) {
        if (!uGUI_PDA.main)
            return null;
        var go = uGUI_PDA.main.gameObject.getChildObject("Content/PingTab/Content/ButtonAll");
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
        b.onClick.AddListener(onClick.Invoke);
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

    public static bool IsPlayerCured() {
        return StoryGoalManager.main.completedGoals.Contains(StoryGoals.CURED);
    }

    public static bool IsSunbeamExpected() {
        return StoryGoalManager.main.IsGoalComplete(StoryGoals.getRadioPlayGoal(StoryGoals.SUNBEAM_TIMER_START)) &&
               !StoryGoalManager.main.IsGoalComplete(StoryGoals.SUNBEAM_DESTROY_FAR) &&
               !StoryGoalManager.main.IsGoalComplete(StoryGoals.SUNBEAM_DESTROY_NEAR);
    }

    public static void MigrateSaveDataFolder(string oldSaveDir, string ext, string saveFileName) {
        if (Directory.Exists(oldSaveDir) && Directory.Exists(SavesDir)) {
            Log("Migrating save data from " + oldSaveDir + " to " + SavesDir);
            var all = true;
            foreach (var dat in Directory.GetFiles(oldSaveDir)) {
                if (dat.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)) {
                    var save = Path.Combine(SavesDir, Path.GetFileNameWithoutExtension(dat));
                    if (Directory.Exists(save)) {
                        Log("Moving save data " + dat + " to " + save);
                        File.Move(dat, Path.Combine(save, saveFileName));
                    } else {
                        Log("No save found for '" + dat + ", skipping");
                        all = false;
                    }
                }
            }

            Log("Migration complete.");
            if (all) {
                Log("All files moved, deleting old folder.");
                Directory.Delete(oldSaveDir);
            } else {
                Log("Some files could not be moved so the old folder will not be deleted.");
            }
        }
    }
}