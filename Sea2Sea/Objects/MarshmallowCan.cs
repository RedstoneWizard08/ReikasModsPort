using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class MarshmallowCan : CustomPrefab {
    [SetsRequiredMembers]
    internal MarshmallowCan() : base("MarshmallowCan", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject go = ObjectUtil.lookupPrefab(TechType.PlanterPot).GetResult()
            .getChildObject("model/Base_interior_Planter_Pot_01").clone();
        go.removeChildObject("pot_generic_plant_01");
        go.transform.localScale = new Vector3(0.2F, 0.2F, 0.5F);
        GameObject lid = go.getChildObject("Base_exterior_Planter_Tray_ground");
        lid.transform.localPosition = new Vector3(0, 0, 0.06F);
        GameObject can = go.getChildObject("Base_interior_Planter_Pot_01 1");
        RenderUtil.swapTextures(SeaToSeaMod.modDLL, lid.GetComponentInChildren<Renderer>(), "Textures/marshmallows");
        Renderer cr = can.GetComponentInChildren<Renderer>();
        RenderUtil.swapTextures(SeaToSeaMod.modDLL, cr, "Textures/marshmallowcan");
        RenderUtil.setGlossiness(cr, 6F, 0, 1);
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        /*
        Vector3[] pos = new Vector3[]{lid.transform.localPosition};
        Vector3[] rot = new Vector3[]{Vector3.zero};
        for (int i = 0; i < pos.Length; i++) {
            GameObject par = new GameObject("Marshmallow");
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            par.transform.localScale = new Vector3(0.2F, 0.4F, 0.2F);
            cyl.name = "Marshmallow";
            par.transform.SetParent(go.transform);
            cyl.transform.SetParent(par.transform);
            par.transform.localPosition = pos[i];
            par.transform.localRotation = Quaternion.Euler(rot[i]);
            cyl.transform.localScale = Vector3.one;
            cyl.transform.localPosition = Vector3.zero;
            cyl.transform.localRotation = Quaternion.identity;
            cyl.removeComponent<Collider>();
            ECCLibrary.ECCHelpers.ApplySNShaders(cyl, new ECCLibrary.UBERMaterialProperties(0, 2, 0));
            Renderer r = cyl.GetComponentInChildren<Renderer>();
            RenderUtil.setGlossiness(r, 2, 0, 1);
        }*/
        return go;
    }
}