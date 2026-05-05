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
		XmlDocument doc = new XmlDocument();
		XmlElement e = doc.CreateElement(this.GetType().Name);
		this.saveToXML(e);
		return this.GetType() + " : " + e.format();
	}

	public virtual bool needsReapplication() {
		return true;
	}

}