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
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    1,
                    OpCodes.Stfld,
                    "uGUI_ResourceTracker+Blip",
                    "techType"
                );
                codes[idx] = InstructionHandlers.CreateMethodCall(
                    "ReikaKalseki.Exscansion.ESHooks",
                    nameof(ESHooks.SetResourcePingType),
                    false,
                    typeof(uGUI_ResourceTracker.Blip),
                    typeof(TechType)
                );
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