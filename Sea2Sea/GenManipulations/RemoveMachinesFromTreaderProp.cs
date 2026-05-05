/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

internal class RemoveMachinesFromTreaderProp : ManipulationBase {

	private static List<string> objects = [];

	static RemoveMachinesFromTreaderProp() {
		objects.Add("BaseCell/Coral");
		objects.Add("BaseCell/Decals");
		//objects.Add("Fabricator");
		objects.Add("Workbench");
		//objects.Add("Bench");
		objects.Add("Locker(Clone)/model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_L");
		objects.Add("Locker(Clone)/model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R");
	}

	public override void applyToObject(GameObject go) {
		foreach (var s in objects) {
			go.removeChildObject(s);
		}
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {

	}

	public override void saveToXML(XmlElement e) {

	}

}