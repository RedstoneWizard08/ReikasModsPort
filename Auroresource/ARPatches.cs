using System;
//Working with Lists and Collections
using System.Collections.Generic;
using System.Linq; //Working with Lists and Collections
//For data read/write methods
//More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

//Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Auroresource;

internal static class ARPatches {
    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("SpawnLoot")]
    public static class DrillableDropsHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.getInstruction(
                    0,
                    0,
                    OpCodes.Call,
                    "Drillable",
                    "ChooseRandomResource",
                    true,
                    Type.EmptyTypes
                );
                codes[idx].operand = InstructionHandlers.convertMethodOperand(
                    "ReikaKalseki.Auroresource.ARHooks",
                    "getDrillableDrop",
                    false,
                    typeof(Drillable)
                );
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(BreakableResource))]
    [HarmonyPatch("Awake")]
    public static class ReefbackGrowthIncrease {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.Auroresource.ARHooks",
                        "onBreakableResourceSpawn",
                        false,
                        typeof(BreakableResource)
                    )
                );
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(MapRoomFunctionality))]
    [HarmonyPatch("Start")]
    public static class MapRoomModuleAddHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.Auroresource.ARHooks",
                        "onMapRoomSpawn",
                        false,
                        typeof(MapRoomFunctionality)
                    )
                );
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(Geyser))]
    [HarmonyPatch("Start")]
    public static class GeyserSpawn {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.patchInitialHook(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.Auroresource.ARHooks",
                        "onGeyserSpawn",
                        false,
                        typeof(Geyser)
                    )
                );
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.logErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }
}