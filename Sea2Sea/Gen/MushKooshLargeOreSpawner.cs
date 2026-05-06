using System.Collections.Generic;
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class MushKooshLargeOreSpawner : WorldGenerator {

	private Quaternion rotation;
	private bool isKoosh;

	private static readonly WeightedRandom<VanillaResources> kooshTable = new();
	private static readonly WeightedRandom<VanillaResources> mushroomTable = new();

	static MushKooshLargeOreSpawner() {
		kooshTable.addEntry(VanillaResources.LARGE_MERCURY, 20);
		kooshTable.addEntry(VanillaResources.LARGE_LITHIUM, 25);
		kooshTable.addEntry(VanillaResources.LARGE_RUBY, 20);

		mushroomTable.addEntry(VanillaResources.LARGE_DIAMOND, 30);
		mushroomTable.addEntry(VanillaResources.LARGE_LITHIUM, 30);
		mushroomTable.addEntry(VanillaResources.LARGE_SALT, 20);
	}

	public MushKooshLargeOreSpawner(Vector3 pos) : base(pos) {

	}

	public override void saveToXML(XmlElement e) {
		PositionedPrefab.saveRotation(e, rotation);
		e.AddProperty("isKoosh", isKoosh);
	}

	public override void loadFromXML(XmlElement e) {
		rotation = PositionedPrefab.readRotation(e);
		isKoosh = e.GetBoolean("isKoosh");
	}

	public override bool generate(List<GameObject> li) {
		if (WorldUtil.getObjectsNearWithComponent<Drillable>(position, 6).Count == 0 && Random.Range(0F, 1F) < (isKoosh ? 0.8F : 0.6F)) {
			var res = (isKoosh ? kooshTable : mushroomTable).getRandomEntry();
			var obj = spawner(res.prefab);
			obj.transform.position = position;
			obj.transform.rotation = rotation;
			li.Add(obj);
		}
		return true;
	}

	public override LargeWorldEntity.CellLevel getCellLevel() {
		return LargeWorldEntity.CellLevel.Medium;
	}

}