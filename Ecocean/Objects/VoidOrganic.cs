using System.Diagnostics.CodeAnalysis;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

public class VoidOrganic : WorldCollectedItem {
    [SetsRequiredMembers]
    internal VoidOrganic(XMLLocale.LocaleEntry e) : base(e, "505e7eff-46b3-4ad2-84e1-0fadb7be306c") {
        AddOnRegister(() => { CraftDataHandler.SetPickupSound(Info.TechType, "event:/loot/pickup_seatreaderpoop"); });
        renderModify = r => {
            GameObject world = r.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
            world.removeComponent<EnzymeBall>();
            var pp = world.EnsureComponent<Pickupable>();
            pp.SetTechTypeOverride(Info.TechType);
            world.EnsureComponent<VoidOrganicTag>();
            var c = new Color(0.75F, 1F, 0.3F);
            r.materials[0].SetColor("_Color", c);
            r.materials[0].SetColor("_SpecColor", c);
            r.materials[0].SetFloat("_Fresnel", 1F);
            r.materials[0].SetFloat("_Shininess", 5F);
            r.materials[0].SetFloat("_SpecInt", 1.5F);
            r.materials[0].SetFloat("_EmissionLM", 200F);
            r.materials[0].SetFloat("_EmissionLMNight", 200F);
            r.materials[0].SetFloat("_MyCullVariable", 1.6F);
            //Light l = world.addLight(1, 5, c);
            //Light l2 = world.addLight(0.5F, 25, c);
            world.GetComponent<Collider>().isTrigger = false;
        };
    }
}

internal class VoidOrganicTag : HeatColumnObject {
    private void Start() {
    }

    private new void Update() {
        base.Update();
        if (body && !gameObject.FindAncestor<StorageContainer>()) {
            body.isKinematic = false;
            body.velocity = Vector3.up * 4;
        }
    }
}