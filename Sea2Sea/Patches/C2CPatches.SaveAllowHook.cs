using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea;

internal static partial class C2CPatches {
    [HarmonyPatch(typeof(IngameMenu))]
    [HarmonyPatch(nameof(IngameMenu.GetAllowSaving))]
    public static class SaveAllowHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.patchEveryReturnPre(
                    InstructionHandlers.createMethodCall(
                        "ReikaKalseki.SeaToSea.C2CHooks",
                        nameof(C2CHooks.AllowSaving),
                        false,
                        typeof(bool)
                    )
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