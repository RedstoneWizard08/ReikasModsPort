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
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class RandomPlant : PieceBase {

	protected readonly WeightedRandom<VanillaFlora> plants = new();

	public bool preferLit;
	public int count = 1;
	public Vector3 fuzz = Vector3.zero;

	public Func<Vector3, string, bool> validPlantPosCheck = null;

	public RandomPlant(Vector3 vec) : base(vec) {

	}

	public override bool generate(List<GameObject> li) {
		//SBUtil.log("Attempting "+count+" plants in "+fuzz+" of "+position+".");
		for (var i = 0; i < count; i++) {
			var vec = MathUtil.getRandomVectorAround(position, fuzz);
			var vf = selectPlant(plants.getRandomEntry());
			//SBUtil.log("Attempted plant "+vf.getName()+" @ "+vec);
			if (validPlantPosCheck != null && !validPlantPosCheck(vec + Vector3.up * 0.2F, vf.getName())) {
				//SBUtil.log("Intersect caused fail");
				continue;
			}
			var type = vf.getRandomPrefab(preferLit);
			var go = generatePlant(vec, type);
			//SBUtil.log("success "+go+" = "+vf.getName()+" @ "+vec);
			li.Add(go);
		}
		return true;
	}

	public override LargeWorldEntity.CellLevel getCellLevel() {
		return LargeWorldEntity.CellLevel.Medium;
	}

	protected virtual VanillaFlora selectPlant(VanillaFlora choice) {
		return choice;
	}

	protected virtual GameObject generatePlant(Vector3 vec, string type) {
		var go = spawner(type);
		go.transform.position = vec;
		go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360F), Vector3.up);
		return go;
	}

	public override void loadFromXML(XmlElement e) {
		foreach (var e2 in e.GetDirectElementsByTagName("plant")) {
			var name = e2.GetProperty("name");
			var wt = e2.GetProperty("weight");
			plants.addEntry(VanillaFlora.getByName(name), double.Parse(wt));
		}
		preferLit = e.GetBoolean("lit");
		count = e.GetInt("count", 1);
		var f = e.GetVector("fuzz", true);
		if (f != null && f.HasValue)
			fuzz = f.Value;
	}

	public override void saveToXML(XmlElement e) {
		foreach (var f in plants.getValues()) {
			var e2 = e.OwnerDocument.CreateElement("plant");
			e2.AddProperty("name", f.getName());
			e2.AddProperty("weight", plants.getWeight(f));
			e.AppendChild(e2);
		}
		e.AddProperty("lit", preferLit);
		e.AddProperty("count", count);
		e.AddProperty("fuzz", fuzz);
	}

}