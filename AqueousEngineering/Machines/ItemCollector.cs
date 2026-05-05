using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class ItemCollector : BasicCraftingItem {
    [SetsRequiredMembers]
    public ItemCollector(XMLLocale.LocaleEntry e) : base(e, "WorldEntities/Tools/Gravsphere") {
        sprite = TextureManager.getSprite(AqueousEngineeringMod.modDLL, "Textures/Items/ItemCollector");
        unlockRequirement = TechType.Unobtanium;

        craftingTime = 6;
        inventorySize = new Vector2int(3, 3);

        addIngredient(TechType.Gravsphere, 1);
        addIngredient(TechType.Titanium, 2);
        addIngredient(TechType.Magnetite, 1);
        addIngredient(TechType.Aerogel, 3);
    }

    public override void prepareGameObject(GameObject go, Renderer[] r0) {
        base.prepareGameObject(go, r0);
        foreach (var r in r0) {
            RenderUtil.setEmissivity(r, 1);
        }

        RenderUtil.swapToModdedTextures(r0, this);
        go.EnsureComponent<ItemCollectorLogic>();
    }

    public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

    public override TechGroup GroupForPDA => TechGroup.Personal;

    public override TechCategory CategoryForPDA => TechCategory.Equipment;

    public override string[] StepsToFabricatorTab => ["Machines"];

    internal class ItemCollectorLogic : MonoBehaviour {
        private Gravsphere gravity;
        private Rigidbody body;

        private float lastInventoryCheckTime = -1;

        private readonly List<StorageContainer> targetInventories = [];

        internal static bool canGrab(GameObject go) {
            //SNUtil.writeToChat("item collector tried to grab "+go);
            var pp = go.FindAncestor<Pickupable>();
            return (pp && pp.isPickupable && !pp.attached) || go.FindAncestor<BreakableResource>();
        }

        private void Update() {
            if (!gravity)
                gravity = GetComponent<Gravsphere>();
            if (!body)
                body = GetComponent<Rigidbody>();

            if (Player.main.currentSub && Player.main.currentSub.isCyclops && Vector3.Distance(
                    transform.position,
                    Player.main.currentSub.transform.position
                ) <= 120) {
                var sub = Player.main.currentSub;
                var mode = sub.GetComponentInChildren<CyclopsMotorMode>();
                if (mode && mode.engineOn) {
                    var lgc = sub.GetComponentInChildren<ItemCollectorCyclopsTetherLogic>();
                    if (lgc) {
                        lgc.itemCollector = gameObject;
                        var tgt = sub.transform.position + sub.transform.up * -9 - sub.transform.forward * 18;
                        var diff = tgt - transform.position;
                        body.velocity = diff.normalized * Mathf.Min(diff.sqrMagnitude * 0.04F, 30);
                        lgc.lineRenderer.attachPoint.position = tgt + Vector3.up * 2;
                    }
                }
            }

            var time = DayNightCycle.main.timePassedAsFloat;
            if (time - lastInventoryCheckTime >= 1) {
                lastInventoryCheckTime = time;
                targetInventories.Clear();
                WorldUtil.getGameObjectsNear(
                    transform.position,
                    20,
                    go => {
                        tryAddTarget(go.FindAncestor<StorageContainer>());
                        var sub = go.FindAncestor<SubRoot>();
                        if (sub) {
                            foreach (var sc2 in sub.GetComponentsInChildren<StorageContainer>()) {
                                tryAddTarget(sc2);
                            }
                        }
                    }
                );
            }

            if (gravity && targetInventories.Count > 0 && Random.Range(0F, 1F) <= Time.deltaTime) {
                var rb = gravity.attractableList.getRandomEntry();
                if (rb && rb.gameObject.activeInHierarchy && !rb.GetComponent<WaterParkItem>()) {
                    var pp = rb.GetComponent<Pickupable>();
                    if (pp && Vector3.Distance(pp.transform.position, transform.position) <= 8) {
                        var sc = targetInventories.getRandomEntry();
                        if (sc && sc.container.AddItem(pp) != null) {
                            pp.PlayPickupSound();
                            pp.gameObject.SetActive(false);
                            gravity.removeList.Add(gravity.attractableList.IndexOf(rb));
                        } else {
                            SoundManager.playSoundAt(
                                SoundManager.buildSound("event:/interface/select"),
                                Player.main.transform.position,
                                false,
                                -1,
                                1
                            );
                        }
                    } else {
                        var res = rb.GetComponent<BreakableResource>();
                        if (res) {
                            res.BreakIntoResources();
                        }
                    }
                }
            }
        }

        private void tryAddTarget(StorageContainer sc) {
            if (sc && sc.name.ToLowerInvariant().Contains("locker") && !targetInventories.Contains(sc)) {
                targetInventories.Add(sc);
            }
        }
    }
}