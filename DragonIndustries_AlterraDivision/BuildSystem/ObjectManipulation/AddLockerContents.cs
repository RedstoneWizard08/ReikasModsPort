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
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class AddLockerContents : ManipulationBase {

	private readonly List<Item> items = [];

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void applyToObject(GameObject go) {
		//SBUtil.log("adding items to "+go.transform.position+" from trace "+System.Environment.StackTrace);
		var con = go.GetComponentInChildren<StorageContainer>();
		con.ResetContainer();
		foreach (var s in items) {
			//SBUtil.writeToChat("Added "+s);
			var amt = UnityEngine.Random.Range(s.amountMin, 1+s.amountMax);
			for (var i = 0; i < amt; i++) {
				var item = ObjectUtil.createWorldObject(s.prefab);
				item.SetActive(false);
				item.refillItem();
				con.container.AddItem(item.GetComponent<Pickupable>());
				//item.destroy(false);
			}
		}
	}

	public override void loadFromXML(XmlElement e) {
		items.Clear();
		foreach (XmlElement e2 in e.ChildNodes) {
			Item i = null;
			var type = e2.getProperty("type");
			var n = e2.getProperty("name");
			switch (type) {
				case "prefab":
					i = new Item(n);
					break;
				case "tech":
					i = new Item(SNUtil.getTechType(n));
					break;
				case "resource":
					i = new Item(VanillaResources.getByName(n.ToUpperInvariant()).prefab);
					break;
			}
			if (i == null)
				throw new Exception("Invalid item ref type '" + type + "'");
			if (e2.hasProperty("min") && e2.hasProperty("max")) {
				i.amountMin = e2.getInt("min", 1);
				i.amountMax = e2.getInt("max", 1);
			}
			else if (e2.hasProperty("amount")) {
				var amt = e2.getInt("amount", 1);
				i.amountMin = amt;
				i.amountMax = amt;
			}
			items.Add(i);
		}
	}

	public override void saveToXML(XmlElement e) {
		foreach (var s in items) {
			var e2 = e.OwnerDocument.CreateElement("item");
			e2.addProperty("type", "prefab");
			e2.addProperty("name", s.prefab);
			e2.addProperty("min", s.amountMin);
			e2.addProperty("max", s.amountMax);
			e.AppendChild(e2);
		}
	}

	public override bool needsReapplication() {
		return false;
	}

	private class Item {

		internal readonly string prefab;
		internal int amountMin = 1;
		internal int amountMax = 1;

		internal Item(TechType tech) : this(CraftData.GetClassIdForTechType(tech)) {

		}

		internal Item(string pfb) {
			prefab = pfb;
		}

		public override string ToString() {
			return prefab + " x[" + amountMin + "-" + amountMax + "]";
		}


	}

}