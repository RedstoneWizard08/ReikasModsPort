using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class GhostGelTracker : MonoBehaviour {
    public static readonly float REGEN_RATE = 60 * 30; //30 min per harvest

    private Sealed cutter;

    private float harvestCount;

    public void setup() {
        cutter = gameObject.EnsureComponent<Sealed>();
        cutter._sealed = true;
        cutter.maxOpenedAmount = 200;
        cutter.openedEvent.AddHandler(
            gameObject,
            new UWE.Event<Sealed>.HandleFunction(se => {
                    se.openedAmount = 0;
                    se._sealed = true;
                    if (PDAScanner.complete.Contains(TechType.GhostLeviathan) ||
                        PDAScanner.complete.Contains(TechType.GhostLeviathanJuvenile)) {
                        if (canHarvest) {
                            InventoryUtil.addItem(CraftingItems.getItem(CraftingItems.Items.GhostGel).Info.TechType);
                            harvestCount++;
                        } else {
                            SNUtil.writeToChat(
                                SeaToSeaMod.MouseoverLocale.getEntry("GhostLeviathanSampleCooldown").desc
                            );
                        }
                    }
                }
            )
        );
        var ht = gameObject.EnsureComponent<GenericHandTarget>();
        ht.onHandHover = new HandTargetEvent();
        ht.onHandHover.AddListener(hte => {
                var held = Inventory.main.GetHeld();
                if (held && held.GetTechType() == TechType.Scanner)
                    return;
                if (held && held.GetTechType() == TechType.LaserCutter &&
                    (PDAScanner.complete.Contains(TechType.GhostLeviathan) ||
                     PDAScanner.complete.Contains(TechType.GhostLeviathanJuvenile))) {
                    if (canHarvest) {
                        HandReticle.main.SetProgress(cutter.GetSealedPercentNormalized());
                        HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1f);
                        HandReticle.main.SetText(
                            HandReticle.TextType.Use,
                            "GhostLeviathanSample",
                            true
                        ); //is a locale key
                    } else {
                        HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                        HandReticle.main.SetText(
                            HandReticle.TextType.Use,
                            "GhostLeviathanSampleCooldown",
                            true
                        ); //is a locale key
                    }
                }
            }
        );
    }

    public bool canHarvest => harvestCount < 3;

    private void Update() {
        if (harvestCount > 0)
            harvestCount -= Time.deltaTime / REGEN_RATE;
    }
}