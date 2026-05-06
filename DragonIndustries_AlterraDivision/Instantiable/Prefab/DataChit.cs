using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using Nautilus.Handlers;
using Story;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public sealed class DataChit : CustomPrefab {
    public readonly StoryGoal goal;

    public Color renderColor = new(229 / 255F, 133 / 255F, 0); //avali aerogel color
    public bool showOnScannerRoom = true;

    private readonly System.Reflection.Assembly ownerMod;

    private static readonly Dictionary<string, SNUtil.PopupData> popupData = new();

    private static bool registeredCommonTechType;
    public static TechType scannerRoomChitType { get; private set; }

    [SetsRequiredMembers]
    public DataChit(string goalKey, string name, string desc, Action<SNUtil.PopupData> a = null) : this(
        new StoryGoal(goalKey, Story.GoalType.Story, 0),
        name,
        desc,
        a
    ) {
    }

    [SetsRequiredMembers]
    public DataChit(StoryGoal g, string name, string desc, Action<SNUtil.PopupData> a = null) : base(
        "DataChit_" + g.key,
        "Data Card - " + name,
        "Unlocks " + g.key
    ) {
        goal = g;
        ownerMod = SNUtil.TryGetModDLL();

        if (!registeredCommonTechType) {
            scannerRoomChitType = EnumHandler.AddEntry<TechType>("DataChit").WithPdaInfo("Data Card", "");
            SpriteHandler.RegisterSprite(
                scannerRoomChitType,
                TextureManager.getSprite(SNUtil.DiDLL, "Textures/ScannerSprites/DataChit")
            );
            registeredCommonTechType = true;
        }

        AddOnRegister(() => {
                var data = new SNUtil.PopupData("Digital Data Downloaded", desc) {
                    Sound = "event:/tools/scanner/scan_complete",
                };
                data.OnUnlock = () => { SNUtil.TriggerUnlockPopup(data); };
                a?.Invoke(data);
                popupData[Info.ClassID] = data;
            }
        );

        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject("1bdbad41-adcb-47db-ab2c-0dc4a7180860");
        world.transform.localScale = new Vector3(0.4F, 1, 1F);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        var tgt = world.EnsureComponent<StoryHandTarget>();
        tgt.goal = goal;
        tgt.primaryTooltip = Info.PrefabFileName;
        tgt.informGameObject = world;
        if (showOnScannerRoom)
            ObjectUtil.makeMapRoomScannable(world, scannerRoomChitType);
        else
            world.removeComponent<ResourceTracker>();
        world.EnsureComponent<DataChitTag>();
        world.removeChildObject("PDALight");
        var r = world.GetComponentInChildren<Renderer>();
        RenderUtil.swapTextures(
            SNUtil.DiDLL,
            r,
            "Textures/DataChit/",
            new Dictionary<int, string> { { 0, "" }, { 1, "" }, { 2, "" } }
        );
        foreach (var m in r.materials)
            m.SetColor("_GlowColor", renderColor.WithAlpha(1));
        var l = world.addLight(0.5F, 6, renderColor);
        l.transform.localPosition = new Vector3(0.0F, 0.5F, 0.15F);
        l = world.addLight(1.5F, 1.2F, renderColor);
        l.transform.localPosition = new Vector3(0.0F, 0.125F, 0.15F);
        return world;
    }

    private class DataChitTag : MonoBehaviour {
        private void Start() {
            if (GetComponent<ResourceTracker>())
                ObjectUtil.makeMapRoomScannable(gameObject, scannerRoomChitType).Register();
        }

        private void OnStoryHandTarget() {
            var popup = popupData[GetComponent<PrefabIdentifier>().ClassId];
            popup.OnUnlock?.Invoke();
        }
    }
}