using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Exscansion;

internal static partial class ESPatches {
    [HarmonyPatch(typeof(MapRoomFunctionality))]
    [HarmonyPatch(nameof(MapRoomFunctionality.OnResourceDiscovered))]
    public static class UpdateScannerResourceDistanceCheck {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                InsnList li = [
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.Exscansion.ESHooks",
                        "getScannerMaxRangeSq",
                        false,
                        new string[0]
                    ),
                ];
                codes.replaceConstantWithMethodCall(250000F, li);
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