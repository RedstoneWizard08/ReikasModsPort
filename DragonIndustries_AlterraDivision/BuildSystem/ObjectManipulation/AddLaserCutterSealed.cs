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

internal class AddLaserCutterSealed : ManipulationBase {

	private int timeToUse;
	private string text;

	public override void applyToObject(GameObject go) {
		var bk = go.GetComponentInChildren<BulkheadDoor>(true);
		if (bk != null)
			go = bk.gameObject;
		var s = go.EnsureComponent<Sealed>();
		s._sealed = true;
		if (!string.IsNullOrEmpty(text)) {

		}
		s.maxOpenedAmount = timeToUse;
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		timeToUse = e.GetInt("timeToUse", 100); //100 is the default
		text = e.GetProperty("mouseover", true);
	}

	public override void saveToXML(XmlElement e) {
		e.AddProperty("mouseover", text);
		e.AddProperty("timeToUse", timeToUse);
	}

}