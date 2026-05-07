using System.Collections.Generic;
using System.Xml;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public sealed class ScatteredPiezoGenerator : WorldGenerator {
    private int numberToGen = 1;
    private Vector3 scatterRange = Vector3.zero;

    static ScatteredPiezoGenerator() {
    }

    public ScatteredPiezoGenerator(int n, Vector3 pos, Vector3 range) : base(pos) {
        numberToGen = n;
        scatterRange = range;
    }

    public override void loadFromXML(XmlElement e) {
        numberToGen = e.GetInt("number", 0, false);
        scatterRange = e.GetVector("range").Value;
    }

    public override void saveToXML(XmlElement e) {
        e.AddProperty("number", numberToGen);
        e.AddProperty("range", scatterRange);
    }

    public override bool generate(List<GameObject> generated) {
        for (var i = 0; i < numberToGen; i++) {
            var pos = MathUtil.getRandomVectorAround(position, scatterRange);
            var go = spawner(EcoceanMod.piezo.Info.ClassID);
            go.transform.position = pos;
            go.transform.rotation = Random.rotationUniform;
            generated.Add(go);
        }

        return true;
    }

    public override LargeWorldEntity.CellLevel getCellLevel() {
        return LargeWorldEntity.CellLevel.VeryFar;
    }

    public override string ToString() {
        return base.ToString() + " x" + numberToGen + " in R=[" + scatterRange + "]";
    }
}