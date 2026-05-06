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

public sealed class ModifyLight : ModifyComponent<Light> {

	private double range = 1;
	private double intensity = 1;
	private Color? color = Color.white;

	public override void modifyComponent(Light c) {
		c.range = (float)range;
		c.intensity = (float)intensity;
		if (color != null && color.HasValue)
			c.color = color.Value;
	}

	public override void loadFromXML(XmlElement e) {
		range = e.GetFloat("range", double.NaN);
		intensity = e.GetFloat("intensity", double.NaN);
		color = e.GetColor("color", true, true);
	}

	public override void saveToXML(XmlElement e) {
		e.AddProperty("intensity", intensity);
		e.AddProperty("range", range);
		if (color != null && color.HasValue)
			e.AddProperty("color", color.Value);
	}

}