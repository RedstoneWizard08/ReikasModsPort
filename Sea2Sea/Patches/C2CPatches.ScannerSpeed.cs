using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(PDAScanner))]
    [HarmonyPatch(nameof(PDAScanner.Scan))]
    public static class ScannerSpeed {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldloc_S, 9);
                codes.Insert(
                    idx + 1,
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.SeaToSea.C2CHooks",
                        nameof(C2CHooks.GetScannerSpeed),
                        false,
                        typeof(float)
                    )
                );
                //FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
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