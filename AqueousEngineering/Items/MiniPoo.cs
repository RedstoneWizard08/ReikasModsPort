using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public class MiniPoo : CustomPrefab {
    [SetsRequiredMembers]
    public MiniPoo(XMLLocale.LocaleEntry e) : base(e.key, e.name, e.desc) {
        AddOnRegister(() => { ItemRegistry.instance.addItem(this); });
        SetGameObject(GetGameObject);
        Info.WithIcon(GetItemSprite());
    }

    public GameObject GetGameObject() {
        var go = ObjectUtil.createWorldObject("61ac1241-e990-4646-a618-bddb6960325b");
        go.transform.localScale = Vector3.one * 0.2F;
        return go;
    }

    protected Sprite GetItemSprite() {
        return SpriteManager.Get(TechType.SeaTreaderPoop);
    }
}