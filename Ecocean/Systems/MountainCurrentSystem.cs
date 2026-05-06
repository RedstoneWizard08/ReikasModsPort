using System;
using System.IO;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class MountainCurrentSystem {
    public static readonly MountainCurrentSystem instance = new();

    private MountainCurrentSystem() {
    }

    public void registerFlowVector(int amt) {
        var t = MainCamera.camera.transform;
        if (Player.main.GetVehicle())
            t = Player.main.GetVehicle().transform;
        Vector3 vec = t.forward.SetLength(amt);
        var flat = vec.XZ().normalized * amt;
        var posf = t.transform.position.XZ();
        var s = $"{vec.x},{vec.y},{vec.z},{t.position.x},{t.position.y},{t.position.z}" + Environment.NewLine;
        var s2 = $"{flat.x},{flat.y},{posf.x},{posf.y}" + Environment.NewLine;
        File.AppendAllText(
            Path.Combine(Path.GetDirectoryName(EcoceanMod.modDLL.Location), "mountain-flow-vectors.csv"),
            s
        );
        File.AppendAllText(
            Path.Combine(Path.GetDirectoryName(EcoceanMod.modDLL.Location), "mountain-flow-vectors-2D.csv"),
            s2
        );
    }

    public float getCurrentExposure(Vector3 position, Vector3 currentVec) {
        if (WorldUtil.isInCave(position) || WorldUtil.isInWreck(position))
            return 0;
        float d = 15;
        var hits = Physics.RaycastAll(position, -currentVec.normalized, d);
        float minDist = 9999;
        foreach (var hit in hits) {
            if (hit.transform) {
                if (hit.distance < minDist)
                    minDist = hit.distance;
            }
        }

        float cutoff = 4;
        return minDist <= cutoff ? 0 : Mathf.Clamp01((minDist - cutoff) / (d - cutoff));
    }

    public Vector3 getCurrentVector(Vector3 position) {
        return Vector3.zero;
    }

    public Vector3 getNetCurrent(Vector3 position) {
        var current = getCurrentVector(position);
        return current * getCurrentExposure(position, current);
    }
}