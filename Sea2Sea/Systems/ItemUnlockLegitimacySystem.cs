using System;
using System.Collections.Generic;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

public class ItemUnlockLegitimacySystem {
    public static readonly ItemUnlockLegitimacySystem instance = new();

    private readonly List<ItemGate> data = [];
    private readonly Dictionary<TechType, ItemGate> keyedData = new();
    private bool anyLoaded;

    private ItemUnlockLegitimacySystem() {
    }

    internal void add(string mod, string item, Func<bool> valid) {
        add(
            mod,
            item,
            valid,
            (pp, ep) => {
                Inventory.main.DestroyItem(pp.GetTechType());
                SoundManager.playSoundAt(
                    SoundManager.buildSound("event:/tools/gravsphere/explode"),
                    ep.transform.position
                );
            }
        );
    }

    internal void add(string mod, string item, Func<bool> valid, Action<Pickupable, Player> take) {
        data.Add(new ItemGate(mod, item, valid, take));
    }

    internal void applyPatches() {
        foreach (var g in data) {
            anyLoaded |= g.load();
            keyedData[g.itemType] = g;
        }
    }

    internal void tick(Player ep) {
        if (anyLoaded) {
            var pp = Inventory.main.GetHeld();
            if (pp) {
                var tt = pp.GetTechType();
                if (keyedData.ContainsKey(tt)) {
                    var ig = keyedData[tt];
                    if (!ig.validityCheck.Invoke()) {
                        ig.failureEffect.Invoke(pp, ep);
                    }
                }
            }
        }
    }

    public void validateModule(Vehicle v, int slotID, TechType tt) {
        var ii = v.GetSlotItem(slotID);
        if (ii != null && ii.item && SpawnedItemTracker.instance.isSpawned(ii.item)) {
            destroyModule(v.modules, ii, v.slotIDs[slotID]);
        }
    }

    public void validateModules(SubRoot sub) {
        var modules = sub.isCyclops && sub.upgradeConsole ? sub.upgradeConsole.modules : null;
        if (modules != null) {
            foreach (var slot in SubRoot.slotNames) {
                var ii = modules.GetItemInSlot(slot);
                if (ii != null && ii.item && SpawnedItemTracker.instance.isSpawned(ii.item)) {
                    destroyModule(modules, ii, slot);
                }
            }
        }
    }

    public void destroyModule(Equipment modules, InventoryItem ii, string slot) {
        var tt = ii.item.GetTechType();
        ((IItemsContainer)modules).RemoveItem(ii, true, false); //cast is necessary to hit the right method
        //((IItemsContainer)modules).UpdateContainer();
        ii.item.gameObject.destroy(false);
        SNUtil.writeToChat(
            "Destroying cheated module: " + Language.main.Get(tt) + " in " + modules.owner.name + " slot " + slot
        );
        SoundManager.playSoundAt(
            SoundManager.buildSound("event:/tools/gravsphere/explode"),
            modules.owner.transform.position
        );
        Player.main.GetPDA().Close();
    }

    private class ItemGate {
        internal readonly string sourceMod;
        internal readonly string techTypeName;

        internal readonly Func<bool> validityCheck;
        internal readonly Action<Pickupable, Player> failureEffect;

        internal bool isModLoaded;
        internal TechType itemType;

        internal ItemGate(string s, string tt, Func<bool> condition, Action<Pickupable, Player> take) {
            sourceMod = s;
            techTypeName = tt;
            validityCheck = condition;
            failureEffect = take;
        }

        internal bool load() {
            isModLoaded = BepInExUtil.IsModLoaded(sourceMod);
            itemType = tryFindItem();
            return itemType != TechType.None;
        }

        private TechType tryFindItem() {
            var tt = TechType.None;
            if (!EnumHandler.TryGetValue(techTypeName, out tt))
                if (!EnumHandler.TryGetValue(techTypeName.ToLowerInvariant(), out tt))
                    EnumHandler.TryGetValue(techTypeName.setLeadingCase(false), out tt);
            if (tt == TechType.None && isModLoaded)
                SNUtil.log("Could not find TechType for '" + techTypeName + "' in mod '" + sourceMod + "'");
            return tt;
        }
    }
}