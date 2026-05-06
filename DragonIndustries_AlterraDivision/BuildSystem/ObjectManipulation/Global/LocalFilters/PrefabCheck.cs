/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal sealed class PrefabCheck : LocalCheck {

	private string id;

	internal override bool apply(GameObject go) {
		return go.getPrefabID() == id;
	}

	internal override void loadFromXML(XmlElement e) {
		id = e.GetProperty("id");
	}

	internal override void saveToXML(XmlElement e) {
		e.AddProperty("id", id);
	}

}