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

internal sealed class SetInfected : ManipulationBase {

	private bool infect;

	public override void applyToObject(GameObject go) {
		var inf = go.EnsureComponent<InfectedMixin>();
		inf.enabled = true;
		inf.infectedAmount = infect ? 0.8F : 0;
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		infect = bool.Parse(e.InnerText);
	}

	public override void saveToXML(XmlElement e) {
		e.InnerText = infect.ToString();
	}

	public override bool needsReapplication() {
		return false;
	}

}