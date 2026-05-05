using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class AcuCallbackSystem {
    public static readonly AcuCallbackSystem Instance = new();

    private static readonly Dictionary<TechType, float> ToyValues = new();

    static AcuCallbackSystem() {
        AddStalkerToy(TechType.Titanium, 0.1F);
        AddStalkerToy(TechType.ScrapMetal, 0.25F);
        AddStalkerToy(TechType.Silver, 0.5F);
        AddStalkerToy(TechType.Gold, 0.75F);
        AddStalkerToy(AqueousEngineeringMod.toy.Info.TechType, 1.5F);
    }

    public static void AddStalkerToy(TechType tt, float amt) {
        ToyValues[tt] = amt;
    }

    public static bool IsStalkerToy(TechType tt) {
        return ToyValues.ContainsKey(tt);
    }

    private readonly string _oldSaveDir = Path.Combine(
        Path.GetDirectoryName(AqueousEngineeringMod.modDLL.Location),
        "acu_data_cache"
    );

    private static readonly string SaveFileName = "ACUCache.dat";

    private readonly Dictionary<Vector3, CachedAcuData> _cache = new();

    private AcuCallbackSystem() {
    }

    internal void Register() {
        // TODO
        // IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
        // IngameMenuHandler.Main.RegisterOnSaveEvent(save);
        SNUtil.migrateSaveDataFolder(_oldSaveDir, ".xml", SaveFileName);
    }

    internal class CreatureCache {
        internal readonly string EntityID;

        internal float Hunger;
        internal float Happy;

        internal CreatureCache(string id) {
            EntityID = id;
        }

        internal void LoadFromXML(XmlElement e) {
            Hunger = (float)e.getFloat("hunger", double.NaN);
            Happy = (float)e.getFloat("happy", double.NaN);
        }

        internal void SaveToXML(XmlElement e) {
            e.addProperty("hunger", Hunger);
            e.addProperty("happy", Happy);
            e.addProperty("entityID", EntityID);
        }

        internal void Apply(Creature c) {
            c.Hunger.Value = Hunger;
            c.Happy.Value = Happy;
        }

        public override string ToString() {
            return $"[CreatureCache EntityID={EntityID}, Hunger={Hunger}, Happy={Happy}]";
        }
    }

    internal class CachedAcuData {
        internal readonly Vector3 AcuRoot;

        internal float LastPlanktonBoost;
        internal float BoostStrength;
        internal float LastTick;
        internal float NextSoundTime;

        internal Dictionary<string, CreatureCache> CreatureData = new();

        internal CachedAcuData(Vector3 pos) {
            AcuRoot = pos;
        }

        internal void LoadFromXML(XmlElement e) {
            LastPlanktonBoost = (float)e.getFloat("plankton", double.NaN);
            BoostStrength = (float)e.getFloat("boost", double.NaN);
            LastTick = (float)e.getFloat("tick", double.NaN);

            foreach (var e2 in e.getDirectElementsByTagName("creatureStatus")) {
                var c = new CreatureCache(e2.getProperty("entityID"));
                c.LoadFromXML(e2);
            }
        }

        internal void SaveToXML(XmlElement e) {
            e.addProperty("position", AcuRoot);
            e.addProperty("plankton", LastPlanktonBoost);
            e.addProperty("boost", BoostStrength);
            e.addProperty("tick", LastTick);

            foreach (var go in CreatureData.Values) {
                var e2 = e.OwnerDocument.CreateElement("creatureStatus");
                go.SaveToXML(e2);
                e.AppendChild(e2);
            }
        }

        public override string ToString() {
            return string.Format(
                "[CachedACUData AcuRoot={0}, LastPlanktonBoost={1}, LastTick={2}, CreatureData={3}]",
                AcuRoot,
                LastPlanktonBoost,
                LastTick,
                CreatureData.toDebugString()
            );
        }
    }

    private void LoadSave() {
        var path = Path.Combine(SNUtil.getCurrentSaveDir(), SaveFileName);
        if (File.Exists(path)) {
            var doc = new XmlDocument();
            doc.Load(path);
            foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
                try {
                    var pfb = new CachedAcuData(e.getVector("position").Value);
                    pfb.LoadFromXML(e);
                    _cache[pfb.AcuRoot] = pfb;
                } catch (Exception ex) {
                    SNUtil.log("Error parsing entry '" + e.InnerXml + "': " + ex.ToString());
                }
            }
        }

        SNUtil.log("Loaded ACU data cache: ");
        SNUtil.log(_cache.toDebugString());
    }

    private void Save() {
        var path = Path.Combine(SNUtil.getCurrentSaveDir(), SaveFileName);
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        doc.AppendChild(rootnode);
        foreach (var go in _cache.Values) {
            var e = doc.CreateElement("cache");
            go.SaveToXML(e);
            doc.DocumentElement.AppendChild(e);
        }

        doc.Save(path);
    }

    public void Tick(WaterPark acu) {
        if (acu && acu.gameObject) {
            var com = acu.gameObject.EnsureComponent<AcuCallback>();
            com.SetAcu(acu);
        }
    }

    private CachedAcuData GetOrCreateCache(AcuCallback acu) {
        var pos = acu.LowestSegment.transform.position;
        if (!_cache.ContainsKey(pos)) {
            _cache[pos] = new CachedAcuData(pos);
        }

        return _cache[pos];
    }

    public void DebugAcu() {
        var wp = Player.main.currentWaterPark;
        if (wp) {
            SNUtil.writeToChat("ACU @ " + wp.transform.position + ": ");
            var call = wp.GetComponent<AcuCallback>();
            if (!call)
                SNUtil.writeToChat("No hook");
            SNUtil.writeToChat("Biome set: [" + string.Join(", ", call.PotentialBiomes) + "]");
            SNUtil.writeToChat("Plant count: " + call.PlantCount);
            SNUtil.writeToChat("Prey count: " + call.HerbivoreCount);
            SNUtil.writeToChat("Predator count: " + call.CarnivoreCount);
            SNUtil.writeToChat("Sparkle count: " + call.SparkleCount);
            call.NextIsDebug = true;
        }
    }

    internal enum AcuWarnings {
        Notheme,
        Noplants,
        Sameplant,
        Noherbs,
        Nocarns,
        Carnprey,
        Carnspace,
        Herbfood,
        Mixedtheme,
    }

    internal class AcuContentView : StorageContainer {
        private AcuCallback _controller;

        private static readonly Type ResourceMonitorLogic =
            InstructionHandlers.getTypeBySimpleName("ResourceMonitor.Components.ResourceMonitorLogic");

        internal void SetController(AcuCallback acu) {
            if (_controller == acu)
                return;
            _controller = acu; /*
            storageRoot = gameObject.EnsureComponent<ChildObjectIdentifier>();
            storageRoot.ClassId = "ACUFakeInv";
            storageRoot.id = "";*/
            container = new AcuContainerRelay(acu);
        }

        private void Update() {
            if (container != null)
                ((AcuContainerRelay)container).Tick();
        }

        public override void Awake() {
            creationTime =
                DayNightCycle.main
                    .timePassedAsFloat; //do not invoke createContainer, which means need to do the below hook manually
            if (ResourceMonitorLogic != null)
                Invoke(nameof(NotifyResourceMonitor), 10);
        }

        private void NotifyResourceMonitor() {
            //SNUtil.writeToChat("Updating StorageMonitors with ACU "+controller.acu.transform.position);
            var t2 = ResourceMonitorLogic.Assembly.GetType("ResourceMonitor.Patchers.StorageContainerAwakePatcher");
            var li = (IList)t2.GetField(
                "registeredResourceMonitors",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
            ).GetValue(null);
            var call = ResourceMonitorLogic.GetMethod(
                "AlertNewStorageContainerPlaced",
                BindingFlags.Public | BindingFlags.Instance
            );
            foreach (var obj in li) {
                call.Invoke(obj, BindingFlags.Default, null, [this], null);
            }
        }
    }

    internal class AcuContainerRelay : ItemsContainer {
        private readonly AcuCallback _controller;

        internal AcuContainerRelay(AcuCallback call) : base(8, 10, call.transform, "ACURelay", null) {
            _controller = call;
            onRemoveItem += (ii) => { _controller.Acu.RemoveItem(ii.item.GetComponent<WaterParkItem>()); };
        }

        internal void Tick() {
            _items.Clear();
            foreach (var wp in _controller.Acu.items) {
                var pp = wp.GetComponent<Pickupable>();
                if (pp) {
                    var tt = pp.GetTechType();
                    var sz = TechData.GetItemSize(tt);
                    var grp = _items.ContainsKey(tt) ? _items[tt] : new ItemGroup(_items.Count, sz.x, sz.y);
                    grp.items.Add(new InventoryItem(pp));
                    _items[tt] = grp;
                }
            }
            //SNUtil.writeToChat(_items.toDebugString());
        }
    }

    internal class AcuCallback : MonoBehaviour {
        internal WaterPark Acu;

        internal BaseRoot Seabase;
        internal BaseCell BaseCell;
        internal AcuContentView ContentView;
        internal StorageContainer Planter;
        internal List<WaterParkGeometry> Column;
        internal GameObject LowestSegment;
        internal GameObject Floor;
        internal List<GameObject> DecoHolders;

        internal HashSet<BiomeRegions.RegionType> PotentialBiomes = [];
        internal BiomeRegions.RegionType CurrentTheme = BiomeRegions.Shallows;
        internal float PlantCount;
        internal float HerbivoreCount;
        internal float CarnivoreCount;
        internal int SparkleCount;
        internal int CuddleCount;
        internal int GasopodCount;
        internal bool ConsistentBiome;

        internal BaseBioReactor IsAboveBioreactor;

        internal float InfectedTotal;
        internal float CurrentBonus;
        internal float StalkerToyValue;

        private readonly List<AcuWarnings> _currentWarnings = [];

        internal bool NextIsDebug;

        internal float LastThemeUpdate;
        internal bool AppliedTheme;

        private GameObject _bubbleVents;
        private ParticleSystem[] _ventBubbleEmitters;

        private CachedAcuData _cache;

        internal void SetAcu(WaterPark w) {
            if (Acu != w) {
                CancelInvoke(nameof(Tick));
                if (ContentView)
                    ContentView.destroy(false);
                ContentView = null;
                Planter = null;
                Seabase = null;
                Column = null;
                DecoHolders = null;
                LowestSegment = null;
                IsAboveBioreactor = null;
                Floor = null;
                _cache = null;

                Acu = w;

                if (Acu) {
                    //SNUtil.writeToChat("Setup ACU Hook");
                    SNUtil.log("Switching ACU " + Acu + " @ " + Acu.transform.position + " to " + this);
                    InvokeRepeating(nameof(Tick), 0, 1);
                    Seabase = Acu.GetComponentInParent<BaseRoot>();
                    Planter = Acu.planter.GetComponentInChildren<StorageContainer>();
                    Column = Instance.GetAcuComponents(Acu);
                    LowestSegment = Instance.GetAcuFloor(Column);
                    BaseCell = ObjectUtil.getBaseRoom(Seabase, LowestSegment);
                    if (LowestSegment) {
                        Floor = LowestSegment.getChildObject("Large_Aquarium_Room_generic_ground");
                        DecoHolders = LowestSegment.getChildObjects(AcuTheming.AcuDecoSlotName);
                        _bubbleVents = LowestSegment.getChildObject("Bubbles");
                    } else {
                        SNUtil.log(
                            "ACU " + Acu.transform.position + " had no lowest segment??? C=" + Column
                                .Select<WaterParkGeometry, string>(wpp => wpp.transform.position.ToString())
                                .toDebugString()
                        );
                    }

                    _ventBubbleEmitters = _bubbleVents.GetComponentsInChildren<ParticleSystem>();
                    ContentView = Acu.gameObject.EnsureComponent<AcuContentView>();
                    ContentView.enabled = true;
                    ContentView.SetController(this);
                    IsAboveBioreactor = Instance.CheckAboveBioreactor(Acu, this);
                    Load();
                }
            }
        }

        private void Load() {
            _cache = Instance.GetOrCreateCache(this);
            foreach (var cc in _cache.CreatureData.Values) {
                var wp = GetItemByID(cc.EntityID);
                if (wp) {
                    var c = wp.GetComponent<Creature>();
                    if (c) {
                        cc.Apply(c);
                        SNUtil.log("Deserializing cached ACU creature status " + cc);
                    }
                }
            }
        }

        internal WaterParkItem GetItemByID(string id) {
            foreach (var wp in Acu.items) {
                var pi = wp.GetComponent<PrefabIdentifier>();
                if (pi && pi.Id == id)
                    return wp;
            }

            return null;
        }

        internal CreatureCache GetOrCreateCreatureStatus(MonoBehaviour wp) {
            if (_cache == null)
                return null;
            var id = wp.gameObject.FindAncestor<PrefabIdentifier>().Id; //NOT classID
            if (!_cache.CreatureData.ContainsKey(id)) {
                _cache.CreatureData[id] = new CreatureCache(id);
            }

            return _cache.CreatureData[id];
        }

        public float GetBoostStrength(float time) {
            if (_cache == null)
                return 0;
            var dt = time - _cache.LastPlanktonBoost;
            return dt <= 15 ? (1 - dt / 15F) * _cache.BoostStrength : 0;
        }

        public void Boost(ACUFuel amt) {
            _cache.LastPlanktonBoost = DayNightCycle.main.timePassedAsFloat;
            _cache.BoostStrength = amt.effectStrength;
        }

        internal void PrintTerminalInfo() { /*
            SNUtil.writeToChat("Biome Archetype: "+currentTheme);
            SNUtil.writeToChat("Plant Count: "+plantCount);
            SNUtil.writeToChat("Herbivore Count: "+herbivoreCount);
            SNUtil.writeToChat("Carnivore Count: "+carnivoreCount);

            SNUtil.writeToChat("Stalker Toy Rating: "+stalkerToyValue.ToString("0.0"));*/
            var values = new Dictionary<string, object>();
            var day = DayNightCycle.main.GetDay();
            var dday = (int)day;
            var frac = day - dday;
            values["day"] = dday;
            values["time"] = (int)(frac * 1200) + "s";
            values["contents"] = GenerateContentList();
            values["biome"] = CurrentTheme.getName();
            values["plants"] = PlantCount.ToString("0.0");
            values["herbivores"] = HerbivoreCount.ToString("0.0");
            values["carnivores"] = CarnivoreCount.ToString("0.0");
            values["sparkles"] = SparkleCount;
            values["infected"] = InfectedTotal.ToString("0.00");
            values["bonus"] = (CurrentBonus * 100).ToString("0.00");
            values["stalkerToy"] = StalkerToyValue.ToString("0.0");
            values["ampeelYield"] = AmpeelAntenna.computeACUValue(Acu).ToString("0.00");
            values["height"] = Acu.height;
            values["count"] = Acu.usedSpace;
            values["capacity"] = Acu.wpPieceCapacity * Acu.height;
            values["alerts"] = _currentWarnings.Count == 0
                ? "[None]"
                : string.Join(
                    "\n",
                    _currentWarnings.Select(w =>
                        AqueousEngineeringMod.acuLocale.getEntry(w.ToString()).desc
                    )
                );

            var e = AqueousEngineeringMod.acuMonitorBlock.locale;
            var pp = PDAManager.getPage(e.key + "PDA");
            pp.unlock();
            pp.setPlaceholderValues(e.pda, values, true);
            pp.show(pda => pp.update(AqueousEngineeringMod.acuLocale.getEntry("NotTerminalPDA").desc, true, false));
        }

        private string GenerateContentList() {
            var counts = new CountMap<TechType>();
            var sizes = new Dictionary<TechType, int>();
            foreach (var wp in new List<WaterParkItem>(Acu.items)) {
                if (!wp)
                    continue;
                var pp = wp.gameObject.GetComponentInChildren<Pickupable>();
                var tt = pp ? pp.GetTechType() : TechType.None;
                if (tt != TechType.None) {
                    var sz = wp.GetSize();
                    if (sz > 0) {
                        sizes[tt] = sz;
                        counts.add(tt);
                    }
                }
            }

            var sb = new StringBuilder();
            foreach (var tt in counts.getItems()) {
                sb.Append(Language.main.Get(tt));
                sb.Append(": ");
                sb.Append(counts.getCount(tt));
                sb.Append(" (");
                sb.Append(sizes[tt]);
                sb.Append(" occupancy slots each)\n");
            }

            return sb.ToString();
        }

        public void Tick() {
            if (!Floor || !LowestSegment) {
                SetAcu(null);
                return;
            }

            var time = DayNightCycle.main.timePassedAsFloat;
            var dT = time - _cache.LastTick;
            _cache.LastTick = time;
            if (dT <= 0.0001)
                return;
            //SNUtil.writeToChat(dT+" s");
            var healthy = false;
            var consistent = true;
            _currentWarnings.Clear();
            PotentialBiomes.Clear();
            PotentialBiomes.AddRange(BiomeRegions.getAllBiomes());
            //SNUtil.writeToChat("SC:"+sc);
            var plants = Planter.GetComponentsInChildren<PrefabIdentifier>();
            PlantCount = 0;
            HerbivoreCount = 0;
            CarnivoreCount = 0;
            var teeth = 0;
            CuddleCount = 0;
            GasopodCount = 0;
            InfectedTotal = 0;
            SparkleCount = 0;
            //SNUtil.writeToChat("@@"+string.Join(",", possibleBiomes));
            List<InfectedMixin> infectedFish = [];
            List<WaterParkCreature> foodFish = [];
            List<Stalker> stalkers = [];
            StalkerToyValue = 0;
            var hasStalkerToy = false;
            var acuRoom = BaseRoomSpecializationSystem.instance.getSavedType(Acu) ==
                          BaseRoomSpecializationSystem.RoomTypes.ACU;
            foreach (var wp in new List<WaterParkItem>(Acu.items)) {
                //clone because might change in ACUEcosystems.handleCreature
                if (!wp)
                    continue;
                var pp = wp.gameObject.GetComponentInChildren<Pickupable>();
                var tt = pp ? pp.GetTechType() : TechType.None;
                if (IsStalkerToy(tt)) {
                    hasStalkerToy |= tt == AqueousEngineeringMod.toy.TechType;
                    StalkerToyValue += ToyValues[tt];
                    pp.gameObject.transform.localScale = Vector3.one * 0.5F;
                } else if (tt == TechType.StalkerTooth) {
                    pp.gameObject.transform.localScale = Vector3.one * 0.125F;
                    teeth++;
                } else if (wp is WaterParkCreature wpc) {
                    // TODO
                    // if (wpc.parameters == null) {
                    //     SNUtil.log(
                    //         "WaterParkCreature had null params: " + wpc.name + " / " + wp.GetComponent<Creature>()
                    //     );
                    //     ObjectUtil.dumpObjectData(wpc.gameObject);
                    //     wpc.parameters = WaterParkCreature.GetParameters(wpc.GetComponent<Pickupable>().GetTechType());
                    //     if (wpc.parameters == null) {
                    //         SNUtil.log("Fetches null params!");
                    //         wpc.parameters = WaterParkCreatureParameters.GetDefaultValue();
                    //     }
                    // }

                    var mix = wp.GetComponent<InfectedMixin>();
                    if (mix) {
                        var amt = mix.GetInfectedAmount();
                        if (amt > 0) {
                            InfectedTotal += amt;
                            infectedFish.Add(mix);
                        }
                    }

                    var c = ACUEcosystems.handleCreature(
                        this,
                        dT,
                        wpc,
                        tt,
                        foodFish,
                        plants,
                        acuRoom,
                        PotentialBiomes
                    );
                    if (tt == TechType.Stalker) {
                        stalkers.Add((Stalker)c);
                    }
                }
            }

            if (AqueousEngineeringMod.config.getBoolean(AEConfig.ConfigEntries.ACUSOUND) &&
                time >= _cache.NextSoundTime && Acu.items.Count > 0) {
                var wpc = Acu.items.GetRandom() as WaterParkCreature;
                if (wpc) {
                    var flag = false;
                    FMOD_CustomEmitter emit = null;
                    var ca = wpc.GetComponentsInChildren<FMOD_CustomLoopingEmitter>();
                    if (ca != null && ca.Length > 0) {
                        emit = ca.GetRandom();
                    } else {
                        var a = wpc.GetComponentsInChildren<AttackLastTarget>();
                        if (a != null && a.Length > 0)
                            emit = a.GetRandom().attackStartSound;
                    }

                    if (emit) {
                        emit.Play();
                        if (emit.evt.isValid())
                            emit.evt.setVolume(0.4F);
                        flag = true;
                    }

                    _cache.NextSoundTime =
                        flag ? time + UnityEngine.Random.Range(5F, 15F) : time + UnityEngine.Random.Range(2F, 5F);
                }
            }

            var plantTypes = ACUEcosystems.collectPlants(this, plants, PotentialBiomes);
            consistent = PotentialBiomes.Count > 0 && PlantCount > 0;
            var max = PotentialBiomes.Count == 1
                ? ACUEcosystems.getPlantsForBiome(PotentialBiomes.First()).Count
                : 99;
            var plantVar = plantTypes.Count >= Mathf.Min(2, max);
            var tooManyCarnisPrey = CarnivoreCount > Math.Max(
                1,
                HerbivoreCount / Mathf.Max(1, 6 - SparkleCount * 0.5F)
            );
            var tooManyCarnisSpace = CarnivoreCount > Acu.height * (acuRoom ? 2F : 1.5F);
            var tooManyHerbis = HerbivoreCount > PlantCount * (4 + SparkleCount * 0.5F) * (acuRoom ? 1.5F : 1F);
            var hasPlants = PlantCount > 0;
            var hasHerbis = HerbivoreCount > 0;
            var hasCarnis = CarnivoreCount > 0;
            healthy = hasPlants && hasHerbis && hasCarnis && !tooManyCarnisPrey && !tooManyCarnisSpace &&
                      !tooManyHerbis;
            if (!hasPlants)
                _currentWarnings.Add(AcuWarnings.Noplants);
            if (!plantVar && hasPlants)
                _currentWarnings.Add(AcuWarnings.Sameplant);
            if (!hasHerbis)
                _currentWarnings.Add(AcuWarnings.Noherbs);
            if (!hasCarnis)
                _currentWarnings.Add(AcuWarnings.Nocarns);
            if (tooManyCarnisPrey)
                _currentWarnings.Add(AcuWarnings.Carnprey);
            if (tooManyCarnisSpace)
                _currentWarnings.Add(AcuWarnings.Carnspace);
            if (tooManyHerbis && hasHerbis)
                _currentWarnings.Add(AcuWarnings.Herbfood);

            CurrentBonus = 0;
            if (consistent)
                CurrentBonus += 1F;
            else
                _currentWarnings.Add(AcuWarnings.Notheme);
            if (healthy)
                CurrentBonus += 2F;
            if (SparkleCount > 0)
                CurrentBonus *= 1 + SparkleCount * 0.5F;
            if (NextIsDebug)
                SNUtil.writeToChat(
                    PlantCount + "/" + HerbivoreCount + "/" + CarnivoreCount + "$" + SparkleCount + " & " +
                    string.Join(", ", PotentialBiomes) + " > " + healthy + " & " + consistent + " > " + CurrentBonus
                );
            var f0 = GetBoostStrength(time);
            if (_ventBubbleEmitters != null) {
                foreach (var p in _ventBubbleEmitters) {
                    if (p && p.gameObject.name == "xBubbleColumn") {
                        var main = p.main;
                        main.startColor = Color.Lerp(Color.white, new Color(0.2F, 1F, 0.4F), f0);
                        main.startSizeMultiplier = 0.5F + 1.5F * f0;
                        main.startLifetimeMultiplier = 1.7F + 2.3F * f0;
                    }
                }
            }

            CurrentBonus += 5F * f0;
            if (InfectedTotal > 0) {
                CurrentBonus -= InfectedTotal * 2;
                if (UnityEngine.Random.Range(0F, 1F) <= InfectedTotal * 0.015F * dT) {
                    var go = ObjectUtil.createWorldObject(VanillaCreatures.WARPER.prefab);
                    var inAcu = UnityEngine.Random.Range(0F, 1F) < 0.2F;
                    go.transform.position =
                        inAcu
                            ? Acu.transform.position
                            : MathUtil.getRandomVectorAround(Acu.transform.position, new Vector3(10, 0, 10));
                    var wp = go.GetComponent<Warper>();
                    wp.WarpIn(null);
                    if (inAcu) {
                        go.EnsureComponent<AcuWarper>();
                    } else {
                        AttractToTarget.attractCreatureToTarget(
                            wp,
                            Acu.gameObject.FindAncestor<BaseCell>().GetComponent<LiveMixin>(),
                            false
                        );
                    }
                }
            }

            if (CurrentBonus > 0) {
                var boost = CurrentBonus * dT;
                foreach (var wp in foodFish) {
                    //SNUtil.writeToChat(wp+" > "+boost+" > "+wp.matureTime+"/"+wp.timeNextBreed);
                    if (wp.GetCanBreed()) {
                        var pp = wp.gameObject.GetComponent<Peeper>();
                        if (pp && pp.isHero)
                            wp.timeNextBreed =
                                DayNightCycle.main.timePassedAsFloat + 1000; //prevent sparkle peepers from breeding
                        else if (wp.isMature)
                            wp.timeNextBreed -= boost;
                        else
                            wp.matureTime -= boost;
                    }
                }
            }

            if (consistent && healthy && PotentialBiomes.Contains(BiomeRegions.Kelp)) {
                var single = PotentialBiomes.Count == 1;
                foreach (var s in stalkers) {
                    if (hasStalkerToy)
                        s.Happy.Add(dT * 0.05F);
                    if (teeth < 6) {
                        var f = dT * Mathf.Min(8, StalkerToyValue) * 0.00012F * (1 + 2 * s.Happy.Value) *
                                (single ? 1 : 0.2F);
                        //SNUtil.writeToChat(s.Happy.Value+" x "+stalkerToyValue+" > "+f);
                        if (UnityEngine.Random.Range(0F, 1F) < f) {
                            //do not use, so can have ref to GO; reimplement // s.LoseTooth();
                            var go = Instantiate(s.toothPrefab);
                            //SNUtil.writeToChat(s+" > "+go);
                            go.transform.position = s.loseToothDropLocation.transform.position;
                            go.transform.rotation = s.loseToothDropLocation.transform.rotation;
                            if (go.activeSelf && s.isActiveAndEnabled) {
                                foreach (var c in go.GetComponentsInChildren<Collider>())
                                    Physics.IgnoreCollision(s.stalkerBodyCollider, c);
                            }

                            Utils.PlayFMODAsset(s.loseToothSound, go.transform, 8f);
                            LargeWorldEntity.Register(go);
                            Acu.AddItem(go.GetComponent<Pickupable>());
                        }
                    }
                }
            }

            if (NextIsDebug)
                SNUtil.writeToChat("Final biome set: [" + string.Join(", ", PotentialBiomes) + "]");
            if (PotentialBiomes.Count == 1) {
                var theme = PotentialBiomes.First();
                if (theme == BiomeRegions.Other)
                    theme = BiomeRegions.Shallows;
                var changed = theme != CurrentTheme;
                CurrentTheme = theme;
                ConsistentBiome = true;
                AcuTheming.UpdateAcuTheming(this, theme, time, changed || time - LastThemeUpdate > 5 || !AppliedTheme);
            } else if (PotentialBiomes.Count > 1) {
                _currentWarnings.Add(AcuWarnings.Mixedtheme);
                ConsistentBiome = false;
            } else {
                ConsistentBiome = false;
            }

            NextIsDebug = false;
        }

        public bool IsHealthy() {
            return _currentWarnings.Count == 0;
        }
    }

    private class AcuWarper : MonoBehaviour {
        private void Update() {
            transform.localScale = Vector3.one * 0.4F;
        }
    }

    internal List<WaterParkGeometry> GetAcuComponents(WaterPark acu) {
        List<WaterParkGeometry> li = [];
        li.AddRange(
            acu.transform.parent.GetComponentsInChildren<WaterParkGeometry>().Where(wp =>
                wp && wp.name.ToLowerInvariant().Contains("bottom") && wp.GetModule() == acu
            )
        );

        return li;
    }

    internal GameObject GetAcuFloor(IEnumerable<WaterParkGeometry> li) {
        return (from wp in li where wp.geometryFace.direction == Base.Direction.Below select wp.gameObject)
            .FirstOrDefault();
    }

    internal GameObject GetAcuCeiling(IEnumerable<WaterParkGeometry> li) {
        return (from wp in li where wp.gameObject.name.Contains("BaseWaterParkCeilingTop") select wp.gameObject)
            .FirstOrDefault();
    }

    internal BaseBioReactor CheckAboveBioreactor(WaterPark acu, AcuCallback call) {
        //BaseCell cell = call.lowestSegment.FindAncestor<BaseCell>();
        var at = acu.transform.position; //cell.transform.position; //acu pos is lowest one
        var seek = at + Vector3.down * 3.5F;
        /*
        BaseCell below = null;
        foreach (BaseCell bc in cell.transform.parent.GetComponentsInChildren<BaseCell>()) {
            if ((bc.transform.position-seek).sqrMagnitude < 0.01) {
                below = bc;
                break;
            }
        }
        if (below) {

        }*/
        foreach (var bio in acu.transform.parent.GetComponentsInChildren<BaseBioReactor>()) {
            //acu is parented directly to base, as is bioreactor
            if ((bio.transform.position - seek).sqrMagnitude < 0.01) {
                return bio;
            }
        }

        return null;
    }
}