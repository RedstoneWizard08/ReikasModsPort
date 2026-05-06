using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class RebreatherRecharger : CustomMachine<RebreatherRechargerLogic> {
    internal const float PowerCostIdle = 0.5F; //per second
    internal const float PowerCostActive = 2.5F; //per second
    internal const float MaxRate = 7.5F; //seconds per second

    private static readonly int Shininess = Shader.PropertyToID("_Shininess");
    private static readonly int Fresnel = Shader.PropertyToID("_Fresnel");
    private static readonly int SpecInt = Shader.PropertyToID("_SpecInt");

    static RebreatherRecharger() {
    }

    [SetsRequiredMembers]
    public RebreatherRecharger(XMLLocale.LocaleEntry e) : base(
        e.key,
        e.name,
        e.desc,
        "bedc40fb-bd97-4b4d-a943-d39360c9c7bd"
    ) { //nuclear waste disposal
        addIngredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1);
        addIngredient(TechType.AramidFibers, 2);
        addIngredient(TechType.Titanium, 1);
        addIngredient(TechType.Pipe, 15);
    }

    public override bool UnlockedAtStart => false;

    public override void initializeMachine(GameObject go) {
        base.initializeMachine(go);
        go.removeComponent<Trashcan>();
        go.removeChildObject("Bubbles");

        var con = go.GetComponentInChildren<StorageContainer>();
        con.hoverText = "Reload Fluid";
        con.storageLabel = "FLUID";
        con.enabled = true;
        con.Resize(3, 3);
        //con.prefabRoot = go;
        var lgc = go.GetComponent<RebreatherRechargerLogic>();
        //lgc.storage = con;

        var air = ObjectUtil.lookupPrefab("7b4b90b8-6294-4354-9ebb-3e5aa49ae453");
        var snd = air.GetComponentInChildren<FMOD_CustomLoopingEmitter>(true);
        var mdl = go.setModel("discovery_trashcan_01_d", air.getChildObject("model"));
        mdl.transform.localScale = new Vector3(3, 4, 3);
        lgc.Sound = go.EnsureComponent<FMOD_CustomLoopingEmitter>();
        lgc.Sound.CopyObject(snd);
        lgc.Turbine = mdl.getChildObject("_pipes_floating_air_intake_turbine_geo");

        var r = mdl.GetComponentInChildren<Renderer>();
        //SNUtil.dumpTextures(r);
        RenderUtil.swapToModdedTextures(r, this);
        r.materials[0].SetFloat(Shininess, 7.5F);
        r.materials[0].SetFloat(Fresnel, 1F);
        r.materials[0].SetFloat(SpecInt, 15F);
        lgc.MainRenderer = r;

        go.GetComponent<Constructable>().model = mdl;
        go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
        go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 1.0F, 0);
    }
}

public class RebreatherRechargerLogic : CustomMachineLogic {
    internal Renderer MainRenderer;

    internal GameObject Turbine;
    internal FMOD_CustomLoopingEmitter Sound;

    private bool _inUse;
    private bool _isPowered;
    private float _secsNoPwr;
    private float _speed;

    private float _available;

    private void Start() {
        SNUtil.Log("Reinitializing rebreather charger");
        C2CItems.rebreatherCharger.initializeMachine(gameObject);
    }

    protected override void load(System.Xml.XmlElement data) {
        _available = (float)data.GetFloat("fuel", float.NaN);
    }

    protected override void save(System.Xml.XmlElement data) {
        data.AddProperty("fuel", _available);
    }

    protected override void updateEntity(float seconds) {
        if (MainRenderer == null)
            MainRenderer = gameObject.getChildObject("model").GetComponent<Renderer>();
        if (!storage) {
            return;
        }

        //SNUtil.writeToChat("I am ticking @ "+go.transform.position);
        if (seconds <= 0)
            return;
        storage.hoverText = "Reload Fluid (" + _available.ToString("0.00") + "s fluid in buffer)";

        var seabase = gameObject.transform.parent;
        if (_available > 0 && seabase != null) {
            //seabase.gameObject.EnsureComponent<RebreatherRechargerSeaBaseLogic>().addMachine(this);
            var p = Player.main;
            if (p.currentSub != null && seabase.gameObject == p.currentSub.gameObject) {
                //SNUtil.writeToChat("Player in base with recharger, has "+available);
                LiquidBreathingSystem.Instance.RefillFrom(this, seconds);
            }

            LiquidBreathingSystem.Instance.ApplyToBasePipes(this, seabase);
        }

        var cost = _inUse ? RebreatherRecharger.PowerCostActive : RebreatherRecharger.PowerCostIdle;
        _isPowered = consumePower(cost * seconds);
        if (_isPowered) {
            _speed = Math.Min(_speed * 1.05F + 0.15F, 150);
            _secsNoPwr = 0;
            Sound.Play();
            if (_available < 6000 && storage.container.GetCount(C2CItems.breathingFluid.TechType) > 0) {
                _available += LiquidBreathingSystem.ItemValue;
                storage.container.DestroyItem(C2CItems.breathingFluid.TechType);
            }
        } else {
            _speed = Math.Max(_speed * 0.98F - 0.02F, 0);
            _secsNoPwr += seconds;
            if (_secsNoPwr >= 1)
                Sound.Stop();
        }

        var angs = Turbine.transform.localEulerAngles;
        angs.y += _speed * seconds;
        Turbine.transform.localEulerAngles = angs;
    }

    public void Refund(float amt) {
        _available += amt;
    }

    public float GetFuel() {
        return _available;
    }

    public float Consume(float time, float seconds) {
        return _isPowered ? ConsumeUpTo(time, seconds) : 0;
    }

    private float ConsumeUpTo(float amt, float seconds) {
        var use = Mathf.Min(amt, _available, RebreatherRecharger.MaxRate * seconds);
        _available -= use;
        _inUse |= use > 0;
        return use;
    }
}

public class RebreatherRechargerSeaBaseLogic {
    private readonly Dictionary<string, RebreatherRechargerLogic> _machines = new();

    private void AddMachine(RebreatherRechargerLogic lgc) {
        _machines[lgc.gameObject.GetComponent<PrefabIdentifier>().id] = lgc;
    }
}