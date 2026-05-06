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
    public static class PingHUDGenerationHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    1,
                    OpCodes.Stfld,
                    "uGUI_ResourceTracker+Blip",
                    "techType"
                );
                codes[idx] = InstructionHandlers.createMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    nameof(ESHooks.SetResourcePingType),
                    false,
                    typeof(uGUI_ResourceTracker.Blip),
                    typeof(TechType)
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