/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using Nautilus.Handlers;
using UnityEngine;


namespace ReikaKalseki.DIAlterra;

[Serializable]
public class DICustomPrefab : PositionedPrefab {
    public static readonly string TAGNAME = "customprefab";

    private static readonly Dictionary<string, ModifiedObjectPrefab> prefabCache = new();

    private static HashSet<string> prefabNamespaces = ["ReikaKalseki.DIAlterra"];

    [SerializeField] public TechType tech = TechType.None;
    [SerializeField] internal readonly List<ManipulationBase> manipulations = [];

    public bool isSeabase { get; protected set; }
    public bool isBasePiece { get; internal set; }
    public bool isCrate { get; private set; }
    public bool isFragment { get; private set; }
    public bool isDatabox { get; private set; }
    public bool isPDA { get; private set; }
    public bool isWreck { get; private set; }
    public bool isO2Pipe { get; private set; }

    public ModifiedObjectPrefab customPrefab { get; private set; }

    public DICustomPrefab(string pfb) : base(pfb) {
    }

    public DICustomPrefab(PositionedPrefab pfb) : base(pfb) {
    }

    static DICustomPrefab() {
        registerType(TAGNAME, e => new DICustomPrefab(e.GetProperty("prefab")));
    }

    public static void addPrefabNamespace(string s) {
        prefabNamespaces.Add(s);
    }

    public override string ToString() {
        return base.ToString().Replace(" @ ", " [" + tech + "] @ ");
    }

    public void setSeabase() {
        isSeabase = true;
        prefabName = "seabase";
    }

    public override string getTagName() {
        return TAGNAME;
    }

    public override void saveToXML(XmlElement e) {
        var n = prefabName;
        if (isBasePiece) {
            e.AddProperty("piece", prefabName);
            prefabName = "basePart";
        }

        base.saveToXML(e);
        prefabName = n;
        if (tech != TechType.None)
            e.AddProperty("tech", Enum.GetName(typeof(TechType), tech));
        if (manipulations.Count > 0) {
            var e1 = e.OwnerDocument.CreateElement("objectManipulation");
            foreach (var mb in manipulations) {
                var e2 = e.OwnerDocument.CreateElement(mb.GetType().Name);
                mb.saveToXML(e2);
                e1.AppendChild(e2);
            }

            e.AppendChild(e1);
        }
    }

    public Action<GameObject> getManipulationsCallable() {
        return go => {
            foreach (var mb in manipulations) {
                mb.applyToObject(go);
            }
        };
    }

    public ReadOnlyCollection<M> getManipulations<M>() where M : ManipulationBase {
        return manipulations.Where(m => m is M).Cast<M>().ToList().AsReadOnly();
    }

    public override GameObject createWorldObject() {
        if (isBasePiece) {
            var go = ObjectUtil.getBasePiece(prefabName);
            if (go != null) {
                go.transform.position = position;
                go.transform.rotation = rotation;
                go.transform.localScale = scale;
            }

            return go;
        } else {
            return base.createWorldObject();
        }
    }

    public override void loadFromXML(XmlElement e) {
        base.loadFromXML(e);
        if (prefabName.StartsWith("res_", StringComparison.InvariantCultureIgnoreCase)) {
            prefabName =
                ((VanillaResources)typeof(VanillaResources).GetField(prefabName.Substring(4).ToUpper()).GetValue(null))
                .prefab;
        } else if (prefabName.StartsWith("fauna_", StringComparison.InvariantCultureIgnoreCase)) {
            prefabName =
                ((VanillaCreatures)typeof(VanillaCreatures).GetField(prefabName.Substring(6).ToUpper()).GetValue(null))
                .prefab;
        } else if (prefabName.StartsWith("flora_", StringComparison.InvariantCultureIgnoreCase)) {
            prefabName = VanillaFlora.getByName(prefabName.Substring(6)).getRandomPrefab(false);
        } else if (prefabName.StartsWith("base_", StringComparison.InvariantCultureIgnoreCase)) {
            isBasePiece = true;
        } else if (prefabName == "o2pipe") {
            isO2Pipe = true;
            prefabName = "08078333-1a00-42f8-8492-e2640c17a961";
            manipulations.Add(new PipeReconnection(e.GetVector("connection").Value));
            SNUtil.Log("Redirected customprefab to pipe " + prefabName, SNUtil.DiDLL);
        } else if (prefabName == "crate") {
            isCrate = true;
            var techn = e.GetProperty("item");
            tech = SNUtil.GetTechType(techn);
            if (tech == TechType.None)
                throw new Exception("Cannot put nonexistent item '" + techn + "' in crate @ " + position + "!");
            prefabName = GenUtil.getOrCreateCrate(tech, e.GetBoolean("sealed"), e.GetProperty("goal", true)).Info
                .ClassID;
            SNUtil.Log("Redirected customprefab to crate " + prefabName, SNUtil.DiDLL);
        } else if (prefabName == "databox") {
            isDatabox = true;
            var techn = e.GetProperty("tech");
            tech = SNUtil.GetTechType(techn);
            prefabName = GenUtil.getOrCreateDatabox(tech).ClassID;
            SNUtil.Log("Redirected customprefab to databox " + prefabName, SNUtil.DiDLL);
        } else if (prefabName == "fragment") {
            isFragment = true;
            var techn = e.GetProperty("tech");
            tech = SNUtil.GetTechType(techn);
            var g = GenUtil.getFragment(tech, e.GetInt("index", 0));
            if (g == null)
                throw new Exception("No such fragment!");
            prefabName = g.ClassID;
            SNUtil.Log("Redirected customprefab to fragment " + prefabName, SNUtil.DiDLL);
        } else if (prefabName == "pda") {
            isPDA = true;
            var pagen = e.GetProperty("page");
            var page = PDAManager.getPage(pagen);
            prefabName = page.getPDAClassID();
            SNUtil.Log("Redirected customprefab to pda " + prefabName, SNUtil.DiDLL);
        } else if (prefabName == "wreck") {
            isWreck = true;
            var template = e.GetProperty("template");
            prefabName = template;
            SNUtil.Log("Redirected customprefab to wreck " + prefabName, SNUtil.DiDLL);
        } else if (prefabName == "basePart") {
            isBasePiece = true;
            prefabName = e.GetProperty("piece");
            var li0 = e.GetDirectElementsByTagName("supportData");
            if (li0.Count == 1)
                manipulations.Add(new SeabaseLegLengthPreservation(li0[0]));
            SNUtil.Log(
                "Redirected customprefab to base piece " + prefabName + " >> " + li0.Count + "::" + string.Join(
                    ", ",
                    li0.Select(el => el.OuterXml)
                ),
                SNUtil.DiDLL
            );
        } else if (prefabName == "seabase") {
            prefabName = SeabaseReconstruction.getOrCreatePrefab(e).Info.ClassID;
            isSeabase = true;
            SNUtil.Log("Redirected customprefab to seabase", SNUtil.DiDLL);
        }

        //else if (prefabName == "fragment") {
        //	prefabName = ?;
        //	isFragment = true;
        //	string techn = e.getProperty("type");
        //	tech = SNUtil.getTechType(techn);
        //}
        var tech2 = e.GetProperty("tech", true);
        if (tech == TechType.None && tech2 != null && tech2 != "None") {
            tech = SNUtil.GetTechType(tech2);
        }

        var xli = e.OwnerDocument.DocumentElement != null
            ? e.OwnerDocument.DocumentElement.GetAllChildrenIn("transforms")
            : null;
        if (xli != null)
            loadManipulations(xli, manipulations);
        var li = e.GetDirectElementsByTagName("objectManipulation");
        if (li.Count == 1) {
            var mod = getManipulatedObject(li[0], this);
            if (mod != null) {
                mod.originalPrefab = prefabName;
                mod.prefabSource = this;
                prefabName = mod.ClassID;
                tech = mod.Info.TechType;
            }
        }
    }

    public static ModifiedObjectPrefab getManipulatedObject(XmlElement e, DICustomPrefab pfb) {
        loadManipulations(e, pfb.manipulations);
        if (pfb.manipulations.Count > 0) {
            var needReapply = false;
            foreach (var mb in pfb.manipulations) {
                if (mb.needsReapplication()) {
                    needReapply = true;
                    break;
                }
            }

            if (needReapply) {
                var xmlKey = pfb.prefabName + "##" + System.Security.SecurityElement.Escape(e.InnerXml);
                return getOrCreateModPrefab(pfb, xmlKey);
            }
        }

        return null;
    }

    private static ModifiedObjectPrefab getOrCreateModPrefab(DICustomPrefab orig, string key) {
        var pfb = prefabCache.ContainsKey(key) ? prefabCache[key] : null;
        if (pfb == null) {
            pfb = new ModifiedObjectPrefab(key, orig.prefabName, orig.manipulations);
            prefabCache[key] = pfb;
            var from = orig.tech != TechType.None
                ? orig.tech
                : CraftData.entClassTechTable.GetOrDefault(key, TechType.None);
            if (from != TechType.None) {
                KnownTechHandler.SetAnalysisTechEntry(pfb.Info.TechType, new List<TechType>() { from });
                var e = new PDAScanner.EntryData {
                    key = pfb.Info.TechType,
                    blueprint = from,
                    destroyAfterScan = false,
                    locked = true,
                    scanTime = 5,
                };
                PDAHandler.AddCustomScannerEntry(e);
            }

            SNUtil.Log("Created customprefab GO template: " + key + " [" + from + "] > " + pfb, SNUtil.DiDLL);
        } else {
            SNUtil.Log("Using already-generated prefab for GO template: " + key + " > " + pfb, SNUtil.DiDLL);
        }

        return pfb;
    }

    internal static void loadManipulations(XmlNodeList es, List<ManipulationBase> li) {
        if (es == null)
            return;
        foreach (XmlElement e2 in es) {
            var mb = loadManipulation(e2);
            if (mb != null)
                li.Add(mb);
        }
    }

    internal static void loadManipulations(XmlElement e, List<ManipulationBase> li) {
        loadManipulations(e.ChildNodes, li);
    }

    public static ManipulationBase loadManipulation(XmlElement e2) {
        try {
            if (e2 == null)
                throw new Exception("Null XML elem");
            Type t = null;
            foreach (var s in prefabNamespaces) {
                t = InstructionHandlers.getTypeBySimpleName(s + "." + e2.Name);
                if (t != null)
                    break;
            }

            if (t == null)
                throw new Exception(
                    "Type '" + e2.Name + "' not found; is a namespace missing from " +
                    string.Join(", ", prefabNamespaces)
                );
            var ct = t.GetConstructor([]);
            if (ct == null)
                throw new Exception("Constructor not found");
            try {
                var mb = (ManipulationBase)ct.Invoke([]);
                mb.loadFromXML(e2);
                return mb;
            } catch (Exception ex) {
                throw new Exception("Construction error " + ex);
            }
        } catch (Exception ex) {
            var err = "Could not rebuild manipulation from XML " + e2.Name + "/" + e2.InnerText + ": " + ex;
            SNUtil.Log(err, SNUtil.DiDLL);
            SNUtil.WriteToChat(err);
            return null;
        }
    }

    internal void prepareGameObject(ModifiedObjectPrefab mod, GameObject go) {
        if (isWreck) {
            //go.EnsureComponent<WreckDataLoader>().
        }
    }
}

public class ModifiedObjectPrefab : GenUtil.CustomPrefabImpl {
    private readonly List<ManipulationBase> mods = [];

    public string originalPrefab { get; internal set; }
    public DICustomPrefab prefabSource { get; internal set; }

    [SetsRequiredMembers]
    internal ModifiedObjectPrefab(string key, string template, List<ManipulationBase> li) : base(key, template) {
        mods = li;
    }

    public override sealed void prepareGameObject(GameObject go, Renderer[] r) {
        SNUtil.Log("Restoring manipulations on modified prefab " + originalPrefab + ":\n" + mods.ToDebugString("\n"));
        foreach (var mb in mods) {
            mb.applyToObject(go);
        }

        if (prefabSource != null)
            prefabSource.prepareGameObject(this, go);
    }

    public override string ToString() {
        return "Modified " + baseTemplate.prefab + getString(mods);
    }

    private static string getString(List<ManipulationBase> li) {
        return " x" + li.Count + "=" + string.Join("/", li.Select(mb => mb.GetType().Name));
    }
}