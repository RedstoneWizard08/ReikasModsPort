using System;
using System.Collections.Generic;
using ECCLibrary;
using Nautilus.Assets;
using Nautilus.Handlers;
using ReikaKalseki.DIAlterra;
//using DeExtinctionMod;
//using DeExtinctionMod.Prefabs.Creatures;
using UnityEngine;

namespace ReikaKalseki.SeaToSea;

public class DeIntegrationSystem {
    public static readonly DeIntegrationSystem Instance = new();
    private static readonly int Color1 = Shader.PropertyToID("_Color");
    private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");
    private static readonly int Fresnel = Shader.PropertyToID("_Fresnel");
    private static readonly int Shininess = Shader.PropertyToID("_Shininess");
    private static readonly int SpecInt = Shader.PropertyToID("_SpecInt");
    private static readonly int EmissionLm = Shader.PropertyToID("_EmissionLM");
    private static readonly int EmissionLmNight = Shader.PropertyToID("_EmissionLMNight");
    private static readonly int MyCullVariable = Shader.PropertyToID("_MyCullVariable");

    private readonly bool _isDeELoaded;

    private readonly HashSet<TechType> _creatures = [];
    private readonly HashSet<TechType> _eggs = [];
    private TechType _thelassaceanType;
    private TechType _lrThelassaceanType;
    private TechType _jellySpinnerType;
    private TechType _rubyPincherType;
    private TechType _gulperType;
    private TechType _axetailType;
    private TechType _filtorbType;

    private CreatureAsset _voidThelassacean;

    public bool SpawnVoidThalaAnywhere;
    public int MaxVoidThala = 12;

    internal WorldCollectedItem ThalassaceanCud;

    private DeIntegrationSystem() {
        _isDeELoaded = BepInExUtil.IsModLoaded(PluginIDs.DeExtinction);
        if (_isDeELoaded) {
        }
    }

    public bool IsLoaded() {
        return _isDeELoaded;
    }

    public TechType GetThalassacean() {
        return _thelassaceanType;
    }

    public TechType GetLrThalassacean() {
        return _lrThelassaceanType;
    }

    public TechType GetRubyPincher() {
        return _rubyPincherType;
    }

    public TechType GetGulper() {
        return _gulperType;
    }

    public TechType GetFiltorb() {
        return _filtorbType;
    }

    public CreatureAsset GetVoidThalassacean() {
        return _voidThelassacean;
    }

    internal void ApplyPatches() {
        if (_isDeELoaded)
            DoApplyPatches();
    }

    private void DoApplyPatches() {
        var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);

        ThalassaceanCud = new WorldCollectedItem(
            SeaToSeaMod.ItemLocale.getEntry("ThalassaceanCud"),
            "bfe8345c-fe3c-4c2b-9a03-51bcc5a2a782"
        ) {
            renderModify = C2CThalassaceanCudTag.SetupRenderer,
            sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/ThalassaceanCud"),
        };
        ThalassaceanCud.Register();

        BioReactorHandler.SetBioReactorCharge(ThalassaceanCud.TechType, BaseBioReactor.GetCharge(TechType.Hoopfish));

        _thelassaceanType = FindCreature("StellarThalassacean");
        _lrThelassaceanType = FindCreature("JasperThalassacean");

        _jellySpinnerType = FindCreature("JellySpinner", true);

        _creatures.Add(_thelassaceanType);
        _creatures.Add(_lrThelassaceanType);
        _creatures.Add(_jellySpinnerType);
        _creatures.Add(FindCreature("Twisteel"));
        _gulperType = FindCreature("GulperLeviathan");
        _creatures.Add(_gulperType);
        _creatures.Add(FindCreature("GulperLeviathanBaby"));
        _creatures.Add(FindCreature("GrandGlider"));
        _axetailType = FindCreature("Axetail", true);
        _creatures.Add(_axetailType);
        _creatures.Add(FindCreature("RibbonRay", true));
        _filtorbType = FindCreature("Filtorb", true);
        _creatures.Add(_filtorbType);
        _creatures.Add(FindCreature("TriangleFish", true));
        _rubyPincherType = FindCreature("RubyClownPincher", true);
        _creatures.Add(_rubyPincherType);
        _creatures.Add(FindCreature("SapphireClownPincher", true));
        _creatures.Add(FindCreature("EmeraldClownPincher", true));
        _creatures.Add(FindCreature("AmberClownPincher", true));
        _creatures.Add(FindCreature("CitrineClownPincher", true));

        _eggs.Add(FindCreature("GrandGliderEgg"));
        _eggs.Add(FindCreature("StellarThalassaceanEgg"));
        _eggs.Add(FindCreature("JasperThalassaceanEgg"));
        _eggs.Add(FindCreature("TwisteelEgg"));
        _eggs.Add(FindCreature("GulperEgg"));

        RecipeUtil.addIngredient(C2CItems.powerSeal.TechType, ThalassaceanCud.TechType, 4);
        RecipeUtil.addIngredient(
            CraftingItems.getItem(CraftingItems.Items.HeatSealant).TechType,
            ThalassaceanCud.TechType,
            2
        );
        RecipeUtil.addIngredient(
            CraftingItems.getItem(CraftingItems.Items.SealFabric).TechType,
            ThalassaceanCud.TechType,
            1
        );
        RecipeUtil.addIngredient(C2CItems.depth1300.TechType, ThalassaceanCud.TechType, 4);
        RecipeUtil.addIngredient(C2CItems.bandage.TechType, ThalassaceanCud.TechType, 1);

        var amt = RecipeUtil.removeIngredient(C2CItems.breathingFluid.TechType, TechType.Eyeye).amount;
        RecipeUtil.addIngredient(C2CItems.breathingFluid.TechType, _jellySpinnerType, amt * 3 / 2); //from 2 to 3

        // TODO
        // foreach (var tt in _eggs) {
        //     CreatureEggAsset egg = (CreatureEggAsset)tt.getModPrefabByTechType();
        //     foreach (LootDistributionData.BiomeData bd in egg.BiomesToSpawnIn) {
        //         var f = bd.probability;
        //         f = Mathf.Min(f, 0.75F) * 0.67F;
        //         f = Mathf.Round(f * 20F) / 20F; //round to nearest 0.05
        //         f = Mathf.Max(f, 0.05F);
        //         SNUtil.log(
        //             "Reducing spawn chance of " + egg.ClassID + " in " + Enum.GetName(typeof(BiomeType), bd.biome) +
        //             " from " + bd.probability + " to " + f
        //         );
        //         LootDistributionHandler.EditLootDistributionData(egg.ClassID, bd.biome, f, 1);
        //     }
        // }
        //
        // var filtorb = (CustomPrefab)_filtorbType.getModPrefabByTechType();
        // foreach (LootDistributionData.BiomeData bd in filtorb.BiomesToSpawnIn) {
        //     var f = bd.probability;
        //     f = Mathf.Min(f, 0.8F) * 0.75F;
        //     f = Mathf.Round(f * 20F) / 20F; //round to nearest 0.05
        //     f = Mathf.Max(f, 0.05F);
        //     SNUtil.log(
        //         "Reducing spawn chance of filtorb in " + Enum.GetName(typeof(BiomeType), bd.biome) + " from " +
        //         bd.probability + " to " + f
        //     );
        //     LootDistributionHandler.EditLootDistributionData(filtorb.ClassID, bd.biome, f, 1);
        // }

        _voidThelassacean = new VoidThalassacean(SeaToSeaMod.ItemLocale.getEntry("VoidThalassacean"));
        _voidThelassacean.Register();

        FinalLaunchAdditionalRequirementSystem.instance.addRequiredItem(
            _filtorbType,
            4,
            "A water-rich organism with a defense mechanism against being grabbed"
        );

        CraftDataHandler.SetItemSize(_axetailType, new Vector2int(2, 1));
    }

    private TechType FindCreature(string id, bool edible = false) {
        if (!EnumHandler.TryGetValue(id, out TechType tt))
            if (!EnumHandler.TryGetValue(id.ToLowerInvariant(), out tt))
                EnumHandler.TryGetValue(id.setLeadingCase(false), out tt);
        if (tt == TechType.None)
            throw new Exception("Could not find DeE TechType for '" + id + "'");
        if (edible) {
            Campfire.addRecipe(tt, tt == _axetailType ? 4 : 2, f => f.itemTemplate = TechType.CuredPeeper);
        }

        return tt;
    }

    [Obsolete("Unimplemented")]
    public void ConvertEgg(string type, float r) {
        foreach (var pi in WorldUtil.getObjectsNearWithComponent<PrefabIdentifier>(Player.main.transform.position, r)) {
            if (pi && pi.ClassId == type) {
                //TODO
            }
        }
    }

    public void TickVoidThalassaceanSpawner(Player ep) {
        if (SpawnVoidThalaAnywhere ||
            (ep.transform.position.y >= -800 && VanillaBiomes.Void.IsInBiome(ep.transform.position))) {
            var has = WorldUtil.getObjectsNearWithComponent<VoidThalassaceanTag>(ep.transform.position, 200);
            if (has.Count < MaxVoidThala) {
                for (var i = has.Count; i < MaxVoidThala; i++) {
                    var pos = MathUtil.getRandomPointAtSetDistance(ep.transform.position, 200);
                    if (pos.y > -25)
                        continue;
                    if (SpawnVoidThalaAnywhere || VanillaBiomes.Void.IsInBiome(pos)) {
                        var go = ObjectUtil.createWorldObject(_voidThelassacean.ClassID);
                        go.transform.position = pos;
                        go.fullyEnable();
                        //SNUtil.writeToChat("spawned void thalassacean at "+go.transform.position+" dist="+Vector3.Distance(pos, ep.transform.position));
                    }
                }
            }
        }
    }

    public bool IsEgg(TechType tt) {
        return _eggs.Contains(tt);
    }

    internal class C2CGulper : MonoBehaviour { //stay out of my damn biome

        private SwimBehaviour _swim;
        private LastTarget _target;
        private Creature _creature;

        private Vector3 _leash = UnderwaterIslandsFloorBiome.biomeCenter.setY(-200);

        private void Update() {
            if (!_swim)
                _swim = GetComponent<SwimBehaviour>();
            if (!_creature)
                _creature = GetComponent<Creature>();
            if (!_target)
                _target = GetComponent<LastTarget>();
            var bb = BiomeBase.GetBiome(transform.position);
            var biome = bb == VanillaBiomes.Underislands || bb == UnderwaterIslandsFloorBiome.instance;
            if (biome) {
                if (_target && _target.target && _target.transform.position.y < -300)
                    _target.target = null;
                if (_creature)
                    _creature.leashPosition = _leash;
                if (transform.position.y < -300)
                    _swim.SwimTo(_leash, 40);
            }
        }
    }

    internal class C2CThalassacean : MonoBehaviour {
        public static readonly string MouthName = "Mouth"; //already has one

        public static readonly float
            RegrowTime = 3600; //60 min, but do not serialize, so will reset if leave and come back

        internal float LastCollect = -9999;

        private GameObject _mouthInteract;

        private GameObject _mouthItem;

        private void Start() {
            _mouthInteract = gameObject.getChildObject(MouthName);
        }

        private void Update() {
            if (!DayNightCycle.main)
                return;
            if (!_mouthInteract)
                _mouthInteract = gameObject.getChildObject(MouthName);

            var act = DayNightCycle.main.timePassedAsFloat - LastCollect >= RegrowTime;
            //mouthInteract.SetActive(act);
            if (act && _mouthInteract && (!_mouthItem || !_mouthItem.activeInHierarchy ||
                                          _mouthItem.transform.parent != _mouthInteract.transform)) {
                _mouthItem = ObjectUtil.createWorldObject(Instance.ThalassaceanCud.ClassID);
                _mouthItem.SetActive(true);
                _mouthItem.transform.SetParent(_mouthInteract.transform);
            }

            if (_mouthItem)
                _mouthItem.transform.localPosition = new Vector3(0, 0, -0.5F);
        }
        /*
        public bool collect() {
            float time = DayNightCycle.main.timePassedAsFloat;
            if (time-lastCollect < REGROW_TIME)
                return false;
            InventoryUtil.addItem(instance.thalassaceanCud.TechType);
            lastCollect = time;
            return true;
        }*/
    }

    /*
    internal class C2CThalassaceanMouthTag : MonoBehaviour, IHandTarget {

        private SphereCollider interact;
        private C2CThalassacean owner;

        void Start() {
            interact = gameObject.EnsureComponent<SphereCollider>();
            interact.radius = 0.5F;
            owner = gameObject.FindAncestor<C2CThalassacean>();
        }

        public void OnHandHover(GUIHand hand) {
            HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
            HandReticle.main.SetInteractText("ThalassaceanMouthClick");
            HandReticle.main.SetTargetDistance(8);
        }

        public void OnHandClick(GUIHand hand) {
            owner.collect();
        }

    }*/
    internal class C2CThalassaceanCudTag : MonoBehaviour {
        private float _lastParentageCheck;

        private void Start() {
            Invoke(nameof(SetupRenderer), 0.5F);
        }

        public void SetupRenderer() {
            SetupRenderer(this);
        }

        public static void SetupRenderer(Component c) {
            var root = c.gameObject.FindAncestor<PrefabIdentifier>().gameObject;
            var gp = root.GetComponent<GasPod>();
            root.removeComponent<UWE.TriggerStayTracker>();
            root.removeComponent<FMOD_StudioEventEmitter>();
            root.removeComponent<ResourceTracker>();
            var pfb = ObjectUtil.lookupPrefab("505e7eff-46b3-4ad2-84e1-0fadb7be306c");
            var mdl = pfb.GetComponentInChildren<Animator>().gameObject.clone();
            mdl.removeChildObject("root", false);
            mdl.transform.SetParent(gp.model.transform.parent);
            mdl.transform.localPosition = gp.model.transform.localPosition;
            gp.model.destroy();
            gp.destroy();
            var r = root.GetComponentInChildren<Renderer>();
            //SNUtil.log("Adjusting Thalassacean cud renderer "+r.gameObject.GetFullHierarchyPath());
            var clr = new Color(0.67F, 0.95F, 0.2F, 0.5F); //new Color(0.4F, 0.3F, 0.1F);
            var a = root.GetComponentInChildren<Animator>();
            a.transform.localScale = Vector3.one * 2;
            a.speed = 0.5F;
            r.materials[0].SetColor(Color1, clr);
            r.materials[0].SetColor(SpecColor, clr);
            r.materials[0].SetFloat(Fresnel, 0.5F);
            r.materials[0].SetFloat(Shininess, 0F);
            r.materials[0].SetFloat(SpecInt, 0.75F);
            r.materials[0].SetFloat(EmissionLm, 15F);
            r.materials[0].SetFloat(EmissionLmNight, 15F);
            r.materials[0].SetFloat(MyCullVariable, 1.6F);
            root.GetComponent<SphereCollider>().radius = 0.7F;
        }

        private void Update() {
            var time = DayNightCycle.main.timePassedAsFloat;
            if (!(time - _lastParentageCheck >= 1)) return;
            _lastParentageCheck = time;
            if (!gameObject.FindAncestor<Creature>())
                gameObject.destroy(false);
        }
    }
}