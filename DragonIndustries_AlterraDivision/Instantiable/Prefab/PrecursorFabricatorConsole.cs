using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nautilus.Assets;
using Story;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

public class PrecursorFabricatorConsole : CustomPrefab {
    //public readonly CraftingIdentifier craftingSet;
    public readonly CraftTree.Type craftingSet;
    public readonly Color renderColor;
    internal readonly Dictionary<string, string> storyGoals = new();

    internal static readonly Dictionary<string, PrecursorFabricatorConsole> map = new();

    private static readonly List<PositionedPrefab> modelParts = [
        new PositionedPrefab("78009225-a9fa-4d21-9580-8719a3368373"),
        new PositionedPrefab("a0a9237e-dee3-4efa-81ff-fea3893a6eb7", new Vector3(0, 1, 0), Quaternion.Euler(0, 0, 90)),
        new PositionedPrefab(
            "a0a9237e-dee3-4efa-81ff-fea3893a6eb7",
            new Vector3(0, 1.5F, 0),
            Quaternion.Euler(0, 0, 270),
            new Vector3(0.1F, 1, 1)
        ),

        new PositionedPrefab(
            "6a01a336-fb46-469a-9f7d-1659e07d11d7",
            new Vector3(0, 1.35F, 0.1F),
            Quaternion.Euler(90, 0, 0),
            new Vector3(0.8F, 0.8F, 0.8F)
        ),

        new PositionedPrefab(
            "6a01a336-fb46-469a-9f7d-1659e07d11d7",
            new Vector3(0.1F, 1.35F, 0),
            Quaternion.Euler(90, 90, 0),
            new Vector3(0.8F, 0.8F, 0.8F)
        ),

        new PositionedPrefab(
            "6a01a336-fb46-469a-9f7d-1659e07d11d7",
            new Vector3(0, 1.35F, -0.1F),
            Quaternion.Euler(90, 180, 0),
            new Vector3(0.8F, 0.8F, 0.8F)
        ),

        new PositionedPrefab(
            "6a01a336-fb46-469a-9f7d-1659e07d11d7",
            new Vector3(-0.1F, 1.35F, 0),
            Quaternion.Euler(90, 270, 0),
            new Vector3(0.8F, 0.8F, 0.8F)
        ),

    ];

    [SetsRequiredMembers]
    public PrecursorFabricatorConsole( /*CraftingIdentifier rec*/ CraftTree.Type rec, string id, Color c) : base(
        "PrecursorFabricator_" + id,
        "",
        ""
    ) {
        craftingSet = rec;
        renderColor = c;
        AddOnRegister(() => { map[Info.ClassID] = this; });
        SetGameObject(GetGameObject);
    }

    public PrecursorFabricatorConsole addStoryGate(string s, string ifNotMet) {
        storyGoals[s] = ifNotMet;
        return this;
    }

    /*
    public abstract class CraftingIdentifier {

        public abstract string getID();

    }

    public class CraftTreeID : CraftingIdentifier {

        public readonly CraftTree.Type craftingTree;

        public CraftTreeID(CraftTree.Type c) {
            craftingTree = c;
        }

        public override string getID() {
            return craftingTree.ToString();
        }

    }

    public class RecipeID : CraftingIdentifier {

        public readonly TechType item;
        public readonly TechData recipe;
        public readonly string id;

        public RecipeID(TechType tt, TechData c, string id) {
            item = tt;
            recipe = c;
            this.id = id;
        }

        public override string getID() {
            return id;
        }

    }
        */
    public GameObject GetGameObject() {
        var world = ObjectUtil.createWorldObject("81cf2223-455d-4400-bac3-a5bcd02b3638");
        world.EnsureComponent<TechTag>().type = Info.TechType;
        world.EnsureComponent<PrefabIdentifier>().ClassId = Info.ClassID;
        world.removeComponent<StoryHandTarget>();
        //if (craftingSet is CraftTreeID) {
        var lgc = world.EnsureComponent<CrafterLogic>();
        var f = world.EnsureComponent<GhostCrafter>();
        f.closeDistance = 5;
        //f.craftTree = ((CraftTreeID)craftingSet).craftingTree;
        f.craftTree = craftingSet;
        f.logic = lgc;
        //}
        world.EnsureComponent<PrecursorFabricatorConsoleTag>();
        var fx = world.GetComponent<PrecursorComputerTerminal>().fx;
        foreach (var r in fx.GetComponentsInChildren<Renderer>()) {
            //r.materials[0].SetColor("_Color", new Color(0.8F, 0.25F, 1F));
            r.materials[0].SetColor("_Color", renderColor);
        }

        world.removeComponent<PrecursorComputerTerminal>(); //do AFTER apply to its renderers

        createModels(world);

        return world;
    }

    internal static GameObject createModels(GameObject world) {
        var mdl = new GameObject("models");
        mdl.transform.SetParent(world.transform);
        mdl.transform.localScale = Vector3.one * 0.75F;
        mdl.transform.localRotation = Quaternion.Euler(0, 45, 0);
        mdl.transform.localPosition = Vector3.zero;
        foreach (var pfb in modelParts) {
            var go = ObjectUtil.createWorldObject(pfb.prefabName);
            go.transform.SetParent(mdl.transform);
            go.transform.localScale = pfb.scale;
            go.transform.localRotation = pfb.rotation;
            go.transform.localPosition = pfb.position;
            go.removeComponent<Collider>();
            if (pfb.prefabName == "6a01a336-fb46-469a-9f7d-1659e07d11d7") {
                var t = go.getChildObject("Precursor_Lab_surgical_machine/Precursor_lab_surgical_machine_base")
                    .transform;
                t.localScale = Vector3.one * 0.99F; //zfight fix
                t.localPosition = new Vector3(0, 2.1F, 0);
                foreach (var r in go.GetComponentsInChildren<Renderer>()) {
                    RenderUtil.swapTextures(SNUtil.DiDLL, r, "Textures/PrecursorFabricatorArms");
                    //r.materials[0].SetFloat("_SpecInt", 5F);
                }
            } else if (pfb.prefabName == "a0a9237e-dee3-4efa-81ff-fea3893a6eb7") {
                foreach (var r in go.GetComponentsInChildren<Renderer>()) {
                    RenderUtil.swapTextures(SNUtil.DiDLL, r, "Textures/PrecursorFabricatorBase");
                    RenderUtil.setEmissivity(r, 1);
                    r.materials[0].SetFloat("_SpecInt", 2F);
                }
            }
        }

        world.getChildObject("Precursor_computer_terminal/Precursor_computer_terminal").SetActive(false);
        world.getChildObject("FX").transform.localPosition = new Vector3(0, -0.4F, -0.375F);
        return mdl;
    }

    private class PrecursorFabricatorConsoleTag : MonoBehaviour /*, IHandTarget {*/ {
        private PrecursorFabricatorConsole template;
        private GhostCrafter fab;

        private GameObject models;

        private void Update() {
            if (template == null)
                template = map[GetComponent<PrefabIdentifier>().ClassId];

            if (!fab /* && template.craftingSet is CraftTreeID*/)
                fab = gameObject.GetComponent<GhostCrafter>();
            if (fab && !fab.logic)
                fab.logic = GetComponent<CrafterLogic>();

            var flag = false;
            if (!models) {
                flag = true;
                models = gameObject.getChildObject("models");
            }

            if (!models) {
                flag = true;
                models = createModels(gameObject);
            }

            if (flag) {
                var sk0 = GetComponent<SkyApplier>();
                foreach (var sk2 in models.GetComponentsInChildren<SkyApplier>()) {
                    sk2.SetCustomSky(sk0.applySky);
                }
            }

            if (template != null && fab) {
                string barrier = null;
                foreach (var kvp in template.storyGoals) {
                    if (!StoryGoalManager.main.IsGoalComplete(kvp.Key)) {
                        barrier = kvp.Value;
                        break;
                    }
                } /*
                if (barrier == null && !RecipeUtil.areAnyRecipesOfTypeKnown(fab.craftTree)) {

                }*/

                var working = barrier == null;
                fab.handOverText = working ? "Craft" : barrier;

                float trash;
                if (!fab.powerRelay)
                    fab.powerRelay = gameObject.EnsureComponent<PowerRelay>();
                if (!fab.powerRelay.internalPowerSource)
                    fab.powerRelay.internalPowerSource = gameObject.EnsureComponent<PowerSource>();
                if (working) {
                    fab.closeDistance = 5F;
                    fab.powerRelay.AddEnergy(5, out trash);
                } else {
                    fab.closeDistance = 0.1F;
                    fab.powerRelay.ConsumeEnergy(9999, out trash);
                }
            }
        }
        /*
        public void OnHandHover(GUIHand hand) {
            if (fab) {
                fab.OnHandHover(hand);
            }
            else if (template.craftingSet is RecipeID) {
                TechData rec = ((RecipeID)template.craftingSet).recipe;
                TechType item = ((RecipeID)template.craftingSet).item;
                bool enough = true;
                foreach (IIngredient ii in rec.Ingredients) {
                    if (Inventory.main.GetPickupCount(ii.techType) < ii.amount) {
                        enough = false;
                        break;
                    }
                }
                if (enough) {
                    HandReticle.main.SetIcon(HandReticle.IconType.Interact, 1f);
                    HandReticle.main.SetInteractText(DIMod.locale.getEntry("PrecursorCraftPrompt").desc+" "+item.AsString(), false);
                    HandReticle.main.SetTargetDistance(8);
                }
                else {
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                    HandReticle.main.SetInteractText("PrecursorCraftNoIngredients");
                    HandReticle.main.SetTargetDistance(8);
                }
            }
        }

        public void OnHandClick(GUIHand hand) {
            if (fab) {
                fab.OnHandClick(hand);
            }
            else if (template.craftingSet is RecipeID) {
                tryCraft();
            }
        }

        private void tryCraft() {

        }
        */
    }
}