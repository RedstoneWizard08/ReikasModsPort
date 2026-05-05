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

class MountainBaseCuredPeeperTag : MonoBehaviour {
    private Rigidbody body;

    void Update() {
        if (!body) {
            body = this.GetComponent<Rigidbody>();
        }

        body.isKinematic = true;
    }
}