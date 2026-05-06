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

public class RandomRotation : ManipulationBase {

	private bool randomX;
	private bool randomY;
	private bool randomZ;

	public override void applyToObject(GameObject go) {
		if (randomX && randomY && randomZ) {
			go.transform.rotation = Random.rotationUniform;
		}
		else {
			var angs = go.transform.rotation.eulerAngles;
			if (randomX)
				angs.x = Random.Range(0F, 360F);
			if (randomY)
				angs.y = Random.Range(0F, 360F);
			if (randomZ)
				angs.z = Random.Range(0F, 360F);
			go.transform.rotation = Quaternion.Euler(angs);
		}
	}

	public override void applyToObject(PlacedObject go) {
		if (randomX && randomY && randomZ) {
			go.setRotation(Random.rotationUniform);
		}
		else {
			var angs = go.rotation.eulerAngles;
			if (randomX)
				angs.x = Random.Range(0F, 360F);
			if (randomY)
				angs.y = Random.Range(0F, 360F);
			if (randomZ)
				angs.z = Random.Range(0F, 360F);
			go.setRotation(Quaternion.Euler(angs));
		}
	}

	public override void loadFromXML(XmlElement e) {
		randomX = e.GetBoolean("x");
		randomY = e.GetBoolean("y");
		randomZ = e.GetBoolean("z");
	}

	public override void saveToXML(XmlElement e) {
		e.AddProperty("x", randomX);
		e.AddProperty("y", randomY);
		e.AddProperty("z", randomZ);
	}

	public override bool needsReapplication() {
		return false;
	}

}