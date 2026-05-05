// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Reflection;
// using System.Reflection.Emit;
// using HarmonyLib;
// using Nautilus.Handlers;
// using ReikaKalseki.DIAlterra;
// using ReikaKalseki.Ecocean;
// using Story;
// using UnityEngine;
//
// namespace ReikaKalseki.SeaToSea;
//
// public class FCSIntegrationSystem {
//     public static readonly FCSIntegrationSystem instance = new();
//
//     private bool isFCSLoaded;
//
//     private Type drillController;
//     private Type drillOreManager;
//     private Type drillStorage;
//
//     private Type teleporterController;
//     private Type teleporterCharger;
//
//     private TechType fcsBiofuel;
//     private DuplicateRecipeDelegateWithRecipe fcsBiofuelAlt;
//
//     private TechType fcsTeleportCard;
//     private TechType vehiclePad;
//
//     private readonly HashSet<TechType> peeperBarFoods = [];
//     private readonly HashSet<TechType> registeredEffectFoods = [];
//     private readonly HashSet<TechType> notBuyableTechs = [];
//     private readonly HashSet<TechType> replacedTechRecipes = [];
//
//     internal BasicCraftingItem fcsDrillFuel;
//     internal BasicCraftingItem luminolDrop;
//     internal BasicCraftingItem paint;
//
//     private float lastTimeUnlock = -1;
//     private HashSet<TechType> unlocksRightNow = [];
//
//
//     private FCSIntegrationSystem() {
//         init();
//     }
//
//     private void init() {
//         // TODO
//         // isFCSLoaded = QModManager.API.QModServices.Main.ModPresent("FCSAlterraHub");
//         isFCSLoaded = false;
//         if (isFCSLoaded) {
//         }
//     }
//
//     public bool isLoaded() {
//         return isFCSLoaded;
//     }
//
//     public Type getFCSDrillController() {
//         return drillController;
//     }
//
//     public Type getFCSDrillOreManager() {
//         return drillOreManager;
//     }
//
//     public Type getFCSDrillStorage() {
//         return drillStorage;
//     }
//
//     public Type getTeleporterController() {
//         return teleporterController;
//     }
//
//     public Type getTeleporterCharger() {
//         return teleporterCharger;
//     }
//
//     public bool isUnlockingTypePurchase(TechType tt) {
//         return replacedTechRecipes.Contains(tt) && !notBuyableTechs.Contains(tt);
//     }
//
//     public TechType getBiofuel() {
//         return fcsBiofuel;
//     }
//
//     public TechType getBiofuelAlt() {
//         return fcsBiofuelAlt.TechType;
//     }
//
//     public TechType getTeleportCard() {
//         return fcsTeleportCard;
//     }
//
//     public TechType getVehiclePad() {
//         return vehiclePad;
//     }
//
//     internal void modifyPeeperFood(Pickupable pp) {
//         if (!isFCSLoaded)
//             return;
//         var tt = pp.GetTechType();
//         if (peeperBarFoods.Contains(tt)) {
//             var ea = pp.GetComponent<Eatable>();
//             if (ea) {
//                 var alc = ea.waterValue > 70;
//                 ea.waterValue = Mathf.Min(ea.waterValue * 0.2F, 25);
//                 ea.foodValue = Mathf.Min(ea.foodValue * 0.75F, 10);
//                 //SNUtil.log("New food and water value: "+ea.foodValue+"/"+ea.waterValue);
//
//                 if (alc && !registeredEffectFoods.Contains(tt)) {
//                     registeredEffectFoods.Add(tt);
//                     FoodEffectSystem.instance.addEffect(tt, makeDrunk, "Causes lasting intoxication.");
//                     //if (ea.waterValue > 15)
//                     //	FoodEffectSystem.instance.addVomitingEffect(tt, 0, ea.waterValue-15, 1, 0, 0);
//                 }
//             }
//         }
//     }
//
//     private static void makeDrunk(Survival s, GameObject eaten) {
//         float duration = 60;
//         var ea = eaten.GetComponent<Eatable>();
//         if (ea)
//             duration += ea.waterValue * 5;
//         Drunk.add(duration).survivalObject = s;
//     }
//
//     internal bool checkTeleporterFunction(MonoBehaviour teleporter) {
//         var c = teleporter.gameObject.EnsureComponent<C2CTeleporterManager>();
//         c.controller = (FcsDevice)teleporter;
//         return c.active;
//     }
//
//     internal float getCurrentGeneratorPowerFactor(MonoBehaviour turbine) {
//         var pos = turbine.transform.position;
//         var bb = BiomeBase.GetBiome(pos);
//         float ret = 1;
//         var geyser =
//             Vector3.Scale(WorldUtil.getNearestGeyserPosition(pos) - pos, new Vector3(1, 0.5F, 1)).sqrMagnitude <= 400;
//         if (bb == VanillaBiomes.Mountains)
//             ret *= (float)MathUtil.linterpolate(-pos.y, 250, 400, 1.5, 4, true);
//         else if (bb == VanillaBiomes.Lostriver || bb == VanillaBiomes.Cove)
//             ret *= 3;
//         else if (bb == VanillaBiomes.Crash || bb == CrashZoneSanctuaryBiome.instance || bb == VanillaBiomes.Sparse ||
//                  bb == VanillaBiomes.Crag)
//             ret *= 0.5F;
//         else if (!geyser && bb == VanillaBiomes.Jellyshroom)
//             ret *= 0.25F;
//         else if (bb == UnderwaterIslandsFloorBiome.instance)
//             ret *= (float)MathUtil.linterpolate(-pos.y, 300, 500, 3, 5, true);
//         else if (bb == VanillaBiomes.Deepgrand || bb == VanillaBiomes.Bloodkelp)
//             ret *= 2F;
//         else if (bb == VanillaBiomes.Bloodkelpnorth)
//             ret *= 1.5F;
//         if ((pos - WorldUtil.LAVA_DOME).sqrMagnitude <= 6400)
//             ret *= 1.5F;
//         else if ((pos - WorldUtil.DUNES_METEOR).sqrMagnitude <= 14400)
//             ret *= 0.4F;
//         if (geyser)
//             ret *= bb == UnderwaterIslandsFloorBiome.instance ? 1.5F : 3;
//         return ret;
//     }
//
//     private class C2CTeleporterManager : MonoBehaviour {
//         private static MethodInfo setState;
//
//         internal FcsDevice controller;
//         private SubRoot seabase;
//         private FcsDevice charger;
//         private float lastChargerCheck = -1;
//
//         internal bool active;
//
//         private void Update() {
//             if (setState == null) {
//                 var t = instance.getTeleporterController();
//                 setState = t.GetMethod(
//                     "TeleporterState",
//                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
//                 );
//             }
//
//             if (!seabase)
//                 seabase = gameObject.FindAncestor<SubRoot>();
//
//             var time = DayNightCycle.main.timePassedAsFloat;
//             if (!charger && time - lastChargerCheck >= 0.5F) {
//                 lastChargerCheck = time;
//                 var sub = gameObject.FindAncestor<SubRoot>();
//                 if (sub && sub.isBase) {
//                     //if (sub.powerRelay.GetPower() <= 10)
//                     //	return false;
//                     charger = (FcsDevice)sub.GetComponentInChildren(instance.getTeleporterCharger());
//                 }
//             }
//
//             var state = false;
//             if (charger && charger.IsOperational) {
//                 StorageContainer sc = charger.GetComponentInChildren<StorageContainer>();
//                 state = sc && sc.container.GetCount(TechType.PrecursorIonCrystal) > 0;
//             }
//
//             state &= seabase && seabase.isBase && seabase.powerRelay.GetPower() > 10 && controller.IsConstructed;
//             setState.Invoke(controller, BindingFlags.Default, null, new object[] { state }, null);
//             active = state;
//         }
//     }
//
//     private static void addDrillOperationHook(InsnList codes) {
//         var idx = InstructionHandlers.getInstruction(
//             codes,
//             0,
//             0,
//             OpCodes.Callvirt,
//             "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOilHandler",
//             "HasOil",
//             true,
//             new Type[0]
//         );
//         codes.InsertRange(
//             idx + 1,
//             new InsnList {
//                 new CodeInstruction(OpCodes.Ldarg_0),
//                 InstructionHandlers.createMethodCall(
//                     "ReikaKalseki.SeaToSea.C2CHooks",
//                     "canFCSDrillOperate",
//                     false,
//                     typeof(bool),
//                     typeof(MonoBehaviour)
//                 )
//             }
//         );
//     }
//     /*
// private static void addDrillFuelPowerHook(InsnList codes) {
//     codes.patchInitialHook(
//         new CodeInstruction(OpCodes.Ldarg_0),
//         InstructionHandlers.createMethodCall("FCS_ProductionSolutions.Mods.DeepDriller.Managers.DrillSystem", "get_OilHandler", true, new Type[0]),
//         new CodeInstruction(OpCodes.Ldarg_0),
//         InstructionHandlers.createMethodCall("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerController", "get_DeepDrillerPowerManager", true, new Type[0]),
//     );
// }*/
//
//     private static void replaceFCSDrillFuel(InsnList codes) {
//         for (var i = codes.Count - 1; i >= 0; i--) {
//             if (codes[i].LoadsConstant((int)TechType.Lubricant)) {
//                 codes[i] = InstructionHandlers.createMethodCall(
//                     "ReikaKalseki.SeaToSea.C2CHooks",
//                     "getFCSDrillFuel",
//                     false,
//                     new Type[0]
//                 );
//             }
//         }
//     }
//
//     private static void preventDuplicateUnlock(InsnList codes) {
//         var idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Stloc_0);
//         codes.InsertRange(
//             idx,
//             new InsnList {
//                 new CodeInstruction(OpCodes.Ldarg_0),
//                 new CodeInstruction(
//                     OpCodes.Ldfld,
//                     InstructionHandlers.convertFieldOperand(
//                         "FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs.CartDropDownHandler",
//                         "_pendingItems"
//                     )
//                 ),
//                 new CodeInstruction(OpCodes.Ldarg_1),
//                 InstructionHandlers.createMethodCall(
//                     "ReikaKalseki.SeaToSea.C2CHooks",
//                     "filterFCSCartAdd",
//                     false,
//                     typeof(int),
//                     typeof(IList),
//                     typeof(TechType)
//                 )
//             }
//         );
//     }
//
//     private static void filterShopList(InsnList codes) {
//         codes.Clear();
//         codes.add(OpCodes.Ldarg_0);
//         codes.invoke("FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems.StoreItem", "get_TechType", true, new Type[0]);
//         codes.invoke("ReikaKalseki.SeaToSea.C2CHooks", "isFCSItemBuyable", false, new Type[] { typeof(TechType) });
//         codes.add(OpCodes.Ret);
//     }
//
//     private static void replacePurchaseAction(InsnList codes) {
//         var idx = InstructionHandlers.getInstruction(
//             codes,
//             0,
//             0,
//             OpCodes.Call,
//             "FCS_AlterraHub.Helpers.PlayerInteractionHelper",
//             "GivePlayerItem",
//             false,
//             new Type[] { typeof(TechType) }
//         );
//         codes[idx] = InstructionHandlers.createMethodCall(
//             "ReikaKalseki.SeaToSea.C2CHooks",
//             "onFCSPurchasedTech",
//             false,
//             typeof(TechType)
//         );
//         codes[idx - 1] = InstructionHandlers.createMethodCall(
//             "FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems.CartItem",
//             "get_TechType",
//             true,
//             new Type[0]
//         ); //change TechType fetch
//         codes.RemoveAt(idx + 1); //remove the pop
//     }
//
//     private static void redirectPurchase(InsnList codes) {
//         var idx = InstructionHandlers.getMethodCallByName(
//             codes,
//             0,
//             0,
//             "FCS_AlterraHub.Mods.FCSPDA.Mono.FCSPDAController",
//             "MakeAPurchase"
//         );
//         codes[idx - 1] = new CodeInstruction(OpCodes.Ldc_I4_1); //true instead of false
//
//         idx = InstructionHandlers.getMethodCallByName(
//             codes,
//             0,
//             0,
//             "FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs.CheckOutPopupDialogWindow",
//             "get_SelectedDestination"
//         ) - 1;
//         //remove the drone location check; leaves the brfalse
//         codes.RemoveAt(idx + 3);
//         codes.RemoveAt(idx + 2);
//         codes.RemoveAt(idx + 1);
//         //codes.RemoveAt(idx);
//         codes[idx].opcode = OpCodes.Ldc_I4_0;
//     }
//
//     private static void controlOreGeneration(InsnList codes) {
//         for (var i = codes.Count - 1; i >= 0; i--) {
//             if (codes[i].opcode == OpCodes.Ldstr && (string)codes[i].operand == "Spawning item {0}") {
//                 patchTechTypeChoice(codes, i - 1);
//             }
//         }
//     }
//
//     private static void patchTechTypeChoice(InsnList codes, int idx) {
//         InsnList added = [
//             new CodeInstruction(OpCodes.Ldarg_0),
//             new CodeInstruction(OpCodes.Ldarg_0),
//             InstructionHandlers.createMethodCall(
//                 "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator",
//                 "get_IsFocused",
//                 true,
//                 new Type[0]
//             ),
//             new CodeInstruction(OpCodes.Ldarg_0),
//             new CodeInstruction(
//                 OpCodes.Ldfld,
//                 InstructionHandlers.convertFieldOperand(
//                     "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator",
//                     "_blacklistMode"
//                 )
//             ),
//             new CodeInstruction(OpCodes.Ldarg_0),
//             new CodeInstruction(
//                 OpCodes.Ldfld,
//                 InstructionHandlers.convertFieldOperand(
//                     "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator",
//                     "_focusOres"
//                 )
//             ),
//             new CodeInstruction(OpCodes.Ldarg_0),
//             InstructionHandlers.createMethodCall(
//                 "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator",
//                 "get_AllowedOres",
//                 true,
//                 new Type[0]
//             ),
//             InstructionHandlers.createMethodCall(
//                 "ReikaKalseki.SeaToSea.C2CHooks",
//                 "pickFCSDrillOre",
//                 false,
//                 typeof(TechType),
//                 typeof(MonoBehaviour),
//                 typeof(bool),
//                 typeof(bool),
//                 typeof(HashSet<>).MakeGenericType(typeof(TechType)),
//                 typeof(List<>).MakeGenericType(typeof(TechType))
//             ),
//         ];
//         codes.InsertRange(idx, added);
//     }
//
//     private static void controlCurrentGeneration(InsnList codes) {
//         var idx = InstructionHandlers.getInstruction(
//             codes,
//             0,
//             0,
//             OpCodes.Stfld,
//             "FCS_EnergySolutions.Mods.JetStreamT242.Mono.JetStreamT242PowerManager",
//             "_energyPerSec"
//         );
//         codes.InsertRange(
//             idx,
//             new InsnList {
//                 new CodeInstruction(OpCodes.Ldarg_0),
//                 InstructionHandlers.createMethodCall(
//                     "ReikaKalseki.SeaToSea.C2CHooks",
//                     "getCurrentGeneratorPower",
//                     false,
//                     typeof(float),
//                     typeof(MonoBehaviour)
//                 )
//             }
//         );
//     }
//
//     internal void onPlayerBuy(TechType tt) {
//         if (!KnownTech.Contains(tt))
//             unlocksRightNow.Add(tt);
//         KnownTech.Add(tt);
//         //SNUtil.triggerTechPopup(tt);
//         StoryGoal.Execute("UnlockFCS" + tt.AsString(), Story.GoalType.Story);
//         lastTimeUnlock = DayNightCycle.main.timePassedAsFloat;
//     }
//
//     internal void tickNotifications(float time) {
//         if (uGUI_PopupNotification.main.isShowingMessage)
//             lastTimeUnlock = time;
//
//         if (time - lastTimeUnlock >= 0.25F && unlocksRightNow.Count > 0) {
//             SNUtil.triggerMultiTechPopup(unlocksRightNow);
//             unlocksRightNow.Clear();
//         }
//     }
//
//     internal void initializeTechUnlocks() {
//         foreach (var tt in replacedTechRecipes) {
//             if (notBuyableTechs.Contains(tt))
//                 continue;
//             SNUtil.log("Relocking tech " + tt.AsString() + " as no storygoal set");
//             if (StoryGoalManager.main.completedGoals.Contains("UnlockFCS" + tt.AsString())) {
//                 KnownTech.Add(tt);
//             } else {
//                 KnownTech.Remove(tt);
//             }
//         }
//     }
//
//     internal void applyPatches() {
//         if (QModManager.API.QModServices.Main.ModPresent("FCSAlterraHub") != isFCSLoaded)
//             throw new Exception("Modlist consistency failure");
//         if (isFCSLoaded)
//             doApplyPatches();
//     }
//
//     private void doApplyPatches() {
//         var hard = SeaToSeaMod.ModConfig.getBoolean(C2CConfig.ConfigEntries.HARDMODE);
//         CustomMachineLogic.powerCostFactor *=
//             hard ? 1.6F : 2; //since hard increases bioproc 2.5x, only increase to 4x instead of 5x
//
//         fcsTeleportCard = findFCSItem("QuantumPowerBank");
//         vehiclePad = findFCSItem("QuantumTeleporterVehiclePad");
//         SNUtil.log("Hiding quantum power bank " + fcsTeleportCard + " & " + vehiclePad);
//
//         var f = typeof(FCSAlterraHubService).GetField(
//             "_storeItems",
//             BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
//         );
//         var core = f.DeclaringType.Assembly;
//         var dict = (IDictionary)f.GetValue(null);
//         foreach (var item in C2CProgression.Instance.GetGatedTechnologies()) {
//             if (dict.Contains(item))
//                 dict.Remove(item);
//         }
//
//         dict.Remove(TechType.Battery);
//         dict.Remove(TechType.PowerCell);
//         dict.Remove(TechType.WiringKit);
//         dict.Remove(TechType.AdvancedWiringKit);
//         dict.Remove(fcsTeleportCard);
//         dict.Remove(vehiclePad);
//
//         f = core.GetType("FCS_AlterraHub.Systems.StoreInventorySystem").GetField(
//             "OrePrices",
//             BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
//         );
//         var oreValues = (Dictionary<TechType, decimal>)f.GetValue(null);
//         oreValues[CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType] = (decimal)Mathf.Lerp(
//             (float)oreValues[TechType.Nickel],
//             (float)oreValues[TechType.Kyanite],
//             0.75F
//         );
//         oreValues[CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType] =
//             oreValues[TechType.Gold] * 1.5M;
//         oreValues[CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType] =
//             oreValues[TechType.Kyanite] * 2M;
//
//         oreValues[TechType.MercuryOre] = (decimal)Mathf.Lerp(
//             (float)oreValues[TechType.Gold],
//             (float)oreValues[TechType.Nickel],
//             0.5F
//         );
//         oreValues[TechType.Salt] = oreValues[TechType.Titanium] * 0.2M;
//         oreValues[CraftingItems.getItem(CraftingItems.Items.TraceMetals).TechType] = oreValues[TechType.Copper];
//         oreValues[CraftingItems.getItem(CraftingItems.Items.GeyserMinerals).TechType] = oreValues[TechType.Lead];
//         oreValues[CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType] = oreValues[TechType.Kyanite] * 4;
//
//         foreach (var ingot in C2CItems.getIngots()) {
//             if (oreValues.ContainsKey(ingot.material))
//                 oreValues[ingot.ingot] = oreValues[ingot.material] * ingot.count;
//         }
//
//         /*
//         oreValues[TechType.Floater] = oreValues[TechType.Titanium]*2.0M;
//         oreValues[TechType.BloodOil] = oreValues[TechType.Titanium]*1.0M;
//         oreValues[TechType.CreepvineSeedCluster] = oreValues[TechType.Titanium]*0.25M;
//         oreValues[TechType.SeaCrownSeed] = oreValues[TechType.Titanium]*0.5M;
//         oreValues[C2CItems.alkali.seed.TechType] = oreValues[TechType.Copper];
//         oreValues[C2CItems.kelp.seed.TechType] = oreValues[TechType.Copper];
//         oreValues[C2CItems.mountainGlow.seed.TechType] = oreValues[TechType.Copper]*1.2M;
//         oreValues[C2CItems.sanctuaryPlant.seed.TechType] = oreValues[TechType.Gold];
//         oreValues[C2CItems.healFlower.seed.TechType] = (decimal)Mathf.Lerp((float)oreValues[TechType.Titanium], (float)oreValues[TechType.Copper], 0.5F);
//         */
//
//         InstructionHandlers.patchMethod(
//             SeaToSeaMod.Harmony,
//             core.GetType("FCS_AlterraHub.Mods.FCSPDA.Mono.FCSPDAController"),
//             "MakeAPurchase",
//             SeaToSeaMod.ModDLL,
//             replacePurchaseAction
//         );
//         InstructionHandlers.patchMethod(
//             SeaToSeaMod.Harmony,
//             core.GetType("FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs.CheckOutPopupDialogWindow"),
//             "MakePurchase",
//             SeaToSeaMod.ModDLL,
//             redirectPurchase
//         );
//         InstructionHandlers.patchMethod(
//             SeaToSeaMod.Harmony,
//             core.GetType("FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems.StoreItem"),
//             "CheckIsUnlocked",
//             SeaToSeaMod.ModDLL,
//             filterShopList
//         );
//         InstructionHandlers.patchMethod(
//             SeaToSeaMod.Harmony,
//             core.GetType("FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs.CartDropDownHandler"),
//             "AddItem",
//             SeaToSeaMod.ModDLL,
//             preventDuplicateUnlock
//         );
//
//         var homeModType = InstructionHandlers.getTypeBySimpleName("FCS_HomeSolutions.Configuration.Mod");
//         if (homeModType != null) {
//             var homeMod = homeModType.Assembly;
//             f = homeModType.GetField(
//                 "PeeperBarFoods",
//                 BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
//             );
//             dict = (IDictionary)f.GetValue(null);
//             if (dict.Contains(TechType.NutrientBlock))
//                 dict.Remove(TechType.NutrientBlock);
//             if (dict.Contains(TechType.BigFilteredWater))
//                 dict.Remove(TechType.BigFilteredWater);
//             peeperBarFoods.AddRange((IEnumerable<TechType>)dict.Keys);
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 homeMod.GetType("FCS_HomeSolutions.Mods.TrashRecycler.Mono.Recycler"),
//                 "Recycle",
//                 SeaToSeaMod.ModDLL,
//                 codes => {
//                     var idx = InstructionHandlers.getInstruction(
//                         codes,
//                         0,
//                         0,
//                         OpCodes.Call,
//                         "FCS_AlterraHub.Helpers.TechDataHelpers",
//                         "GetIngredientsWithOutBatteries",
//                         false,
//                         new Type[] { typeof(TechType) }
//                     );
//                     codes.Insert(
//                         idx + 1,
//                         InstructionHandlers.createMethodCall(
//                             "ReikaKalseki.SeaToSea.C2CHooks",
//                             "filterFCSRecyclerOutput",
//                             false,
//                             typeof(List<>).MakeGenericType(typeof(Ingredient))
//                         )
//                     );
//                 }
//             );
//
//             teleporterController =
//                 homeMod.GetType("FCS_HomeSolutions.Mods.QuantumTeleporter.Mono.QuantumTeleporterController");
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 teleporterController,
//                 "get_IsOperational",
//                 SeaToSeaMod.ModDLL,
//                 codes => {
//                     codes.patchEveryReturnPre(
//                         new CodeInstruction(OpCodes.Ldarg_0),
//                         InstructionHandlers.createMethodCall(
//                             "ReikaKalseki.SeaToSea.C2CHooks",
//                             "isTeleporterFunctional",
//                             false,
//                             typeof(bool),
//                             typeof(MonoBehaviour)
//                         )
//                     );
//                 }
//             );
//
//             teleporterCharger = homeMod.GetType(
//                 "FCS_HomeSolutions.Mods.QuantumTeleporter.Mono.QuantumPowerBankChargerController"
//             );
//         }
//
//         var drillHelper =
//             InstructionHandlers.getTypeBySimpleName("FCS_ProductionSolutions.Mods.DeepDriller.Helpers.Helpers");
//         if (drillHelper != null) {
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 drillHelper,
//                 "GetBiomeData",
//                 SeaToSeaMod.ModDLL,
//                 codes => {
//                     codes.patchEveryReturnPre(
//                         InstructionHandlers.createMethodCall(
//                             "ReikaKalseki.SeaToSea.C2CHooks",
//                             "filterFCSDrillerOutput",
//                             false,
//                             typeof(List<>).MakeGenericType(typeof(TechType))
//                         )
//                     );
//                 }
//             );
//
//             var prodMod = drillHelper.Assembly;
//             drillOreManager = prodMod.GetType(
//                 "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator"
//             );
//             DrillDepletionSystem.Instance.Register();
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 drillOreManager,
//                 "SetAllowTick",
//                 SeaToSeaMod.ModDLL,
//                 codes => {
//                     var idx = InstructionHandlers.getInstruction(
//                         codes,
//                         0,
//                         0,
//                         OpCodes.Callvirt,
//                         "FCS_AlterraHub.Mono.FcsDevice",
//                         "get_IsOperational",
//                         true,
//                         new Type[0]
//                     );
//                     codes.InsertRange(
//                         idx + 1,
//                         new InsnList {
//                             new CodeInstruction(OpCodes.Ldarg_0),
//                             InstructionHandlers.createMethodCall(
//                                 "ReikaKalseki.SeaToSea.C2CHooks",
//                                 "canFCSDrillOperate",
//                                 false,
//                                 typeof(bool),
//                                 typeof(MonoBehaviour)
//                             )
//                         }
//                     );
//                 }
//             );
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 drillOreManager,
//                 "Update",
//                 SeaToSeaMod.ModDLL,
//                 codes => {
//                     var idx = InstructionHandlers.getInstruction(
//                         codes,
//                         0,
//                         0,
//                         OpCodes.Stfld,
//                         "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOreGenerator",
//                         "_passedTime"
//                     );
//                     codes.InsertRange(
//                         idx + 1,
//                         new InsnList {
//                             new CodeInstruction(OpCodes.Ldarg_0),
//                             InstructionHandlers.createMethodCall(
//                                 "ReikaKalseki.SeaToSea.C2CHooks",
//                                 "tickFCSDrill",
//                                 false,
//                                 typeof(MonoBehaviour)
//                             )
//                         }
//                     );
//                 }
//             );
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 drillOreManager,
//                 "GenerateOre",
//                 SeaToSeaMod.ModDLL,
//                 controlOreGeneration
//             );
//
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.Managers.DrillSystem"),
//                 "get_IsOperational",
//                 SeaToSeaMod.ModDLL,
//                 addDrillOperationHook
//             );
//             drillController = prodMod.GetType(
//                 "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerController"
//             );
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 drillController,
//                 "get_IsOperational",
//                 SeaToSeaMod.ModDLL,
//                 addDrillOperationHook
//             );
//             //InstructionHandlers.patchMethod(SeaToSeaMod.harmony, t, "Update", SeaToSeaMod.modDLL, addDrillFuelPowerHook);
//
//             drillStorage = prodMod.GetType(
//                 "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerContainer"
//             );
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 drillStorage.GetMethod(
//                     "AddItemToContainer",
//                     BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
//                     null,
//                     [typeof(TechType)],
//                     null
//                 ),
//                 SeaToSeaMod.ModDLL,
//                 codes => {
//                     var idx = InstructionHandlers.getInstruction(
//                         codes,
//                         0,
//                         0,
//                         OpCodes.Call,
//                         "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerContainer",
//                         "get_OnContainerUpdate",
//                         true,
//                         new Type[0]
//                     );
//                     codes.InsertRange(
//                         idx + 1,
//                         new InsnList {
//                             new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_0),
//                             new CodeInstruction(
//                                 OpCodes.Ldfld,
//                                 InstructionHandlers.convertFieldOperand(
//                                     "FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerContainer",
//                                     "_container"
//                                 )
//                             ),
//                             InstructionHandlers.createMethodCall(
//                                 "ReikaKalseki.SeaToSea.C2CHooks",
//                                 "cleanupFCSContainer",
//                                 false,
//                                 typeof(Action<,>).MakeGenericType(typeof(int), typeof(int)),
//                                 typeof(MonoBehaviour),
//                                 typeof(Dictionary<,>).MakeGenericType(typeof(TechType), typeof(int))
//                             )
//                         }
//                     );
//                 }
//             );
//
//             fcsDrillFuel = new FCSFuel();
//             //C2CItems.fcsDrillFuel.addIngredient(TechType.Benzene, 1);
//             fcsDrillFuel.addIngredient(TechType.Lubricant, 2);
//             fcsDrillFuel.addIngredient(EcoceanMod.glowOil.TechType, 1);
//             fcsDrillFuel.addIngredient(TechType.JellyPlant, 3);
//             fcsDrillFuel.addIngredient(C2CItems.alkali.seed.TechType, 1);
//             fcsDrillFuel.Patch();
//             TechnologyUnlockSystem.instance.addDirectUnlock(findFCSItem("DeepDrillerLightDuty"), fcsDrillFuel.TechType);
//             TechnologyUnlockSystem.instance.addDirectUnlock(findFCSItem("DeepDrillerMK3"), fcsDrillFuel.TechType);
//             var t = prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono.FCSDeepDrillerOilHandler");
//             foreach (var m in t.GetMethods(
//                          BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
//                          BindingFlags.NonPublic
//                      ))
//                 InstructionHandlers.patchMethod(SeaToSeaMod.Harmony, m, SeaToSeaMod.ModDLL, replaceFCSDrillFuel);
//
//             t = prodMod.GetType("FCS_ProductionSolutions.Mods.DeepDriller.Patchers.DeepDrillerGUIOilPage");
//             foreach (var m in t.GetMethods(
//                          BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
//                          BindingFlags.NonPublic
//                      ))
//                 InstructionHandlers.patchMethod(SeaToSeaMod.Harmony, m, SeaToSeaMod.ModDLL, replaceFCSDrillFuel);
//         }
//
//         var alterraGen =
//             InstructionHandlers.getTypeBySimpleName("FCS_EnergySolutions.Mods.AlterraGen.Mono.AlterraGenPowerManager");
//         if (alterraGen != null) {
//             var energyMod = alterraGen.Assembly;
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 alterraGen,
//                 "Update",
//                 SeaToSeaMod.ModDLL,
//                 codes => {
//                     codes[InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Ldc_R4)].operand = 2.4F; //from 1.167F
//                 }
//             );
//
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 alterraGen,
//                 "GetMultiplier",
//                 SeaToSeaMod.ModDLL,
//                 codes => {
//                     codes.patchEveryReturnPre(
//                         new CodeInstruction(OpCodes.Ldarg_0),
//                         new CodeInstruction(OpCodes.Ldarg_1),
//                         InstructionHandlers.createMethodCall(
//                             "ReikaKalseki.SeaToSea.C2CHooks",
//                             "getFCSBioGenPowerFactor",
//                             false,
//                             typeof(float),
//                             typeof(MonoBehaviour),
//                             typeof(TechType)
//                         )
//                     );
//                 }
//             );
//             /* this is animation only, does not matter
//             Type t = energyMod.GetType("FCS_EnergySolutions.Configuration.Config");
//             PropertyInfo p = t.GetProperty("JetStreamT242BiomeSpeeds", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
//             Dictionary<string, float> biomeSpeeds = new Dictionary<string, float>();
//             p.SetValue(obj, biomeSpeeds);*/
//
//             InstructionHandlers.patchMethod(
//                 SeaToSeaMod.Harmony,
//                 energyMod.GetType("FCS_EnergySolutions.Mods.JetStreamT242.Mono.JetStreamT242PowerManager"),
//                 "ProducePower",
//                 SeaToSeaMod.ModDLL,
//                 controlCurrentGeneration
//             );
//         }
//
//         luminolDrop = new BasicCraftingItem(
//             SeaToSeaMod.ItemLocale.getEntry("LuminolDrop"),
//             "WorldEntities/Natural/polyaniline"
//         ) {
//             numberCrafted = 6,
//             craftingTime = 0.5F,
//             renderModify = CraftingItems.getItem(CraftingItems.Items.Luminol).renderModify,
//             sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/LuminolDrop"),
//         };
//         luminolDrop.addIngredient(CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
//         luminolDrop.Patch();
//
//         paint = new BasicCraftingItem(SeaToSeaMod.ItemLocale.getEntry("Paint"), "WorldEntities/Natural/Lubricant") {
//             numberCrafted = 4,
//             craftingTime = 0.5F,
//             sprite = TextureManager.getSprite(SeaToSeaMod.ModDLL, "Textures/Items/Paint"),
//         };
//         paint.addIngredient(TechType.Lubricant, 1);
//         paint.addIngredient(TechType.BloodOil, 1);
//         paint.addIngredient(TechType.JellyPlant, 1);
//         paint.addIngredient(EcoceanMod.glowOil.TechType, 2);
//         paint.addIngredient(TechType.PurpleStalkSeed, 1);
//         paint.Patch();
//
//         doAssignRecipe("ahsLeftCornerRailing", 1);
//         doAssignRecipe("ahsLeftCornerwGlassRailing", 1, basicGlass());
//         doAssignRecipe("ahsRightCornerRailing", 1);
//         doAssignRecipe("ahsRightCornerwGlassRailing", 1, basicGlass());
//         doAssignRecipe("ahsrailing", 1);
//         doAssignRecipe("ahsrailingglass", 1, basicGlass());
//         doAssignRecipe("CabinetMediumTall", 2);
//         doAssignRecipe("CabinetTall", 3);
//         doAssignRecipe("CabinetTallWide", 4);
//         doAssignRecipe("CabinetTVStand", 3);
//         doAssignRecipe("CabinetWide", 3);
//         doAssignRecipe("Curtain", 0, fabric());
//         doAssignRecipe("DisplayBoard", 1, electronicsTier2(2));
//         assignRecipe("Elevator", 3, electronicsTier2(), motorized(2));
//         doAssignRecipe("EmptyObservationTank", 2, strongGlass());
//         doAssignRecipe("FCSCrewBunkBed", 2, fabric(2));
//         doAssignRecipe("FCSCrewLocker", 2);
//         doAssignRecipe(
//             "FCSCuringCabinet",
//             2,
//             new Ingredient(TechType.CopperWire, 1),
//             new Ingredient(TechType.Glass, 1)
//         );
//         assignRecipe("FCSJukeBox", 1, [new(TechType.ComputerChip, 1), new(luminolDrop.TechType, 1)], speaker());
//         doAssignRecipe("FCSJukeBoxSpeaker", 2, speaker());
//         doAssignRecipe("FCSJukeBoxSubWoofer", 2, speaker());
//         doAssignRecipe("FCSMicrowave", 1, electronicsTier1());
//         assignRecipe("FCSRug", 0, fabric(3), [new(paint.TechType, 1)]);
//         doAssignRecipe("FCSShower", 2, new Ingredient(TechType.Glass, 2), new Ingredient(TechType.Pipe, 5));
//         doAssignRecipe("FCSSink", 2, new Ingredient(TechType.Pipe, 5));
//         doAssignRecipe("FCSToilet", 1, new Ingredient(TechType.Pipe, 5));
//         doAssignRecipe("FCSStairs", 2);
//         assignRecipe("FCSStove", 2, electronicsTier1(2), strongGlass(1));
//         doAssignRecipe(
//             "FireExtinguisherRefueler",
//             1,
//             new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1),
//             new Ingredient(TechType.Pipe, 5)
//         );
//         doAssignRecipe("FloodLEDLight", 1, new Ingredient(TechType.Quartz, 2));
//         doAssignRecipe("FloorShelf01", 1);
//         doAssignRecipe("FloorShelf02", 3);
//         doAssignRecipe("FloorShelf03", 2);
//         doAssignRecipe("FloorShelf04", 3);
//         doAssignRecipe("FloorShelf05", 3);
//         doAssignRecipe("FloorShelf06", 2);
//         doAssignRecipe("FloorShelf07", 2);
//         doAssignRecipe(
//             "HologramPoster",
//             0,
//             new Ingredient(TechType.Quartz, 1),
//             new Ingredient(luminolDrop.TechType, 1),
//             new Ingredient(TechType.Magnetite, 1),
//             new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1)
//         );
//         doAssignRecipe("LedLightStickLong", 1, new Ingredient(TechType.Quartz, 3));
//         doAssignRecipe("LedLightStickShort", 1, new Ingredient(TechType.Quartz, 1));
//         doAssignRecipe("LedLightStickWall", 1, new Ingredient(TechType.Quartz, 2));
//         doAssignRecipe("MiniFountainFilter", 1, new Ingredient(TechType.Pipe, 5));
//         doAssignRecipe(
//             "MountSmartTV",
//             1,
//             new Ingredient(TechType.AdvancedWiringKit, 1),
//             new Ingredient(paint.TechType, 2),
//             new Ingredient(EcoceanMod.glowOil.TechType, 3),
//             new Ingredient(TechType.Quartz, 2),
//             new Ingredient(luminolDrop.TechType, 3)
//         );
//         doAssignRecipe("NeonBarStool", 2, new Ingredient(luminolDrop.TechType, 1));
//         doAssignRecipe("NeonPlanter", 2, new Ingredient(luminolDrop.TechType, 1));
//         doAssignRecipe("NeonShelf01", 3, new Ingredient(luminolDrop.TechType, 1));
//         doAssignRecipe("NeonShelf02", 2, new Ingredient(luminolDrop.TechType, 1));
//         doAssignRecipe("NeonShelf03", 1, new Ingredient(luminolDrop.TechType, 1));
//         doAssignRecipe("NeonTable01", 1, new Ingredient(luminolDrop.TechType, 1));
//         doAssignRecipe("NeonTable02", 1, new Ingredient(luminolDrop.TechType, 1));
//         doAssignRecipe(
//             "OutsideSign",
//             2,
//             new Ingredient(TechType.Quartz, 2),
//             new Ingredient(TechType.CopperWire, 1),
//             new Ingredient(TechType.Silver, 1)
//         );
//         doAssignRecipe(
//             "pccpu",
//             2,
//             new Ingredient(TechType.AdvancedWiringKit, 1),
//             new Ingredient(TechType.ComputerChip, 2)
//         );
//         doAssignRecipe(
//             "pcmonitor",
//             1,
//             new Ingredient(TechType.WiringKit, 1),
//             new Ingredient(paint.TechType, 2),
//             new Ingredient(EcoceanMod.glowOil.TechType, 2),
//             new Ingredient(TechType.Quartz, 2),
//             new Ingredient(luminolDrop.TechType, 2)
//         );
//         doAssignRecipe(
//             "PeeperLoungeBar",
//             3,
//             new Ingredient(TechType.ComputerChip, 1),
//             new Ingredient(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1)
//         );
//         assignRecipe(
//             "QuantumPowerBankCharger",
//             0,
//             [
//                 new(TechType.AdvancedWiringKit, 1),
//                 new(CraftingItems.getItem(CraftingItems.Items.Nanocarbon).TechType, 1),
//                 new(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 3),
//             ],
//             reinforced(2),
//             electronicsTier1(4)
//         );
//         assignRecipe(
//             "QuantumTeleporter",
//             4,
//             [
//                 new(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 1),
//                 new(EcoceanMod.glowOil.TechType, 4),
//             ],
//             electronicsTier3()
//         );
//         //assignRecipe("QuantumTeleporterVehiclePad", 8, new Ingredient[]{new Ingredient(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 4), new Ingredient(TechType.PrecursorIonCrystal, 5), new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 2), new Ingredient(EcoceanMod.glowOil.TechType, 8)}, electronicsTier3());
//         doAssignRecipe(
//             "Recycler",
//             1,
//             new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1),
//             new Ingredient(TechType.Polyaniline, 1),
//             new Ingredient(TechType.CrashPowder, 2),
//             new Ingredient(TechType.Diamond, 2),
//             new Ingredient(TechType.ComputerChip, 1),
//             new Ingredient(TechType.Magnetite, 2)
//         );
//         doAssignRecipe("RingLight", 1, new Ingredient(TechType.Quartz, 2));
//         doAssignRecipe("Seabreeze", 3, electronicsTier1());
//         doAssignRecipe("Sofa1", 1, fabric(1));
//         doAssignRecipe("Sofa2", 3, fabric(2));
//         doAssignRecipe("Sofa3", 2, fabric(2));
//         assignRecipe("TableSmartTV", "MountSmartTV");
//         doAssignRecipe("TrashReceptacle", 3);
//         assignRecipe("WallSign", "OutsideSign");
//
//         notBuyableTechs.Add(doAssignRecipe("AlterraHubDepot", 6, electronicsTier2(1)));
//         notBuyableTechs.Add(doAssignRecipe("DronePortPad", 8, reinforcedStrong(2)));
//         notBuyableTechs.Add(
//             doAssignRecipe(
//                 "OreConsumer",
//                 5,
//                 new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 3),
//                 new Ingredient(TechType.Polyaniline, 1),
//                 new Ingredient(TechType.CrashPowder, 2),
//                 new Ingredient(TechType.Diamond, 2),
//                 new Ingredient(TechType.AdvancedWiringKit, 1)
//             )
//         );
//         assignRecipe("PatreonStatue", 2, [new(TechType.Quartz, 1)], electronicsTier1());
//
//         doAssignRecipe(
//             "AlterraGen",
//             5,
//             new Ingredient[] {
//                 new(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 1), new(TechType.AdvancedWiringKit, 1)
//             }
//         );
//         doAssignRecipe(
//             "AlterraSolarCluster",
//             0,
//             new Ingredient[] {
//                 new(C2CItems.getIngot(TechType.Quartz).ingot, 1), new(TechType.TitaniumIngot, 1),
//                 new(TechType.CopperWire, 2), new(TechType.Gold, 3), new(TechType.WiringKit, 1)
//             }
//         );
//         doAssignRecipe(
//             "JetStreamT242",
//             4,
//             new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 2),
//             new Ingredient(TechType.Silicone, 4)
//         );
//         doAssignRecipe(
//             "PowerStorage",
//             6,
//             new Ingredient(TechType.Silver, 2),
//             new Ingredient(TechType.ComputerChip, 1),
//             new Ingredient(TechType.PowerCell, 4)
//         );
//         assignRecipe(
//             "TelepowerPylon",
//             4,
//             [
//                 new(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 3),
//             ],
//             electronicsTier3()
//         );
//         doAssignRecipe("UniversalCharger", 2, electronicsTier2(4));
//
//         var plat = findFCSItem("WindSurferPlatform_Kit");
//         RecipeUtil.addRecipe(plat, TechGroup.Machines, TechCategory.Machines, ["Machines"]);
//         CraftDataHandler.SetItemSize(plat, new Vector2int(3, 3));
//         doAssignRecipe("WindSurferPlatform_Kit", 0, floatingPlatform(10));
//
//         var turb = findFCSItem("WindSurfer_Kit");
//         RecipeUtil.addRecipe(turb, TechGroup.Machines, TechCategory.Machines, ["Machines"]);
//         CraftDataHandler.SetItemSize(turb, new Vector2int(3, 3));
//         assignRecipe(
//             "WindSurfer_Kit",
//             0,
//             floatingPlatform(30),
//             [
//                 new(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 4), new(TechType.Magnetite, 5),
//             ]
//         );
//
//         var tt = findFCSItem("WindSurferOperator");
//         //RecipeUtil.addRecipe(tt, TechGroup.Constructor, TechCategory.Constructor, null, 1, CraftTree.Type.Constructor);
//         doAssignRecipe(
//             "WindSurferOperator",
//             0,
//             new Ingredient[] {
//                 new(TechType.Silicone, 8), new(TechType.TitaniumIngot, 2), new(TechType.Polyaniline, 2),
//                 new(TechType.CopperWire, 5), new(TechType.AdvancedWiringKit, 2),
//                 new(CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1),
//                 new(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 5)
//             }
//         );
//         TechnologyUnlockSystem.instance.addDirectUnlock(tt, plat);
//         TechnologyUnlockSystem.instance.addDirectUnlock(tt, turb);
//
//         assignRecipe("BaseOxygenTank", 4, motorized(), electronicsTier1());
//         assignRecipe("BaseOxygenTankKitType", "BaseOxygenTank");
//         assignRecipe("BaseUtilityUnit", 6, motorized(), electronicsTier1());
//         assignRecipe("EnergyPillVendingMachine", 2, [new(TechType.Quartz, 4)], electronicsTier2());
//         assignRecipe("MiniMedBay", 4, [new(C2CItems.bandage.TechType, 4)], electronicsTier3());
//
//         assignRecipe("AutoCrafter", 4, [new(TechType.AluminumOxide, 3)], motorized(2), electronicsTier3());
//         doAssignRecipe(
//             "DeepDrillerLightDuty",
//             0,
//             new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 3),
//             new Ingredient(TechType.Diamond, 6),
//             new Ingredient(TechType.PlasteelIngot, 1)
//         );
//         doAssignRecipe(
//             "DeepDrillerMK3",
//             0,
//             new Ingredient(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, 8),
//             new Ingredient(TechType.Diamond, 9),
//             new Ingredient(TechType.PlasteelIngot, 2)
//         );
//         assignRecipe("HydroponicHarvester", 2, strongGlass(), motorized());
//         assignRecipe(
//             "MatterAnalyzer",
//             3,
//             [
//                 new(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1),
//             ],
//             electronicsTier3()
//         );
//         assignRecipe(
//             "Replicator",
//             4,
//             [new(CraftingItems.getItem(CraftingItems.Items.LathingDrone).TechType, 1)],
//             electronicsTier3(),
//             strongGlass(1)
//         );
//
//         doAssignRecipe("AlterraStorage", 5, electronicsTier3());
//         assignRecipe(
//             "DSSAntenna",
//             6,
//             electronicsTier1(3),
//             [
//                 new(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 1),
//                 new(TechType.Polyaniline, 2),
//             ]
//         );
//         doAssignRecipe(
//             "DSSFloorServerRack",
//             3,
//             new Ingredient(TechType.AdvancedWiringKit, 1),
//             new Ingredient(TechType.ComputerChip, 4)
//         );
//         doAssignRecipe(
//             "DSSWallServerRack",
//             2,
//             new Ingredient(TechType.AdvancedWiringKit, 1),
//             new Ingredient(TechType.ComputerChip, 3)
//         );
//         assignRecipe("DSSItemDisplay", "MountSmartTV");
//         assignRecipe("DSSTerminalMonitor", "MountSmartTV");
//
//         //assignRecipe("QuantumPowerBank", 2, new Ingredient[]{new Ingredient(TechType.PowerCell, 3)}, reinforcedStrong(), electronicsTier3());
//         RecipeUtil.addRecipe(
//             findFCSItem("PaintTool"),
//             TechGroup.Personal,
//             TechCategory.Tools,
//             [
//                 "Personal", "Tools",
//             ]
//         );
//         RecipeUtil.addRecipe(
//             findFCSItem("PaintCan"),
//             TechGroup.Personal,
//             TechCategory.Tools,
//             ["Personal", "Tools"]
//         );
//         RecipeUtil.addRecipe(
//             findFCSItem("DSSServer"),
//             TechGroup.Personal,
//             TechCategory.Tools,
//             [
//                 "Personal", "Tools",
//             ]
//         );
//         RecipeUtil.addRecipe(
//             findFCSItem("DSSTransceiver"),
//             TechGroup.Personal,
//             TechCategory.Tools,
//             [
//                 "Personal", "Tools",
//             ]
//         );
//         doAssignRecipe("PaintTool", 2, new Ingredient(TechType.Battery, 1), new Ingredient(TechType.Pipe, 5));
//         doAssignRecipe("PaintCan", 1, new Ingredient(paint.TechType, 4));
//         doAssignRecipe(
//             "DSSServer",
//             0,
//             new Ingredient(TechType.ComputerChip, 1),
//             new Ingredient(TechType.Magnetite, 4),
//             new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 1)
//         );
//         doAssignRecipe(
//             "DSSTransceiver",
//             0,
//             new Ingredient(TechType.ComputerChip, 2),
//             new Ingredient(TechType.WiringKit, 1),
//             new Ingredient(TechType.Magnetite, 4),
//             new Ingredient(TechType.Gold, 2),
//             new Ingredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType, 1)
//         );
//
//         fcsBiofuel = findFCSItem("FCSBioFuel");
//         TechData td = RecipeUtil.addRecipe(
//             fcsBiofuel,
//             TechGroup.Resources,
//             C2CItems.chemistryCategory,
//             [
//                 "Resources", "C2Chemistry",
//             ]
//         );
//         td.Ingredients.Add(new Ingredient(TechType.Oculus, 3));
//         td.Ingredients.Add(new Ingredient(EcoceanMod.glowOil.TechType, 5));
//         td.Ingredients.Add(new Ingredient(C2CItems.mountainGlow.seed.TechType, 1));
//         td.Ingredients.Add(new Ingredient(CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType, 1));
//         td.Ingredients.Add(new Ingredient(TechType.CreepvineSeedCluster, 1));
//         td.Ingredients.Add(new Ingredient(TechType.RedConePlantSeed, 1));
//         td.Ingredients.Add(new Ingredient(TechType.RedRollPlantSeed, 1));
//         CraftDataHandler.SetTechData(fcsBiofuel, td);
//         CraftDataHandler.SetItemSize(fcsBiofuel, new Vector2int(4, 4));
//         CraftDataHandler.SetItemSize(TechType.RedConePlantSeed, new Vector2int(2, 1));
//         CraftDataHandler.SetItemSize(TechType.RedRollPlantSeed, new Vector2int(1, 2));
//         CraftDataHandler.SetCraftingTime(fcsBiofuel, 10);
//         BioReactorHandler.SetBioReactorCharge(fcsBiofuel, 18000);
//
//         TechData rec = RecipeUtil.copyRecipe(td);
//         rec.Ingredients.Add(new Ingredient(C2CItems.purpleBoomerang.TechType, 2));
//         rec.Ingredients.Add(new Ingredient(TechType.Benzene, 1));
//         rec.Ingredients.Add(new Ingredient(C2CItems.sanctuaryPlant.seed.TechType, 1));
//         rec.Ingredients.ForEach(i => {
//                 if (i.techType == EcoceanMod.glowOil.TechType ||
//                     i.techType == CraftingItems.getItem(CraftingItems.Items.WeakAcid).TechType) i.amount *= 2;
//             }
//         );
//         fcsBiofuelAlt = new DuplicateRecipeDelegateWithRecipe(fcsBiofuel, rec);
//         fcsBiofuelAlt.category = C2CItems.chemistryCategory;
//         fcsBiofuelAlt.group = TechGroup.Resources;
//         fcsBiofuelAlt.craftingType = CraftTree.Type.Fabricator;
//         fcsBiofuelAlt.craftingMenuTree = ["Resources", "C2Chemistry"];
//         fcsBiofuelAlt.ownerMod = SeaToSeaMod.ModDLL;
//         fcsBiofuelAlt.craftTime = 15;
//         fcsBiofuelAlt.setRecipe(2);
//         fcsBiofuelAlt.unlock = TechType.Unobtanium;
//         fcsBiofuelAlt.allowUnlockPopups = true;
//         Spawnable sp = (Spawnable)fcsBiofuel.getModPrefabByTechType();
//         fcsBiofuelAlt.sprite = sp.getPrefabSprite();
//         fcsBiofuelAlt.Patch();
//         StoryHandler.instance.registerTrigger(
//             new TechTrigger(fcsBiofuel),
//             new TechUnlockEffect(fcsBiofuelAlt.TechType)
//         );
//     }
//
//     public TechType assignRecipe(string id, params string[] refItem) {
//         List<Ingredient> li = [];
//         foreach (var item in refItem) {
//             li = RecipeUtil.combineIngredients(li, RecipeUtil.getRecipe(findFCSItem(item)).Ingredients);
//         }
//
//         return doAssignRecipe(id, 0, li.ToArray());
//     }
//
//     public TechType assignRecipe(
//         string id,
//         int titanium,
//         Ingredient[] set1,
//         Ingredient[] set2,
//         Ingredient[] set3 = null
//     ) {
//         List<Ingredient> li = [];
//         li.AddRange(set1);
//         li.AddRange(set2);
//         if (set3 != null)
//             li.AddRange(set3);
//         return doAssignRecipe(id, titanium, li.ToArray());
//     }
//
//     public TechType doAssignRecipe(string id, int titanium, params Ingredient[] items) {
//         var tt = TechType.None;
//         try {
//             tt = findFCSItem(id);
//         } catch (Exception ex) {
//             SNUtil.log(ex.ToString());
//             return tt;
//         }
//
//         TechData td = RecipeUtil.getRecipe(tt, false);
//         if (td == null) {
//             SNUtil.log("No recipe found for '" + id + "'.");
//             //td = RecipeUtil.addRecipe(tt, TechGroup.Personal, TechCategory.Tools, new string[]{"Personal", "Tools"});
//             return tt;
//         }
//
//         replacedTechRecipes.Add(tt);
//         td.Ingredients.Clear();
//         if (titanium > 0)
//             td.Ingredients.Add(new Ingredient(TechType.Titanium, titanium));
//         foreach (var i in items)
//             td.Ingredients.Add(i);
//         CraftDataHandler.SetTechData(tt, td);
//         return tt;
//     }
//
//     private TechType findFCSItem(string id) {
//         var tt = TechType.None;
//         if (!TechTypeHandler.TryGetModdedTechType(id, out tt))
//             if (!TechTypeHandler.TryGetModdedTechType(id.ToLowerInvariant(), out tt))
//                 TechTypeHandler.TryGetModdedTechType(id.setLeadingCase(false), out tt);
//         return tt == TechType.None ? throw new Exception("Could not find FCS TechType for '" + id + "'") : tt;
//     }
//
//     private Ingredient[] fabric(int amt = 2) {
//         return [new(TechType.FiberMesh, amt)];
//     }
//
//     private Ingredient[] speaker() {
//         return [
//             new(TechType.Silicone, 1), new(TechType.CopperWire, 3), new(TechType.Magnetite, 3),
//         ];
//     }
//
//     private Ingredient[] basicGlass(int amt = 1) {
//         return [
//             new(ItemRegistry.instance.getItem("BaseGlass").TechType, amt),
//         ];
//     }
//
//     private Ingredient[] strongGlass(int amt = 2) {
//         return [new(TechType.EnameledGlass, amt)];
//     }
//
//     private Ingredient[] motorized(int mot = 1) {
//         return [
//             new(TechType.WiringKit, 1), new(CraftingItems.getItem(CraftingItems.Items.Motor).TechType, mot),
//         ];
//     }
//
//     private Ingredient[] electronicsTier1(int gold = 1, int wire = 1) {
//         return [
//             new(TechType.CopperWire, wire), new(TechType.Gold, gold),
//         ];
//     }
//
//     private Ingredient[] electronicsTier2(int mag = 1) {
//         return [
//             new(TechType.ComputerChip, 1), new(TechType.Magnetite, mag),
//         ];
//     }
//
//     private Ingredient[] electronicsTier3() {
//         return [new(TechType.AdvancedWiringKit, 1), new(TechType.Polyaniline, 1)];
//     }
//
//     private Ingredient[] reinforced(int lead = 1) {
//         return [new(TechType.TitaniumIngot, 1), new(TechType.Lead, lead)];
//     }
//
//     private Ingredient[] reinforcedStrong(int amt = 1) {
//         return [
//             new(TechType.Lead, 2), new(CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, amt),
//         ];
//     }
//
//     private Ingredient[] floatingPlatform(int titanium) {
//         var ingot = titanium >= 10;
//         return [
//             new(TechType.Silicone, 4),
//             new(ingot ? TechType.TitaniumIngot : TechType.Titanium, ingot ? titanium / 10 : titanium),
//         ];
//     }
// }