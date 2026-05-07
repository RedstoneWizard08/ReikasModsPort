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

[Obsolete]
internal sealed class AddComponent : ManipulationBase {

	private Type type;

	public override void applyToObject(GameObject go) {
		go.EnsureComponent(type);
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		type = InstructionHandlers.GetTypeBySimpleName(e.InnerText);
	}

	public override void saveToXML(XmlElement e) {
		e.InnerText = type.Name;
	}

}