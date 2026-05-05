using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class BKelpBumpWorm : InteractableSpawnable {
    [SetsRequiredMembers]
    internal BKelpBumpWorm(XMLLocale.LocaleEntry e) : base(e) {
        scanTime = 3;
        AddOnRegister(registerEncyPage);
    }

    public override GameObject GetGameObject() {
        var go = ObjectUtil.createWorldObject(VanillaFlora.TIGER.getPrefabID());
        go.removeComponent<SpikePlant>();
        go.removeComponent<LiveMixin>();
        go.removeComponent<RangeAttacker>();
        go.removeComponent<RangeTargeter>();
        go.removeComponent<RangedAttackLastTarget>();
        go.removeComponent<AttackLastTarget>();
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Medium;
        go.transform.localScale = new Vector3(2, 2, 2);
        foreach (var r in go.GetComponentsInChildren<Renderer>()) {
            RenderUtil.swapTextures(SeaToSeaMod.ModDLL, r, "Textures/Creature/BKelpBumpWorm");
            RenderUtil.enableAlpha(r.materials[0], 0.2F);
            //looks bad RenderUtil.makeTransparent(r.materials[0]);
            r.materials[0].SetFloat("_Shininess", 0);
            r.materials[0].SetFloat("_SpecInt", 0.2F);
            r.materials[0].SetFloat("_Fresnel", 0F);
            RenderUtil.setEmissivity(r, 0.75F);
        }

        var a = go.GetComponentInChildren<Animator>();
        a.speed = 4;
        var sc = go.GetComponentInChildren<SphereCollider>();
        sc.radius *= 1.25F;
        sc.transform.localPosition += Vector3.up * 0.4F;
        sc.gameObject.EnsureComponent<BKelpBumpWormInteractTag>();
        go.EnsureComponent<BKelpBumpWormTag>();
        ObjectUtil.makeMapRoomScannable(go, C2CItems.bkelpBumpWormItem.Info.TechType);
        return go;
    }

    public class BKelpBumpWormTag : MonoBehaviour {
        public static readonly float
            REGROW_TIME = 5400; //90 min, but do not serialize, so will reset if leave and come back

        private Animator animator;
        private SphereCollider collider;

        private float lastCollect = -9999;

        private void Start() {
            animator = GetComponentInChildren<Animator>();
            Invoke(nameof(cleanup), 1);
            collider = GetComponentInChildren<SphereCollider>();
        }

        private void Update() {
            animator.speed = 4;
            var visible = DayNightCycle.main.timePassedAsFloat - lastCollect >= REGROW_TIME;
            animator.gameObject.SetActive(visible);
            collider.gameObject.SetActive(visible);
        }

        private void cleanup() {
            cleanup(4.5F);
        }

        private void cleanup(float r) {
            var trig = Physics.queriesHitTriggers;
            Physics.queriesHitTriggers = true;
            foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(
                         transform.position,
                         r
                     )) {
                if (VanillaFlora.DEEP_MUSHROOM.includes(pi.ClassId)) {
                    pi.gameObject.destroy(false);
                }
            }

            Physics.queriesHitTriggers = trig;
        }

        public bool collect() {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (time - lastCollect < REGROW_TIME)
                return false;
            InventoryUtil.addItem(C2CItems.bkelpBumpWormItem.Info.TechType);
            lastCollect = time;
            return true;
        }
    }

    private class BKelpBumpWormInteractTag : MonoBehaviour, IHandTarget {
        private BKelpBumpWormTag owner;

        private void Start() {
            owner = gameObject.FindAncestor<BKelpBumpWormTag>();
        }

        public void OnHandHover(GUIHand hand) {
            HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
            HandReticle.main.SetText(HandReticle.TextType.Use, "BKelpBumpWormClick", true);
            HandReticle.main.SetTargetDistance(8);
        }

        public void OnHandClick(GUIHand hand) {
            owner.collect();
        }
    }
}