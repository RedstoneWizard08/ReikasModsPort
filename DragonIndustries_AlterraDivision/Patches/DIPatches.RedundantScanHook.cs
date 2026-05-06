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
    [HarmonyPriority(Priority.Last)]
    public static class RedundantScanHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Call,
                    "CraftData",
                    "AddToInventory",
                    true,
                    new Type[] { typeof(TechType), typeof(int), typeof(bool), typeof(bool) }
                );
                var idx0 = InstructionHandlers.getLastOpcodeBefore(codes, idx - 1, OpCodes.Call);
                codes.RemoveRange(idx0 + 1, idx - idx0);
                codes.Insert(
                    idx0 + 1,
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        "onRedundantFragmentScan",
                        false,
                        new Type[0]
                    )
                );
                InstructionHandlers.logCompletedPatch(MethodBase.GetCurrentMethod(), instructions);
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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