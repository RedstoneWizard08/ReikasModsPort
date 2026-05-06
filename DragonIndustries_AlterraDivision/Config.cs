using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;

namespace ReikaKalseki.DIAlterra;

public class Config<E> {
	private readonly string filename;
	private readonly Dictionary<string, float> data = new();
	private readonly Dictionary<string, string> dataString = new();
	private readonly Dictionary<E, ConfigEntry> entryCache = new();

	private readonly Assembly owner;

	private bool loaded;

	private readonly Dictionary<string, Func<float, float>> overrides = new();
	private readonly Dictionary<string, Func<string, string>> overridesString = new();

	public Config(Assembly owner) {
		this.owner = owner;
		filename = /*Environment.UserName+"_"+*/owner.GetName().Name + "_Config.xml";
		populateDefaults();
	}

	public void attachOverride(E key, bool val) {
		attachOverride(key, val ? 1 : 0);
	}

	public void attachOverride(E key, float val) {
		attachOverride(key, f => val);
	}

	public void attachOverride(E key, Func<bool, bool> val) {
		attachOverride(key, f => val(f > 0.001) ? 1 : 0);
	}

	public void attachOverride(E key, Func<float, float> val) {
		var k = getKey(key);
		overrides[k] = val;
	}

	public void attachOverride(E key, Func<string, string> val) {
		var k = getKey(key);
		overridesString[k] = val;
	}
	/*
	private void applyOverrides() {
		foreach (KeyValuePair<string, Func<float>> kvp in overrides) {
			data[kvp.Key] = kvp.Value(data[kvp.Key]);
		}
		foreach (KeyValuePair<string, Func<string>> kvp in overridesString) {
			dataString[kvp.Key] = kvp.Value(dataString[kvp.Key]);
		}
	}
	*/
	private void populateDefaults() {
		foreach (E key in Enum.GetValues(typeof(E))) {
			var name = Enum.GetName(typeof(E), key);
			var e = getEntry(key);
			e.enumIndex = name;
			data[name] = e.defaultValue;
			//SNUtil.log("Initializing config entry "+name+" to "+e.formatValue(e.defaultValue)+" hash = "+RuntimeHelpers.GetHashCode(e), owner);
		}
	}

	public void load(bool force = false) {
		if (loaded && !force)
			return;
		var folder = Path.Combine(Path.GetDirectoryName(owner.Location), "Config");
		Directory.CreateDirectory(folder);
		var path = Path.Combine(folder, filename);
		if (File.Exists(path)) {
			SNUtil.Log("Loading config file at " + path, owner);
			try {
				var doc = new XmlDocument();
				doc.Load(path);
				var root = (XmlElement)doc.GetElementsByTagName("Settings")[0];
				var missing = new HashSet<string>(data.Keys);
				foreach (XmlNode e in root.ChildNodes) {
					if (!(e is XmlElement element))
						continue;
					var name = element.Name;
					try {
						var val = (XmlElement)element.GetElementsByTagName("value")[0];
						var key = (E)Enum.Parse(typeof(E), name);
						var entry = getEntry(key);
						var raw = entry.parse(val.InnerText);
						var get = raw;
						if (!entry.validate(ref get)) {
							SNUtil.Log("Chosen " + name + " value (" + raw + ") was out of bounds, clamped to " + get, owner);
						}
						data[name] = get;
						dataString[name] = val.InnerText;
						missing.Remove(name);
					}
					catch (Exception ex) {
						SNUtil.Log("Config entry " + name + " failed to load: " + ex.ToString(), owner);
					}
				}
				var vals = string.Join(";", data.Select(x => x.Key + "=" + x.Value).ToArray());
				SNUtil.Log("Config successfully loaded: " + vals, owner);
				if (missing.Count > 0) {
					var keys = string.Join(";", missing.ToArray());
					SNUtil.Log("Note: " + missing.Count + " entries were missing from the config and so stayed the default values.", owner);
					SNUtil.Log("Missing keys: " + keys, owner);
					//SNUtil.log("It is recommended that you regenerate your config by renaming your current config file, letting a new one generate," +
					// "then copying your changes into the new one.", owner);
					SNUtil.Log("Your config will be regenerated (keeping your changes) to add them to the file.", owner);
					File.Delete(path);
					generateFile(path, e => getFloat(getEnum(e)));
				}
			}
			catch (Exception ex) {
				SNUtil.Log("Config failed to load: " + ex.ToString(), owner);
			}
		}
		else {
			SNUtil.Log("Config file does not exist at " + path + "; generating.", owner);
			generateFile(path, e => e.defaultValue);
		}
		//applyOverrides();
		loaded = true;
	}

	private void generateFile(string path, Func<ConfigEntry, float> valGetter) {
		try {
			var doc = new XmlDocument();
			var root = doc.CreateElement("Settings");
			doc.AppendChild(root);
			foreach (E key in Enum.GetValues(typeof(E))) {
				try {
					createNode(doc, root, key, valGetter);
				}
				catch (Exception e) {
					SNUtil.Log("Could not generate XML node for " + key + ": " + e.ToString(), owner);
				}
			}
			doc.Save(path);
			SNUtil.Log("Config successfully generated at " + path, owner);
		}
		catch (Exception ex) {
			SNUtil.Log("Config failed to generate: " + ex.ToString(), owner);
		}
	}

	private void createNode(XmlDocument doc, XmlElement root, E key, Func<ConfigEntry, float> valGetter) {
		var e = getEntry(key);
		var node = doc.CreateElement(Enum.GetName(typeof(E), key));

		var com = doc.CreateComment(e.desc);

		var val = doc.CreateElement("value");
		var amt = valGetter(e);
		//SNUtil.log(valGetter+": Parsed value "+amt+" for "+key, owner);
		val.InnerText = e.formatValue(amt);
		node.AppendChild(val);

		var def = doc.CreateElement("defaultValue");
		def.InnerText = e.formatValue(e.defaultValue);
		node.AppendChild(def);
		if (!float.IsNaN(e.vanillaValue)) {
			var van = doc.CreateElement("vanillaValue");
			van.InnerText = e.formatValue(e.vanillaValue);
			node.AppendChild(van);
		}

		//XmlElement desc = doc.CreateElement("description");
		//desc.InnerText = e.desc;
		//node.AppendChild(desc);

		if (e.type != typeof(bool)) {
			var min = doc.CreateElement("minimumValue");
			min.InnerText = e.formatValue(e.minValue);
			node.AppendChild(min);
			var max = doc.CreateElement("maximumValue");
			max.InnerText = e.formatValue(e.maxValue);
			node.AppendChild(max);
		}
		root.AppendChild(com);
		root.AppendChild(node);
	}

	private float getValue(string key) {
		var ret = data.ContainsKey(key) ? data[key] : 0;
		if (overrides.ContainsKey(key))
			ret = overrides[key](ret);
		return ret;
	}

	private string getStringValue(string key) {
		var ret = dataString.ContainsKey(key) ? dataString[key] : null;
		if (overridesString.ContainsKey(key))
			ret = overridesString[key](ret);
		return ret;
	}

	public bool getBoolean(E key) {
		var ret = getFloat(key);
		return ret > 0.001;
	}

	public int getInt(E key) {
		var ret = getFloat(key);
		return (int)Math.Floor(ret);
	}

	public float getFloat(E key) {
		return getValue(getKey(key));
	}

	public string getString(E key) {
		return getStringValue(getKey(key));
	}

	private string getKey(E key) {
		return Enum.GetName(typeof(E), key);
	}
	/*
	public void setValue(E key, float val) {
		data[getKey(key)] = val;
	}

	public void setValue(E key, bool val) {
		setValue(key, 0);
	}

	public void setValue(E key, string val) {
		dataString[getKey(key)] = val;
	}
	*/
	public ConfigEntry getEntry(E key) {
		if (!entryCache.ContainsKey(key)) {
			entryCache[key] = lookupEntry(key);
		}
		return entryCache[key];
	}

	private ConfigEntry lookupEntry(E key) {
		MemberInfo info = typeof(E).GetField(Enum.GetName(typeof(E), key));
		return (ConfigEntry)Attribute.GetCustomAttribute(info, typeof(ConfigEntry));
	}

	private E getEnum(ConfigEntry e) {
		return e.enumIndex == null
			? throw new Exception("Missing index - could not lookup matching enum for " + e + " hash = " + RuntimeHelpers.GetHashCode(e))
			: (E)Enum.Parse(typeof(E), e.enumIndex);
	}
}