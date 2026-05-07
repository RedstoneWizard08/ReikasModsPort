using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Exscansion;

internal static partial class ESPatches {
    [HarmonyPatch]
    public static class LoadMapRangeHook {
        public static MethodBase TargetMethod() {
            return AccessTools.Method(
                typeof(MapRoomFunctionality).GetNestedType(
                    "<LoadMapWorld>d__51",
                    BindingFlags.NonPublic | BindingFlags.Instance
                ),
                "MoveNext"
            );
        }

        public static Type TargetType() {
            return typeof(MapRoomFunctionality);
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                RangePatchLib.replaceMaxRangeReference(codes);
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