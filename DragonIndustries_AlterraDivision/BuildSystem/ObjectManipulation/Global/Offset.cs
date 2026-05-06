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

internal class Offset : GlobalManipulation {

	private Vector3 translate = Vector3.zero;

	internal override void applyToGlobalObject(GameObject go) {
		go.transform.position += translate;
	}

	internal override void applyToGlobalObject(PlacedObject go) {
		go.move(translate.x, translate.y, translate.z);
	}

	internal override void applyToSpecificObject(PlacedObject go) {
		applyToObject(go);
	}

	internal override void applyToSpecificObject(GameObject go) {
		applyToObject(go);
	}

	public override void loadFromXML(XmlElement e) {
		base.loadFromXML(e);
		translate.x = (float)e.GetFloat("x", double.NaN);
		translate.y = (float)e.GetFloat("y", double.NaN);
		translate.z = (float)e.GetFloat("z", double.NaN);
	}

	public override void saveToXML(XmlElement e) {
		base.saveToXML(e);
		e.AddProperty("x", translate.x);
		e.AddProperty("y", translate.y);
		e.AddProperty("z", translate.z);
	}

}