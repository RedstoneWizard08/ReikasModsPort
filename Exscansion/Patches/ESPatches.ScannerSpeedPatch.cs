using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Exscansion;

internal static partial class ESPatches {
    [HarmonyPatch(typeof(MapRoomFunctionality))]
    [HarmonyPatch(nameof(MapRoomFunctionality.UpdateScanRangeAndInterval))]
    public static class ScannerSpeedPatch {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                RangePatchLib.replaceBaseSpeedReference(codes);
                RangePatchLib.replaceSpeedBonusReference(codes);
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