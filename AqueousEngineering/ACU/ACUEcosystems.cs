using System.Collections.Generic;
using System.Linq;
using Nautilus.Assets;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.AqueousEngineering;

public static class ACUEcosystems {
    internal static readonly float
        FOOD_SCALAR = 0.2F; //all food values and metabolism multiplied by this, to give granularity

    private static readonly Dictionary<TechType, AnimalFood> edibleFish = new();
    private static readonly Dictionary<string, PlantFood> ediblePlants = new();

    private static readonly Dictionary<TechType, ACUMetabolism> metabolisms = new() {
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
        addFood(
            new AnimalFood(
                TechType.Reginald,
                BiomeRegions.RedGrass,
                BiomeRegions.BloodKelp,
                BiomeRegions.LostRiver,
                BiomeRegions.GrandReef,
                BiomeRegions.Other
            )
        );
        addFood(
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
        addFood(new AnimalFood(TechType.HoleFish, BiomeRegions.Shallows));
        addFood(new AnimalFood(TechType.Oculus, BiomeRegions.Jellyshroom));
        addFood(new AnimalFood(TechType.GarryFish, BiomeRegions.Shallows, BiomeRegions.Other));
        addFood(
            new AnimalFood(
                TechType.Boomerang,
                BiomeRegions.Shallows,
                BiomeRegions.RedGrass,
                BiomeRegions.Koosh,
                BiomeRegions.GrandReef,
                BiomeRegions.Other
            )
        );
        addFood(
            new AnimalFood(
                TechType.Spadefish,
                BiomeRegions.RedGrass,
                BiomeRegions.GrandReef,
                BiomeRegions.Mushroom,
                BiomeRegions.Other
            )
        );
        addFood(
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
        addFood(new AnimalFood(TechType.Eyeye, BiomeRegions.Jellyshroom, BiomeRegions.GrandReef, BiomeRegions.Koosh));
        addFood(new AnimalFood(TechType.LavaEyeye, BiomeRegions.LavaZone));
        addFood(new AnimalFood(TechType.LavaBoomerang, BiomeRegions.LavaZone));
        addFood(
            new AnimalFood(
                TechType.Hoopfish,
                BiomeRegions.Kelp,
                BiomeRegions.Koosh,
                BiomeRegions.GrandReef,
                BiomeRegions.Other
            )
        );
        addFood(new AnimalFood(TechType.Spinefish, BiomeRegions.BloodKelp, BiomeRegions.LostRiver));
        addFood(new AnimalFood(TechType.Hoverfish, BiomeRegions.Kelp));
        addFood(new AnimalFood(TechType.Jumper, BiomeRegions.Shallows, BiomeRegions.Kelp, BiomeRegions.Other));

        addFood(new PlantFood(VanillaFlora.CREEPVINE, 0.15F, BiomeRegions.Kelp));
        addFood(new PlantFood(VanillaFlora.CREEPVINE_FERTILE, 0.25F, BiomeRegions.Kelp));
        addFood(new PlantFood(VanillaFlora.BLOOD_KELP, 0.25F, BiomeRegions.BloodKelp));
        addFood(new PlantFood(VanillaFlora.JELLYSHROOM_SMALL, 0.25F, BiomeRegions.Jellyshroom));
        addFood(new PlantFood(VanillaFlora.EYE_STALK, 0.15F, BiomeRegions.Other));
        addFood(new PlantFood(VanillaFlora.GABE_FEATHER, 0.15F, BiomeRegions.BloodKelp, BiomeRegions.Other));
        addFood(new PlantFood(VanillaFlora.GHOSTWEED, 0.25F, BiomeRegions.LostRiver));
        addFood(new PlantFood(VanillaFlora.HORNGRASS, 0.05F, BiomeRegions.Other));
        addFood(new PlantFood(VanillaFlora.KOOSH, 0.15F, BiomeRegions.Koosh));
        addFood(new PlantFood(VanillaFlora.MEMBRAIN, 0.3F, BiomeRegions.GrandReef));
        addFood(
            new PlantFood(
                VanillaFlora.PAPYRUS,
                0.15F,
                BiomeRegions.RedGrass,
                BiomeRegions.Jellyshroom,
                BiomeRegions.Other
            )
        );
        addFood(
            new PlantFood(
                VanillaFlora.VIOLET_BEAU,
                0.2F,
                BiomeRegions.Jellyshroom,
                BiomeRegions.RedGrass,
                BiomeRegions.Koosh,
                BiomeRegions.Other
            )
        );
        addFood(
            new PlantFood(
                VanillaFlora.CAVE_BUSH,
                0.05F,
                BiomeRegions.Koosh,
                BiomeRegions.Jellyshroom,
                BiomeRegions.Other
            )
        );
        addFood(new PlantFood(VanillaFlora.REGRESS, 0.2F, BiomeRegions.GrandReef, BiomeRegions.Other));
        addFood(
            new PlantFood(VanillaFlora.REDWORT, 0.15F, BiomeRegions.RedGrass, BiomeRegions.Koosh, BiomeRegions.Other)
        );
        addFood(new PlantFood(VanillaFlora.ROUGE_CRADLE, 0.05F, BiomeRegions.RedGrass, BiomeRegions.Other));
        addFood(new PlantFood(VanillaFlora.SEACROWN, 0.4F, BiomeRegions.Koosh, BiomeRegions.RedGrass));
        addFood(new PlantFood(VanillaFlora.SPOTTED_DOCKLEAF, 0.25F, BiomeRegions.Koosh, BiomeRegions.Other));
        addFood(new PlantFood(VanillaFlora.VEINED_NETTLE, 0.15F, BiomeRegions.Shallows));
        addFood(new PlantFood(VanillaFlora.WRITHING_WEED, 0.15F, BiomeRegions.Shallows, BiomeRegions.Mushroom));
        addFood(new PlantFood(VanillaFlora.BLUE_PALM, 0.25F, BiomeRegions.Shallows, BiomeRegions.Mushroom));
        addFood(new PlantFood(VanillaFlora.PYGMY_FAN, 0.33F, BiomeRegions.Mushroom));
        addFood(new PlantFood(VanillaFlora.TIGER, 0.5F, BiomeRegions.RedGrass));
        addFood(new PlantFood(VanillaFlora.DEEP_MUSHROOM, 0.1F, BiomeRegions.LostRiver, BiomeRegions.LavaZone));
    }

    public static void addPost() {
        var tt = SNUtil.getTechType("StellarThalassacean");
        if (tt != TechType.None)
            addPredatorType(tt, 6F, 1.5F, 0.3F, false, BiomeRegions.GrandReef, BiomeRegions.Koosh, BiomeRegions.Other);

        tt = SNUtil.getTechType("JasperThalassacean");
        if (tt != TechType.None)
            addPredatorType(tt, 6F, 1.5F, 0.3F, false, BiomeRegions.LostRiver);

        tt = SNUtil.getTechType("Twisteel");
        if (tt != TechType.None)
            addPredatorType(tt, 2F, 0.5F, 0.8F, true, BiomeRegions.BloodKelp, BiomeRegions.Koosh);

        tt = SNUtil.getTechType("JellySpinner");
        if (tt != TechType.None)
            addFood(new AnimalFood(tt, BiomeRegions.BloodKelp, BiomeRegions.LostRiver));

        tt = SNUtil.getTechType("TriangleFish");
        if (tt != TechType.None)
            addFood(new AnimalFood(tt, BiomeRegions.Shallows));

        tt = SNUtil.getTechType("Axetail");
        if (tt != TechType.None)
            addFood(new AnimalFood(tt, BiomeRegions.RedGrass));

        tt = SNUtil.getTechType("RibbonRay");
        if (tt != TechType.None)
            addFood(new AnimalFood(tt, BiomeRegions.Shallows, BiomeRegions.Mushroom));

        tt = SNUtil.getTechType("GrandGlider");
        if (tt != TechType.None) {
            addFood(new AnimalFood(tt, 2, BiomeRegions.GrandReef, BiomeRegions.Koosh, BiomeRegions.Other));
            addPredatorType(
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

        tt = SNUtil.getTechType("Filtorb");
        if (tt != TechType.None)
            addFood(
                new AnimalFood(
                    tt,
                    0.1F,
                    BiomeRegions.Shallows,
                    BiomeRegions.RedGrass,
                    BiomeRegions.GrandReef,
                    BiomeRegions.Other
                )
            );

        addClownPincher("EmeraldClownPincher", BiomeRegions.Kelp);
        addClownPincher("SapphireClownPincher", BiomeRegions.GrandReef);
        addClownPincher("RubyClownPincher", BiomeRegions.LavaZone);
        addClownPincher("AmberClownPincher", BiomeRegions.Other);
        addClownPincher("CitrineClownPincher", BiomeRegions.Other);

        tt = SNUtil.getTechType("GulperLeviathanBaby");
        if (tt != TechType.None)
            addPredatorType(tt, 5F, 4F, 0.2F, true, BiomeRegions.GrandReef);
        tt = SNUtil.getTechType("GulperLeviathan");
        if (tt != TechType.None)
            addPredatorType(tt, 8F, 8F, 0.2F, true, BiomeRegions.BloodKelp, BiomeRegions.GrandReef, BiomeRegions.Other);
    }

    private static void addClownPincher(string id, BiomeRegions.RegionType br) {
        var tt = SNUtil.getTechType(id);
        if (tt != TechType.None) {
            addFood(new AnimalFood(tt, br));
            addPredatorType(tt, 1.5F, 2F, 1.6F, false, br);
        }
    }

    public static void addPredatorType(
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
        metabolisms[tt] = am;
    }

    public static void addFood(Food f) {
        if (f is AnimalFood food) {
            edibleFish[food.item] = food;
        } else if (f is PlantFood plantFood) {
            foreach (var s in plantFood.classIDs)
                ediblePlants[s] = plantFood;
        }
    }

    public static ACUMetabolism getMetabolismForAnimal(TechType tt) {
        return metabolisms.ContainsKey(tt) ? metabolisms[tt] : null;
    }

    public static AnimalFood getAnimalFood(TechType tt) {
        return edibleFish.ContainsKey(tt) ? edibleFish[tt] : null;
    }

    public static PlantFood getPlantFood(string pfb) {
        return ediblePlants.ContainsKey(pfb) ? ediblePlants[pfb] : null;
    }

    public static List<PlantFood> getPlantsForBiome(BiomeRegions.RegionType r) {
        List<PlantFood> li = [];
        foreach (var f in ediblePlants.Values) {
            if (f.isRegion(r))
                li.Add(f);
        }

        return li;
    }

    public static List<AnimalFood> getSmallFishForBiome(BiomeRegions.RegionType r) {
        List<AnimalFood> li = [];
        foreach (var f in edibleFish.Values) {
            if (f.isRegion(r))
                li.Add(f);
        }

        return li;
    }

    public static List<TechType> getPredatorsForBiome(BiomeRegions.RegionType r) {
        List<TechType> li = [];
        foreach (var kvp in metabolisms) {
            if (kvp.Value.isRegion(r, false))
                li.Add(kvp.Key);
        }

        return li;
    }

    internal static Creature handleCreature(
        AcuCallbackSystem.AcuCallback acu,
        float dT,
        WaterParkCreature wp,
        TechType tt,
        List<WaterParkCreature> foodFish,
        PrefabIdentifier[] plants,
        bool acuRoom,
        HashSet<BiomeRegions.RegionType> possibleBiomes
    ) {
        if (edibleFish.ContainsKey(tt)) {
            if (tt == TechType.Peeper && wp.gameObject.GetComponent<Peeper>().isHero)
                acu.SparkleCount++;
            else if (tt == TechType.Cutefish)
                acu.CuddleCount++;
            else if (tt == TechType.Gasopod)
                acu.GasopodCount++;
            else //sparkle peepers and cuddlefish are always valid
                possibleBiomes.IntersectWith(edibleFish[tt].regionType);
            //if (possibleBiomes.Count <= 0)
            //	SNUtil.writeToChat("Biome list empty after "+tt+" > "+edibleFish[tt]);
            if (acu.NextIsDebug)
                SNUtil.writeToChat(tt + " > " + edibleFish[tt] + " > " + string.Join(",", possibleBiomes));
            foodFish.Add(wp);
            acu.HerbivoreCount++;
        } else if (metabolisms.ContainsKey(tt)) {
            var am = metabolisms[tt];
            if (am.isCarnivore)
                acu.CarnivoreCount += am.relativeValue;
            else
                acu.HerbivoreCount += am.relativeValue;
            List<BiomeRegions.RegionType> li = [..am.additionalRegions, am.primaryRegion];
            possibleBiomes.IntersectWith(li);
            if (acu.NextIsDebug)
                SNUtil.writeToChat(tt + " > " + am + " > " + string.Join(",", possibleBiomes));
            //if (possibleBiomes.Count <= 0)
            //	SNUtil.writeToChat("Biome list empty after "+tt+" > "+am);
            var c = wp.gameObject.GetComponentInChildren<Creature>();
            if (wp.isMature) {
                c.Hunger.Add(dT * am.metabolismPerSecond * FOOD_SCALAR);
                c.Hunger.Falloff = 0;
                if (c.Hunger.Value >= 0.5F) {
                    eat(acu, wp, c, am, plants, acuRoom);
                }
            }

            return c;
        }

        return null;
    }

    internal static HashSet<PlantFood> collectPlants(
        AcuCallbackSystem.AcuCallback acu,
        PrefabIdentifier[] plants,
        HashSet<BiomeRegions.RegionType> possibleBiomes
    ) {
        HashSet<PlantFood> set = [];
        foreach (var pi in plants) {
            if (pi) {
                if (ediblePlants.ContainsKey(pi.ClassId)) {
                    var pf = ediblePlants[pi.ClassId];
                    possibleBiomes.IntersectWith(pf.regionType);
                    //if (possibleBiomes.Count <= 0)
                    //	SNUtil.writeToChat("Biome list empty after "+vf+" > "+pf);
                    if (acu.NextIsDebug)
                        SNUtil.writeToChat(
                            pi + " > " + pf + " & " + string.Join(",", pf.regionType) + " > " +
                            string.Join(",", possibleBiomes)
                        );
                    set.Add(pf);
                    acu.PlantCount += getPlantValue(pi);
                }
            }
        }

        return set;
    }

    private static float getPlantValue(PrefabIdentifier pi) {
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

    private static void eat(
        AcuCallbackSystem.AcuCallback acu,
        WaterParkCreature wp,
        Creature c,
        ACUMetabolism am,
        PrefabIdentifier[] plants,
        bool acuRoom
    ) {
        if (tryEat(acu, c, am, plants, out var amt, out var eaten)) {
            onEaten(acu, wp, c, am, amt, eaten, acuRoom);
        }
    }

    private static bool tryEat(
        AcuCallbackSystem.AcuCallback acu,
        Creature c,
        ACUMetabolism am,
        PrefabIdentifier[] pia,
        out Food amt,
        out GameObject eaten
    ) {
        if (am.isCarnivore) {
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
                if (edibleFish.ContainsKey(tt)) {
                    eaten = pp.gameObject;
                    amt = edibleFish[tt];
                    //SNUtil.writeToChat(c+" ate a "+tt+" and got "+amt+", is now "+c.Hunger.Value);
                    return true;
                }
            }

            amt = null;
            eaten = null;
            return false;
        } else if (pia.Length > 0) {
            var idx = Random.Range(0, pia.Length);
            var tt = pia[idx];
            if (tt) {
                //SNUtil.writeToChat(tt+" > "+vf+" > "+ediblePlants.ContainsKey(vf));
                if (ediblePlants.ContainsKey(tt.ClassId)) {
                    amt = ediblePlants[tt.ClassId];
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

    private static void onEaten(
        AcuCallbackSystem.AcuCallback acu,
        WaterParkCreature wp,
        Creature c,
        ACUMetabolism am,
        Food amt,
        GameObject eaten,
        bool acuRoom
    ) {
        var food = amt.foodValue * FOOD_SCALAR * 2.5F;
        if (acuRoom)
            food *= 1.2F;
        if (amt.isRegion(am.primaryRegion)) {
            food *= 3;
        } else {
            foreach (var r in am.additionalRegions) {
                if (amt.isRegion(r)) {
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

        if (c.Hunger.Value >= food) {
            c.Happy.Add(1F);
            c.Hunger.Add(-food);
            var f = am.normalizedPoopChance * amt.foodValue * Mathf.Pow(wp.age, 2F);
            f *= AqueousEngineeringMod.config.getFloat(AEConfig.ConfigEntries.POO_RATE);
            if (acuRoom)
                f *= 1.5F;
            //SNUtil.writeToChat(c+" ate > "+f);
            amt.consume(c, acu, acu.Planter, eaten);
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
    }

    public abstract class Food {
        public readonly float foodValue;
        internal readonly HashSet<BiomeRegions.RegionType> regionType = [];

        internal Food(float f, params BiomeRegions.RegionType[] r) {
            foodValue = f;
            regionType.AddRange(r.ToList());
        }

        public bool isRegion(BiomeRegions.RegionType r) {
            return regionType.Contains(r);
        }

        public override string ToString() {
            return $"[Food FoodValue={foodValue}, BiomeRegions.RegionType=[{string.Join(",", regionType)}]]";
        }

        public void addBiome(BiomeRegions.RegionType r) {
            regionType.Add(r);
        }

        internal abstract void consume(
            Creature c,
            AcuCallbackSystem.AcuCallback acu,
            StorageContainer sc,
            GameObject go
        );
    }

    public class AnimalFood : Food {
        internal readonly TechType item;

        public AnimalFood(CustomPrefab s, params BiomeRegions.RegionType[] r) : this(s.TechType, r) {
        }

        internal AnimalFood(TechType tt, params BiomeRegions.RegionType[] r) : base(calculateFoodValue(tt), r) {
            item = tt;
        }

        internal AnimalFood(TechType tt, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
            item = tt;
        }

        public AnimalFood(CustomPrefab s, float f, params BiomeRegions.RegionType[] r) : base(f, r) {
            item = s.TechType;
        }

        public static float calculateFoodValue(TechType tt) {
            GameObject go = ObjectUtil.lookupPrefab(SNUtil.getTechType("Cooked" + tt)).GetResult();
            if (!go)
                go = ObjectUtil.lookupPrefab(SNUtil.getTechType(tt + "Cooked")).GetResult();
            var ea = go ? go.GetComponent<Eatable>() : null;
            return ea ? ea.foodValue * 0.01F : 0.2F; //so a reginald is ~40%
        }

        internal override void consume(
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
        internal readonly HashSet<string> classIDs = [];

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
            classIDs.AddRange(ids);
        }

        internal override void consume(
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
        public readonly float relativeValue;
        public readonly bool isCarnivore;
        public readonly float metabolismPerSecond;
        public readonly float normalizedPoopChance;
        public readonly BiomeRegions.RegionType primaryRegion;
        internal readonly HashSet<BiomeRegions.RegionType> additionalRegions = [];

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
            relativeValue = v;
            normalizedPoopChance = pp * 6;
            metabolismPerSecond = mf * 0.0003F;
            isCarnivore = isc;
            primaryRegion = r;
            additionalRegions.AddRange(rr);
        }

        public bool isRegion(BiomeRegions.RegionType r, bool primaryOnly) {
            return r == primaryRegion || (!primaryOnly && additionalRegions.Contains(r));
        }

        public void addBiome(BiomeRegions.RegionType r) {
            additionalRegions.Add(r);
        }

        public override string ToString() {
            return
                $"[ACUMetabolism IsCarnivore={isCarnivore}, MetabolismPerSecond={metabolismPerSecond.ToString("0.0000")}, NormalizedPoopChance={normalizedPoopChance}, PrimaryRegion={primaryRegion}, AdditionalRegions=[{string.Join(",", additionalRegions)}]]]";
        }
    }
}