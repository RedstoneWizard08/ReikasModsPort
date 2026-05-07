using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(RepulsionCannon))]
    [HarmonyPatch(nameof(RepulsionCannon.OnToolUseAnim))]
    public static class RepulsabilityHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                PatchLib.PatchPropulsability(
                    codes,
                    codes.GetInstruction(0, 0, OpCodes.Ldc_R4, 1300F),
                    true,
                    new CodeInstruction(OpCodes.Ldloc_S, 11)
                );
                PatchLib.PatchPropulsability(
                    codes,
                    codes.GetInstruction(0, 0, OpCodes.Ldc_R4, 400F),
                    false,
                    new CodeInstruction(OpCodes.Ldloc_S, 11)
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