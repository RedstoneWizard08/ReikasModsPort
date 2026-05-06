using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(TemperatureDamage))]
    [HarmonyPatch(nameof(TemperatureDamage.Start))]
    public static class EnvironmentalDamageRateChange {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getFirstOpcode(codes, 0, OpCodes.Ldc_R4);
                codes[idx].operand = 1F / EnvironmentalDamageSystem.EnviroRateScalar;
                codes[idx + 1].operand = 1F / EnvironmentalDamageSystem.EnviroRateScalar;
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