using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public static class BiomeRegions {

	private static readonly Dictionary<string, RegionType> BiomeList = new();

	public static readonly RegionType Shallows = new("Shallows", "SafeShallows", 1F, 1F, 1F, 0.3F);
	public static readonly RegionType Kelp = new("Kelp", "KelpForest", 0.3F, 0.6F, 0.3F, 0.67F);
	public static readonly RegionType RedGrass = new("RedGrass", "GrassyPlateaus", 1F, 1F, 1F, 0.3F);
	public static readonly RegionType Mushroom = new("Mushroom", "MushroomForest", 1F, 1F, 1F, 0.3F);
	public static readonly RegionType Jellyshroom = new("Jellyshroom", "JellyshroomCaves", 0.8F, 0.2F, 0.8F, 0.8F);
	public static readonly RegionType Koosh = new("Koosh", "KooshZone", 0.6F, 0.3F, 0.8F, 0.8F);
	public static readonly RegionType BloodKelp = new("BloodKelp", "BloodKelp", 0, 0, 0, 0.95F);
	public static readonly RegionType GrandReef = new("GrandReef", "GrandReef", 0, 0, 0.5F, 0.9F);
	public static readonly RegionType LostRiver = new("LostRiver", "lostriver_bonesfield", 0.1F, 0.5F, 0.2F, 0.92F);
	public static readonly RegionType LavaZone = new("LavaZone", "ilzchamber", 0.7F, 0.5F, 0.1F, 0.75F);
	public static readonly RegionType Other = new("Other", "Dunes", 0.1F, 0.4F, 0.7F, 0.5F);

	public static IEnumerable<RegionType> GetAllBiomes() {
		return new ReadOnlyCollection<RegionType>(BiomeList.Values.ToList());
	}

	public class RegionType {

		public readonly string ID;
		public readonly string BaseBiome;
		internal readonly Color WaterColor;

		public RegionType(string id, string b, float r, float g, float bl, float a) : this(id, b, new Color(r, g, bl, a)) {

		}

		public RegionType(string id, string b, Color c) {
			ID = id;
			BaseBiome = b;
			WaterColor = c;
			BiomeList[id] = this;
		}

		public string GetName() {
			return BiomeBase.GetBiome(BaseBiome).DisplayName;
		}

		public override string ToString() {
			return BaseBiome;
		}
	}
}