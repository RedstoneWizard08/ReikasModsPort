using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Auroresource;

internal static partial class ARPatches {
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
}