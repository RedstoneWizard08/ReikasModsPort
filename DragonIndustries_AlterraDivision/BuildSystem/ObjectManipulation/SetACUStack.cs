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
		isBottomOfStack = e.getBoolean("Bottom");
		isTopOfStack = e.getBoolean("Top");
		glassTop = e.getBoolean("GlassTop");
	}

	public override void saveToXML(XmlElement e) {
		e.addProperty("Bottom", isBottomOfStack);
		e.addProperty("Top", isTopOfStack);
		e.addProperty("GlassTop", glassTop);
	}

}