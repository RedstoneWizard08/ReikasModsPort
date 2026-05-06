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

internal class ParentTo : ManipulationBase {

	private SeekType type;
	private string seekID;

	public ParentTo() {

	}

	public override void applyToObject(GameObject go) {
		var find = findObject(go);
		if (find != null) {
			go.transform.parent = find.transform;
		}
	}

	public sealed override void applyToObject(PlacedObject go) {
		applyToObject(go.obj);
	}

	public override void loadFromXML(XmlElement e) {
		type = (SeekType)Enum.Parse(typeof(SeekType), e.GetProperty("type"));
		seekID = e.GetProperty("key");
	}

	public override void saveToXML(XmlElement e) {
		e.AddProperty("type", Enum.GetName(typeof(SeekType), type));
		e.AddProperty("key", seekID);
	}

	private GameObject findObject(GameObject from) {
		switch (type) {
			case SeekType.FindNearTechType:
				return findNear(from, go => go.GetComponent<TechTag>().type == SNUtil.GetTechType(seekID));
			case SeekType.FindNearClassID:
				return findNear(from, go => go.GetComponent<PrefabIdentifier>().classId == seekID);
			default:
				return null;
		}
	}

	private GameObject findNear(GameObject from, Func<GameObject, bool> f) {
		var hit = Physics.SphereCastAll(from.transform.position, 4, new Vector3(1, 1, 1), 4);
		if (hit == null || hit.Length == 0)
			return null;
		foreach (var rh in hit) {
			if (rh.transform != null && rh.transform.gameObject != null && f(rh.transform.gameObject))
				return rh.transform.gameObject;
		}
		return null;
	}

}

internal enum SeekType {
	FindNearTechType,
	FindNearClassID,
}