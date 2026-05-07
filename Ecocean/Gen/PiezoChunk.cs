using System.Collections.Generic;
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public sealed class PiezoChunk : WorldGenerator {
    static PiezoChunk() {
    }

    public PiezoChunk(Vector3 pos) : base(pos) {
    }

    public override void loadFromXML(XmlElement e) {
    }

    public override void saveToXML(XmlElement e) {
    }

    public override bool generate(List<GameObject> generated) {
        var n = Random.Range(4, 7); //4-6 horizontal
        var angs = new Vector3[n + 2];
        for (var i = 0; i < n; i++) {
            var ang = 360F / n * i;
            ang += Random.Range(-15F, 15F);
            angs[i] = new Vector3(Random.Range(0, 360F), ang, Random.Range(60F, 120F));
        }

        angs[angs.Length - 1] = new Vector3(Random.Range(-30F, 30F), Random.Range(0, 360F), 0);
        angs[angs.Length - 2] = new Vector3(Random.Range(150F, 210F), Random.Range(0, 360F), 0);
        foreach (var ang in angs) {
            var go = spawner(EcoceanMod.piezo.Info.ClassID);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(ang.x, ang.y, ang.z); //UnityEngine.Random.rotationUniform;
            go.transform.position += go.transform.up * Random.Range(0.25F, 0.5F);
            generated.Add(go);
        }

        return true;
    }

    public override LargeWorldEntity.CellLevel getCellLevel() {
        return LargeWorldEntity.CellLevel.VeryFar;
    }
}