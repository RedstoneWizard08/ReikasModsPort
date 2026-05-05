using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class ExplodingGrabbable : CustomPrefab {
    public readonly TechType template;

    internal static readonly Dictionary<string, ExplodingGrabbable> templates =
        new Dictionary<string, ExplodingGrabbable>();

    [SetsRequiredMembers]
    public ExplodingGrabbable(string classID, string baseTemplate) : this(
        classID,
        CraftData.entClassTechTable[baseTemplate]
    ) {
    }

    [SetsRequiredMembers]
    public ExplodingGrabbable(string classID, TechType tt) : base(classID, "", "") {
        template = tt;
        AddOnRegister(() => { templates[Info.ClassID] = this; });
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        GameObject world = ObjectUtil.createWorldObject(template);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.EnsureComponent<ExplodeOnCollection>();
        world.removeComponent<Pickupable>();
        return world;
    }

    class ExplodeOnCollection : MonoBehaviour, IHandTarget {
        private ExplodingGrabbable template;

        void Update() {
            if (template == null) {
                string id = this.GetComponent<PrefabIdentifier>().classId;
                template = ExplodingGrabbable.templates.ContainsKey(id) ? ExplodingGrabbable.templates[id] : null;
                if (template == null)
                    SNUtil.log("No template for exploding grabbable prefab " + id + " @ " + transform.position);
            }
        }

        public void OnHandHover(GUIHand hand) {
            HandReticle.main.SetIcon(HandReticle.IconType.Hand, 1f);
            HandReticle.main.SetText(
                HandReticle.TextType.Use,
                template == null ? "None" : template.template.AsString(),
                true
            );
            HandReticle.main.SetTargetDistance(8);
        }

        public void OnHandClick(GUIHand hand) {
            this.explode();
        }

        public void explode() {
            Player.main.liveMixin.TakeDamage(10, transform.position, DamageType.Explosive, gameObject);
            //WorldUtil.spawnParticlesAt(transform.position, "", 1, true);
            //SoundManager.playSound("event:/tools/gravsphere/explode");
            GameObject sm = ObjectUtil.lookupPrefab("1c34945a-656d-4f70-bf86-8bc101a27eee");
            GameObject fx = sm.GetComponent<SeaMoth>().destructionEffect.clone();
            fx.transform.position = transform.position;
            fx.transform.localScale = Vector3.one * 0.5F;
            gameObject.destroy(false);
        }
    }
}