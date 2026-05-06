using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UWE;

namespace ReikaKalseki.DIAlterra;

public static class PrefabUtil {
    public static GameObject GetPrefabForTechType(TechType techType, bool verbose = true) {
        CraftData.PreparePrefabIDCache();

        if (!CraftData.techMapping.TryGetValue(techType, out var classId)) {
            if (verbose) {
                Debug.LogErrorFormat(
                    "Could not find prefab class id for tech type {0}. Probably missing from EntTechData.asset",
                    techType
                );
            }

            return null;
        }

        var prefab = GetPrefab(classId);

        if (prefab != null) return prefab;

        if (verbose) {
            Debug.LogErrorFormat(
                "Could not find prefab for class id {0} (tech type {1}). Probably mising from prefab database",
                classId,
                techType
            );
        }

        return null;
    }

    public static GameObject GetPrefab(string classId) {
        if (ScenePrefabDatabase.TryGetScenePrefab(classId, out var prefab)) {
            return prefab;
        }

        if (PrefabDatabase.TryGetPrefabFilename(classId, out var filename)) return GetPrefabForFilename(filename);
        Debug.LogWarningFormat(
            "No filename for prefab {0} in database containing {1} entries",
            classId,
            PrefabDatabase.prefabFiles.Count
        );
        return null;
    }

    public static GameObject GetPrefabForFilename(string filename) {
        return Addressables.LoadAssetAsync<GameObject>(filename).WaitForCompletion();
    }

    public static GameObject ChooseRandomResource(this Drillable drillable) {
        return (from resourceType in drillable.resources
            where resourceType.chance >= 1f || Player.main.gameObject.GetComponent<PlayerEntropy>()
                .CheckChance(resourceType.techType, resourceType.chance)
            select GetPrefabForTechType(resourceType.techType)).FirstOrDefault();
    }
}