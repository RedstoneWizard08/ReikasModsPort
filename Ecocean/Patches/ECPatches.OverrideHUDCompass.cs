using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Ecocean;

internal static partial class ECPatches {
    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch(nameof(uGUI_DepthCompass.UpdateCompass))]
    public static class OverrideHUDCompass {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "uGUI_Compass",
                    "set_direction",
                    true,
                    new Type[] { typeof(float) }
                );
                codes[idx].operand = InstructionHandlers.ConvertMethodOperand(
                    "ReikaKalseki.Ecocean.ECHooks",
                    nameof(ECHooks.SetHUDCompassDirection),
                    false,
                    typeof(uGUI_Compass),
                    typeof(float)
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