using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(Stalker))]
    [HarmonyPatch(nameof(Stalker.CheckLoseTooth))]
    public static class StalkerToothDropHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Call,
                    "Stalker",
                    "LoseTooth",
                    true,
                    new Type[0]
                );
                codes[idx].operand = InstructionHandlers.convertMethodOperand(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.StalkerTryDropTooth),
                    false,
                    typeof(Stalker)
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