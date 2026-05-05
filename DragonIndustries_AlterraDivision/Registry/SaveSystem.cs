using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class SaveSystem {
    private static readonly Dictionary<string, SaveHandler> handlers = new();
    private static readonly Dictionary<string, XmlElement> saveData = new();

    private static readonly Dictionary<string, PlayerSaveHook> playerSaveHandlers = new();

    private static readonly string oldSaveDir;
    private static readonly string saveFileName = "ModData.dat";
    private static bool loaded;

    public static bool debugSave = false;

    static SaveSystem() {
        // TODO
        // IngameMenuHandler.Main.RegisterOnLoadEvent(handleLoad);
        // IngameMenuHandler.Main.RegisterOnSaveEvent(handleSave);

        oldSaveDir = Path.Combine(Path.GetDirectoryName(SNUtil.diDLL.Location), "persistentData");
        SNUtil.migrateSaveDataFolder(oldSaveDir, ".dat", saveFileName);
    }

    public static void addSaveHandler(CustomPrefab pfb, SaveHandler h) {
        addSaveHandler(pfb.Info.ClassID, h);
    }

    public static void addSaveHandler(string classID, SaveHandler h) {
        if (handlers.ContainsKey(classID))
            throw new Exception("A save handler is already registered to id '" + classID + "': " + handlers[classID]);
        handlers[classID] = h;
    }

    public static void addPlayerSaveCallback<O>(Type t, string fieldName, Func<O> instance) {
        MemberInfo field = t.GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
        );
        if (field == null)
            field = t.GetProperty(
                fieldName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            );
        if (field == null)
            throw new Exception("No such field or property '" + fieldName + "' in '" + t.Name + "'!");
        addPlayerSaveCallback(new FieldHook<O>(field, instance));
    }

    public static void addPlayerSaveCallback(PlayerSaveHook s) {
        if (!XmlReader.IsName(s.id))
            throw new Exception("Invalid non-XMLSafe ID '" + s.id + "'");
        if (playerSaveHandlers.ContainsKey(s.id))
            throw new Exception("Player save hook ID '" + s.id + "' already registered!");
        playerSaveHandlers[s.id] = s;
    }

    public static void handleSave() {
        var path = Path.Combine(SNUtil.getCurrentSaveDir(), saveFileName);
        var doc = new XmlDocument();
        var rootnode = doc.CreateElement("Root");
        doc.AppendChild(rootnode);
        foreach (var pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
            if (!pi)
                continue;
            var sh = getHandler(pi, false);
            if (sh != null) {
                try {
                    if (debugSave)
                        SNUtil.log("Found " + sh + " save handler for " + pi.ClassId, SNUtil.diDLL);
                    sh.data = doc.CreateElement("object");
                    sh.data.SetAttribute("objectID", pi.Id);
                    sh.save(pi);
                    doc.DocumentElement.AppendChild(sh.data);
                } catch (Exception ex) {
                    SNUtil.writeToChat(
                        "Failed to save data for object " + pi.name + " [" + pi.ClassId + "]: " + ex.ToString()
                    );
                }
            }
        }

        var e = doc.CreateElement("player");
        foreach (var t in playerSaveHandlers.Values) {
            if (t.saveAction == null) {
                SNUtil.log("Could not run save handler " + t + " on player: no save hook", SNUtil.diDLL);
                continue;
            }

            try {
                var e2 = doc.CreateElement(t.id);
                t.saveAction.Invoke(Player.main, e2);
                e.AppendChild(e2);
            } catch (Exception ex) {
                SNUtil.log("Save handler " + t + " on player threw " + ex, SNUtil.diDLL);
            }
        }

        doc.DocumentElement.AppendChild(e);
        e = doc.CreateElement("components");
        foreach (var cs in Player.main.GetComponents<CustomSerializedComponent>()) {
            var t = cs.GetType();
            var e2 = doc.CreateElement(t.Namespace + "." + t.Name);
            cs.saveToXML(e2);
            e.AppendChild(e2);
        }

        doc.DocumentElement.AppendChild(e);
        SNUtil.log("Saving " + doc.DocumentElement.ChildNodes.Count + " objects to disk", SNUtil.diDLL);
        Directory.GetParent(path).Create();
        doc.Save(path);
    }

    public static void handleLoad() {
        var dir = SNUtil.getCurrentSaveDir();
        var path = Path.Combine(dir, saveFileName);
        if (!File.Exists(path))
            path = Path.Combine(dir, saveFileName.Replace(".dat", ".xml"));
        if (File.Exists(path)) {
            var doc = new XmlDocument();
            doc.Load(path);
            saveData.Clear();
            foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
                saveData[e.Name == "player" || e.Name == "components" ? e.Name : e.GetAttribute("objectID")] = e;
            }

            SNUtil.log("Loaded " + saveData.Count + " object entries from disk", SNUtil.diDLL);
        }
    }

    public static void populateLoad() {
        if (loaded)
            return;
        loaded = true;
        SNUtil.log("Applying saved object entries", SNUtil.diDLL);
        if (saveData.ContainsKey("player")) {
            var e = saveData["player"];
            foreach (XmlElement e2 in e.ChildNodes) {
                if (!playerSaveHandlers.ContainsKey(e2.Name)) {
                    SNUtil.log("Skipping player save tag '" + e2.Name + "'; no mapping to a handler", SNUtil.diDLL);
                    continue;
                }

                var t = playerSaveHandlers[e2.Name];
                if (t.loadAction == null) {
                    SNUtil.log("Could not run load handler " + t + " on player: no load hook", SNUtil.diDLL);
                    continue;
                }

                try {
                    t.loadAction.Invoke(Player.main, e2);
                    SNUtil.log("Applied player load action " + t.id, SNUtil.diDLL);
                } catch (Exception ex) {
                    SNUtil.log("Save handler " + t + " on player threw " + ex, SNUtil.diDLL);
                }
            }
        }

        if (saveData.ContainsKey("components")) {
            var e = saveData["components"];
            foreach (XmlElement e2 in e.ChildNodes) {
                try {
                    var t = InstructionHandlers.getTypeBySimpleName(e2.Name);
                    if (t == null) {
                        SNUtil.log(
                            "Could not reinstantiate custom serialized component: no type found for '" + e2.Name + "'",
                            SNUtil.diDLL
                        );
                        continue;
                    }

                    var cs = (CustomSerializedComponent)Player.main.gameObject.AddComponent(t);
                    cs.readFromXML(e2);
                    SNUtil.log("Deserialized new " + t + " onto player", SNUtil.diDLL);
                } catch (Exception ex) {
                    SNUtil.log("Trying to deserialize " + e2.Name + " on player threw " + ex, SNUtil.diDLL);
                }
            }
        }

        foreach (var pi in UnityEngine.Object.FindObjectsOfType<PrefabIdentifier>()) {
            var sh = getHandler(pi, true);
            if (sh != null) {
                SNUtil.log("Found " + sh + " load handler for " + pi.ClassId + " [" + pi.id + "]", SNUtil.diDLL);
                try {
                    sh.load(pi);
                } catch (Exception e) {
                    SNUtil.log("Threw error loading object " + pi.ClassId + " " + pi.Id + ": " + e, SNUtil.diDLL);
                }
            }
        }
    }

    private static SaveHandler getHandler(PrefabIdentifier pi, bool needSaveData) {
        if (pi && !string.IsNullOrEmpty(pi.ClassId)) {
            XmlElement elem = null;
            //SNUtil.log("Attempting to load "+pi+" ["+pi.id+"]", SNUtil.diDLL);
            if (needSaveData && handlers.ContainsKey(pi.ClassId) && !saveData.ContainsKey(pi.Id))
                SNUtil.log("Object " + pi + " [" + pi.id + "] had no data to load!", SNUtil.diDLL);
            if (handlers.TryGetValue(pi.ClassId, out var ret) &&
                (!needSaveData || saveData.TryGetValue(pi.Id, out elem))) {
                if (elem != null)
                    ret.data = elem;
                return ret;
            }
        }

        return null;
    }

    public abstract class SaveHandler {
        protected internal XmlElement data;

        public abstract void save(PrefabIdentifier pi);
        public abstract void load(PrefabIdentifier pi);
    }

    public static void saveToXML(XmlElement e, string s, object val) {
        if (val is string val1)
            e.addProperty(s, val1);
        else if (val is int i)
            e.addProperty(s, i);
        else if (val is bool b)
            e.addProperty(s, b);
        if (val is float f)
            e.addProperty(s, f);
        if (val is double d)
            e.addProperty(s, d);
        else if (val is Vector3 vector3)
            e.addProperty(s, vector3);
        else if (val is Quaternion quaternion)
            e.addProperty(s, quaternion);
        else if (val is Color color)
            e.addProperty(s, color);
    }

    public static void setField(XmlElement e, string s, MemberInfo f, object inst) {
        object put = null;
        var t = f is FieldInfo info ? info.FieldType : ((PropertyInfo)f).PropertyType;

        if (t == typeof(string))
            put = e.getProperty(s, true);
        else if (t == typeof(bool))
            put = e.getBoolean(s);
        else if (t == typeof(int))
            put = e.getInt(s, 0, true);
        else if (t == typeof(float))
            put = (float)e.getFloat(s, 0);
        else if (t == typeof(double))
            put = e.getFloat(s, 0);
        else if (t == typeof(Vector3))
            put = e.getVector(s, true).GetValueOrDefault();
        else if (t == typeof(Quaternion))
            put = e.getQuaternion(s, true).GetValueOrDefault();
        else if (t == typeof(Color))
            put = e.getColor(s, true, true).GetValueOrDefault();

        if (f is FieldInfo fi)
            fi.SetValue(inst, put);
        else
            ((PropertyInfo)f).SetValue(inst, put);
    }

    public class FieldHook<O> : PlayerSaveHook {
        public readonly MemberInfo field;
        public readonly Func<O> instanceGetter;

        public FieldHook(MemberInfo f, Func<O> inst) : base(
            buildID(f),
            (ep, e) => saveToXML(e, "value", getValue(f, inst.Invoke())),
            (ep, e) => setField(e, "value", f, inst.Invoke())
        ) {
            field = f;
            instanceGetter = inst;
        }

        private static object getValue(MemberInfo f, O obj) {
            return f is FieldInfo fi ? fi.GetValue(obj) : ((PropertyInfo)f).GetValue(obj);
        }

        private static string buildID(MemberInfo f) {
            return f.DeclaringType.Namespace + "." + f.DeclaringType.Name + "_" + f.Name;
        }
    }

    public class PlayerSaveHook {
        public readonly string id;
        public readonly Action<Player, XmlElement> saveAction;
        public readonly Action<Player, XmlElement> loadAction;

        public PlayerSaveHook(string id, Action<Player, XmlElement> s, Action<Player, XmlElement> l) {
            this.id = id;
            saveAction = s;
            loadAction = l;
        }
    }

    public sealed class ComponentFieldSaveHandler<C> : SaveHandler where C : MonoBehaviour {
        private readonly List<string> fields = [];

        public ComponentFieldSaveHandler() {
        }

        public ComponentFieldSaveHandler(params string[] f) : this(f.ToList()) {
        }

        public ComponentFieldSaveHandler(IEnumerable<string> f) {
            fields.AddRange(f);
        }

        public ComponentFieldSaveHandler<C> addField(string f) {
            fields.Add(f);
            return this;
        }

        public ComponentFieldSaveHandler<C> addAllFields() {
            foreach (var fi in typeof(C).GetFields(
                         BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                     )) {
                addField(fi.Name);
            }

            return this;
        }

        public override void save(PrefabIdentifier pi) {
            var com = pi.GetComponentInChildren<C>();
            if (!com)
                return;
            foreach (var s in fields) {
                var m = getField(s);
                var val = m is FieldInfo fi ? fi.GetValue(com) : ((PropertyInfo)m).GetValue(com);
                saveToXML(data, s, val);
            }
        }

        public override void load(PrefabIdentifier pi) {
            var com = pi.GetComponentInChildren<C>();
            if (!com)
                return;
            foreach (var s in fields) {
                var fi = getField(s);
                setField(data, s, fi, com);
            }
        }

        private MemberInfo getField(string s) {
            MemberInfo ret = typeof(C).GetField(
                s,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );
            if (ret == null)
                ret = typeof(C).GetProperty(s, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (ret == null)
                throw new Exception(typeof(C).Name + " has no field or property named '" + s + "'!");
            return ret;
        }

        public override string ToString() {
            return $"[ComponentFieldSaveHandler Fields={fields.toDebugString()}]";
        }
    }
}