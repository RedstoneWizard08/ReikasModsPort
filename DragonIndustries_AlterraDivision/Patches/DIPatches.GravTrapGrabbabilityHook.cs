using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ReikaKalseki.DIAlterra;

internal static partial class DIPatches {
    [HarmonyPatch(typeof(Gravsphere))]
    [HarmonyPatch(nameof(Gravsphere.IsValidTarget))]
    public static class GravTrapGrabbabilityHook {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            InstructionHandlers.logPatchStart(MethodBase.GetCurrentMethod(), instructions);
            InsnList codes = [];
            try {
                codes.add(OpCodes.Ldarg_0);
                codes.add(OpCodes.Ldarg_1);
                codes.invoke(
                    "ReikaKalseki.DIAlterra.DIHooks",
                    "canGravTrapGrab",
                    false,
                    new Type[] { typeof(Gravsphere), typeof(GameObject) }
                );
                codes.add(OpCodes.Ret);
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