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

public abstract class ManipulationBase {

	public abstract void applyToObject(PlacedObject go);
	public abstract void applyToObject(GameObject go);

	public abstract void loadFromXML(XmlElement e);
	public abstract void saveToXML(XmlElement e);

	public override string ToString() {
		var doc = new XmlDocument();
		var e = doc.CreateElement(GetType().Name);
		saveToXML(e);
		return GetType() + " : " + e.Format();
	}

	public virtual bool needsReapplication() {
		return true;
	}

}