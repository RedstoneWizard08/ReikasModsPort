/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 08/04/2022
 * Time: 4:55 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ReikaKalseki.DIAlterra;

using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class DataboxTypingMap {
	public static readonly DataboxTypingMap instance = new();

	private readonly Dictionary<Vector3, Dictionary<Vector3, TechType>> data = new();

	private DataboxTypingMap() {

	}

	public void load() {
		var xml = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "XML/databoxes.xml");
		if (File.Exists(xml)) {
			SNUtil.Log("Loading databox map from XML @ " + xml);
			var doc = new XmlDocument();
			doc.Load(xml);
			foreach (XmlElement e in doc.DocumentElement.ChildNodes) {
				try {
					var pos = e.GetVector("position").Value;
					var tech = e.GetProperty("tech");
					var techt = (TechType)Enum.Parse(typeof(TechType), tech);
					addValue(pos, techt);
				}
				catch (Exception ex) {
					SNUtil.Log("Could not load element " + e.InnerText);
					SNUtil.Log(ex.ToString());
				}
			}
		}
		else {
			SNUtil.Log("Databox XML not found!");
		}
	}

	public bool isEmpty() {
		return data.Count <= 0;
	}

	public void addValue(double x, double y, double z, TechType type) {
		addValue(new Vector3((float)x, (float)y, (float)z), type);
	}

	public void addValue(Vector3 pos, TechType type) {
		var rnd = getRounded(pos);
		if (!data.ContainsKey(rnd)) {
			data[rnd] = new Dictionary<Vector3, TechType>();
		}
		data[rnd][pos] = type;
		GenUtil.getOrCreateDatabox(type);
		SNUtil.Log("Registered databox mapping " + type + " @ " + pos);
	}

	public TechType getOverride(BlueprintHandTarget bpt) {
		var pos = bpt.gameObject.transform.position;
		var rounded = getRounded(pos);
		if (data.TryGetValue(rounded, out var map)) {
			foreach (var kvp in map) {
				if (kvp.Key.DistanceSqrXZ(pos) <= 1) {
					return kvp.Value;
				}
			}
		}
		return TechType.None;
	}

	private Vector3 getRounded(Vector3 vec) {
		var x = (int)Math.Floor(vec.x);
		var y = (int)Math.Floor(vec.y);
		var z = (int)Math.Floor(vec.z);
		return new Vector3(x / 64, y / 64, z / 64);
	}

}