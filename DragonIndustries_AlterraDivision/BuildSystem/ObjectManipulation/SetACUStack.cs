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

internal class SetACUStack : ManipulationBase {

	private bool isBottomOfStack;
	private bool isTopOfStack;
	private bool glassTop;

	public override void applyToObject(GameObject go) {
		var floor = go.getChildObject("BaseWaterParkFloorBottom");
		var middleBottom = go.getChildObject("BaseWaterParkFloorMiddle");
		var middleTop = go.getChildObject("BaseWaterParkCeilingMiddle");
		var ceiling = go.getChildObject("BaseWaterParkCeilingTop");
		var gt = go.getChildObject("BaseWaterParkCeilingGlass");
		floor.SetActive(isBottomOfStack);
		middleBottom.SetActive(!isBottomOfStack);
		ceiling.SetActive(isTopOfStack);
		middleTop.SetActive(!isTopOfStack);
		gt.SetActive(isTopOfStack && glassTop);
	}

	public override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		isBottomOfStack = e.GetBoolean("Bottom");
		isTopOfStack = e.GetBoolean("Top");
		glassTop = e.GetBoolean("GlassTop");
	}

	public override void saveToXML(XmlElement e) {
		e.AddProperty("Bottom", isBottomOfStack);
		e.AddProperty("Top", isTopOfStack);
		e.AddProperty("GlassTop", glassTop);
	}

}