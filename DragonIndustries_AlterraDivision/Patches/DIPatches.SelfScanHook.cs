using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(PDAScanner))]
    [HarmonyPatch(nameof(PDAScanner.Scan))]
    public static class SelfScanHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Callvirt,
                    "Story.StoryGoal",
                    "Trigger",
                    false,
                    new Type[0]
                );
                codes.Insert(
                    idx + 1,
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        "onSelfScan",
                        false,
                        new Type[0]
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