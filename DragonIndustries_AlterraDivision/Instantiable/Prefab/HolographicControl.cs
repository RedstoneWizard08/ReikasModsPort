using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using Nautilus.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.DIAlterra;

public sealed class HolographicControl : CustomPrefab {
    private readonly System.Reflection.Assembly ownerMod;

    private Sprite[] spr = null;

    internal static readonly Dictionary<string, HolographicControl> controlTypes = new();

    internal readonly Action<HolographicControlTag> actionData;
    internal readonly Func<HolographicControlTag, bool> validityData;
    internal readonly bool isToggleable;
    internal Sprite[] icons;

    internal static readonly Sprite defaultOffIcon = Sprite.Create(
        TextureManager.getTexture(SNUtil.DiDLL, "Textures/HoloButton_false"),
        new Rect(0, 0, 200, 200),
        new Vector2(0, 0)
    );

    internal static readonly Sprite defaultOnIcon = Sprite.Create(
        TextureManager.getTexture(SNUtil.DiDLL, "Textures/HoloButton_true"),
        new Rect(0, 0, 200, 200),
        new Vector2(0, 0)
    );

    [SetsRequiredMembers]
    public HolographicControl(
        string name,
        string desc,
        bool tg,
        Action<HolographicControlTag> a,
        Func<HolographicControlTag, bool> f
    ) : base("HoloControl_" + name, "Holographic Control - " + name, desc) {
        ownerMod = SNUtil.TryGetModDLL();

        isToggleable = tg;
        actionData = a;
        validityData = f;
        icons = spr != null ? spr :
            isToggleable ? new Sprite[] { defaultOffIcon, defaultOnIcon } : new Sprite[] { defaultOffIcon };

        AddOnRegister(() => {
                controlTypes[Info.ClassID] = this;
                LanguageHandler.SetLanguageLine("holocontrol_" + Info.ClassID, desc);
                SaveSystem.addSaveHandler(
                    Info.ClassID,
                    new SaveSystem.ComponentFieldSaveHandler<HolographicControlTag>().addField("isToggled")
                );
            }
        );

        SetGameObject(GetGameObject);
    }

    public HolographicControl setIcons(string pathAndName, int size) {
        if (isToggleable) {
            var off = Sprite.Create(
                TextureManager.getTexture(ownerMod, pathAndName + "_false"),
                new Rect(0, 0, size, size),
                new Vector2(0, 0)
            );
            var on = Sprite.Create(
                TextureManager.getTexture(ownerMod, pathAndName + "_true"),
                new Rect(0, 0, size, size),
                new Vector2(0, 0)
            );
            return setIcons(off, on);
        } else {
            icons = [
                Sprite.Create(
                    TextureManager.getTexture(ownerMod, pathAndName),
                    new Rect(0, 0, size, size),
                    new Vector2(0, 0)
                ),
            ];
            return this;
        }
    }

    public HolographicControl setIcons(Sprite off, Sprite on) {
        icons = [off, on];
        return this;
    }

    public GameObject GetGameObject() {
        var world = new GameObject(Info.ClassID);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
        var c = world.EnsureComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.scaleFactor = 1;
        c.planeDistance = 100;
        c.referencePixelsPerUnit = 100;
        c.normalizedSortingGridSize = 0.1F;
        c.pixelPerfect = false;
        c.overrideSorting = false;
        c.overridePixelPerfect = false;
        world.EnsureComponent<CanvasScaler>().scaleFactor = 1;
        world.EnsureComponent<GraphicRaycaster>();
        var gph = new GameObject("graphic");
        var cr = gph.EnsureComponent<CanvasRenderer>();
        gph.transform.SetParent(world.transform);
        gph.transform.localScale = new Vector3(0.0025F, 0.0025F, 1F);
        var img = gph.EnsureComponent<Image>();
        img.sprite = icons[0];
        var box = gph.EnsureComponent<SphereCollider>();
        box.center = Vector3.zero;
        box.radius = 0.5F;
        box.isTrigger = true;
        gph.layer = LayerID.Useable;
        world.layer = LayerID.Useable;
        gph.EnsureComponent<HolographicControlTag>();
        return world;
    }

    public override string ToString() {
        return "Button_" + Info.ClassID;
    }


    public static HolographicControlTag addButton(GameObject box, HolographicControl control) {
        foreach (var pi in box.transform.GetComponentsInChildren<PrefabIdentifier>()) {
            if (pi && pi.classId == control.Info.ClassID) {
                var tag = pi.GetComponentInChildren<HolographicControlTag>();
                if (tag)
                    return tag;
                else
                    pi.gameObject.destroy(false);
            }
        }

        var btn = ObjectUtil.createWorldObject(control.Info.ClassID);
        var com = btn.GetComponentInChildren<HolographicControlTag>();
        btn.transform.SetParent(box.transform);
        return com;
    }

    public static HolographicControlTag[] addButtons(GameObject box, params HolographicControl[] control) {
        var add = new HolographicControlTag[control.Length];
        for (var i = 0; i < add.Length; i++) {
            add[i] = addButton(box, control[i]);
        }

        return add;
    }

    public class HolographicControlTag : MonoBehaviour, IHandTarget {
        private bool isToggled;

        public HolographicControl controlRef { get; private set; }

        public void setState(bool toggle) {
            if (!controlRef.isToggleable)
                return;
            if (toggle != isToggled)
                GetComponent<Image>().sprite = controlRef.icons[toggle ? 1 : 0];
            isToggled = toggle;
            SendMessageUpwards("SetHolographicControlState", this, SendMessageOptions.DontRequireReceiver);
        }

        private void Start() {
            controlRef = controlTypes[GetComponentInParent<PrefabIdentifier>().ClassId];
        }

        public bool getState() {
            return isToggled;
        }

        public void disable() {
            setState(false);
        }

        public void enableForDuration(float time) {
            setState(true);
            Invoke(nameof(disable), time);
        }

        public void OnHandHover(GUIHand hand) {
            HandReticle.main.SetText(HandReticle.TextType.Use, "holocontrol_" + controlRef.Info.ClassID, true);
            HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
        }

        public void OnHandClick(GUIHand hand) {
            controlRef.actionData.Invoke(this);
            SoundManager.playSoundAt(
                SoundManager.buildSound("event:/sub_module/fabricator/fabricator_click"),
                transform.position
            );
        }

        public bool isStillValid() {
            return controlRef.validityData.Invoke(this);
        }

        public void destroy() {
            GetComponentInParent<PrefabIdentifier>().gameObject.destroy();
        }
    }
}