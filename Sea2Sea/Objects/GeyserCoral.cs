using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class GeyserCoral : InteractableSpawnable {
    //public static readonly Color GLOW_COLOR = new Color(1, 112/255F, 0, 1);

    [SetsRequiredMembers]
    internal GeyserCoral(XMLLocale.LocaleEntry e) : base(e) {
        scanTime = 3;
        AddOnRegister(() => {
                SaveSystem.addSaveHandler(
                    Info.ClassID,
                    new SaveSystem.ComponentFieldSaveHandler<GeyserCoralTag>().addField("scanned")
                );
            }
        );
    }

    public override GameObject GetGameObject() {
        var world = new GameObject(Info.ClassID);
        world.EnsureComponent<TechTag>().type = Info.TechType;
        var pi = world.EnsureComponent<PrefabIdentifier>();
        pi.ClassId = Info.ClassID;
        world.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
        world.EnsureComponent<GeyserCoralTag>();
        if (!SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE)) {
            var rt = world.EnsureComponent<ResourceTracker>();
            rt.techType = Info.TechType;
            rt.overrideTechType = Info.TechType;
            rt.prefabIdentifier = pi;
        }

        world.layer = LayerID.Useable;

        //rotate to 270, 0, 0

        world.EnsureComponent<ImmuneToPropulsioncannon>().immuneToRepulsionCannon = true;

        var l = world.addLight(0.75F, 6);
        var f = l.gameObject.EnsureComponent<FlickeringLight>();
        f.dutyCycle = 0.8F;
        f.updateRate = 0.25F;
        f.fadeRate = 2F;
        l.transform.localPosition = Vector3.up * 1.2F;

        return world;
    }

    public void register() {
        Register();
    }

    public void PostRegister() {
        countGen(SeaToSeaMod.WorldGen);
        setFragment(C2CItems.geyserFilter.TechType, fragmentCount - 1);
        registerEncyPage();
    }
}

internal class GeyserCoralTag : MonoBehaviour {
    private static readonly int ColorProp = Shader.PropertyToID("_Color");
    private static readonly int SpecColorProp = Shader.PropertyToID("_SpecColor");
    private static readonly int GlowColorProp = Shader.PropertyToID("_GlowColor");

    private bool _scanned;
    private float _scannedFade;

    private GameObject _plateHolder;
    private readonly List<GameObject> _plates = [];

    private Light _light;
    private FlickeringLight _flicker;

    private Renderer[] _render;

    private bool _didTerrainCheck;
    private float _age;

    private bool _isHot;

    private void Update() {
        var time = DayNightCycle.main.timePassedAsFloat;
        if (!_plateHolder)
            Clear();
        if (!_light)
            _light = GetComponentInChildren<Light>();
        if (!_flicker)
            _flicker = GetComponentInChildren<FlickeringLight>();
        if (!_plateHolder)
            _plateHolder = gameObject.getChildObject("plateHolder");
        if (!_plateHolder)
            _plateHolder = new GameObject("plateHolder");
        _plateHolder.transform.SetParent(transform);
        _plateHolder.transform.localRotation = Quaternion.Euler(270, 0, 0);
        _plateHolder.transform.localPosition = Vector3.zero;
        var targetCount = (int)transform.position.magnitude % 3 + 3; //3-5
        if (_plates.Count == 0) {
            foreach (Transform t in _plateHolder.transform) {
                _plates.Add(t.gameObject);
            }
        }

        _age += Time.deltaTime;
        if (_plates.Count != targetCount) {
            Clear();
            Random.InitState((int)transform.position.magnitude);
            while (_plates.Count < targetCount) {
                var pfb = ObjectUtil.lookupPrefab(VanillaFlora.TABLECORAL_ORANGE.getRandomPrefab(false));
                var i = _plates.Count;
                var plate = new GameObject("plate" + i);
                var model = pfb.GetComponentInChildren<Renderer>().gameObject.clone();
                var collider = pfb.GetComponentInChildren<BoxCollider>().gameObject.clone();
                model.transform.SetParent(plate.transform);
                collider.transform.SetParent(plate.transform);

                plate.transform.SetParent(_plateHolder.transform);
                var d = -0.5F + 0.25F * i;
                plate.transform.localPosition = new Vector3(0, d + Random.Range(-0.05F, 0.05F), 0);
                plate.transform.localRotation = Quaternion.identity; //Quaternion.Euler(270, 0, 0);
                plate.transform.localScale = Vector3.one;
                _plates.Add(plate);
            }

            _render = null;
        }

        var hot = VanillaBiomes.Koosh.IsInBiome(transform.position) ||
                  WaterTemperatureSimulation.main.GetTemperature(transform.position) > 40;
        var retexture = _isHot != hot;
        _isHot = hot;
        if (_isHot) {
            _flicker.maxIntensity = 1.25F; //from 0.75
            _flicker.fadeRate = 4F;
            _flicker.updateRate = 0.2F;
            _light.range = 3F;
        }

        _light.color = hot ? new Color(1F, 0.4F, 0.0F, 1) : new Color(0.2F, 0.7F, 1F, 1);
        if (_render == null) {
            _render = GetComponentsInChildren<Renderer>();
            retexture = true;
        }

        if (retexture) {
            foreach (var r2 in _render) {
                RenderUtil.swapTextures(
                    SeaToSeaMod.ModDLL,
                    r2,
                    _isHot ? "Textures/GeyserCoral" : "Textures/GeyserCoral2"
                );
                RenderUtil.setGlossiness(r2, 6, -200, -10);
                RenderUtil.setEmissivity(r2, 120);
                r2.material.SetColor(ColorProp, Color.white);
                r2.material.SetColor(SpecColorProp, Color.white);
                r2.material.SetColor(GlowColorProp, Color.white);
            }
        }

        if (_scanned)
            _scannedFade = Mathf.Min(_scannedFade + 0.5F * Time.deltaTime, 1);

        if (!_didTerrainCheck && _age >= 2 &&
            Vector3.Distance(Player.main.transform.position, transform.position) < 120) {
            _didTerrainCheck = true;
            var any = false;
            Random.InitState((int)transform.position.magnitude);
            foreach (var go in _plates) {
                var hit = WorldUtil.getTerrainVectorAt(go.transform.position + transform.up * 1.5F, 4, -transform.up);
                if (hit.HasValue) {
                    go.SetActive(true);
                    go.transform.forward = hit.Value.normal;
                    go.transform.position = hit.Value.point + go.transform.forward * -0.05F;
                    //go.transform.up = hit.Value.normal;
                    go.transform.Rotate(new Vector3(0, 0, Random.Range(-15F, 15F)), Space.Self);
                    any = true;
                } else {
                    go.SetActive(false);
                }
            }

            if (!any) {
                transform.position += transform.up * 0.1F;
                _didTerrainCheck = false;
            }
        }

        foreach (var r in _render) {
            var f = 100 + (20 + _scannedFade * 80) * Mathf.Sin(
                time * (0.617F + _scannedFade * 1.4F) + r.transform.position.magnitude * 10 % 1781
            );
            RenderUtil.setEmissivity(
                r,
                f * (0.25F + Mathf.Max(0.25F, 0.75F * _flicker.currentIntensity / _flicker.maxIntensity))
            );
        }
    }

    private void Clear() {
        foreach (var go in _plates) {
            go.destroy();
        }

        _plates.Clear();
    }

    private void OnScanned() {
        _scanned = true;
        SNUtil.AddBlueprintNotification(C2CItems.geyserFilter.TechType);
    }
}