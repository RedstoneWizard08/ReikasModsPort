using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch(nameof(Survival.UpdateHunger))]
    public static class AmbientHealHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.getInstruction(0, 0, OpCodes.Ldc_R4, 0.041666668F);
                codes.Insert(
                    idx + 1,
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.SeaToSea.C2CHooks",
                        nameof(C2CHooks.GetAmbientHealAmount),
                        false,
                        typeof(float)
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