using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class LockedPrecursorDoor : CustomPrefab {
    public readonly string id;

    public readonly PrecursorKeyTerminal.PrecursorKeyType key;
    public readonly PositionedPrefab barrierLocation;
    public readonly PositionedPrefab keyTerminalLocation;

    internal static readonly Dictionary<string, LockedPrecursorDoor> prefabs = new();

    [SetsRequiredMembers]
    public LockedPrecursorDoor(
        string id,
        PrecursorKeyTerminal.PrecursorKeyType key,
        PositionedPrefab barrier,
        PositionedPrefab terminal
    ) : base("LockedPrecursorDoor_" + id, "", "") {
        this.id = id;
        this.key = key;

        barrierLocation = barrier;
        keyTerminalLocation = terminal;

        AddOnRegister(() => { prefabs[Info.ClassID] = this; });

        SetGameObject(GetGameObject);
    }

    public virtual GameObject GetGameObject() {
        var go = new GameObject("LockedPrecursorDoor_" + id + "(Clone)");
        //SNUtil.log("Spawning LockedPrecursorDoor_"+id+" @ "+go.transform.position);
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<PrecursorGlobalKeyActivator>().doorActivationKey = id;
        go.EnsureComponent<LockedPrecursorDoorTag>();
        go.EnsureComponent<ConstructionObstacle>();
        return go;
    }

    // protected override void ProcessPrefab(GameObject go) {
    // 	base.ProcessPrefab(go);
    // }

    private class LockedPrecursorDoorTag : MonoBehaviour {
        private PrecursorKeyTerminal terminal;
        private PrecursorDoorway barrier;
        private LockedPrecursorDoor template;

        private ChangePrecursorDoor doorColor;

        private void Update() {
            if (template == null)
                template = prefabs[GetComponent<PrefabIdentifier>().ClassId];
            if (template == null)
                return;
            if (doorColor == null)
                doorColor = new ChangePrecursorDoor(template.key);
            if (!terminal)
                terminal = GetComponentInChildren<PrecursorKeyTerminal>();
            if (!barrier)
                barrier = GetComponentInChildren<PrecursorDoorway>();
            if (!terminal) {
                terminal = ObjectUtil.createWorldObject("c718547d-fe06-4247-86d0-efd1e3747af0")
                    .GetComponent<PrecursorKeyTerminal>();
                terminal.transform.SetParent(transform);
                doorColor.applyToObject(terminal);
            }

            if (!barrier) {
                barrier = ObjectUtil.createWorldObject("d26276ab-0c29-4642-bcb8-1a5f8ee42cb2")
                    .GetComponent<PrecursorDoorway>();
                barrier.transform.SetParent(transform);
            }

            if (!terminal || !barrier)
                return;
            terminal.transform.rotation = template.keyTerminalLocation.rotation;
            terminal.transform.position = template.keyTerminalLocation.position;
            doorColor.applyToObject(terminal);
            barrier.transform.rotation = template.barrierLocation.rotation;
            barrier.transform.position = template.barrierLocation.position;
            barrier.transform.localScale = template.barrierLocation.scale;
        }
    }
}