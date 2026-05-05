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

internal abstract class LocalCheck {

	internal abstract bool apply(GameObject go);
	internal abstract void loadFromXML(XmlElement e);
	internal abstract void saveToXML(XmlElement e);

}