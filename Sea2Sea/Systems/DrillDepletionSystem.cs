using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Nautilus.Assets;
using ReikaKalseki.Auroresource;
using ReikaKalseki.DIAlterra;
using UnityEngine;
using UnityEngine.UI;

namespace ReikaKalseki.SeaToSea;

public class DrillDepletionSystem {
    public static readonly DrillDepletionSystem Instance = new();

    private static readonly float DrillLife = 3600; //seconds
    internal static readonly float Radius = 200; //300;

    internal DrillDepletionAoe AoeEntity;

    private DrillDepletionSystem() {
    }

    internal void Register() {
        AoeEntity = new DrillDepletionAoe();
        AoeEntity.Register();
        SaveSystem.addSaveHandler(
            AoeEntity.ClassID,
            new SaveSystem.ComponentFieldSaveHandler<DrillDepletionAoeTag>().addField("totalDrillTime")
        );
    }

    private DrillDepletionAoeTag GetAoEForDrill(MonoBehaviour drill) {
        var set = WorldUtil.getObjectsNearWithComponent<DrillDepletionAoeTag>(drill.transform.position, 5);
        //SNUtil.writeToChat("Drill "+drill+" @ "+drill.transform.position+" fetching tag = "+tag.toDebugString());
        float initialValue = 0;
        if (set.Count > 1) {
            foreach (var tag in set) {
                initialValue += tag.TotalDrillTime;
                tag.gameObject.destroy(false);
            }

            set.Clear();
        }

        if (set.Count == 0) {
            var go = ObjectUtil.createWorldObject(AoeEntity.ClassID);
            go.transform.position = drill.transform.position;
            var tag = go.GetComponent<DrillDepletionAoeTag>();
            tag.TotalDrillTime = initialValue;
            set.Add(tag);
        }

        return set.First();
    }

    internal bool HasRemainingLife(MonoBehaviour drill) {
        var aoe = GetMotherlode(drill);
        if (aoe != null) {
            drill.gameObject.EnsureComponent<MotherlodeDrillTag>().Deposit = aoe;
            return true;
        }

        var tag = GetAoEForDrill(drill);
        return tag && tag.TotalDrillTime <= DrillLife;
    }

    internal void Deplete(MonoBehaviour drill) {
        var aoe = GetMotherlode(drill);
        //SNUtil.writeToChat("motherlode = "+aoe);
        if (aoe != null) {
            drill.gameObject.EnsureComponent<MotherlodeDrillTag>().Deposit = aoe;
            return;
        }

        var tag = GetAoEForDrill(drill);
        if (tag)
            tag.TotalDrillTime += DayNightCycle.main.deltaTime; //this is the time step they use too
    }

    internal DrillableResourceArea GetMotherlode(MonoBehaviour drill) {
        foreach (var d in WorldUtil.getObjectsNearWithComponent<DrillableResourceArea.DrillableResourceAreaTag>(
                     drill.transform.position,
                     DrillableResourceArea.getMaxRadius() + 10
                 )) {
            var aoe = d.GetComponentInChildren<SphereCollider>();
            var ctr = aoe.transform.position + aoe.center;
            if (ctr.y < drill.transform.position.y && MathUtil.isPointInCylinder(
                    ctr,
                    drill.transform.position,
                    aoe.radius - 10,
                    aoe.radius * 1.5F + 10
                )) {
                return DrillableResourceArea.getResourceNode(d.GetComponent<PrefabIdentifier>().ClassId);
            }
            //SNUtil.writeToChat("motherlode too far away @ "+ctr+" for "+drill.transform.position+" R="+aoe.radius);
        }

        return null;
    }
}

internal class MotherlodeDrillTag : MonoBehaviour {
    private static readonly int MotherlodeOresPerDay = 60;
    private static readonly int MotherlodeStorageCapacity = 1200;

    private static PropertyInfo _allowedOreField;
    private static FieldInfo _oresPerDayField;
    private static MethodInfo _oresPerDaySet;

    private static Type _drillerDisplay;

    private static MethodInfo _updateDisplay;

    //private static MethodInfo refreshDisplayStorage;
    private static FieldInfo _filterGridField;
    private static FieldInfo _filterListField;
    private static FieldInfo _storageText;

    private static PropertyInfo _controllerStorage;

    private static FieldInfo _storageCapacity;

    private static MethodInfo _showGridPage;

    //private static FieldInfo currentGridPage;
    private static FieldInfo _gridGo;

    private static FieldInfo _buttonItem;
    private static FieldInfo _buttonIcon;

    internal DrillableResourceArea Deposit;

    private Component _drillerDisplayComponent;

    private float _lastOreTableAssignTime = -1;

    private void Start() {
        SNUtil.WriteToChat(
            "Drill at " + WorldUtil.getRegionalDescription(transform.position, true) + " is mining deposit: " +
            Language.main.Get(Deposit.TechType.AsString())
        );
    }

    private void Update() {
        // TODO: FCS Compat
        // if (_allowedOreField == null) {
        //     var t = FCSIntegrationSystem.instance.getFCSDrillOreManager();
        //     _allowedOreField = t.GetProperty(
        //         "AllowedOres",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //     _oresPerDayField = t.GetField(
        //         "_oresPerDay",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //     _oresPerDaySet = t.GetMethod(
        //         "SetOresPerDay",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //
        //     _drillerDisplay = t.Assembly.GetType(
        //         "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerDisplay"
        //     );
        //     _updateDisplay = _drillerDisplay.GetMethod(
        //         "UpdateDisplayValues",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //     //refreshDisplayStorage = drillerDisplay.GetMethod("RefreshStorageAmount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //     _filterGridField = _drillerDisplay.GetField(
        //         "_filterGrid",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //     _filterListField = _drillerDisplay.GetField(
        //         "_trackedFilterState",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //     _storageText = _drillerDisplay.GetField(
        //         "_itemCounter",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //
        //     _controllerStorage = FCSIntegrationSystem.instance.getFCSDrillController().GetProperty(
        //         "DeepDrillerContainer",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //     _storageCapacity = FCSIntegrationSystem.instance.getFCSDrillStorage().GetField(
        //         "_storageSize",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //
        //     //currentGridPage = gridHelper.GetField("_currentPage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //     _showGridPage = typeof(GridHelper).GetMethod(
        //         "DrawPage",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
        //         null,
        //         CallingConventions.HasThis,
        //         [],
        //         null
        //     );
        //     _gridGo = typeof(GridHelper).GetField(
        //         "_itemsGrid",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //
        //     _buttonItem = typeof(uGUI_FCSDisplayItem).GetField(
        //         "_techType",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //     _buttonIcon = typeof(uGUI_FCSDisplayItem).GetField(
        //         "_icon",
        //         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        //     );
        //
        //     GridHelper grid = (GridHelper)_filterGridField.GetValue(_drillerDisplayComponent);
        //     grid.OnLoadDisplay += (data) => RebuildDisplay();
        // }
        //
        // var time = DayNightCycle.main.timePassedAsFloat;
        // if (time - _lastOreTableAssignTime >= 1) {
        //     _lastOreTableAssignTime = time;
        //     var com = GetComponent(FCSIntegrationSystem.instance.getFCSDrillOreManager());
        //     if (com) {
        //         _allowedOreField.SetValue(com, Deposit.getAllAvailableResources());
        //
        //         //set ores per day count too; default is 25 but increase to 60 on a motherlode
        //         var get = (int)_oresPerDayField.GetValue(com);
        //         if (get != MotherlodeOresPerDay)
        //             _oresPerDaySet.Invoke(com, [MotherlodeOresPerDay]);
        //
        //         RebuildDisplay();
        //     }
        //
        //     var com2 = GetComponent(FCSIntegrationSystem.instance.getFCSDrillController());
        //     if (com2) {
        //         var storage = _controllerStorage.GetValue(com2);
        //         _storageCapacity.SetValue(storage, MotherlodeStorageCapacity); //defaults to 300
        //     }
        // }
        //
        // GridHelper grid2 = (GridHelper)_filterGridField.GetValue(_drillerDisplayComponent);
        // if (grid2 != null) {
        //     var go = (GameObject)_gridGo.GetValue(grid2);
        //     foreach (uGUI_FCSDisplayItem c in go.GetComponentsInChildren<uGUI_FCSDisplayItem>()) {
        //         var tt = (TechType)_buttonItem.GetValue(c);
        //         var ico = (uGUI_Icon)_buttonIcon.GetValue(c);
        //         ico.sprite = SpriteManager.Get(C2CHooks.isFCSDrillMaterialAllowed(tt, true) ? tt : TechType.None);
        //     }
        //
        //     var t = (Text)_storageText.GetValue(_drillerDisplayComponent);
        //     t.text = t.text.Substring(0, t.text.LastIndexOf('/') + 1) + MotherlodeStorageCapacity;
        //     _updateDisplay.Invoke(_drillerDisplayComponent, []);
        // }
    }

    private void RebuildDisplay() {
        // TODO: FCS Compat
        // _drillerDisplayComponent = GetComponent(_drillerDisplay);
        // if (_drillerDisplayComponent) {
        //     var dict = (IDictionary)_filterListField.GetValue(_drillerDisplayComponent);
        //     dict.Clear();
        //
        //     GridHelper grid = (GridHelper)_filterGridField.GetValue(_drillerDisplayComponent);
        //     if (grid != null) {
        //         var go = (GameObject)_gridGo.GetValue(grid);
        //         go.removeChildObject("OreBTN");
        //         _showGridPage.Invoke(grid, new object[0]);
        //     }
        // }
    }
}

internal class DrillDepletionAoe : CustomPrefab {
    [SetsRequiredMembers]
    internal DrillDepletionAoe() : base("DrillDepletionAOE", "", "") {
        SetGameObject(GetGameObject);
    }

    public GameObject GetGameObject() {
        var go = new GameObject("DrillDepletionAOE(Clone)");
        go.EnsureComponent<PrefabIdentifier>().classId = Info.ClassID;
        go.EnsureComponent<TechTag>().type = Info.TechType;
        go.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Global;
        go.EnsureComponent<DrillDepletionAoeTag>();
        var sc = go.EnsureComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = DrillDepletionSystem.Radius;
        sc.center = Vector3.zero;
        go.layer = LayerID.NotUseable;
        return go;
    }
}

internal class DrillDepletionAoeTag : MonoBehaviour {
    internal float TotalDrillTime;
}