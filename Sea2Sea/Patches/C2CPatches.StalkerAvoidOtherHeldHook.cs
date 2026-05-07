using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(CollectShiny))]
    [HarmonyPatch(nameof(CollectShiny.Perform))]
    public static class StalkerAvoidOtherHeldHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx2 = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Stfld,
                    "CollectShiny",
                    "shinyTarget"
                );
                var idx1 = codes.GetLastOpcodeBefore(idx2, OpCodes.Ldc_I4_0);
                InstructionHandlers.NullInstructions(codes, idx1, idx2);
                codes[idx1] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.OnShinyTargetIsCurrentlyHeldByStalker),
                    false,
                    typeof(CollectShiny)
                );
                //codes.RemoveRange();
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.LogErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }
    }
}