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
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                InsnList li = [
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.Exscansion.ESHooks",
                        nameof(ESHooks.GetScannerMaxRangeSq),
                        false,
                        new string[0]
                    ),
                ];
                codes.ReplaceConstantWithMethodCall(250000F, li);
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