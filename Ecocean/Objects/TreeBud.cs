using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ReikaKalseki.Ecocean;

[Obsolete]
public class TreeBud : CustomPrefab {
    private static readonly List<TechType> drops = [];

    private readonly XMLLocale.LocaleEntry locale;

    [SetsRequiredMembers]
    internal TreeBud(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
        locale = e;
        SetGameObject(GetGameObject);
    }

    static TreeBud() {
        //addDrop(TechType.TreeMushroomPiece, 250);
        addDrop(TechType.Lithium);
        addDrop(TechType.Diamond);
    }

    public static void addDrop(TechType drop) {
        drops.Add(drop);
    }

    public GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject(VanillaFlora.PINECONE.getRandomPrefab(false));
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        var r = world.GetComponentInChildren<Renderer>();
        RenderUtil.swapTextures(EcoceanMod.modDLL, r, "Textures/Plants/TreeBud");
        r.material.EnableKeyword("UWE_WAVING");
        r.material.SetFloat("_Shininess", 0F);
        r.material.SetFloat("_SpecInt", 0F);
        r.material.SetColor("_Color", Color.white);
        r.material.SetVector("_Scale", new Vector4(0.24F, 0.1F, 0.24F, 0.2F));
        r.material.SetVector("_Frequency", new Vector4(1.0F, 1.5F, 1.5F, 1.2F));
        r.material.SetVector("_Speed", new Vector4(0.1F, 0.05F, 0.0F, 0.0F));
        r.material.SetVector("_ObjectUp", new Vector4(0F, 0F, 1F, 0F));
        r.material.SetFloat("_WaveUpMin", 0F);
        RenderUtil.setEmissivity(r, 1);
        var res = world.GetComponent<BreakableResource>();
        res.breakText = "Harvest fungal bud";
        res.prefabList.Clear();
        res.numChances = 0;
        res.defaultPrefabTechType = TechType.TreeMushroomPiece;
        // res.defaultPrefab = ObjectUtil.lookupPrefab(TechType.TreeMushroomPiece);
        res.breakSound = SoundManager.getSound(TechData.GetSoundPickup(TechType.SeaTreaderPoop));
        world.EnsureComponent<TreeBudTag>();
        return world;
    }

    private class TreeBudTag : MonoBehaviour {
        private void OnBreakResource() {
            var res = GetComponent<BreakableResource>();
            foreach (var tt in drops) {
                res.SpawnResourceFromPrefab(new AssetReferenceGameObject(CraftData.GetClassIdForTechType(tt)));
            }

            var n = UnityEngine.Random.Range(2, 6); //2-5
            for (var i = 0; i < n; i++)
                dropFungalSample(res);
            SoundManager.playSoundAt(SoundManager.buildSound("event:/loot/pickup_seatreaderpoop"), transform.position);
        }

        private void dropFungalSample(BreakableResource res) {
            var go = Instantiate<GameObject>(
                ObjectUtil.lookupPrefab("01de572d-5549-44c6-97cf-645b07d1c79d"),
                transform.position + transform.up * res.verticalSpawnOffset,
                Quaternion.identity
            );
            if (!go.GetComponent<Rigidbody>()) {
                go.AddComponent<Rigidbody>();
            }

            go.GetComponent<Rigidbody>().isKinematic = false;
            go.GetComponent<Rigidbody>().AddTorque(Vector3.right * UnityEngine.Random.Range(3, 6));
            go.GetComponent<Rigidbody>().AddForce(transform.up * 0.1f);
            go.GetComponent<Pickupable>().SetTechTypeOverride(TechType.TreeMushroomPiece);
        }

        private void Update() {
            transform.localScale = new Vector3(1, 2, 1);
        }
    }
}