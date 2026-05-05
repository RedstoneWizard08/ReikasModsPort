using System;
//Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
//For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using ReikaKalseki.DIAlterra;

using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.AqueousEngineering;

public static class AEPatches {

	[HarmonyPatch(typeof(WaterPark))]
	[HarmonyPatch("Update")]
	public static class ACUHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.AqueousEngineering.AEHooks", "tickACU", false, typeof(WaterPark)));
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(WaterPark))]
	[HarmonyPatch("TryBreed")]
	public static class ACUBreedHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			InsnList codes = [];
			try {
				codes.add(OpCodes.Ldarg_0);
				codes.add(OpCodes.Ldarg_1);
				codes.invoke("ReikaKalseki.AqueousEngineering.AEHooks", "tryBreedACU", false, new Type[] { typeof(WaterPark), typeof(WaterParkCreature) });
				codes.add(OpCodes.Ret);
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(WaterPark))]
	[HarmonyPatch("CanDropItemInside")]
	public static class WaterParkItemDroppabilityHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			InsnList codes = [];
			try {
				codes.add(OpCodes.Ldarg_0);
				codes.invoke("ReikaKalseki.AqueousEngineering.AEHooks", "canAddItemToACU", false, typeof(Pickupable));
				codes.add(OpCodes.Ret);
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(VoxelandGrassBuilder))]
	[HarmonyPatch("CreateUnityMeshes")]
	public static class TerrainGrassHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				codes.patchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.AqueousEngineering.AEHooks", "onChunkGenGrass", false, typeof(IVoxelandChunk2)));
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(uGUI_CameraDrone))]
	[HarmonyPatch("LateUpdate")]
	public static class CameraFuzzHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				for (var i = 0; i < codes.Count; i++) {
					var ci = codes[i];
					if (ci.opcode == OpCodes.Callvirt) {
						var mi = (MethodInfo)ci.operand;
						if (mi.Name == "GetScreenDistance") {
							ci.operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.AqueousEngineering.AEHooks", "getCameraDistanceForRenderFX", false, typeof(MapRoomCamera), typeof(MapRoomScreen));
						}
					}
				}
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	/* handled in NuclearReactorFuelSystem
	[HarmonyPatch(typeof(BaseNuclearReactor))]
	[HarmonyPatch("Update")]
	public static class NuclearReactorPowerHook {

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			InsnList codes = new InsnList(instructions);
			try {
				PatchLib.addPowerGenHook("BaseNuclearReactor", codes);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}*/

	[HarmonyPatch(typeof(BaseBioReactor))]
	[HarmonyPatch("Update")]
	public static class BioReactorPowerHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				PatchLib.addPowerGenHook("BaseBioReactor", codes);
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(FiltrationMachine))]
	[HarmonyPatch("UpdateFiltering")]
	public static class WaterFilterPowerCostHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				for (var i = codes.Count - 1; i >= 0; i--) {
					if (codes[i].LoadsConstant(0.85F)) {
						codes.InsertRange(i + 1, new InsnList{
							new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.AqueousEngineering.AEHooks", "getWaterFilterPowerCost", false, typeof(float), typeof(FiltrationMachine)),
						});
					}
				}
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(Charger))]
	[HarmonyPatch("Update")]
	public static class ChargerSpeedHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				for (var i = codes.Count - 1; i >= 0; i--) {
					if (InstructionHandlers.matchOperands(codes[i].operand, InstructionHandlers.convertFieldOperand("Charger", "chargeSpeed"))) {
						codes.InsertRange(i + 1, new InsnList{
							new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.AqueousEngineering.AEHooks", "getChargerSpeed", false, typeof(float), typeof(Charger)),
						});
					}
				}
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(PlaceTool))]
	[HarmonyPatch("OnPlace")]
	public static class PlaceableDecoHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.AqueousEngineering.AEHooks", "onPlacedItem", false, typeof(PlaceTool)));
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(BaseNuclearReactor))]
	[HarmonyPatch("Start")]
	public static class NuclearReactorHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.AqueousEngineering.AEHooks", "onNuclearReactorSpawn", false, typeof(BaseNuclearReactor)));
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(BaseNuclearReactor))]
	[HarmonyPatch("Update")]
	public static class NuclearReactorOverride {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			InsnList codes = [];
			try {
				codes.add(OpCodes.Ret);
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(uGUI_EquipmentSlot))]
	[HarmonyPatch("SetActive")]
	public static class ReactorSlotHook {
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
			var codes = new InsnList(instructions);
			try {
				codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_1), InstructionHandlers.createMethodCall("ReikaKalseki.AqueousEngineering.AEHooks", "onEquipmentSlotActivated", false, typeof(uGUI_EquipmentSlot), typeof(bool)));
				InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	private static class PatchLib {

		internal static void addPowerGenHook(string caller, InsnList codes) {
			var idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, caller, "ProducePower", true, new Type[]{typeof(float)});
			codes.InsertRange(idx + 1, new InsnList { new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.AqueousEngineering.AEHooks", "getReactorGeneration", false, typeof(float), typeof(MonoBehaviour)) });
		}

	}

}