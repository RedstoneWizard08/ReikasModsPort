using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(uGUI_Pings))]
    [HarmonyPatch(nameof(uGUI_Pings.UpdatePings))]
    public static class PingPositionHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "PingInstance", "origin");
                codes[idx] = InstructionHandlers.createMethodCall(
                    "ReikaKalseki.SeaToSea.C2CHooks",
                    nameof(C2CHooks.GetApparentPingPosition),
                    false,
                    typeof(PingInstance)
                );
                codes.RemoveAt(idx + 1);
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