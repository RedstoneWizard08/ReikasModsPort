using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(
        typeof(WaterTemperatureSimulation),
        nameof(WaterTemperatureSimulation.GetTemperature),
        typeof(Vector3)
    )]
    public static class WaterTempOverride {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchEveryReturnPre(InjectHook);
                InstructionHandlers.LogCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
            } catch (Exception e) {
                InstructionHandlers.LogErroredPatch(MethodBase.GetCurrentMethod());
                FileLog.Log(e.Message);
                FileLog.Log(e.StackTrace);
                FileLog.Log(e.ToString());
            }

            return codes.AsEnumerable();
        }

        private static void InjectHook(InsnList codes, int idx) {
            codes.Insert(
                idx,
                InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.GetWaterTemperature),
                    false,
                    typeof(float),
                    typeof(WaterTemperatureSimulation),
                    typeof(Vector3)
                )
            );
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_1));
            codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
        }
    }
}