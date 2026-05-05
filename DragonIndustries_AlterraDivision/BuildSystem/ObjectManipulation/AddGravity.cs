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

internal sealed class AddGravity : ManipulationBase {

	public override void applyToObject(GameObject go) {
		ObjectUtil.applyGravity(go);
	}

	public override void applyToObject(PlacedObject go) {
		this.applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {

	}

	public override void saveToXML(XmlElement e) {

	}

}