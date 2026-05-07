using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Exscansion;

internal static partial class ESPatches {
    [HarmonyPatch(typeof(uGUI_ResourceTracker))]
    [HarmonyPatch(nameof(uGUI_ResourceTracker.UpdateBlips))]
    public static class PingHUDVisibility {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var call = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    nameof(ESHooks.UpdatePingHUDVisibility),
                    false,
                    typeof(uGUI_ResourceTracker)
                );
                codes.PatchEveryReturnPre(new CodeInstruction(OpCodes.Ldarg_0), call);
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