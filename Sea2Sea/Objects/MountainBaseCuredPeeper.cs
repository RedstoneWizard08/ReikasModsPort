using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class MountainBaseCuredPeeper : PickedUpAsOtherItem {
    [SetsRequiredMembers]
    internal MountainBaseCuredPeeper() : base("MountainBaseCuredPeeper", TechType.CuredPeeper) {
    }

    protected override void prepareGameObject(GameObject go) {
        go.GetComponent<Rigidbody>().isKinematic = true;
        go.EnsureComponent<MountainBaseCuredPeeperTag>();
        go.removeComponent<EcoTarget>();
    }
}

internal class MountainBaseCuredPeeperTag : MonoBehaviour {
    private Rigidbody body;

    private void Update() {
        if (!body) {
            body = GetComponent<Rigidbody>();
        }

        body.isKinematic = true;
    }
}