using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public abstract class WorldGenerator : ObjectTemplate {

	public static readonly string TAGNAME = "generator";

	public readonly Vector3 position;

	public Func<string, GameObject> spawner = s => ObjectUtil.createWorldObject(s, true, true);

	private string savedID;

	public string uniqueID {
		get {
			if (string.IsNullOrEmpty(savedID)) {
				var doc = new XmlDocument();
				var e = doc.CreateElement("id");
				saveToXML(e);
				savedID = e.InnerXml;
			}
			return savedID;
		}
	}

	static WorldGenerator() {
		registerType(TAGNAME, e => {
			var typeName = e.GetProperty("type");
			var pos = e.GetVector("position").Value;
			var scatt = e.GetVector("scatter", true);
			if (scatt != null && scatt.HasValue)
				pos += MathUtil.getRandomVectorBetween(-scatt.Value, scatt.Value);
			var tt = InstructionHandlers.getTypeBySimpleName(typeName);
			if (tt == null)
				throw new Exception("No class found for '" + typeName + "'!");
			var gen = (WorldGenerator)Activator.CreateInstance(tt, new object[]{pos});
			return gen;
		});
	}

	protected WorldGenerator(Vector3 pos) {
		position = pos;
	}

	/// <returns>True if the generator completed and the holder should be destroyed</returns>
	public abstract bool generate(List<GameObject> generated);

	public abstract LargeWorldEntity.CellLevel getCellLevel();

	public override sealed string getTagName() {
		return TAGNAME;
	}

	protected bool isColliding(Vector3 vec, List<GameObject> li) {
		foreach (var go in li) {
			if (ObjectUtil.objectCollidesPosition(go, vec))
				return true;
		}
		return false;
	}

	public override string ToString() {
		return GetType().Name + " @ " + position;
	}
}