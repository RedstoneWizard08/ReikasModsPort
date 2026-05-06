/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

[Serializable]
public sealed class PlacedObject : DICustomPrefab {

	public static readonly new string TAGNAME = "object";

	public static readonly string BUBBLE_PREFAB = "fca5cdd9-1d00-4430-8836-a747627cdb2f";

	private static GameObject bubblePrefab;

	private static readonly Dictionary<string, PlacedObject> ids = new();

	[SerializeField]
	internal int referenceID;
	[SerializeField]
	public GameObject obj;
	[SerializeField]
	internal GameObject fx;

	[SerializeField]
	internal bool isSelected;

	internal PlacedObject parent;

	static PlacedObject() {
		registerType(TAGNAME, e => fromXML(e, false));
	}

	public override string getTagName() {
		return TAGNAME;
	}

	internal PlacedObject(GameObject go, string pfb) : base(pfb) {
		if (go == null)
			throw new Exception("Tried to make a place of a null obj!");
		if (go.transform == null)
			SNUtil.Log("Place of obj " + go + " has null transform?!", SNUtil.DiDLL);
		position = go.transform.position;
		rotation = go.transform.rotation;
		scale = go.transform.localScale;
		tech = CraftData.GetTechType(go);
		isBasePiece = pfb.StartsWith("Base_", StringComparison.InvariantCultureIgnoreCase);
		if (isBasePiece)
			prefabName = prefabName.Substring(5);
		key(go);
		createFX();
	}

	private void createFX() {
		try {
			if (fx != null) {
				fx.destroy(false);
			}
			if (bubblePrefab == null) {
				bubblePrefab = ObjectUtil.lookupPrefab(BUBBLE_PREFAB);
			}
			if (bubblePrefab) {
				fx = Utils.SpawnFromPrefab(bubblePrefab, obj.transform);
				if (fx != null) {
					if (fx.transform != null)
						fx.transform.position = obj.transform.position;
					fx.SetActive(false);
				}
				else {
					SNUtil.WriteToChat("Bubbles not constructable!");
				}
			}
			else {
				SNUtil.WriteToChat("Bubbles not found.");
			}
		}
		catch (Exception e) {
			throw new Exception("Error in bubbles", e);
		}
	}

	public void sync() {
		position = obj.transform.position;
		rotation = obj.transform.rotation;
		scale = obj.transform.localScale;
	}

	public sealed override void replaceObject(string pfb) {
		base.replaceObject(pfb);

		var put = ObjectUtil.createWorldObject(pfb);
		if (put != null && put.transform != null) {
			obj.destroy(false);
			key(put);
			put.transform.position = position;
			put.transform.rotation = rotation;
			put.transform.localScale = scale;
			createFX();
		}
	}

	private void key(GameObject go) {
		obj = go;
		referenceID = BuildingHandler.genID(go);
		xmlID = Guid.NewGuid();
		ids[xmlID.ToString()] = this;
		tech = CraftData.GetTechType(go);
	}

	internal void destroy() {
		if (xmlID != null && xmlID.HasValue)
			ids.Remove(xmlID.Value.ToString());
	}

	public void setSelected(bool sel) {
		isSelected = sel;
		if (fx == null) {
			SNUtil.WriteToChat("Could not set enabled visual of " + this + " due to null FX GO");
			return;
		}
		try {
			fx.SetActive(isSelected);
		}
		catch (Exception ex) {
			SNUtil.WriteToChat("Could not set enabled visual of " + this + " due to FX (" + fx + ") GO error");
			SNUtil.Log("Could not set enabled visual of " + this + " due to FX (" + fx + ") GO error: " + ex.ToString(), SNUtil.DiDLL);
		}
	}

	public void setPosition(Vector3 pos) {
		position = pos;
		obj.transform.position = position;
		if (fx != null && fx.transform != null)
			fx.transform.position = position;
	}

	public void move(Vector3 mov) {
		move(mov.x, mov.y, mov.z);
	}

	public void move(double x, double y, double z) {
		var vec = obj.transform.position;
		vec.x += (float)x;
		vec.y += (float)y;
		vec.z += (float)z;
		setPosition(vec);
		//SNUtil.writeToChat(go.obj.transform.position.ToString());
	}

	public void rotateYaw(double ang, Vector3? relTo) {
		rotate(0, ang, 0, relTo);
	}

	public void rotate(double roll, double yaw, double pitch, Vector3? relTo) {
		var ctr = position;
		var up = obj.transform.up;
		var forward = obj.transform.forward;
		var right = obj.transform.right;
		if (relTo != null && relTo.HasValue) {
			ctr = relTo.Value;
			up = Vector3.up;
			forward = Vector3.forward;
			right = Vector3.right;
			if (Math.Abs(yaw) > 0.001)
				obj.transform.RotateAround(ctr, up, (float)yaw);
			if (Math.Abs(roll) > 0.001)
				obj.transform.RotateAround(ctr, forward, (float)roll);
			if (Math.Abs(pitch) > 0.001)
				obj.transform.RotateAround(ctr, right, (float)pitch);
			setRotation(obj.transform.rotation);
		}
		else {
			var euler = obj.transform.rotation.eulerAngles;
			setRotation(Quaternion.Euler(euler.x + (float)roll, euler.y + (float)yaw, euler.z + (float)pitch));
			//SNUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
		}
	}

	public void setRotation(Quaternion rot) {
		obj.transform.rotation = rot;
		if (fx != null && fx.transform != null)
			fx.transform.rotation = rot;
		//SNUtil.writeToChat(go.obj.transform.rotation.eulerAngles.ToString());
		rotation = rot;
	}

	public override string ToString() {
		try {
			var t = obj.transform;
			var pos = t == null ? "null-transform @ "+position+" / "+rotation+" / "+scale : t.position+" / "+t.rotation.eulerAngles+" / "+t.localScale;
			return prefabName + " [" + tech + "] @ " + pos + " (" + referenceID + ")" + " " + (isSelected ? "*" : "");
		}
		catch (Exception ex) {
			return "Errored " + prefabName + " @ " + position + ": " + ex.ToString();
		}
	}

	private void nestObject(GameObject go, XmlElement e) {
		var p = createNewObject(go);
		if (p != null) {
			var e2 = e.OwnerDocument.CreateElement("child");
			p.saveToXML(e2);
			e.AppendChild(e2);
			foreach (Transform t in go.transform) {
				nestObject(t.gameObject, e2);
			}
		}
	}

	public override void saveToXML(XmlElement e) {
		base.saveToXML(e);

		SNUtil.Log("Serializing " + obj + " to xml as '" + prefabName + "':", SNUtil.DiDLL);

		if (parent != null && parent.xmlID != null && parent.xmlID.HasValue) {
			e.AddProperty("parent", parent.xmlID.ToString());
		}
		if (isSeabase) {
			foreach (Transform t in obj.transform) {
				var go2 = t.gameObject;
				var p2 = createNewObject(go2);
				if (p2 == null) {
					SNUtil.Log("Could not find an identifier for " + t, SNUtil.DiDLL);
				}
				else {
					var cell = e.OwnerDocument.CreateElement("part");
					p2.saveToXML(cell);
					var bc = go2.GetComponent<BaseCell>();
					var sc = go2.GetComponent<StorageContainer>();
					var cg = go2.GetComponent<Charger>();
					if (bc != null) {
						var e2 = e.OwnerDocument.CreateElement("cellData");
						foreach (Transform t2 in t) {
							var p3 = createNewObject(t2.gameObject);
							if (p3 == null) {
								SNUtil.Log("Could not find an identifier for " + t2, SNUtil.DiDLL);
							}
							else {
								var e3 = e.OwnerDocument.CreateElement("component");
								p3.saveToXML(e3);
								e2.AppendChild(e3);
								foreach (Transform t3 in t2) {
									nestObject(t3.gameObject, e3);
								}
							}
						}
						cell.AppendChild(e2);
					}
					else if (sc != null) {
						var e2 = e.OwnerDocument.CreateElement("inventory");
						foreach (var tt in sc.container.GetItemTypes()) {
							var e3 = e.OwnerDocument.CreateElement("item");
							e3.AddProperty("type", "" + tt);
							e3.AddProperty("amount", sc.container.GetItems(tt).Count);
							e2.AppendChild(e3);
						}
						cell.AppendChild(e2);
					}
					else if (cg != null) {
						var e2 = e.OwnerDocument.CreateElement("inventory");
						foreach (var kvp in cg.equipment.equipment) {
							if (kvp.Value == null || kvp.Value.item == null)
								continue;
							var e3 = e.OwnerDocument.CreateElement("item");
							e3.AddProperty("type", "" + kvp.Value.item.GetTechType());
							e3.AddProperty("slot", kvp.Key);
							e2.AppendChild(e3);
						}
						cell.AppendChild(e2);
					}
					e.AppendChild(cell);
				}
			}
		}
		else if (isBasePiece) {
			var bf = obj.GetComponent<BaseFoundationPiece>();
			if (bf != null) {
				var e2 = e.OwnerDocument.CreateElement("supportData");
				e2.AddProperty("maxHeight", bf.maxPillarHeight);
				e2.AddProperty("extra", bf.extraHeight);
				e2.AddProperty("minHeight", bf.minHeight);
				if (bf.pillars != null) {
					foreach (var p in bf.pillars) {
						var l = p.adjustable;
						if (l) {
							var e3 = e.OwnerDocument.CreateElement("pillar");
							e3.AddProperty("position", l.position);
							e3.AddProperty("rotation", l.rotation);
							e3.AddProperty("scale", l.localScale);
							e2.AppendChild(e3);
						}
					}
				}
				e.AppendChild(e2);
			}
		}
		SNUtil.Log("Finished XML serialization of " + prefabName, SNUtil.DiDLL);
	}

	public override void loadFromXML(XmlElement e) {
		if (xmlID != null && xmlID.HasValue)
			ids.Remove(xmlID.Value.ToString());
		base.loadFromXML(e);
		if (xmlID != null && xmlID.HasValue)
			ids[xmlID.Value.ToString()] = this;

		if (isDatabox) {
			//SNUtil.writeToChat("Reprogramming databox");
			//SNUtil.setDatabox(obj.EnsureComponent<BlueprintHandTarget>(), tech);
		}
		else if (isCrate) {
			//SNUtil.writeToChat("Reprogramming crate");
			//SNUtil.setCrateItem(obj.EnsureComponent<SupplyCrate>(), tech);
		}
		else if (isFragment) {
			//TechFragment frag = b.obj.EnsureComponent<TechFragment>();
		}
		else if (isPDA) {
			//SNUtil.setPDAPage(obj.EnsureComponent<StoryHandTarget>(), page);
		}
		else if (isWreck) {
			//obj.EnsureComponent<WreckDataLoader>();
		}
		else if (isBasePiece) {
			//SNUtil
		}

		setPosition(position);
		setRotation(rotation);
		obj.transform.localScale = scale;
		if (fx != null && fx.transform != null)
			fx.transform.localScale = scale;

		var pp = e.GetProperty("parent", true);
		if (!string.IsNullOrEmpty(pp) && ids.ContainsKey(pp)) {
			parent = ids[pp];
			if (parent != null)
				obj.transform.parent = parent.obj.transform;
		}

		foreach (var mb in manipulations) {
			mb.applyToObject(this);
		}
	}

	protected override void setPrefabName(string name) {
		var old = prefabName;
		base.setPrefabName(name);
		if (old != name)
			replaceObject(name);
	}

	public static PlacedObject fromXML(XmlElement e, bool readXML = true) {
		var pfb = new DICustomPrefab("");
		pfb.loadFromXML(e);
		SNUtil.Log("Building placed object from custom prefab " + pfb + " > " + e.Format(), SNUtil.DiDLL);
		var b = createNewObject(pfb);
		if (readXML)
			b.loadFromXML(e);
		return b;
	}

	internal static PlacedObject createNewObject(string id) {
		return createNewObject(id, id.StartsWith("base_", StringComparison.InvariantCultureIgnoreCase));
	}

	internal static PlacedObject createNewObject(DICustomPrefab pfb) {
		return createNewObject(pfb.prefabName, pfb.isBasePiece);
	}

	internal static PlacedObject createNewObject(GameObject go) {
		string id = null;
		//SNUtil.log("Attempting builderObject from '"+go.name+"'");
		var pi = go.GetComponent<PrefabIdentifier>();
		if (pi != null)
			id = pi.classId;
		if (id == BUBBLE_PREFAB)
			return null;
		if (pi == null && go.name.StartsWith("Base", StringComparison.InvariantCulture)) {
			var name = go.name.Replace("(Clone)", "").Substring(4);
			var get = Base.Piece.Invalid;
			if (name.Contains("WaterPark") && !name.Contains("RoomWaterPark"))
				name = name.Replace("WaterPark", "RoomWaterPark");
			else if (name.Contains("CorridorLadder"))
				name = name.Replace("Corridor", "CorridorIShape");
			if (Enum.TryParse(name, out get)) {
				if (get != Base.Piece.Invalid) {
					id = "Base_" + name;
				}
			}
			if (id == null) {
				var tt = go.GetComponent<TechTag>();
				if (tt != null && tt.type == TechType.BaseFoundation) {
					id = "Base_Foundation";
				}
			}
		}
		return string.IsNullOrEmpty(id) ? null : createNewObject(go, id);
	}

	private static PlacedObject createNewObject(string id, bool basePiece) {
		if (id == null) {
			SNUtil.WriteToChat("Prefab not placed; ID was null");
			return null;
		}
		var go = basePiece ? ObjectUtil.getBasePiece(id) : ObjectUtil.createWorldObject(id);
		return go == null ? null : createNewObject(go);
	}

	private static PlacedObject createNewObject(GameObject go, string id) {
		var sel = go.AddComponent<BuilderPlaced>();
		var ret = new PlacedObject(go, id);
		sel.placement = ret;
		//SNUtil.dumpObjectData(ret.obj);
		return ret;
	}

}