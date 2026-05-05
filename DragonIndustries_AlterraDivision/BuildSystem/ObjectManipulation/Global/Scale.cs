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

internal class Scale : GlobalManipulation {

	private Vector3 min = Vector3.one;
	private Vector3 max = Vector3.one;

	internal override void applyToGlobalObject(GameObject go) {
		var sc = MathUtil.getRandomVectorBetween(min, max);
		var vec = go.transform.position;
		vec.x *= sc.x;
		vec.y *= sc.y;
		vec.z *= sc.z;
		go.transform.position = vec;
	}

	internal override void applyToGlobalObject(PlacedObject go) {
		applyToObject(go.obj);
		go.setPosition(go.obj.transform.position);
	}

	internal override void applyToSpecificObject(PlacedObject go) {
		applyToSpecificObject(go.obj);
		go.scale = go.obj.transform.localScale;
	}

	internal override void applyToSpecificObject(GameObject go) {
		var rot = MathUtil.getRandomVectorBetween(min, max);
		go.transform.localScale = rot;
	}

	public override void loadFromXML(XmlElement e) {
		base.loadFromXML(e);
		min = e.getVector("min").Value;
		max = e.getVector("max").Value;
	}

	public override void saveToXML(XmlElement e) {
		base.saveToXML(e);
		e.addProperty("min", min);
		e.addProperty("max", max);
	}

}