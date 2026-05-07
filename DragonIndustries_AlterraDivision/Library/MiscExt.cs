using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UWE;

namespace ReikaKalseki.DIAlterra;

public static class MiscExt {
    public static void SetInteractText(this HandReticle hand, string msg, bool translate = true) {
        hand.SetText(HandReticle.TextType.Use, msg, translate);
    }

    public static void SpawnResourceFromPrefab(this BreakableResource res, GameObject breakPrefab) {
        CoroutineHost.StartCoroutine(
            SpawnResourceFromPrefab(
                breakPrefab,
                res.transform.position + res.transform.up * res.verticalSpawnOffset,
                res.transform.up
            )
        );
    }

    private static IEnumerator SpawnResourceFromPrefab(
        GameObject breakPrefab,
        Vector3 position,
        Vector3 up
    ) {
        var result2 = Object.Instantiate(
            breakPrefab,
            new Transform {
                position = position,
            }
        );

        if (result2 == null) {
            Debug.LogErrorFormat($"Failed to spawn {breakPrefab.name}");
            yield break;
        }

        var rigidbody = result2.EnsureComponent<Rigidbody>();
        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, isKinematic: false);
        rigidbody.AddTorque(Vector3.right * Random.Range(3, 6));
        rigidbody.AddForce(up * 0.1f);
    }
}