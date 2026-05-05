using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public class DataCollectionTracker {

	private static readonly LocationDescriptor unknownLocation = new(() => true, "Unknown Location");
	private static readonly LocationDescriptor auroraGoal = new(() => true, "Aboard The Aurora"); //always known location

	public static readonly DataCollectionTracker instance = new();

	internal static readonly string NEED_DATA_PDA = "needencydata";

	private readonly string saveFileName = "data_collection.dat";

	private readonly Dictionary<string, DataDownloadEntry> requiredAuroraData = new();
	private readonly Dictionary<string, DataDownloadEntry> requiredDegasiData = new();
	private readonly Dictionary<string, DataDownloadEntry> requiredAlienData = new();
	//private readonly HashSet<Area> discoveredAlienFacilities = new HashSet<Area>();

	private readonly List<AlienScanEntry> alienBaseScans = [];

	private float needsPDAUpdate = -1;

	public static bool showAll = false;

	private DataCollectionTracker() {

	}

	private void addAlienScanEntry(LocationDescriptor f, TechType tt) {
		alienBaseScans.Add(new AlienScanEntry(f, tt));
	}

	private DataDownloadEntry addLifepodLog(string key, int pod) {
		return addRequiredData(key, "Lifepod " + pod + " Log", unknownLocation, requiredAuroraData);
	}

	private DataDownloadEntry addRequiredData(string key, string hint, LocationDescriptor loc, Dictionary<string, DataDownloadEntry> map) {
		var e = new DataDownloadEntry(loc, key, hint);
		map[key] = e;
		return e;
	}

	public void register() {
		//IngameMenuHandler.Main.RegisterOnLoadEvent(loadSave);
		//IngameMenuHandler.Main.RegisterOnSaveEvent(save);

		StoryHandler.instance.addListener(s => { needsPDAUpdate = DayNightCycle.main.timePassedAsFloat + 1; });
	}

	public void buildSet() {
		if (requiredAuroraData.Count > 0)
			return;
		var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
		var genericPDA = "PDA Log";
		var genericHint = "Data Download";
		addRequiredData("Aurora_DriveRoom_Terminal1", "Black Box Data", auroraGoal, requiredAuroraData);
		addRequiredData("Aurora_RingRoom_Terminal3", "Escape Rocket Data", auroraGoal, requiredAuroraData).setVisible("RadioCaptainsQuartersCode");
		addLifepodLog("bkelpbase", 1).setVisible("SeeBkelpBase");
		addLifepodLog("bkelpbase2", 1).setVisible("SeeBkelpBase");
		addLifepodLog("lrpowerseal", 1).setVisible("bkelpbase2");
		addLifepodLog(StoryGoals.POD2, 2).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD2RADIO));
		addLifepodLog(StoryGoals.POD3, 3).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD3RADIO));
		addLifepodLog(StoryGoals.POD4, 4).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD4RADIO));
		addLifepodLog(StoryGoals.POD6A, 6).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD6RADIO));
		addLifepodLog(StoryGoals.POD6B, 6).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD6RADIO));
		addLifepodLog(StoryGoals.POD7, 7).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD7RADIO));
		addLifepodLog("treaderpod", 9).setVisible(StoryGoals.getRadioPlayGoal(SeaToSeaMod.TreaderSignal.storyGate));
		addLifepodLog("treadercave", 9).setVisible("treaderpod");
		addLifepodLog("crashmesa", 10).setVisible("crashmesahint");
		addLifepodLog(StoryGoals.POD12, 12).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD12RADIO));
		addLifepodLog(StoryGoals.POD13, 13).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD13RADIO));
		addLifepodLog("rescuepdalog", 13).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD13));
		addLifepodLog("treepda", 13).setVisible("rescuepdalog");
		addLifepodLog("mountainpodearly", 14).setVisible(C2CProgression.MountainPodEntryVisibilityGoal);
		addLifepodLog("mountainpodlate", 14).setVisible(C2CProgression.MountainPodEntryVisibilityGoal);
		addLifepodLog("mountaincave", 14).setVisible(C2CProgression.MountainPodEntryVisibilityGoal);
		addLifepodLog("islandpda", 14).setVisible(C2CProgression.MountainPodEntryVisibilityGoal);
		addLifepodLog("islandcave", 14).setVisible(C2CProgression.MountainPodEntryVisibilityGoal);
		addLifepodLog("voidpod", 15).setVisible(StoryGoals.getRadioPlayGoal(VoidSpikesBiome.instance.getSignalKey()));
		addLifepodLog("voidspike", 15).setVisible("voidpod");
		addLifepodLog(StoryGoals.POD17, 17).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD17RADIO));
		addLifepodLog(StoryGoals.POD19RADIO, 19).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD19RADIO));
		addLifepodLog(StoryGoals.POD19AUDIO, 19).setVisible(StoryGoals.getRadioPlayGoal(StoryGoals.POD19RADIO));
		addRequiredData("dunearch", "Unknown Survivor Log", unknownLocation, requiredAuroraData).setVisible("dunearchhint");
		addRequiredData("RendezvousFloatingIsland", "Rendezvous Log", unknownLocation, requiredAuroraData).setVisible("LifepodKeenLog");
		addRequiredData("CaptainPDA", "Aurora Captain Log", unknownLocation, requiredAuroraData).setVisible("RadioCaptainsQuartersCode");
		if (hard) {
			addRequiredData("Aurora_Locker_PDA1", "Aurora Data Log", auroraGoal, requiredAuroraData); //Degasi secondary mission
			addRequiredData("Aurora_Cargo_PDA1", "Aurora Conversation Log", auroraGoal, requiredAuroraData); //Yu and Berkeley
			addRequiredData("Aurora_Living_Area_PDA2b", "Aurora Conversation Log", auroraGoal, requiredAuroraData); //"You're dumping me"
			addRequiredData("InnerBiomeWreckLore7", "Aurora Conversation Log", unknownLocation, requiredAuroraData); //"you've both been equally incompetent"
			addRequiredData("OuterBiomeWreckLore9", "Aurora Conversation Log", unknownLocation, requiredAuroraData); //"suspicious keyword 'religious'"
		}

		var floatislandBaseGoal = new LocationDescriptor(C2CProgression.Instance.GetLocationGoal("FLOATISLAND_DEGASI"), "Detected at the Floating Island Degasi Base");
		var jellyBaseGoal = new LocationDescriptor(C2CProgression.Instance.GetLocationGoal("JELLY_DEGASI"), "Detected in the Jellyshroom Caves Degasi Base");
		var dgrBaseGoal = new LocationDescriptor(C2CProgression.Instance.GetLocationGoal("DGR_DEGASI"), "Detected in the Deep Grand Reef Degasi Base");
		addRequiredData("IslandsPDABase1bDesk", genericPDA, floatislandBaseGoal, requiredDegasiData); //1
		addRequiredData("IslandsPDABase1Desk", genericPDA, floatislandBaseGoal, requiredDegasiData); //2
		addRequiredData("IslandsPDAExterior", genericPDA, floatislandBaseGoal, requiredDegasiData); //3
		addRequiredData("IslandsPDABase1Interior", genericPDA, floatislandBaseGoal, requiredDegasiData); //paul1
		if (hard)
			addRequiredData("JellyPDARoom2Locker", genericPDA, floatislandBaseGoal, requiredDegasiData); //4, tablet
		addRequiredData("IslandsPDABase1a", genericPDA, floatislandBaseGoal, requiredDegasiData); //bart3
		if (hard)
			addRequiredData("JellyPDABreadcrumb", genericPDA, jellyBaseGoal, requiredDegasiData);
		addRequiredData("JellyPDABrokenCorridor", genericPDA, jellyBaseGoal, requiredDegasiData); //5
		addRequiredData("JellyPDARoom2Desk", genericPDA, jellyBaseGoal, requiredDegasiData); //6
		addRequiredData("JellyPDARoom1Desk", genericPDA, jellyBaseGoal, requiredDegasiData); //bart1
		addRequiredData("JellyPDAObservatory", genericPDA, jellyBaseGoal, requiredDegasiData); //bart2
		addRequiredData("JellyPDARoom1Locker", genericPDA, jellyBaseGoal, requiredDegasiData); //paul2
		if (hard)
			addRequiredData("JellyPDAExterior", genericPDA, dgrBaseGoal, requiredDegasiData); //rant
		addRequiredData("DeepPDA1", genericPDA, dgrBaseGoal, requiredDegasiData); //7
		addRequiredData("DeepPDA2", genericPDA, dgrBaseGoal, requiredDegasiData); //8
		addRequiredData("DeepPDA3", genericPDA, dgrBaseGoal, requiredDegasiData); //9
		addRequiredData("DeepPDA4", genericPDA, dgrBaseGoal, requiredDegasiData); //paul3

		var anywhere = new LocationDescriptor(() => true, "No Specific Location");
		var gunGoal = new LocationDescriptor("Precursor_Gun_DataDownload2", "Detected in the Quarantine Enforcement Platform");
		var drfGoal = new LocationDescriptor("Precursor_LostRiverBase_Log2", "Detected in the Disease Research Facility");
		var atpGoal = new LocationDescriptor(C2CProgression.Instance.GetLocationGoal("SEE_ATP"), "Detected in the Alien Thermal Plant");
		var pcfGoal = new LocationDescriptor("Precursor_Prison_MoonPool_Log1", "Detected in the Primary Containment Facility");
		var lrlabGoal = new LocationDescriptor(C2CProgression.Instance.GetLocationGoal("LR_LAB"), "Detected in the Lost River Lab Cache");
		addRequiredData("Precursor_Gun_DataDownload1", genericHint, gunGoal, requiredAlienData);
		addRequiredData("Precursor_Gun_DataDownload2", genericHint, gunGoal, requiredAlienData);
		addRequiredData("Precursor_SparseReefCache_DataDownload1", genericHint, new LocationDescriptor(C2CProgression.Instance.GetLocationGoal("SPARSE_CACHE"), "Detected in the Sparse Reef Sanctuary"), requiredAlienData);
		addRequiredData("Precursor_Cache_DataDownload2", genericHint, new LocationDescriptor(C2CProgression.Instance.GetLocationGoal("NBKELP_CACHE"), "Detected in the Blood Kelp Sanctuary"), requiredAlienData);
		addRequiredData("Precursor_Cache_DataDownload3", genericHint, new LocationDescriptor(C2CProgression.Instance.GetLocationGoal("DUNES_CACHE"), "Detected in the Dunes Sanctuary"), requiredAlienData);
		addRequiredData("Precursor_Cache_DataDownloadLostRiver", genericHint, lrlabGoal, requiredAlienData);
		addRequiredData("Precursor_LostRiverBase_DataDownload1", genericHint, drfGoal, requiredAlienData);
		addRequiredData("Precursor_LostRiverBase_DataDownload3", genericHint, drfGoal, requiredAlienData);
		addRequiredData("Precursor_LostRiverBase_Log3", genericHint, drfGoal, requiredAlienData); //drf cinematic
		addRequiredData("Precursor_LavaCastleBase_ThermalPlant2", genericHint, atpGoal, requiredAlienData);
		addRequiredData("Precursor_LavaCastleBase_ThermalPlant3", genericHint, atpGoal, requiredAlienData);
		addRequiredData("Precursor_LavaCastleBase_DataDownload1", genericHint, atpGoal, requiredAlienData); //ion power
		addRequiredData("Precursor_Prison_DataDownload1", genericHint, pcfGoal, requiredAlienData);
		addRequiredData("Precursor_Prison_DataDownload2", genericHint, pcfGoal, requiredAlienData);
		addRequiredData("Precursor_Prison_DataDownload3", genericHint, pcfGoal, requiredAlienData);

		addAlienScanEntry(gunGoal, TechType.PrecursorEnergyCore);
		addAlienScanEntry(gunGoal, TechType.PrecursorPrisonArtifact6); //bomb
		addAlienScanEntry(gunGoal, TechType.PrecursorPrisonArtifact7); //rifle
		addAlienScanEntry(lrlabGoal, TechType.PrecursorSensor);
		addAlienScanEntry(lrlabGoal, TechType.PrecursorLostRiverLabBones);
		addAlienScanEntry(lrlabGoal, TechType.PrecursorLabCacheContainer1);
		addAlienScanEntry(lrlabGoal, TechType.PrecursorLabCacheContainer2);
		addAlienScanEntry(lrlabGoal, TechType.PrecursorLabTable);
		addAlienScanEntry(drfGoal, TechType.PrecursorWarper);
		addAlienScanEntry(drfGoal, TechType.PrecursorFishSkeleton);
		addAlienScanEntry(drfGoal, TechType.PrecursorLostRiverLabRays);
		addAlienScanEntry(drfGoal, TechType.PrecursorLostRiverLabEgg);
		addAlienScanEntry(atpGoal, TechType.PrecursorThermalPlant);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact1);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact2);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact3);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact4);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact5);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact8);
		//does not exist addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact9);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact10);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact11);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact12);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonArtifact13);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonLabEmperorEgg);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonLabEmperorFetus);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonAquariumIncubator);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonAquariumIncubatorEggs);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPipeRoomIncomingPipe);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPipeRoomOutgoingPipe);
		addAlienScanEntry(pcfGoal, TechType.PrecursorPrisonIonGenerator);
		addAlienScanEntry(anywhere, TechType.PrecursorTeleporter);
		addAlienScanEntry(anywhere, TechType.PrecursorPrisonAquariumFinalTeleporter); //this is a unique scan
	}

	public void tick(float time) {
		if (!Story.StoryGoalManager.main.IsGoalComplete("Goal_Scanner")) {
			return;
		}
		if (needsPDAUpdate >= 0 && time >= needsPDAUpdate) {
			PDAManager.getPage(NEED_DATA_PDA).update(generatePDAContent(), true);
			PDAManager.getPage(NEED_DATA_PDA).unlock();
			needsPDAUpdate = -1;
		}
	}
	/*
	private void loadSave() {
		string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
		if (File.Exists(path)) {
			XmlDocument doc = new XmlDocument();
			doc.Load(path);
			loadList(doc.DocumentElement, "Aurora", requiredAuroraData);
			loadList(doc.DocumentElement, "Degasi", requiredDegasiData);
			loadList(doc.DocumentElement, "Alien", requiredAlienData);
			XmlElement e = doc.DocumentElement.getDirectElementsByTagName("Scans")[0];
			foreach (XmlElement e2 in e.ChildNodes) {

			}
		}
		SNUtil.log("Loaded data collection cache: ");
		SNUtil.log(requiredAuroraData.toDebugString());
		SNUtil.log(requiredDegasiData.toDebugString());
		SNUtil.log(requiredAlienData.toDebugString());
	}

	private void save() {
		string path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
		XmlDocument doc = new XmlDocument();
		XmlElement rootnode = doc.CreateElement("Root");
		doc.AppendChild(rootnode);
		saveList(rootnode, "Aurora", requiredAuroraData);
		saveList(rootnode, "Degasi", requiredDegasiData);
		saveList(rootnode, "Alien", requiredAlienData);
		XmlElement e = doc.CreateElement("Scans");
		foreach (AlienScanEntry a in alienBaseScans) {

		}
		rootnode.AppendChild(e);
		doc.Save(path);
	}

	private void saveList(XmlElement node, string key, Dictionary<string, DataDownloadEntry> li) {
		XmlElement wrap = node.OwnerDocument.CreateElement(key);
		foreach (DataDownloadEntry le in li.Values) {
			XmlElement e = node.OwnerDocument.CreateElement("entry");
			le.saveToXML(e);
			wrap.AppendChild(e);
		}
		node.AppendChild(wrap);
	}

	private void loadList(XmlElement node, string key, Dictionary<string, DataDownloadEntry> li) {
		XmlElement wrap = node.getDirectElementsByTagName(key)[0];
		foreach (XmlElement e in wrap.ChildNodes) {
			string ency = e.getProperty("encyKey");
			if (li.ContainsKey(ency))
				li[ency].loadFromXML(e);
		}
	}
	*/
	public void onScanComplete(PDAScanner.EntryData data) {
		needsPDAUpdate = DayNightCycle.main.timePassedAsFloat + 1;
	}

	private string generatePDAContent() {
		if (!Language.main) {
			SNUtil.log("Initialized DataCollect PDA before language!");
			return "ERROR";
		}
		buildSet();
		var ll = SeaToSeaMod.PdaLocale.getEntry(NEED_DATA_PDA);
		var desc = ll.pda;
		var alien = requiredAlienData.Any(e => e.Value.isCollected());
		if (alien)
			desc += "\n" + ll.getString("alien");
		desc += "\n\n" + ll.getString("prefix") + "\n";
		desc = appendDataList(desc, "Aurora Data", requiredAuroraData);
		desc = appendDataList(desc, "Degasi Data", requiredDegasiData);
		if (alien)
			desc = appendDataList(desc, "Alien Data", requiredAlienData);
		desc += "\n\nAlien Artifacts:\n";
		foreach (var le in alienBaseScans) {
			var has = le.isScanned();
			var seen = le.location != null && (le.location.checkSeen == null || le.location.checkSeen.Invoke());
			var name = has ? Language.main.Get(le.tech) : "Unknown Object";
			if (showAll)
				name += " [" + Language.main.Get(le.tech) + "]";
			var color = has ? "20FF40" : seen ? "FFE020" : "FF2040";
			desc +=
				$"\t<color=#{color}>{name}</color> ({(has ? "Collected" : seen ? le.location.getDescription() : unknownLocation.getDescription())})\n";
		}
		return desc;
	}

	private string appendDataList(string desc, string title, Dictionary<string, DataDownloadEntry> li) {
		if (li == null) {
			SNUtil.writeToChat("Null data collect map under title=" + title);
			return "ERROR";
		}
		desc += title + ":\n";
		foreach (var kvp in li) {
			var le = kvp.Value;
			if (le == null) {
				SNUtil.writeToChat("Null entry in data collect PDA, key=" + kvp.Key);
				continue;
			}
			if (!le.isVisible())
				continue;
			var has = le.isCollected();
			if (le.location == null)
				SNUtil.writeToChat("No location for " + le);
			else if (le.location.checkSeen == null)
				SNUtil.writeToChat("No location check for " + le);

			var seen = le.location != null && (le.location.checkSeen == null || le.location.checkSeen.Invoke());
			var name = has ? Language.main.Get("Ency_"+le.encyKey) : le.hint;
			if (showAll)
				name += " [" + Language.main.Get("Ency_" + le.encyKey) + "]";
			var color = has ? "20FF40" : seen ? "FFE020" : "FF2040";
			desc +=
				$"\t<color=#{color}>{name}</color> ({(has ? "Collected" : seen ? le.location.getDescription() : unknownLocation.getDescription())})\n";
		}
		desc += "\n\n";
		return desc;
	}

	public bool isFullyComplete() {
		return getMissingAuroraData().Count == 0 && getMissingDegasiData().Count == 0 && getMissingAlienData().Count == 0 && alienBaseScans.All(e => e.isScanned());
	}

	public List<DataDownloadEntry> getMissingAlienData() {
		return getMissingData(requiredAlienData);
	}

	public List<DataDownloadEntry> getMissingAuroraData() {
		return getMissingData(requiredAuroraData);
	}

	public List<DataDownloadEntry> getMissingDegasiData() {
		return getMissingData(requiredDegasiData);
	}

	private List<DataDownloadEntry> getMissingData(Dictionary<string, DataDownloadEntry> dict) {
		List<DataDownloadEntry> li = [];
		foreach (var e in dict.Values) {
			if (!e.isCollected())
				li.Add(e);
		}
		return li;
	}

	public class DataDownloadEntry : IComparable<DataDownloadEntry> {

		public readonly string encyKey;
		public readonly LocationDescriptor location;
		public readonly string category;
		internal readonly PDAEncyclopedia.EntryData pdaPage;

		internal readonly string hint;

		private Func<bool> visiblityTrigger;

		internal DataDownloadEntry(LocationDescriptor f, string ency, string h) {
			encyKey = ency;
			location = f;
			hint = h;

			pdaPage = getEncyData();
			category = pdaPage == null ? "General" : SNUtil.getDescriptiveEncyPageCategoryName(pdaPage);
		}

		public void setVisible(string goal) {
			setVisible(() => Story.StoryGoalManager.main.IsGoalComplete(goal));
		}

		public void setVisible(Func<bool> f) {
			visiblityTrigger = f;
		}

		public bool isVisible() {
			return visiblityTrigger == null || visiblityTrigger.Invoke();
		}

		public bool isCollected() {
			return Story.StoryGoalManager.main.IsGoalComplete(encyKey);//pdaPage != null && pdaPage.unlocked;
		}

		public PDAEncyclopedia.EntryData getEncyData() {
			return PDAEncyclopedia.mapping.ContainsKey(encyKey) ? PDAEncyclopedia.mapping[encyKey] : null;
		}

		public int CompareTo(DataDownloadEntry ro) {
			var us = getEncyData();
			var them = ro.getEncyData();
			return us == null && them == null
				? string.Compare(encyKey, ro.encyKey, StringComparison.InvariantCultureIgnoreCase)
				: us == null ? -1 : them == null ? 1 : string.Compare(us.path, them.path, StringComparison.InvariantCultureIgnoreCase);
		}

		internal void saveToXML(XmlElement n) {
			n.addProperty("encyKey", encyKey);
		}

		internal void loadFromXML(XmlElement e) {

		}

		public override string ToString() {
			return
				$"[DataDownloadEntry EncyKey={encyKey}, Location={location}, Category={category}, PdaPage={pdaPage}, Hint={hint}, VisiblityTrigger={visiblityTrigger}]";
		}



	}

	public class AlienScanEntry : IComparable<AlienScanEntry> {

		public readonly LocationDescriptor location;
		public readonly TechType tech;

		internal AlienScanEntry(LocationDescriptor f, TechType tt) {
			location = f;
			tech = tt;
		}

		public bool isScanned() {
			return PDAScanner.complete.Contains(tech);
		}

		public int CompareTo(AlienScanEntry ro) {
			return tech.CompareTo(ro.tech);
		}

		internal void saveToXML(XmlElement n) {
			//if (location != null)
			//	n.addProperty("location", location.ToString());
			n.addProperty("tech", tech.ToString());
		}

		internal void loadFromXML(XmlElement e) {

		}


	}

	public class LocationDescriptor {

		public readonly Func<bool> checkSeen;
		public readonly Func<string> getDescription;

		internal LocationDescriptor(Story.StoryGoal goal, string desc) : this(goal.key, desc) {

		}

		internal LocationDescriptor(string goal, string desc) : this(() => Story.StoryGoalManager.main.IsGoalComplete(goal), desc) {

		}

		internal LocationDescriptor(Func<bool> see, string desc) : this(see, () => desc) {

		}

		internal LocationDescriptor(Func<bool> see, Func<string> desc) {
			checkSeen = see;
			getDescription = desc;
		}


	}
	/*
	enum Area {
		//Aurora
		AURORA,
		POD1,
		POD2,
		POD3,
		POD4,
		POD6,
		POD7,
		POD9,
		POD10,
		POD12,
		POD13, //khasar
		POD14,
		POD15,
		POD17, //ozzy
		POD19, //keen
		DUNEARCH,
		MOUNTAINISLAND,
		GARGSKULL,

		//Degasi
		FLOATISLAND,
		JELLYSHROOM,
		DGR,

		//Precursor
		GUN,
		SPARSECACHE,
		DUNESCACHE,
		NBKELPCACHE,
		LRLAB,
		DRF,
		ATP,
		PCF
	}
	*/
}