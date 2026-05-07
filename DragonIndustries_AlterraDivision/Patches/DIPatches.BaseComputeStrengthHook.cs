using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(BaseHullStrength))]
    [HarmonyPatch(nameof(BaseHullStrength.OnPostRebuildGeometry))]
    public static class BaseComputeStrengthHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.LogPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = [];
            try {
                codes.Add(OpCodes.Ldarg_0);
                codes.Invoke(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    nameof(DIHooks.RecomputeBaseHullStrength),
                    false,
                    new Type[] { typeof(BaseHullStrength) }
                );
                codes.Add(OpCodes.Ret);
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