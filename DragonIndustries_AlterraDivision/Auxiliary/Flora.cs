using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public interface Flora {

	string getPrefabID();
	bool isNativeToBiome(Vector3 pos);
	bool isNativeToBiome(BiomeBase b, bool cave);

}