using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(PropulsionCannon))]
    [HarmonyPatch(nameof(PropulsionCannon.TraceForGrabTarget))]
    public static class PropulsionGrabPositionFix {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Call,
                    "UWE.Utils",
                    "SpherecastIntoSharedBuffer",
                    false,
                    new Type[] {
                        typeof(Vector3), typeof(float), typeof(Vector3), typeof(float), typeof(int),
                        typeof(QueryTriggerInteraction),
                    }
                );
                codes[idx - 1] = new CodeInstruction(OpCodes.Ldc_I4_1);
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