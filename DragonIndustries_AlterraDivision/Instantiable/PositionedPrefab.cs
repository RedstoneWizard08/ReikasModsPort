using System;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class PositionedPrefab : ObjectTemplate {

	[SerializeField]
	public string prefabName;
	[SerializeField]
	public Vector3 position;
	[SerializeField]
	public Quaternion rotation;
	[SerializeField]
	public Vector3 scale = Vector3.one;

	protected Guid? xmlID;

	public PositionedPrefab(string pfb, Vector3? pos = null, Quaternion? rot = null, Vector3? sc = null) {
		prefabName = pfb;
		position = GenUtil.getOrZero(pos);
		rotation = GenUtil.getOrIdentity(rot);
		if (sc != null && sc.HasValue)
			scale = sc.Value;
	}

	public PositionedPrefab(PrefabIdentifier pi) {
		prefabName = pi.classId;
		position = pi.transform.position;
		rotation = pi.transform.rotation;
		scale = pi.transform.localScale;
	}

	public PositionedPrefab(PositionedPrefab pfb) {
		prefabName = pfb.prefabName;
		position = pfb.position;
		rotation = pfb.rotation;
		scale = pfb.scale;
	}

	public override string getTagName() {
		return "basicprefab";
	}

	public string getXMLID() {
		return xmlID.HasValue ? xmlID.Value.ToString() : null;
	}

	public virtual void replaceObject(string pfb) {
		prefabName = pfb;
	}

	public string getPrefab() {
		return prefabName;
	}

	public virtual GameObject createWorldObject() {
		var ret = ObjectUtil.createWorldObject(prefabName);
		if (ret != null) {
			ret.transform.position = position;
			ret.transform.rotation = rotation;
			ret.transform.localScale = scale;
		}
		return ret;
	}

	public Vector3 getPosition() {
		return new Vector3(position.x, position.y, position.z);
	}

	public Quaternion getRotation() {
		return new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
	}

	public Vector3 getScale() {
		return new Vector3(scale.x, scale.y, scale.z);
	}

	public override void saveToXML(XmlElement n) {
		n.AddProperty("prefab", prefabName);
		n.AddProperty("position", position);
		var rot = n.AddProperty("rotation", rotation.eulerAngles);
		rot.AddProperty("quaternion", rotation);
		n.AddProperty("scale", scale);

		if (xmlID != null && xmlID.HasValue)
			n.AddProperty("xmlID", xmlID.Value.ToString());
	}

	public override string ToString() {
		return prefabName + " @ " + position + " / " + rotation.eulerAngles + " / " + scale;
	}

	public override void loadFromXML(XmlElement e) {
		setPrefabName(e.GetProperty("prefab"));
		position = e.GetVector("position").Value;
		var rot = e.GetVector("rotation", out var elem, true);
		//SBUtil.log("rot: "+rot);
		var quat = rotation;
		if (rot != null && rot.HasValue) {
			var specify = elem.GetQuaternion("quaternion", true);
			//SBUtil.log("quat: "+specify);
			quat = specify != null && specify.HasValue ? specify.Value : Quaternion.Euler(rot.Value.x, rot.Value.y, rot.Value.z);
		}
		rotation = quat;
		//SBUtil.log("use rot: "+rotation+" / "+rotation.eulerAngles);
		var sc = e.GetVector("scale", true);
		if (sc != null && sc.HasValue)
			scale = sc.Value;

		var xmlid = e.GetProperty("xmlID", true);
		if (!string.IsNullOrEmpty(xmlid)) {
			xmlID = new Guid(xmlid);
		}
	}

	protected virtual void setPrefabName(string name) {
		prefabName = name;
	}

	public static Quaternion readRotation(XmlElement e) {
		var rot = e.GetDirectElementsByTagName("rotation")[0];
		return rot.HasProperty("quaternion") ? rot.GetQuaternion("quaternion").Value : Quaternion.Euler((float)rot.GetFloat("x", double.NaN), (float)rot.GetFloat("y", double.NaN), (float)rot.GetFloat("z", double.NaN));
	}

	public static void saveRotation(XmlElement e, Quaternion quat) {
		e = e.AddChild("rotation");
		var vec = quat.eulerAngles;
		e.AddProperty("x", vec.x);
		e.AddProperty("y", vec.x);
		e.AddProperty("z", vec.x);
		e.AddProperty("quaternion", quat);
	}

	#region Equals and GetHashCode implementation
	public override int GetHashCode() {
		var hashCode = 0;
		unchecked {
			if (prefabName != null)
				hashCode += 1000000007 * prefabName.GetHashCode();
			hashCode += 1000000009 * position.GetHashCode();
			hashCode += 1000000021 * rotation.GetHashCode();
			hashCode += 1000000033 * scale.GetHashCode();
		}
		return hashCode;
	}

	public override bool Equals(object obj) {
		var other = obj as PositionedPrefab;
		return other != null && prefabName == other.prefabName && (position - other.position).sqrMagnitude < 0.0001 && rotation == other.rotation && (scale - other.scale).sqrMagnitude < 0.0001;
	}

	public static bool operator ==(PositionedPrefab lhs, PositionedPrefab rhs) {
		return ReferenceEquals(lhs, rhs) || (!ReferenceEquals(lhs, null) && !ReferenceEquals(rhs, null) && lhs.Equals(rhs));
	}

	public static bool operator !=(PositionedPrefab lhs, PositionedPrefab rhs) {
		return !(lhs == rhs);
	}

	#endregion
}