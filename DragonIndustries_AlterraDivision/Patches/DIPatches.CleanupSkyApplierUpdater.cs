using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(SkyApplierUpdater))]
    [HarmonyPatch(nameof(SkyApplierUpdater.Update))]
    public static class CleanupSkyApplierUpdater {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = InstructionHandlers.getInstruction(
                    codes,
                    0,
                    0,
                    OpCodes.Callvirt,
                    "SkyApplier",
                    "UpdateSkyIfNecessary",
                    true,
                    new Type[0]
                );
                codes[idx] = InstructionHandlers.createMethodCall(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    "updateSkyApplier",
                    false,
                    typeof(SkyApplier)
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