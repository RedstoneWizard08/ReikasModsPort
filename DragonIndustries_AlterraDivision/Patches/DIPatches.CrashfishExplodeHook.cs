using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Crash))]
    [HarmonyPatch(nameof(Crash.Detonate))]
    public static class CrashfishExplodeHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            var codes = new InsnList(instructions);
            try {
                codes.PatchInitialHook(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    InstructionHandlers.CreateMethodCall(
                        "ReikaKalseki.DIAlterra.DIHooks",
                        nameof(DIHooks.OnCrashfishExplode),
                        false,
                        typeof(Crash)
                    )
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