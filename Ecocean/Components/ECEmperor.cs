using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

internal class ECEmperor : MonoBehaviour {
    private void Start() {
        InvokeRepeating(nameof(applyPassivity), 0, 0.5F);
    }

    private void OnDisable() {
        CancelInvoke(nameof(applyPassivity));
    }

    private void OnDestroy() {
        OnDisable();
    }

    private void applyPassivity() {
        foreach (AggressiveWhenSeeTarget a in WorldUtil.getObjectsNearWithComponent<AggressiveWhenSeeTarget>(
                     transform.position,
                     100
                 )) {
            a.creature.Aggression.Add(-1);
            a.lastTarget.target = null;
        }
    }
}