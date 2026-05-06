using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public static class InventoryUtil {
    public static List<TechType> getVehicleUpgrades(this Vehicle v) {
        List<TechType> set = [];
        foreach (var idx in v.slotIndexes.Values) {
            var ii = v.GetSlotItem(idx);
            if (ii != null && ii.item)
                set.Add(ii.item.GetTechType());
        }

        return set;
    }

    public static bool vehicleHasUpgrade(this Vehicle v, TechType tt) { /*
        foreach (int idx in v.slotIndexes.Values) {
            InventoryItem ii = v.GetSlotItem(idx);
            if (ii != null && ii.item && ii.item.GetTechType() == tt)
                return true;
        }
        return false;*/
        return v.modules.GetCount(tt) > 0;
    }

    public static bool isVehicleUpgradeSelected(this Vehicle v, TechType tt) {
        if (!v || v.activeSlot < 0)
            return false;
        var ii = v.GetSlotItem(v.activeSlot);
        return ii != null && ii.item.GetTechType() == tt;
    }

    public static List<TechType> getCyclopsUpgrades(this SubRoot sub) {
        List<TechType> set = [];
        var modules = sub.isCyclops && sub.upgradeConsole ? sub.upgradeConsole.modules : null;
        if (modules != null) {
            foreach (var slot in SubRoot.slotNames) {
                var tt = modules.GetTechTypeInSlot(slot);
                if (tt != TechType.None)
                    set.Add(tt);
            }
        }

        return set;
    }

    public static bool cyclopsHasUpgrade(this SubRoot sub, TechType tt) {
        var modules = sub.isCyclops && sub.upgradeConsole ? sub.upgradeConsole.modules : null; /*
        if (modules != null) {
            foreach (string slot in SubRoot.slotNames) {
                TechType tt2 = modules.GetTechTypeInSlot(slot);
                if (tt == tt2)
                    return true;
            }
        }
        return false;*/
        return modules != null && modules.GetCount(tt) > 0;
    }

    public static List<Battery> getCyclopsPowerCells(this SubRoot sub) {
        if (!sub.isCyclops)
            return null;
        List<Battery> ret = [];
        foreach (var p in sub.powerRelay.inboundPowerSources) {
            if (p is BatterySource b)
                ret.Add((Battery)b.battery);
        }

        return ret;
    }

    public static void addItem(TechType tt) {
        var obj = ObjectUtil.lookupPrefab(tt).clone();
        if (!obj) {
            SNUtil.WriteToChat("Could not spawn item " + tt + ", no prefab");
            return;
        }

        obj.SetActive(false);
        var pp = obj.GetComponent<Pickupable>();
        if (!pp) {
            SNUtil.WriteToChat("Could not add " + Language.main.Get(tt) + " to inventory - no Pickupable");
            return;
        }

        if (!Inventory.main.ForcePickup(pp))
            SoundManager.playSound("event:/env/keypad_wrong");
    }
    /*
    public static bool removeItem(ItemsContainer sc, InventoryItem ii) {
        return sc.DestroyItem(ii.item.GetTechType());
    }*/

    public static bool forceRemoveItem(this StorageContainer sc, Pickupable pp) {
        return sc.container.forceRemoveItem(sc.container.getItem(pp));
    }

    public static bool forceRemoveItem(this StorageContainer sc, InventoryItem ii) {
        return sc.container.forceRemoveItem(ii);
    }

    public static bool forceRemoveItem(this ItemsContainer sc, InventoryItem ii) {
        if (sc.RemoveItem(ii.item, true)) {
            ii.item.gameObject.destroy(false);
            return true;
        }

        return false;
    }

    public static bool isEmpty(this StorageContainer sc) {
        if (!sc)
            return true;
        var li = sc.container.GetItemTypes();
        return li == null || li.Count == 0;
    }

    public static void forEachOfType(this ItemsContainer sc, TechType tt, Action<InventoryItem> act) {
        var il = sc.GetItems(tt);
        if (il == null || il.Count == 0)
            return;
        var li = new List<InventoryItem>(il); //recache since may be removing
        foreach (var ii in li) {
            if (ii != null && ii.item)
                act.Invoke(ii);
        }
    }

    public static void forEach(this ItemsContainer sc, Action<InventoryItem> act) {
        foreach (var kvp in sc._items) {
            foreach (var ii in kvp.Value.items) {
                if (ii != null && ii.item)
                    act.Invoke(ii);
            }
        }
    }

    public static InventoryItem getItem(this ItemsContainer sc, Pickupable pp) {
        return sc.getItem(pp.GetTechType(), ii => ii.item == pp);
    }

    public static InventoryItem getItem(this ItemsContainer sc, TechType tt, Predicate<InventoryItem> acceptor = null) {
        var il = sc.GetItems(tt);
        if (il == null || il.Count == 0)
            return null;
        foreach (var ii in il) {
            if (ii != null && (acceptor == null || acceptor.Invoke(ii)))
                return ii;
        }

        return null;
    }

    public static IEnumerable<EnergyMixin> getAllHeldChargeables(bool heldOnly = false, bool includeHeld = true) {
        List<EnergyMixin> li = [];
        if (includeHeld) {
            var pt = Inventory.main.GetHeldTool();
            if (pt && pt.energyMixin)
                li.Add(pt.energyMixin);
        }

        if (!heldOnly) {
            li.AddRange(
                Inventory.main.storageRoot.GetComponentsInChildren<PlayerTool>().Select(t => t ? t.energyMixin : null)
                    .Where(e => (bool)e)
            );
        }

        return li;
    }

    public static int getActiveQuickslot() {
        var held = Inventory.main.quickSlots.heldItem;
        for (var i = 0; i < Inventory.main.quickSlots.binding.Length; i++) {
            var ii = Inventory.main.quickSlots.binding[i];
            if (ii == held)
                return i;
        }

        return -1;
    }
}