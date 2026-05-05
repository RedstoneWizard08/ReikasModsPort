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

public class DeleteChildObject : ManipulationBase {

	private string path;

	public override void applyToObject(GameObject go) {
		go.removeChildObject(path);
	}

	public override void applyToObject(PlacedObject go) {
		this.applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		path = e.InnerText;
	}

	public override void saveToXML(XmlElement e) {
		e.InnerText = path;
	}

}