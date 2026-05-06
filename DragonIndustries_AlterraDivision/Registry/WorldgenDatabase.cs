using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class WorldgenDatabase {
    private readonly Assembly ownerMod;

    private readonly List<PositionedPrefab> objects = [];
    private readonly List<WorldGenerator> generators = [];

    public WorldgenDatabase() {
        ownerMod = SNUtil.TryGetModDLL(true);
    }

    public void load(Predicate<string> loadFile = null) {
        var root = Path.GetDirectoryName(ownerMod.Location);
        var folder = Path.Combine(root, "XML/WorldgenSets");
        objects.Clear();
        if (Directory.Exists(folder)) {
            var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                .Where(isLoadableWorldgenXML);
            SNUtil.Log("Loading worldgen maps from folder '" + folder + "': " + string.Join(", ", files), ownerMod);
            foreach (var file in files) {
                if (loadFile != null && !loadFile.Invoke(Path.GetFileNameWithoutExtension(file))) {
                    SNUtil.Log("Skipping worldgen map file @ " + file, ownerMod);
                    continue;
                }

                loadXML(file);
            }
        } else {
            SNUtil.Log("Worldgen XMLs not found!", ownerMod);
        }
    }

    private bool isLoadableWorldgenXML(string file) {
        var ext = Path.GetExtension(file);
        if (ext == ".xml")
            return true;
        if (ext == ".gen") {
            var xml = File.Exists(file.Replace(".gen", ".xml"));
            if (xml)
                SNUtil.Log("Skipping packed worldgen XML " + file + " as an unpacked version is present", ownerMod);
            return !xml;
        }

        return false;
    }

    private void loadXML(string file) {
        SNUtil.Log("Loading worldgen map from XML file @ " + file, ownerMod);
        string xml;
        if (file.EndsWith(".gen", StringComparison.InvariantCultureIgnoreCase)) {
            byte[] arr;
            using (var inp = File.OpenRead(file)) {
                using (var zip = new GZipStream(inp, CompressionMode.Decompress, true)) {
                    using (var mem = new MemoryStream()) {
                        zip.CopyTo(mem);
                        arr = mem.ToArray();
                    }
                }
            }

            arr = arr.Reverse().Skip(8).Where((b, idx) => idx % 2 == 0).ToArray();
            xml = System.Text.Encoding.UTF8.GetString(arr);
        } else {
            xml = File.ReadAllText(file);
        }

        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var loaded = 0;
        foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
            try {
                var count = e.GetAttribute("count");
                var ch = e.GetAttribute("chance");
                var amt = string.IsNullOrEmpty(count) ? 1 : int.Parse(count);
                var chance = string.IsNullOrEmpty(ch) ? 1 : double.Parse(ch);
                for (var i = 0; i < amt; i++) {
                    if (UnityEngine.Random.Range(0F, 1F) <= chance) {
                        var ot = ObjectTemplate.construct(e);
                        if (ot == null) {
                            throw new Exception("No worldgen loadable for '" + e.Name + "' " + e.Format() + ": NULL");
                        } else if (ot is DICustomPrefab pfb) {
                            /*
                            if (pfb.isCrate) {
                                //GenUtil.spawnItemCrate(pfb.position, pfb.tech, pfb.rotation);
                                GenUtil.registerWorldgen(pfb.prefabName, )

                                //CrateFillMap.instance.addValue(gen.position, gen.tech);
                            }
                            else */
                            if (pfb.isDatabox) {
                                GenUtil.spawnDatabox(pfb.position, pfb.tech, pfb.rotation);
                                //DataboxTypingMap.instance.addValue(gen.position, gen.tech);
                            }
                            //else if (gen.isFragment) {
                            //    GenUtil.spawnFragment(gen.position, gen.rotation);
                            //	FragmentTypingMap.instance.addValue(gen.position, gen.tech);
                            //}
                            else {
                                GenUtil.registerWorldgen(pfb, pfb.getManipulationsCallable());
                            }

                            //SNUtil.log("Loaded worldgen prefab "+pfb+" for "+e.format(), ownerMod);
                            objects.Add(pfb);
                            loaded++;
                        } else if (ot is WorldGenerator gen) {
                            GenUtil.registerWorldgen(gen);
                            generators.Add(gen);
                            //SNUtil.log("Loaded worldgenator "+gen+" for "+e.format(), ownerMod);
                        } else {
                            throw new Exception("No worldgen loadable for '" + e.Name + "' " + e.Format());
                        }
                    }
                }
            } catch (Exception ex) {
                SNUtil.Log("Could not load element " + e.Format(), ownerMod);
                SNUtil.Log(ex.ToString(), ownerMod);
            }
        }

        SNUtil.Log("Loaded " + loaded + " worldgen elements from file " + file);
    }

    public int getCount() {
        return objects.Count;
    }

    public int getCount(string classID, Vector3? near = null, float dist = -1) {
        return getPositions(classID, near, dist).Count;
    }

    public int getCount<G>(Vector3? near = null, float dist = -1) where G : WorldGenerator {
        var ret = 0;
        foreach (var pfb in generators) {
            if (pfb is G) {
                if (dist < 0 || near == null || !near.HasValue || Vector3.Distance(near.Value, pfb.position) <= dist)
                    ret++;
            }
        }

        return ret;
    }

    public List<PositionedPrefab> getPositions(string classID, Vector3? near = null, float dist = -1) {
        List<PositionedPrefab> ret = [];
        foreach (var pfb in objects) {
            if (pfb.prefabName == classID || classID == "*") {
                if (dist < 0 || near == null || !near.HasValue || Vector3.Distance(near.Value, pfb.position) <= dist)
                    ret.Add(pfb);
            }
        }

        if (ret.Count == 0) {
            SNUtil.Log(
                "Found no prefabs of ID '" + classID + "' during a search! All prefabs:\n" + objects.ToDebugString("\n")
            );
        }

        return ret;
    }

    public PositionedPrefab getByID(string id) {
        foreach (var pfb in objects) {
            if (pfb.getXMLID() == id) {
                return pfb;
            }
        }

        return null;
    }
}