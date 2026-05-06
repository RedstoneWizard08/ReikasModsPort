/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class ChangePrecursorDoor : ManipulationBase {

	private PrecursorKeyTerminal.PrecursorKeyType targetType;

	static ChangePrecursorDoor() {

	}

	internal ChangePrecursorDoor() {

	}

	public ChangePrecursorDoor(PrecursorKeyTerminal.PrecursorKeyType t) {
		targetType = t;
	}

	public void applyToObject(PrecursorKeyTerminal pk) {
		pk.acceptKeyType = targetType;
		pk.keyFace.material = pk.keyMats[(int)pk.acceptKeyType];
	}

	public override void applyToObject(GameObject go) {
		var pk = go.GetComponentInChildren<PrecursorKeyTerminal>();
		if (pk == null) {
			foreach (var c in go.GetComponentsInChildren<Component>()) {
				SNUtil.Log("extra Component " + c + "/" + c.GetType() + " in " + c.gameObject);
			}
		}
		applyToObject(pk);
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		targetType = (PrecursorKeyTerminal.PrecursorKeyType)Enum.Parse(typeof(PrecursorKeyTerminal.PrecursorKeyType), e.InnerText);
	}

	public override void saveToXML(XmlElement e) {
		e.InnerText = Enum.GetName(typeof(PrecursorKeyTerminal.PrecursorKeyType), targetType);
	}

}