using System.Collections.Generic;
using System.Linq;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public static class ACUEcosystems {
    internal static readonly float
        FoodScalar = 0.2F; //all food values and metabolism multiplied by this, to give granularity

    private static readonly Dictionary<TechType, AnimalFood> EdibleFish = new();
    private static readonly Dictionary<string, PlantFood> EdiblePlants = new();

    private static readonly Dictionary<TechType, ACUMetabolism> Metabolisms = new() {
        { TechType.RabbitRay, new ACUMetabolism(2F, 0.2F, 0.2F, false, BiomeRegions.Shallows) },
        { TechType.Crash, new ACUMetabolism(1.0F, 0.1F, 0.8F, true, BiomeRegions.Shallows) },
        { TechType.Biter, new ACUMetabolism(0.5F, 0.2F, 0.4F, true, BiomeRegions.RedGrass, BiomeRegions.Other) },
        { TechType.Blighter, new ACUMetabolism(0.33F, 0.1F, 0.2F, true, BiomeRegions.BloodKelp) },
        { TechType.Gasopod, new ACUMetabolism(4F, 1F, 0.8F, false, BiomeRegions.Shallows, BiomeRegions.Other) },
        { TechType.Jellyray, new ACUMetabolism(2.5F, 0.8F, 0.6F, false, BiomeRegions.Mushroom) },
        { TechType.Stalker, new ACUMetabolism(0.75F, 1F, 1F, true, BiomeRegions.Kelp) },
        { TechType.Sandshark, new ACUMetabolism(0.67F, 0.6F, 1.2F, true, BiomeRegions.RedGrass) }, {
            TechType.BoneShark,
            new ACUMetabolism(1.5F, 0.6F, 1.6F, true, BiomeRegions.Koosh, BiomeRegions.Mushroom, BiomeRegions.Other)
        },
        { TechType.Shocker, new ACUMetabolism(1F, 2F, 1F, true, BiomeRegions.Koosh, BiomeRegions.BloodKelp) },
        { TechType.Crabsnake, new ACUMetabolism(1.25F, 1.6F, 2F, true, BiomeRegions.Jellyshroom) }, {
            TechType.CrabSquid,
            new ACUMetabolism(
                1.5F,
                3F,
                2F,
                true,
                BiomeRegions.BloodKelp,
                BiomeRegions.LostRiver,
                BiomeRegions.GrandReef
            )
        },
        { TechType.LavaLizard, new ACUMetabolism(1F, 1F, 1F, true, BiomeRegions.LavaZone) },
        { TechType.SpineEel, new ACUMetabolism(0.75F, 0.6F, 3F, true, BiomeRegions.LostRiver) },
        { TechType.GhostRayBlue, new ACUMetabolism(3F, 0.67F, 0.6F, false, BiomeRegions.LostRiver) },
        { TechType.GhostRayRed, new ACUMetabolism(3F, 1.25F, 0.6F, false, BiomeRegions.LavaZone) },
        { TechType.Mesmer, new ACUMetabolism(0.5F, 0.1F, 0.7F, true, BiomeRegions.Koosh, BiomeRegions.LostRiver) },
    };

    static ACUEcosystems() {
        AddFood(
            new AnimalFood(
                TechType.Reginald,
                BiomeRegions.RedGrass,
                BiomeRegions.BloodKelp,
                BiomeRegions.LostRiver,
                BiomeRegions.GrandReef,
                BiomeRegions.Other
            )
        );
        AddFood(
            new AnimalFood(
                TechType.Peeper,
                BiomeRegions.Shallows,
                BiomeRegions.RedGrass,
                BiomeRegions.Mushroom,
                BiomeRegions.GrandReef,
                BiomeRegions.Koosh,
                BiomeRegions.Other
            )
        );
        AddFood(new AnimalFood(TechType.HoleFish, BiomeRegions.Shallows));
        AddFood(new AnimalFood(TechType.Oculus, BiomeRegions.Jellyshroom));
        AddFood(new AnimalFood(TechType.GarryFish, BiomeRegions.Shallows, BiomeRegions.Other));
        AddFood(
            new AnimalFood(
                TechType.Boomerang,
                BiomeRegions.Shallows,
                BiomeRegions.RedGrass,
                BiomeRegions.Koosh,
                BiomeRegions.GrandReef,
                BiomeRegions.Other
            )
        );
        AddFood(
            new AnimalFood(
                TechType.Spadefish,
                BiomeRegions.RedGrass,
                BiomeRegions.GrandReef,
                BiomeRegions.Mushroom,
                BiomeRegions.Other
            )
        );
        AddFood(
            new AnimalFood(
                TechType.Bladderfish,
                BiomeRegions.Shallows,
                BiomeRegions.RedGrass,
                BiomeRegions.Mushroom,
                BiomeRegions.GrandReef,
                BiomeRegions.LostRiver,
                BiomeRegions.Other
            )
        );
        AddFood(new AnimalFood(TechType.Eyeye, BiomeRegions.Jellyshroom, BiomeRegions.GrandReef, BiomeRegions.Koosh));
        AddFood(new AnimalFood(TechType.LavaEyeye, BiomeRegions.LavaZone));
        AddFood(new AnimalFood(TechType.LavaBoomerang, BiomeRegions.LavaZone));
        AddFood(
            new AnimalFood(
                TechType.Hoopfish,
                BiomeRegions.Kelp,
                BiomeRegions.Koosh,
                BiomeRegions.GrandReef,
                BiomeRegions.Other
            )
        );
        AddFood(new AnimalFood(TechType.Spinefish, BiomeRegions.BloodKelp, BiomeRegions.LostRiver));
        AddFood(new AnimalFood(TechType.Hoverfish, BiomeRegions.Kelp));
        AddFood(new AnimalFood(TechType.Jumper, BiomeRegions.Shallows, BiomeRegions.Kelp, BiomeRegions.Other));

        AddFood(new PlantFood(VanillaFlora.CREEPVINE, 0.15F, BiomeRegions.Kelp));
        AddFood(new PlantFood(VanillaFlora.CREEPVINE_FERTILE, 0.25F, BiomeRegions.Kelp));
        AddFood(new PlantFood(VanillaFlora.BLOOD_KELP, 0.25F, BiomeRegions.BloodKelp));
        AddFood(new PlantFood(VanillaFlora.JELLYSHROOM_SMALL, 0.25F, BiomeRegions.Jellyshroom));
        AddFood(new PlantFood(VanillaFlora.EYE_STALK, 0.15F, BiomeRegions.Other));
        AddFood(new PlantFood(VanillaFlora.GABE_FEATHER, 0.15F, BiomeRegions.BloodKelp, BiomeRegions.Other));
        AddFood(new PlantFood(VanillaFlora.GHOSTWEED, 0.25F, BiomeRegions.LostRiver));
        AddFood(new PlantFood(VanillaFlora.HORNGRASS, 0.05F, BiomeRegions.Other));
        AddFood(new PlantFood(VanillaFlora.KOOSH, 0.15F, BiomeRegions.Koosh));
        AddFood(new PlantFood(VanillaFlora.MEMBRAIN, 0.3F, BiomeRegions.GrandReef));
        AddFood(
            new PlantFood(
                VanillaFlora.PAPYRUS,
                0.15F,
                BiomeRegions.RedGrass,
                BiomeRegions.Jellyshroom,
                BiomeRegions.Other
            )
        );
        AddFood(
            new PlantFood(
                VanillaFlora.VIOLET_BEAU,
                0.2F,
                BiomeRegions.Jellyshroom,
                BiomeRegions.RedGrass,
                BiomeRegions.Koosh,
                BiomeRegions.Other
            )
        );
        AddFood(
            new PlantFood(
                VanillaFlora.CAVE_BUSH,
                0.05F,
                BiomeRegions.Koosh,
                BiomeRegions.Jellyshroom,
                BiomeRegions.Other
            )
        );
        AddFood(new PlantFood(VanillaFlora.REGRESS, 0.2F, BiomeRegions.GrandReef, BiomeRegions.Other));
        AddFood(
            new PlantFood(VanillaFlora.REDWORT, 0.15F, BiomeRegions.RedGrass, BiomeRegions.Koosh, BiomeRegions.Other)
        );
        AddFood(new PlantFood(VanillaFlora.ROUGE_CRADLE, 0.05F, BiomeRegions.RedGrass, BiomeRegions.Other));
        AddFood(new PlantFood(VanillaFlora.SEACROWN, 0.4F, BiomeRegions.Koosh, BiomeRegions.RedGrass));
        AddFood(new PlantFood(VanillaFlora.SPOTTED_DOCKLEAF, 0.25F, BiomeRegions.Koosh, BiomeRegions.Other));
        AddFood(new PlantFood(VanillaFlora.VEINED_NETTLE, 0.15F, BiomeRegions.Shallows));
        AddFood(new PlantFood(VanillaFlora.WRITHING_WEED, 0.15F, BiomeRegions.Shallows, BiomeRegions.Mushroom));
        AddFood(new PlantFood(VanillaFlora.BLUE_PALM, 0.25F, BiomeRegions.Shallows, BiomeRegions.Mushroom));
        AddFood(new PlantFood(VanillaFlora.PYGMY_FAN, 0.33F, BiomeRegions.Mushroom));
        AddFood(new PlantFood(VanillaFlora.TIGER, 0.5F, BiomeRegions.RedGrass));
        AddFood(new PlantFood(VanillaFlora.DEEP_MUSHROOM, 0.1F, BiomeRegions.LostRiver, BiomeRegions.LavaZone));
    }

    public static void AddPost() {
        var tt = SNUtil.GetTechType("StellarThalassacean");
        if (tt != TechType.None)
            AddPredatorType(tt, 6F, 1.5F, 0.3F, false, BiomeRegions.GrandReef, BiomeRegions.Koosh, BiomeRegions.Other);

        tt = SNUtil.GetTechType("JasperThalassacean");
        if (tt != TechType.None)
            AddPredatorType(tt, 6F, 1.5F, 0.3F, false, BiomeRegions.LostRiver);

        tt = SNUtil.GetTechType("Twisteel");
        if (tt != TechType.None)
            AddPredatorType(tt, 2F, 0.5F, 0.8F, true, BiomeRegions.BloodKelp, BiomeRegions.Koosh);

        tt = SNUtil.GetTechType("JellySpinner");
        if (tt != TechType.None)
            AddFood(new AnimalFood(tt, BiomeRegions.BloodKelp, BiomeRegions.LostRiver));

        tt = SNUtil.GetTechType("TriangleFish");
        if (tt != TechType.None)
            AddFood(new AnimalFood(tt, BiomeRegions.Shallows));

        tt = SNUtil.GetTechType("Axetail");
        if (tt != TechType.None)
            AddFood(new AnimalFood(tt, BiomeRegions.RedGrass));

        tt = SNUtil.GetTechType("RibbonRay");
        if (tt != TechType.None)
            AddFood(new AnimalFood(tt, BiomeRegions.Shallows, BiomeRegions.Mushroom));

        tt = SNUtil.GetTechType("GrandGlider");
        if (tt != TechType.None) {
            AddFood(new AnimalFood(tt, 2, BiomeRegions.GrandReef, BiomeRegions.Koosh, BiomeRegions.Other));
            AddPredatorType(
                tt,
                3.0F,
                0.8F,
                0.75F,
                false,
                BiomeRegions.GrandReef,
                BiomeRegions.Koosh,
                BiomeRegions.Other
            );
        }

        tt = SNUtil.GetTechType("Filtorb");
        if (tt != TechType.None)
            AddFood(
                new AnimalFood(
                    tt,
                    0.1F,
                    BiomeRegions.Shallows,
                    BiomeRegions.RedGrass,
                    BiomeRegions.GrandReef,
                    BiomeRegions.Other
                )
            );

        AddClownPincher("EmeraldClownPincher", BiomeRegions.Kelp);
        AddClownPincher("SapphireClownPincher", BiomeRegions.GrandReef);
        AddClownPincher("RubyClownPincher", BiomeRegions.LavaZone);
        AddClownPincher("AmberClownPincher", BiomeRegions.Other);
        AddClownPincher("CitrineClownPincher", BiomeRegions.Other);

        tt = SNUtil.GetTechType("GulperLeviathanBaby");
        if (tt != TechType.None)
            AddPredatorType(tt, 5F, 4F, 0.2F, true, BiomeRegions.GrandReef);
        tt = SNUtil.GetTechType("GulperLeviathan");
        if (tt != TechType.None)
            AddPredatorType(tt, 8F, 8F, 0.2F, true, BiomeRegions.BloodKelp, BiomeRegions.GrandReef, BiomeRegions.Other);
    }

    private static void AddClownPincher(string id, BiomeRegions.RegionType br) {
        var tt = SNUtil.GetTechType(id);
        if (tt == TechType.None) return;
        AddFood(new AnimalFood(tt, br));
        AddPredatorType(tt, 1.5F, 2F, 1.6F, false, br);
    }

    public static void AddPredatorType(
        TechType tt,
        float relativeValue,
        float metaRate,
        float pooChance,
        bool carn,
        params BiomeRegions.RegionType[] rr
    ) {
        var li = rr.ToList();
        li.RemoveAt(0);
        var am = new ACUMetabolism(relativeValue, metaRate, pooChance, carn, rr[0], li);
        Metabolisms[tt] = am;
    }

    public static void AddFood(Food f) {
        if (f is AnimalFood food) {
            EdibleFish[food.Item] = food;
        } else if (f is PlantFood plantFood) {
            foreach (var s in plantFood.ClassIDs)
                EdiblePlants[s] = plantFood;
        }
    }

    public static ACUMetabolism GetMetabolismForAnimal(TechType tt) {
        return Metabolisms.ContainsKey(tt) ? Metabolisms[tt] : null;
    }

    public static AnimalFood GetAnimalFood(TechType tt) {
        return EdibleFish.ContainsKey(tt) ? EdibleFish[tt] : null;
    }

    public static PlantFood GetPlantFood(string pfb) {
        return EdiblePlants.ContainsKey(pfb) ? EdiblePlants[pfb] : null;
    }

    public static List<PlantFood> GetPlantsForBiome(BiomeRegions.RegionType r) {
        List<PlantFood> li = [];
        foreach (var f in EdiblePlants.Values) {
            if (f.IsRegion(r))
                li.Add(f);
        }

        return li;
    }

    public static List<AnimalFood> GetSmallFishForBiome(BiomeRegions.RegionType r) {
        List<AnimalFood> li = [];
        foreach (var f in EdibleFish.Values) {
            if (f.IsRegion(r))
                li.Add(f);
        }

        return li;
    }

    public static List<TechType> GetPredatorsForBiome(BiomeRegions.RegionType r) {
        List<TechType> li = [];
        foreach (var kvp in Metabolisms) {
            if (kvp.Value.IsRegion(r, false))
                li.Add(kvp.Key);
        }

        return li;
    }

    internal static Creature HandleCreature(
        AcuCallbackSystem.AcuCallback acu,
        float dT,
        WaterParkCreature wp,
        TechType tt,
        List<WaterParkCreature> foodFish,
        PrefabIdentifier[] plants,
        bool acuRoom,
        HashSet<BiomeRegions.RegionType> possibleBiomes
    ) {
        if (EdibleFish.ContainsKey(tt)) {
            if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero)
                acu.SparkleCount++;
            else if (tt == TechType.Cutefish)
                acu.CuddleCount++;
            else if (tt == TechType.Gasopod)
                acu.GasopodCount++;
            else //sparkle peepers and cuddlefish are always valid
                possibleBiomes.IntersectWith(EdibleFish[tt].RegionType);
            //if (possibleBiomes.Count <= 0)
            //	SNUtil.writeToChat("Biome list empty after "+tt+" > "+edibleFish[tt]);
            if (acu.NextIsDebug)
                SNUtil.WriteToChat(tt + " > " + EdibleFish[tt] + " > " + string.Join(",", possibleBiomes));
            foodFish.Add(wp);
            acu.HerbivoreCount++;
        } else if (Metabolisms.ContainsKey(tt)) {
            var am = Metabolisms[tt];
            if (am.IsCarnivore)
                acu.CarnivoreCount += am.RelativeValue;
            else
                acu.HerbivoreCount += am.RelativeValue;
            List<BiomeRegions.RegionType> li = [..am.AdditionalRegions, am.PrimaryRegion];
            possibleBiomes.IntersectWith(li);
            if (acu.NextIsDebug)
                SNUtil.WriteToChat(tt + " > " + am + " > " + string.Join(",", possibleBiomes));
            //if (possibleBiomes.Count <= 0)
            //	SNUtil.writeToChat("Biome list empty after "+tt+" > "+am);
            var c = wp.gameObject.GetComponentInChildren<Creature>();
            if (wp.isMature) {
                c.Hunger.Add(dT * am.MetabolismPerSecond * FoodScalar);
                c.Hunger.Falloff = 0;
                if (c.Hunger.Value >= 0.5F) {
                    Eat(acu, wp, c, am, plants, acuRoom);
                }
            }

            return c;
        }

        return null;
    }

    internal static HashSet<PlantFood> CollectPlants(
        AcuCallbackSystem.AcuCallback acu,
        PrefabIdentifier[] plants,
        HashSet<BiomeRegions.RegionType> possibleBiomes
    ) {
        HashSet<PlantFood> set = [];
        foreach (var pi in plants) {
            if (pi) {
                if (EdiblePlants.ContainsKey(pi.ClassId)) {
                    var pf = EdiblePlants[pi.ClassId];
                    possibleBiomes.IntersectWith(pf.RegionType);
                    //if (possibleBiomes.Count <= 0)
                    //	SNUtil.writeToChat("Biome list empty after "+vf+" > "+pf);
                    if (acu.NextIsDebug)
                        SNUtil.WriteToChat(
                            pi + " > " + pf + " & " + string.Join(",", pf.RegionType) + " > " +
                            string.Join(",", possibleBiomes)
                        );
                    set.Add(pf);
                    acu.PlantCount += GetPlantValue(pi);
                }
            }
        }

        return set;
    }

    private static float GetPlantValue(PrefabIdentifier pi) {
        if (VanillaFlora.WRITHING_WEED.includes(pi.ClassId) || VanillaFlora.GELSACK.includes(pi.ClassId))
            return 0.5F;
        if (VanillaFlora.ACID_MUSHROOM.includes(pi.ClassId) || VanillaFlora.DEEP_MUSHROOM.includes(pi.ClassId))
            return 0.33F;
        if (VanillaFlora.BLOOD_KELP.includes(pi.ClassId) || VanillaFlora.CREEPVINE.includes(pi.ClassId) ||
            VanillaFlora.CREEPVINE_FERTILE.includes(pi.ClassId))
            return 2.5F;
        var bp = BasicCustomPlant.getPlant(pi.ClassId);
        return bp != null ? bp.getSize() == Plantable.PlantSize.Large ? 1 : 0.5F : 1;
    }

    private static void Eat(
        AcuCallbackSystem.AcuCallback acu,
        WaterParkCreature wp,
        Creature c,
        ACUMetabolism am,
        PrefabIdentifier[] plants,
        bool acuRoom
    ) {
        if (TryEat(acu, c, am, plants, out var amt, out var eaten)) {
            OnEaten(acu, wp, c, am, amt, eaten, acuRoom);
        }
    }

    private static bool TryEat(
        AcuCallbackSystem.AcuCallback acu,
        Creature c,
        ACUMetabolism am,
        PrefabIdentifier[] pia,
        out Food amt,
        out GameObject eaten
    ) {
        if (am.IsCarnivore) {
            var wp = acu.Acu.items[Random.Range(0, acu.Acu.items.Count)];
            if (wp) {
                var pp = wp.gameObject.GetComponentInChildren<Pickupable>();
                var tt = pp ? pp.GetTechType() : TechType.None;
                if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero) {
                    //do not allow eating sparkle peepers
                    amt = null;
                    eaten = null;
                    return false;
                }

                //SNUtil.writeToChat(pp+" > "+tt+" > "+edibleFish.ContainsKey(tt));
                if (EdibleFish.ContainsKey(tt)) {
                    eaten = pp.gameObject;
                    amt = EdibleFish[tt];
                    //SNUtil.writeToChat(c+" ate a "+tt+" and got "+amt+", is now "+c.Hunger.Value);
                    return true;
                }
            }
        } else if (pia.Length > 0) {
            var idx = Random.Range(0, pia.Length);
            var tt = pia[idx];
            if (tt) {
                //SNUtil.writeToChat(tt+" > "+vf+" > "+ediblePlants.ContainsKey(vf));
                if (EdiblePlants.TryGetValue(tt.ClassId, out var plant)) {
                    amt = plant;
                    //SNUtil.writeToChat(c+" ate a "+vf+" and got "+amt);
                    eaten = tt.gameObject;
                    return true;
                }
            }
        }

        amt = null;
        eaten = null;
        return false;
    }

    private static void OnEaten(
        AcuCallbackSystem.AcuCallback acu,
        WaterParkCreature wp,
        Creature c,
        ACUMetabolism am,
        Food amt,
        GameObject eaten,
        bool acuRoom
    ) {
        var food = amt.FoodValue * FoodScalar * 2.5F;
        if (acuRoom)
            food *= 1.2F;
        if (amt.IsRegion(am.PrimaryRegion)) {
            food *= 3;
        } else {
            foreach (var r in am.AdditionalRegions) {
                if (amt.IsRegion(r)) {
                    food *= 2;
                    break;
                }
            }
        }

        var inf = eaten ? eaten.GetComponent<InfectedMixin>() : null;
        if (inf && inf.IsInfected()) {
            food *= 0.4F;
            c.gameObject.EnsureComponent<InfectedMixin>().IncreaseInfectedAmount(0.2F);
        }

        if (!(c.Hunger.Value >= food)) return;
        c.Happy.Add(1F);
        c.Hunger.Add(-food);
        var f = am.NormalizedPoopChance * amt.FoodValue * Mathf.Pow(wp.age, 2F);
        f *= AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.POO_RATE);
        if (acuRoom)
            f *= 1.5F;
        //SNUtil.writeToChat(c+" ate > "+f);
        amt.Consume(c, acu, acu.Planter, eaten);
        if (f > 0 && Random.Range(0F, 1F) < f) {
            var poo = ObjectUtil.createWorldObject(AqueousEngineeringMod.poo.Info.ClassID);
            poo.transform.position = c.transform.position + Vector3.down * 0.05F;
            poo.transform.rotation = Random.rotationUniform;
            acu.Acu.AddItem(poo.GetComponent<Pickupable>());
            //SNUtil.writeToChat("Poo spawned");
        }

        var cache = acu.GetOrCreateCreatureStatus(c);
        if (cache != null) {
            cache.Hunger = c.Hunger.Value;
            cache.Happy = c.Happy.Value;
        }
    }

    public abstract class Food {
        public readonly float FoodValue;
        internal readonly HashSet<BiomeRegions.RegionType> RegionType = [];

        internal Food(float f, params BiomeRegions.RegionType[] r) {
            FoodValue = f;
            RegionType.AddRange(r.ToList());
        }

        public bool IsRegion(BiomeRegions.RegionType r) {
            return RegionType.Contains(r);
        }

        public override string ToString() {
            return $"[Food FoodValue={FoodValue}, BiomeRegions.RegionType=[{string.Join(",", RegionType)}]]";
        }

        public void AddBiome(BiomeRegions.RegionType r) {
            RegionType.Add(r);
        }

        internal abstract void Consume(
            Creature c,
            AcuCallbackSystem.AcuCallback acu,
            StorageContainer sc,
            GameObject go
        );
    }

    public class AnimalFood : Food {
        internal readonly TechType Item;

        public AnimalFood(CustomPrefab s, params BiomeRegions.RegionType[] r) : this(s.TechType, r) {
        }

        internal AnimalFood(TechType tt, params BiomeRegions.RegionType[] r) : base(CalculateFoodValue(tt), r) {
            Item = tt;
        }

        internal AnimalFood(TechType tt, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
            Item = tt;
        }

        public AnimalFood(CustomPrefab s, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
            Item = s.TechType;
        }

        public static float CalculateFoodValue(TechType tt) {
            var go = ObjectUtil.lookupPrefab(SNUtil.GetTechType("Cooked" + tt));
            if (!go)
                go = ObjectUtil.lookupPrefab(SNUtil.GetTechType(tt + "Cooked"));
            var ea = go ? go.GetComponent<Eatable>() : null;
            return ea ? ea.foodValue * 0.01F : 0.2F; //so a reginald is ~40%
        }

        internal override void Consume(
            Creature c,
            AcuCallbackSystem.AcuCallback acu,
            StorageContainer sc,
            GameObject go
        ) {
            acu.Acu.RemoveItem(go.GetComponent<WaterParkCreature>());
            go.destroy();
        }
    }

    public class PlantFood : Food {
        internal readonly HashSet<string> ClassIDs = [];

        internal PlantFood(VanillaFlora vf, float f, params BiomeRegions.RegionType[] r) : this(
            vf.getPrefabs(true, true),
            f,
            r
        ) {
        }

        public PlantFood(CustomPrefab sp, float f, params BiomeRegions.RegionType[] r) : this(
            new List<string> { sp.ClassID },
            f,
            r
        ) {
        }

        internal PlantFood(IEnumerable<string> ids, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
            ClassIDs.AddRange(ids);
        }

        internal override void Consume(
            Creature c,
            AcuCallbackSystem.AcuCallback acu,
            StorageContainer sc,
            GameObject go
        ) {
            if (Random.Range(0F, 1F) <= acu.GetBoostStrength(DayNightCycle.main.timePassedAsFloat))
                return;
            var lv = go.GetComponent<LiveMixin>();
            if (lv && lv.IsAlive())
                lv.TakeDamage(10, c.transform.position, DamageType.Normal, c.gameObject);
            else
                sc.container.DestroyItem(CraftData.GetTechType(go));
        }
    }

    public class ACUMetabolism {
        public readonly float RelativeValue;
        public readonly bool IsCarnivore;
        public readonly float MetabolismPerSecond;
        public readonly float NormalizedPoopChance;
        public readonly BiomeRegions.RegionType PrimaryRegion;
        internal readonly HashSet<BiomeRegions.RegionType> AdditionalRegions = [];

        internal ACUMetabolism(
            float v,
            float mf,
            float pp,
            bool isc,
            BiomeRegions.RegionType r,
            params BiomeRegions.RegionType[] rr
        ) : this(v, mf, pp, isc, r, rr.ToList()) {
        }

        internal ACUMetabolism(
            float v,
            float mf,
            float pp,
            bool isc,
            BiomeRegions.RegionType r,
            List<BiomeRegions.RegionType> rr
        ) {
            RelativeValue = v;
            NormalizedPoopChance = pp * 6;
            MetabolismPerSecond = mf * 0.0003F;
            IsCarnivore = isc;
            PrimaryRegion = r;
            AdditionalRegions.AddRange(rr);
        }

        public bool IsRegion(BiomeRegions.RegionType r, bool primaryOnly) {
            return r == PrimaryRegion || (!primaryOnly && AdditionalRegions.Contains(r));
        }

        public void AddBiome(BiomeRegions.RegionType r) {
            AdditionalRegions.Add(r);
        }

        public override string ToString() {
            return
                $"[ACUMetabolism IsCarnivore={IsCarnivore}, MetabolismPerSecond={MetabolismPerSecond.ToString("0.0000")}, NormalizedPoopChance={NormalizedPoopChance}, PrimaryRegion={PrimaryRegion}, AdditionalRegions=[{string.Join(",", AdditionalRegions)}]]]";
        }
    }
}