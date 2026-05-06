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

public class Scatter : ManipulationBase {

	private Vector3 range = Vector3.zero;

	public override void applyToObject(GameObject go) {
		var dx = Random.Range(-range.x, range.x);
		var dy = Random.Range(-range.y, range.y);
		var dz = Random.Range(-range.z, range.z);
		go.transform.position += new Vector3(dx, dy, dz);
	}

	public override void applyToObject(PlacedObject go) {
		double dx = Random.Range(-range.x, range.x);
		double dy = Random.Range(-range.y, range.y);
		double dz = Random.Range(-range.z, range.z);
		go.move(dx, dy, dz);
	}

	public override void loadFromXML(XmlElement e) {
		range = ((XmlElement)e.ParentNode).GetVector("Scatter").Value;
	}

	public override void saveToXML(XmlElement e) {
		e.AddProperty("x", range.x);
		e.AddProperty("y", range.y);
		e.AddProperty("z", range.z);
	}

	public override bool needsReapplication() {
		return false;
	}

}