using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Ecocean;

internal static partial class ECPatches {
    [HarmonyPatch(typeof(CyclopsHornButton))]
    [HarmonyPatch(nameof(CyclopsHornButton.OnPress))]
    public static class CyclopsHornHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                var idx = codes.GetInstruction(
                    0,
                    0,
                    OpCodes.Callvirt,
                    "FMOD_CustomEmitter",
                    "Play",
                    true,
                    new Type[0]
                );
                codes.Insert(
                    idx,
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.Ecocean.ECHooks",
                        nameof(ECHooks.HonkCyclopsHorn),
                        false,
                        typeof(CyclopsHornButton)
                    )
                );
                codes.Insert(idx, new CodeInstruction(OpCodes.Ldarg_0));
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