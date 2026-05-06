/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea;

internal class BreakThermalPlant : ManipulationBase {

	private bool disablePowergen;
	private bool deleteHead;
	private bool disableBar;
	private bool deleteBar;
	private string textOverride;
	private Color? textColor;

	public override void applyToObject(GameObject go) {
		go.removeComponent<ThermalPlant>();
		if (deleteHead)
			go.removeChildObject("model/root/head");
		var text = go.getChildObject("UI/Canvas/Text");
		var t = text.GetComponent<Text>();
		if (!string.IsNullOrEmpty(textOverride))
			t.text = textOverride;
		if (textColor != null && textColor.HasValue)
			t.color = textColor.Value;
		if (deleteBar)
			go.removeChildObject("UI/Canvas/temperatureBar");
		if (disableBar)
			go.removeChildObject("UI/Canvas/temperatureBar/temperatureBarForeground");
		if (disablePowergen) {
			go.removeComponent<PowerSource>();
			go.removeComponent<PowerFX>();
			go.removeComponent<PowerRelay>();
			go.removeComponent<PowerSystemPreview>();
		}
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		disablePowergen = e.GetBoolean("RemovePower");
		deleteHead = e.GetBoolean("DeleteHead");
		disableBar = e.GetBoolean("DisableBar");
		deleteBar = e.GetBoolean("DeleteBar");
		textOverride = e.GetProperty("SetText", true);
		textColor = e.GetColor("TextColor", false, true);
	}

	public override void saveToXML(XmlElement e) {
		e.AddProperty("RemovePower", disablePowergen);
		e.AddProperty("DeleteHead", deleteHead);
		e.AddProperty("DisableBar", disableBar);
		e.AddProperty("DeleteBar", deleteBar);
		if (!string.IsNullOrEmpty(textOverride))
			e.AddProperty("SetText", textOverride);
		if (textColor != null && textColor.HasValue)
			e.AddProperty("TextColor", textColor.Value);
	}

}