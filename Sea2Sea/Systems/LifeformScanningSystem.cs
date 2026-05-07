using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Nautilus.Utility;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class LifeformScanningSystem {
    public static readonly LifeformScanningSystem instance = new();

    internal static readonly string NEED_SCANS_PDA = "needencyscans";

    internal static readonly string UNEXPLORED_LOCATION_TEXT = "Unexplored Area";

    private readonly string oldSaveDir;
    private readonly string saveFileName = "lifeform_scans.dat";

    //private static readonly Regex eggRegex = new Regex("(?i).*\begg\b.*(?-i)");

    private readonly Dictionary<TechType, LifeformEntry> requiredLifeforms = new();
    private readonly SortedDictionary<string, List<LifeformEntry>> byCategory = new();

    private readonly HashSet<TechType> additionalScans = [
        TechType.HugeSkeleton,
        TechType.CaveSkeleton,
        TechType.PrecursorSeaDragonSkeleton,
        TechType.ReaperSkeleton,
    ];

    private float needsPDAUpdate = -1;

    private float lastAoECheckTime = -1;

    public static bool showAll = false;

    private LifeformScanningSystem() {
        oldSaveDir = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "lifeform_scans"
        );
        if (Directory.Exists(oldSaveDir) && Directory.Exists(SNUtil.SavesDir)) {
            migrateSaveData();
        }
    }

    public void register() {
        //loaded on first use, not load event IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
        SaveUtils.RegisterOnSaveEvent(save);
    }

    public void tick(float time) {
        if (!Story.StoryGoalManager.main.IsGoalComplete("Goal_Scanner")) {
            return;
        }

        if (needsPDAUpdate >= 0 && time >= needsPDAUpdate) {
            PDAManager.getPage(NEED_SCANS_PDA).update(generatePDAContent(), true);
            PDAManager.getPage(NEED_SCANS_PDA).unlock();
            needsPDAUpdate = -1;
        }

        if (time - lastAoECheckTime >= 1.0F) {
            lastAoECheckTime = time;
            WorldUtil.getGameObjectsNear(
                Player.main.transform.position,
                60,
                go => {
                    if (go.isVisible()) {
                        onObjectSeen(go, false);
                    }
                }
            );
        }
    }

    private void migrateSaveData() {
        SNUtil.Log("Migrating lifeform scan data from " + oldSaveDir + " to " + SNUtil.SavesDir);
        var all = true;
        foreach (var xml in Directory.GetFiles(oldSaveDir)) {
            if (xml.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)) {
                var save = Path.Combine(SNUtil.SavesDir, Path.GetFileNameWithoutExtension(xml));
                if (Directory.Exists(save)) {
                    SNUtil.Log("Moving lifeform scan data " + xml + " to " + save);
                    File.Move(xml, Path.Combine(save, saveFileName));
                } else {
                    SNUtil.Log("No save found for '" + xml + ", skipping");
                    all = false;
                }
            }
        }

        SNUtil.Log("Migration complete.");
        if (all) {
            SNUtil.Log("All files moved, deleting old folder.");
            Directory.Delete(oldSaveDir);
        } else {
            SNUtil.Log("Some files could not be moved so the old folder will not be deleted.");
        }
    }

    private void loadSave() {
        var path = Path.Combine(SNUtil.GetCurrentSaveDir(), saveFileName);
        if (File.Exists(path)) {
            var doc = new XmlDocument();
            doc.Load(path);
            foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
                var tt = SNUtil.GetTechType(e.GetProperty("techtype"));
                if (tt != TechType.None && requiredLifeforms.ContainsKey(tt))
                    requiredLifeforms[tt].loadFromXML(e);
            }
        }

        SNUtil.Log("Loaded lifeform scan cache: ");
        SNUtil.Log(requiredLifeforms.ToDebugString());
    }

    private void save() {
        var path = Path.Combine(SNUtil.GetCurrentSaveDir(), saveFileName);
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        doc.AppendChild(rootnode);
        foreach (var le in requiredLifeforms.Values) {
            var e = doc.CreateElement("entry");
            le.saveToXML(e);
            doc.DocumentElement.AppendChild(e);
        }

        doc.Save(path);
    }

    public void onScanComplete(PDAScanner.EntryData data) {
        needsPDAUpdate = DayNightCycle.main.timePassedAsFloat + 1;
    }

    private string generatePDAContent() {
        getRequiredLifeforms();
        var desc = SeaToSeaMod.PdaLocale.getEntry(NEED_SCANS_PDA).pda + "\n";
        foreach (var kvp in byCategory) {
            desc += kvp.Key + ":\n";
            foreach (var le in kvp.Value) {
                var has = le.isScanned();
                var seen = le.getLastSeen();
                var name = has || le.isIdentityKnown() ? Language.main.Get(le.objectType) : le.getHint(seen != null);
                if (showAll)
                    name += " [" + le.objectType.AsString() + "=" + Language.main.Get(le.objectType) + "]";
                var color = has ? "20FF40" : seen != null ? "FFE020" : "FF2040";
                desc +=
                    $"\t<color=#{color}>{name}</color> ({(has ? "Analyzed" : seen != null ? "Last Seen Near " + seen : "Not Yet Encountered")})\n";
            }

            desc += "\n\n";
        }

        return desc;
    }

    public bool hasScannedEverything() {
        foreach (var tt in getRequiredLifeforms()) {
            if (!requiredLifeforms[tt].isScanned())
                return false;
        }

        return true;
    }

    private IEnumerable<TechType> getRequiredLifeforms() {
        if (requiredLifeforms.Count == 0) {
            foreach (var tt in PDAScanner.mapping.Keys) {
                if (!isDummiedOut(tt) && mustScanToLeave(tt)) {
                    var le = new LifeformEntry(tt);
                    requiredLifeforms[tt] = le;
                    addOrCreateEntry(le);
                    //SNUtil.log("Adding "+le.objectType.AsString()+" to lifeform scanning system: "+le.category);
                }
            }

            loadSave();
        }

        return requiredLifeforms.Keys;
    }

    private void addOrCreateEntry(LifeformEntry le) {
        if (byCategory.ContainsKey(le.category)) {
            byCategory[le.category].Add(le);
            byCategory[le.category].Sort();
        } else {
            byCategory[le.category] = [le];
        }
    }

    internal bool isDummiedOut(TechType tt) {
        return (tt == C2CItems.voidSpikeLevi.TechType && !VoidSpikeLeviathanSystem.instance.isLeviathanEnabled()) ||
               tt == TechType.BasaltChunk || tt == TechType.SeaEmperor || tt == TechType.SeaEmperorJuvenile ||
               tt == TechType.BloodGrass || tt == TechType.SmallFan;
    }

    internal bool mustScanToLeave(TechType tt) {
        var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
        if (tt == C2CItems.brineCoral || tt == C2CItems.emperorRootCommon)
            return true;
        if (tt == Ecocean.EcoceanMod.lavaBomb.TechType)
            return hard;
        if (CustomMaterials.getItemByTech(tt) != null)
            return true;
        if (BasicCustomPlant.getPlant(tt) != null)
            return true;
        if (tt == Ecocean.EcoceanMod.glowOil.TechType ||
            tt == Ecocean.EcoceanMod.tongue.TechType) //NOT the hand collected one or the abyssal terror!
            return false;
        if (tt == Ecocean.EcoceanMod.naturalOil.TechType || tt == Ecocean.EcoceanMod.celeryTree ||
            tt == Ecocean.EcoceanMod.piezo.TechType || tt == Ecocean.EcoceanMod.plankton.TechType ||
            tt == Ecocean.EcoceanMod.voidBubble.TechType)
            return true;
        if (DeIntegrationSystem.Instance.IsLoaded() &&
            tt == DeIntegrationSystem.Instance.GetVoidThalassacean().TechType)
            return true;
        var pfb = CraftData.GetClassIdForTechType(tt);
        if (pfb != null && VanillaFlora.getFromID(pfb) != null)
            return true;
        if (pfb != null && VanillaResources.getFromID(pfb) != null)
            return true;
        if (hard && additionalScans.Contains(tt))
            return true;
        var prefab = ObjectUtil.lookupPrefab(tt);
        if (prefab) {
            if (prefab.GetComponent<Creature>())
                return true;
            if (SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE) &&
                CraftData.GetTechType(prefab) == tt && prefab.GetComponentInChildren<Collider>()) { //interactable
                var key = PDAScanner.GetEntryData(tt).encyclopedia;
                if (!string.IsNullOrEmpty(key) && PDAEncyclopedia.mapping.ContainsKey(key)) { //scannable
                    var ed = PDAEncyclopedia.mapping[key];
                    if (ed != null && ed.key != "") { //has page, skip one dummied page
                        if (ed.path.StartsWith("planetarygeology", StringComparison.InvariantCultureIgnoreCase) ||
                            ed.path.StartsWith("lifeforms", StringComparison.InvariantCultureIgnoreCase))
                            return true;
                    }
                }
            }
        }

        return false;
    }

    internal void onObjectSeen(GameObject go, bool identity, bool allowACU = false) {
        //call from getting attacked by, from mousing over, from touching, entering their ACU
        getRequiredLifeforms(); //populate list
        var tt = CraftData.GetTechType(go);
        if (requiredLifeforms.ContainsKey(tt)) {
            if (allowACU || !go.GetComponent<WaterParkItem>()) {
                requiredLifeforms[tt].seeAt(go, identity);
                needsPDAUpdate = DayNightCycle.main.timePassedAsFloat + 0.5F;
            }
        }
    }

    internal void onBiomeDiscovered() {
        needsPDAUpdate = DayNightCycle.main.timePassedAsFloat + 0.5F;
    }

    public string getLocalDescription(Vector3 pos) {
        var bb = BiomeBase.GetBiome(pos);
        string ret;
        if (bb != null && BiomeDiscoverySystem.instance.isDiscovered(bb)) {
            ret = WorldUtil.getRegionalDescription(pos, false);
        } else {
            ret = UNEXPLORED_LOCATION_TEXT;
            foreach (var kvp in WorldUtil.compassAxes) {
                var dp = pos + kvp.Value * 250;
                var near = BiomeBase.GetBiome(dp);
                if (near != null && BiomeDiscoverySystem.instance.isDiscovered(near)) {
                    var opp = WorldUtil.getOpposite(kvp.Key).ToString();
                    ret += " (" + opp[0] + opp.Substring(1).ToLowerInvariant() + " of " +
                           WorldUtil.getRegionalDescription(dp, false) + ")";
                    break;
                }
            }
        }

        ret += ", " + (int)-pos.y + "m depth";
        if (bb.IsCaveBiome())
            ret += " (Cave)";
        return ret;
    }

    private class LifeformEntry : IComparable<LifeformEntry> {
        internal readonly TechType objectType;
        internal readonly string category;
        internal readonly PDAEncyclopedia.EntryData pdaPage;

        private readonly string hint;

        private Vector3 seenAt = Vector3.zero;
        private bool identityKnown;
        private bool seenACU;

        internal LifeformEntry(TechType tt) {
            objectType = tt;

            pdaPage = getEncyData();
            category = pdaPage == null ? "General" : SNUtil.GetDescriptiveEncyPageCategoryName(pdaPage);
            if (tt == TechType.PrecursorDroid)
                category = Language.main.Get("EncyPath_Lifeforms/Fauna");
            else if (tt == TechType.PrecursorIonCrystal)
                category = Language.main.Get("EncyPath_PlanetaryGeology");
            if (DeIntegrationSystem.Instance.IsLoaded() && DeIntegrationSystem.Instance.IsEgg(tt))
                category = "Fauna Eggs";

            hint = getHint(false);
            var pfb = ObjectUtil.lookupPrefab(tt);
            if (pfb) {
                var c = pfb.GetComponent<Creature>();
                var leviA = c is ReaperLeviathan or GhostLeviatanVoid or GhostLeviathan or SeaDragon;
                var leviP = c is Reefback or SeaEmperorJuvenile or SeaEmperorBaby;
                if (DeIntegrationSystem.Instance.IsLoaded())
                    leviA |= tt == DeIntegrationSystem.Instance.GetVoidThalassacean().TechType ||
                             tt == DeIntegrationSystem.Instance.GetGulper();
                if (leviA || leviP) {
                    hint = leviA ? "Unknown Aggressive Leviathan" : "Unknown Leviathan";
                } else if (c is Warper || pfb.GetComponent<MeleeAttack>() || pfb.GetComponent<RangeAttacker>()) {
                    hint = "Unknown Aggressive Fauna";
                } else if (c) {
                    hint = "Unknown Fauna";
                }

                var fauna = category.Contains("Fauna");
                var flora = category.Contains("Flora");
                if (!leviA && !leviP && (fauna || flora)) {
                    float size = 0;
                    foreach (var cc in pfb.GetComponentsInChildren<Renderer>(true))
                        size += cc.bounds.size.magnitude;
                    if (size >= 96 && fauna)
                        hint += " - Leviathan";
                    else if (size >= 32)
                        hint += " - Large";
                    else if (size <= 6F)
                        hint += " - Small";
                    else if (size <= 1.5F)
                        hint += " - Tiny";
                }

                if (pdaPage != null && !leviA && !leviP && (flora || fauna)) {
                    hint += ", " + Language.main.Get(pdaPage.nodes[pdaPage.nodes.Length - 1]);
                }
            }
        }

        public bool isScanned() {
            return PDAScanner.complete.Contains(objectType);
        }

        public string getLastSeen() {
            return seenAt.magnitude > 0.5F ? seenACU ? "an ACU" : instance.getLocalDescription(seenAt) : null;
        }

        public bool isIdentityKnown() {
            return identityKnown;
        }

        internal bool seeAt(GameObject go, bool identity) {
            var vec = go.transform.position;
            if (!(identity && !identityKnown) && seenAt.magnitude > 0.5F && (vec - seenAt).sqrMagnitude < 100)
                return false;
            seenAt = vec;
            identityKnown |= identity;
            seenACU = go.GetComponent<WaterParkItem>();
            return true;
        }

        public string getHint(bool seen) {
            return seen ? hint : "Unknown " + category.Replace(" Data", "") + " Entity";
        }

        public PDAScanner.EntryData getScannerData() {
            return PDAScanner.mapping[objectType];
        }

        public PDAEncyclopedia.EntryData getEncyData() {
            var key = getScannerData().encyclopedia;
            return string.IsNullOrEmpty(key) ? null :
                PDAEncyclopedia.mapping.ContainsKey(key) ? PDAEncyclopedia.mapping[key] : null;
        }

        public int CompareTo(LifeformEntry ro) {
            var us = getEncyData();
            var them = ro.getEncyData();
            return us == null && them == null
                ? objectType.CompareTo(ro.objectType)
                : us == null
                    ? -1
                    : them == null
                        ? 1
                        : string.Compare(us.path, them.path, StringComparison.InvariantCultureIgnoreCase);
        }

        internal void saveToXML(XmlElement n) {
            n.AddProperty("techtype", objectType.AsString());
            n.AddProperty("seen", seenAt);
            n.AddProperty("known", identityKnown);
        }

        internal void loadFromXML(XmlElement e) {
            seenAt = e.GetVector("seen").Value;
            identityKnown = e.GetBoolean("known");
        }

        public override string ToString() {
            return
                $"[ObjectType={objectType.AsString()}, Category={category}, Hint={hint}, SeenAt={seenAt}, IdentityKnown={identityKnown}]";
        }
    }
}