using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class TemporaryFloatingLocker : CustomPrefab {
    [SetsRequiredMembers]
    public TemporaryFloatingLocker() : base("TemporaryFloatingLocker", "Temporary Locker", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject("9d9ed0b0-df64-45ee-9b90-34386a98b233");
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        var sc = world.getChildObject("StorageContainer").EnsureComponent<StorageContainer>();
        sc.Resize(6, 10);
        return world;
    }

    public static void createFloatingLocker(Vector3 pos, IEnumerable<Pickupable> li) {
        var go = ObjectUtil.createWorldObject(DIMod.floatingLocker.Info.ClassID);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        var tag = go.EnsureComponent<TemporaryLockerControlTag>();
        tag.allowAdd = true;
        var sc = go.GetComponentInChildren<StorageContainer>();
        foreach (var pp in li) {
            sc.container.AddItem(pp);
        }

        tag.allowAdd = false;
    }

    private class TemporaryLockerControlTag : MonoBehaviour {
        public bool allowAdd;

        private void Start() {
            var sc = GetComponentInChildren<StorageContainer>();
            //sc.container.onAddItem += this.updateStoredItem;
            sc.container.onRemoveItem += ii => { Invoke(nameof(checkEmpty), 0.25F); };
            sc.container.isAllowedToAdd = new IsAllowedToAdd((pp, vb) => allowAdd);
        }

        internal void checkEmpty() {
            var sc = GetComponentInChildren<StorageContainer>();
            if (sc.isEmpty()) {
                var pda = Player.main.GetPDA();
                if (pda && pda.isOpen && pda.ui && pda.ui.currentTab is uGUI_InventoryTab it &&
                    it.storage.container == sc.container)
                    pda.Close();
                gameObject.destroy();
            }
        }
    }
}