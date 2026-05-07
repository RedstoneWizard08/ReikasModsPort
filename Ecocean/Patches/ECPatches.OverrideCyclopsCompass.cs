using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;
using UnityEngine;

namespace ReikaKalseki.Ecocean;

internal static partial class ECPatches {
    [HarmonyPatch(typeof(CyclopsCompassHUD))]
    [HarmonyPatch(nameof(CyclopsCompassHUD.Update))]
    public static class OverrideCyclopsCompass {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "UnityEngine.Transform",
                    "set_rotation",
                    true,
                    new Type[] { typeof(Quaternion) }
                );
                codes[idx].operand = InstructionHandlers.ConvertMethodOperand(
                    "ReikaKalseki.Ecocean.ECHooks",
                    nameof(ECHooks.SetCyclopsCompassDirection),
                    false,
                    typeof(Transform),
                    typeof(Quaternion)
                );
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