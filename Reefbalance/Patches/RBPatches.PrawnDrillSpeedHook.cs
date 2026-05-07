using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Reefbalance;

internal static partial class RBPatches {
    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch(nameof(Drillable.OnDrill))]
    public static class PrawnDrillSpeedHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(0, 0, OpCodes.Ldc_R4, 5F);
                codes[idx] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Reefbalance.ReefbalanceMod",
                    nameof(ReefbalanceMod.GetDrillingSpeed),
                    false,
                    typeof(Drillable),
                    typeof(Exosuit)
                );
                codes.InsertRange(
                    idx,
                    new InsnList { new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2) }
                );
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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