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

internal sealed class RemoveGravity : ManipulationBase {

	public override void applyToObject(GameObject go) {
		go.GetComponentInChildren<Rigidbody>().isKinematic = true;
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {

	}

	public override void saveToXML(XmlElement e) {

	}

}